using System.Collections.Concurrent;
using StackExchange.Redis;
using System.Text.Json;
using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Areas.Enums;
using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Lots;
using Nexus.Core.Domain.Models.Lots.Enums;
using Nexus.Core.Domain.Models.Lots.Interfaces;
using Nexus.Core.Domain.Models.Lots.DTO;
using Nexus.Core.Domain.Models.Plans;
using Nexus.Core.Domain.Models.Plans.Enums;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Models.Robots;
using Nexus.Core.Domain.Models.Robots.Interfaces;
using Nexus.Orchestrator.Application.Robots.Simulation;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Orchestrator.Application.Scheduler.Services
{
    public class SchedulerService
    {
        #region Fields

        private readonly IAreaService _areaService;
        private readonly ILocationService _locationService;
        private readonly ILotRepository _lotRepository;
        private readonly ILogger<SchedulerService> _logger;
        private readonly IConnectionMultiplexer _redis;
        private readonly IRobotRepository _robotRepository;
        private readonly RobotMotionService _motionService;

        private readonly ConcurrentQueue<string> _pendingLotIds = new();

        #endregion

        #region Constructor

        public SchedulerService(
            IAreaService areaService,
            ILocationService locationService,
            ILotRepository lotRepository,
            ILogger<SchedulerService> logger,
            IConnectionMultiplexer redis,
            IRobotRepository robotRepository,
            RobotMotionService motionService)
        {
            _areaService = areaService;
            _locationService = locationService;
            _lotRepository = lotRepository;
            _logger = logger;
            _redis = redis;
            _robotRepository = robotRepository;
            _motionService = motionService;
        }

        #endregion

        #region Public Methods

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            await SubscribeToLotCreatedEventsAsync(stoppingToken);
        }

        #endregion

        #region Message Subscription

        private async Task SubscribeToLotCreatedEventsAsync(CancellationToken stoppingToken)
        {
            ISubscriber sub = _redis.GetSubscriber();
            string channel = "events:lot:publish";
            await sub.SubscribeAsync(RedisChannel.Literal(channel), (redisChannel, value) =>
            {
                try
                {
                    string message = value.ToString();
                    HandleLotCreatedEventMessage(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling lot publish message");
                }
            });
        }

        private void HandleLotCreatedEventMessage(string message)
        {
          try
          {
              LotPublishedEventDto? dto = JsonSerializer.Deserialize<LotPublishedEventDto>(message);
              if (dto == null)
              {
                  return;
              }
              if (string.IsNullOrEmpty(dto.LotId))
              {
                  return;
              }
              _ = ProcessLotAsync(dto.LotId, CancellationToken.None);
          }
          catch (Exception ex)
          {
              _logger.LogError(ex, "Failed to parse lot publish message: {Message}", message);
          }
        }

        #endregion

        #region Lot Processing


        private async Task ProcessLotAsync(string lotId, CancellationToken cancellationToken)
        {
            try
            {
                // Lot 정보 조회
                Lot? lot = await _lotRepository.GetByIdAsync(lotId, cancellationToken);
                if (lot == null)
                {
                    _logger.LogWarning("Lot not found with ID: {LotId}", lotId);
                    return;
                }

                // PlanGroup 생성 (조건 체크 없이 무조건 생성)
                CreatePlanGroups(lot);

                // Simulate robot motions for Stocker->Area paths
                await SimulateStockerToAreaAsync(lot, cancellationToken);

                // Lot 상태를 Assigned로 변경
                lot.Status = ELotStatus.Assigned;
                await _lotRepository.UpdateAsync(lot, cancellationToken);

                _logger.LogInformation("Successfully created plan groups for LotId: {LotId}", lotId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing lot {LotId}", lotId);

                // 에러 발생 시 다시 대기열에 추가 (재시도)
                _pendingLotIds.Enqueue(lotId);
            }
        }

        private async Task<bool> MoveRobotToAsync(string robotId, uint targetX, uint targetY, double speed, CancellationToken cancellationToken)
        {
            _motionService.ScheduleMove(robotId, (double)targetX, (double)targetY, speed);

            const int pollMs = 200;
            const int maxWaitMs = 60000;
            int waited = 0;
            while (waited < maxWaitMs)
            {
                Position? pos = await _robotRepository.GetPositionAsync(robotId, cancellationToken);
                if (pos != null)
                {
                    if (pos.X == targetX && pos.Y == targetY)
                    {
                        return true;
                    }
                }
                try
                {
                    await Task.Delay(pollMs, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                waited += pollMs;
            }
            return false;
        }

        private async Task SimulateStockerToAreaAsync(Lot lot, CancellationToken cancellationToken)
        {
            IReadOnlyList<Robot> robots = await _robotRepository.GetAllAsync(cancellationToken);
            if (robots == null || robots.Count == 0)
            {
                _logger.LogWarning("No robots available for simulation");
                return;
            }
            Robot robot = robots[0];
            double speed = 80.0; // units per second

            foreach (LotStep step in lot.LotSteps)
            {
                Area? emptyArea = GetEmptyAreaForCassettes(step.Cassettes.Count);
                if (emptyArea == null)
                {
                    emptyArea = _areaService.GetAvailableAreaForCassette();
                }
                if (emptyArea == null)
                {
                    _logger.LogWarning("No area available for simulation of lot step {LotStepId}", step.Id);
                    continue;
                }

                string amrIdSim = $"{robot.Id}.CP01";
                MarkerLocation amrLocation = GetOrCreateAMRLocation(amrIdSim, $"AMR Port of {robot.Id}");

                foreach (Cassette cassette in step.Cassettes)
                {
                    CassetteLocation? stockerLocation = _locationService.GetCassetteLocationById(cassette.Id);
                    if (stockerLocation == null)
                    {
                        _logger.LogWarning("Cannot find stocker location for cassette {CassetteId}", cassette.Id);
                        continue;
                    }

                    CassetteLocation? targetCassettePort = _areaService.GetAvailableCassettePort(emptyArea);
                    if (targetCassettePort == null)
                    {
                        _logger.LogWarning("No available cassette port in area {AreaId} for cassette {CassetteId}", emptyArea.Id, cassette.Id);
                        continue;
                    }

                    // Move robot to AMR location first
                    bool ok1 = await MoveRobotToAsync(robot.Id, amrLocation.Position.X, amrLocation.Position.Y, speed, cancellationToken);
                    if (!ok1)
                    {
                        _logger.LogWarning("Robot {RobotId} did not reach AMR location in time", robot.Id);
                    }

                    // Then move to target cassette port in area
                    bool ok2 = await MoveRobotToAsync(robot.Id, targetCassettePort.Position.X, targetCassettePort.Position.Y, speed, cancellationToken);
                    if (!ok2)
                    {
                        _logger.LogWarning("Robot {RobotId} did not reach target cassette port {PortId} in time", robot.Id, targetCassettePort.Id);
                    }
                }
            }
        }

        private void CreatePlanGroups(Lot lot)
        {
            foreach (LotStep lotStep in lot.LotSteps)
            {
                _logger.LogInformation("Creating plan groups for LotStep: {LotStepId}", lotStep.Id);

                // 1. StockerToArea PlanGroup 생성 (우선적으로 비어있는 Area 찾기)
                PlanGroup stockerToAreaPlanGroup = CreateStockerToAreaPlanGroup(lotStep);
                lotStep.PlanGroups.Add(stockerToAreaPlanGroup);

                // 2. AreaToSet PlanGroup 생성
                PlanGroup areaToSetPlanGroup = CreateAreaToSetPlanGroup(lotStep);
                lotStep.PlanGroups.Add(areaToSetPlanGroup);

                // 3. SetToArea PlanGroup 생성
                PlanGroup setToAreaPlanGroup = CreateSetToAreaPlanGroup(lotStep);
                lotStep.PlanGroups.Add(setToAreaPlanGroup);

                // 4. AreaToStocker PlanGroup 생성
                PlanGroup areaToStockerPlanGroup = CreateAreaToStockerPlanGroup(lotStep);
                lotStep.PlanGroups.Add(areaToStockerPlanGroup);

                _logger.LogInformation("Created {PlanGroupCount} plan groups for LotStep: {LotStepId}",
                    lotStep.PlanGroups.Count, lotStep.Id);
            }
        }

        #endregion

        #region PlanGroup Creation Methods

        private PlanGroup CreateStockerToAreaPlanGroup(LotStep lotStep)
        {
            string planGroupId = Guid.NewGuid().ToString();
            PlanGroup planGroup = new PlanGroup(planGroupId, $"{lotStep.Name}_StockerToArea", EPlanGroupType.StockerToArea);

            _logger.LogInformation("Creating StockerToArea plan group for {CassetteCount} cassettes", lotStep.Cassettes.Count);

            // 비어있는 Area를 우선적으로 찾기
            Area? emptyArea = GetEmptyAreaForCassettes(lotStep.Cassettes.Count);
            if (emptyArea == null)
            {
                _logger.LogWarning("No empty area available for {CassetteCount} cassettes. Using available area instead", lotStep.Cassettes.Count);
                emptyArea = _areaService.GetAvailableAreaForCassette();
            }

            if (emptyArea == null)
            {
                _logger.LogError("No available area found for StockerToArea plan group");
                return planGroup;
            }

            _logger.LogInformation("Selected Area {AreaId} (Status: {AreaStatus}) for cassette placement",
                emptyArea.Id, emptyArea.Status);

            // AMR 마커 Location 생성/조회 (경유지) - robot-specific
            string robotIdForAmr = "RBT01";
            try
            {
                IReadOnlyList<Robot> robots = _robotRepository.GetAllAsync().GetAwaiter().GetResult();
                if (robots != null && robots.Count > 0 && robots[0] != null)
                {
                    robotIdForAmr = robots[0].Id;
                }
            }
            catch { }
            string amrId = $"{robotIdForAmr}.CP01";
            MarkerLocation amrLocation = GetOrCreateAMRLocation(amrId, $"AMR Port of {robotIdForAmr}");

            foreach (Cassette cassette in lotStep.Cassettes)
            {
                try
                {
                    // 1. 카세트의 현재 위치 조회 (스토커)
                    CassetteLocation? stockerLocation = _locationService.GetCassetteLocationById(cassette.Id);
                    if (stockerLocation == null)
                    {
                        _logger.LogWarning("Cassette location not found for CassetteId: {CassetteId}", cassette.Id);
                        continue;
                    }

                    // 2. Area에서 사용 가능한 카세트 포트 찾기 (목표 위치)
                    CassetteLocation? targetCassettePort = _areaService.GetAvailableCassettePort(emptyArea);
                    if (targetCassettePort == null)
                    {
                        _logger.LogWarning("No available cassette port found in area {AreaId} for cassette {CassetteId}",
                            emptyArea.Id, cassette.Id);
                        continue;
                    }

                    // 3. Plan 생성
                    Plan plan = new Plan(Guid.NewGuid().ToString(), $"StockerToArea_{cassette.Id}");

                    // PlanStep 1: CassetteLoad (스토커 → AMR)
                    PlanStep cassetteLoadStep = new PlanStep(
                        Guid.NewGuid().ToString(),
                        $"1. Load_from_Stocker",
                        1,
                        EPlanStepAction.CassetteLoad,
                        stockerLocation.Id);

                    // Job 1: 스토커에서 물류로봇으로 카세트 로드
                    Job loadJob = new Job(
                        Guid.NewGuid().ToString(),
                        $"1. Load_{cassette.Id}_from_{stockerLocation.Id}",
                        1,
                        stockerLocation,    // from: 스토커 포트
                        amrLocation         // to: AMR 카세트 포트
                    );

                    cassetteLoadStep.Jobs.Add(loadJob);
                    cassetteLoadStep.CarrierIds.Add(cassette.Id);
                    plan.PlanSteps.Add(cassetteLoadStep);

                    // PlanStep 2: CassetteUnload (AMR → Area)
                    PlanStep cassetteUnloadStep = new PlanStep(
                        Guid.NewGuid().ToString(),
                        "2. Unload_to_Area",
                        2,
                        EPlanStepAction.CassetteUnload,
                        targetCassettePort.Id);

                    // Job 1: 물류로봇에서 Area로 카세트 언로드
                    Job unloadJob = new Job(
                        Guid.NewGuid().ToString(),
                        $"1. Unload_{cassette.Id}_to_Area",
                        1,
                        amrLocation,        // from: AMR 카세트 포트
                        targetCassettePort  // to: Area 카세트 포트
                    );

                    cassetteUnloadStep.Jobs.Add(unloadJob);
                    cassetteUnloadStep.CarrierIds.Add(cassette.Id);
                    plan.PlanSteps.Add(cassetteUnloadStep);

                    planGroup.Plans.Add(plan);

                    _logger.LogInformation("Created StockerToArea plan: {CassetteId} from {From} via {Via} to {To}",
                        cassette.Id, stockerLocation.Id, amrLocation.Id, targetCassettePort.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating StockerToArea plan for cassette {CassetteId}", cassette.Id);
                }
            }

            _logger.LogInformation("Created StockerToArea plan group with {PlanCount} plans", planGroup.Plans.Count);
            return planGroup;
        }

        private PlanGroup CreateAreaToSetPlanGroup(LotStep lotStep)
        {
            string planGroupId = Guid.NewGuid().ToString();
            PlanGroup planGroup = new PlanGroup(planGroupId, $"{lotStep.Name}_AreaToSet", EPlanGroupType.AreaToSet);

            foreach (Cassette cassette in lotStep.Cassettes)
            {
                try
                {
                    Area? availableArea = _areaService.GetAvailableAreaForCassette();
                    if (availableArea == null)
                    {
                        _logger.LogWarning("No available area found for AreaToSet plan");
                        continue;
                    }

                    Plan plan = new Plan(Guid.NewGuid().ToString(), $"AreaToSet_{cassette.Id}");

                    // 트레이 로드 및 메모리 픽앤플레이스 작업
                    PlanStep trayLoadStep = new PlanStep(
                        Guid.NewGuid().ToString(),
                        "TrayLoad_from_Area",
                        1,
                        EPlanStepAction.TrayLoad,
                        $"{availableArea.Id}.CP01.TP01"); // 구체적인 위치 명시

                    PlanStep memoryPickPlaceStep = new PlanStep(
                        Guid.NewGuid().ToString(),
                        "MemoryPickAndPlace_to_Set",
                        2,
                        EPlanStepAction.MemoryPickAndPlace,
                        $"{availableArea.Id}.SET01.MP01"); // 구체적인 위치 명시

                    plan.PlanSteps.Add(trayLoadStep);
                    plan.PlanSteps.Add(memoryPickPlaceStep);
                    planGroup.Plans.Add(plan);

                    _logger.LogDebug("Created AreaToSet plan for cassette {CassetteId}", cassette.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating AreaToSet plan for cassette {CassetteId}", cassette.Id);
                }
            }

            return planGroup;
        }

        private PlanGroup CreateSetToAreaPlanGroup(LotStep lotStep)
        {
            string planGroupId = Guid.NewGuid().ToString();
            PlanGroup planGroup = new PlanGroup(planGroupId, $"{lotStep.Name}_SetToArea", EPlanGroupType.SetToArea);

            foreach (Cassette cassette in lotStep.Cassettes)
            {
                try
                {
                    Area? availableArea = _areaService.GetAvailableAreaForCassette();
                    if (availableArea == null)
                    {
                        _logger.LogWarning("No available area found for SetToArea plan");
                        continue;
                    }

                    Plan plan = new Plan(Guid.NewGuid().ToString(), $"SetToArea_{cassette.Id}");

                    // 메모리 회수 및 트레이 언로드 작업
                    PlanStep memoryPickPlaceStep = new PlanStep(
                        Guid.NewGuid().ToString(),
                        "MemoryPickAndPlace_from_Set",
                        1,
                        EPlanStepAction.MemoryPickAndPlace,
                        $"{availableArea.Id}.SET01.MP01"); // from 위치

                    PlanStep trayUnloadStep = new PlanStep(
                        Guid.NewGuid().ToString(),
                        "TrayUnload_to_Area",
                        2,
                        EPlanStepAction.TrayUnload,
                        $"{availableArea.Id}.CP01.TP01"); // to 위치

                    plan.PlanSteps.Add(memoryPickPlaceStep);
                    plan.PlanSteps.Add(trayUnloadStep);
                    planGroup.Plans.Add(plan);

                    _logger.LogDebug("Created SetToArea plan for cassette {CassetteId}", cassette.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating SetToArea plan for cassette {CassetteId}", cassette.Id);
                }
            }

            return planGroup;
        }

        private PlanGroup CreateAreaToStockerPlanGroup(LotStep lotStep)
        {
            string planGroupId = Guid.NewGuid().ToString();
            PlanGroup planGroup = new PlanGroup(planGroupId, $"{lotStep.Name}_AreaToStocker", EPlanGroupType.AreaToStocker);

            // AMR 마커 Location 조회 - robot-specific
            string robotIdForAmr2 = "RBT01";
            try
            {
                IReadOnlyList<Robot> robots2 = _robotRepository.GetAllAsync().GetAwaiter().GetResult();
                if (robots2 != null && robots2.Count > 0 && robots2[0] != null)
                {
                    robotIdForAmr2 = robots2[0].Id;
                }
            }
            catch { }
            string amrId2 = $"{robotIdForAmr2}.CP01";
            MarkerLocation amrLocation = GetOrCreateAMRLocation(amrId2, $"AMR Port of {robotIdForAmr2}");

            const string stockerPrefix = "ST01.CP";
            int stockerPortIndex = 1;

            foreach (Cassette cassette in lotStep.Cassettes)
            {
                try
                {
                    Area? availableArea = _areaService.GetAvailableAreaForCassette();
                    if (availableArea == null)
                    {
                        _logger.LogWarning("No available area found for AreaToStocker plan");
                        continue;
                    }

                    CassetteLocation? availableCassettePort = _areaService.GetAvailableCassettePort(availableArea);
                    if (availableCassettePort == null)
                    {
                        _logger.LogWarning("No available cassette port found for AreaToStocker plan");
                        continue;
                    }

                    // 스토커 반납 위치 생성
                    CassetteLocation stockerPortLocation = CreateOrGetStockerLocation($"{stockerPrefix}{stockerPortIndex:D2}");
                    Plan plan = new Plan(Guid.NewGuid().ToString(), $"AreaToStocker_{cassette.Id}");

                    // Job 1: Area에서 AMR로 카세트 로드
                    Job loadJob = new Job(
                        Guid.NewGuid().ToString(),
                        $"Load_{cassette.Id}_from_Area",
                        1,
                        availableCassettePort,  // from: Area 카세트 포트
                        amrLocation            // to: AMR 카세트 포트
                    );

                    // Job 2: AMR에서 스토커로 카세트 언로드
                    Job unloadJob = new Job(
                        Guid.NewGuid().ToString(),
                        $"Unload_{cassette.Id}_to_Stocker",
                        1,
                        amrLocation,           // from: AMR 카세트 포트
                        stockerPortLocation    // to: 스토커 포트
                    );

                    // PlanStep 1: CassetteLoad (Area → AMR)
                    PlanStep cassetteLoadStep = new PlanStep(
                        Guid.NewGuid().ToString(),
                        "Load_from_Area",
                        1,
                        EPlanStepAction.CassetteLoad,
                        availableCassettePort.Id);

                    cassetteLoadStep.Jobs.Add(loadJob);
                    cassetteLoadStep.CarrierIds.Add(cassette.Id);

                    // PlanStep 2: CassetteUnload (AMR → Stocker)
                    PlanStep cassetteUnloadStep = new PlanStep(
                        Guid.NewGuid().ToString(),
                        "Unload_to_Stocker",
                        2,
                        EPlanStepAction.CassetteUnload,
                        stockerPortLocation.Id);

                    cassetteUnloadStep.Jobs.Add(unloadJob);
                    cassetteUnloadStep.CarrierIds.Add(cassette.Id);

                    plan.PlanSteps.Add(cassetteLoadStep);
                    plan.PlanSteps.Add(cassetteUnloadStep);
                    planGroup.Plans.Add(plan);

                    _logger.LogDebug("Created AreaToStocker plan: {CassetteId} from {From} via {Via} to {To}",
                        cassette.Id, availableCassettePort.Id, amrLocation.Id, stockerPortLocation.Id);

                    stockerPortIndex++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating AreaToStocker plan for cassette {CassetteId}", cassette.Id);
                }
            }

            return planGroup;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// AMR Location을 조회하거나 생성합니다.
        /// </summary>
        /// <param name="locationId">AMR Location ID (예: AMR.CP01)</param>
        /// <param name="locationName">AMR Location 이름</param>
        /// <returns>AMR MarkerLocation</returns>
        private MarkerLocation GetOrCreateAMRLocation(string locationId, string locationName)
        {
            MarkerLocation? amrLocation = _locationService.GetMarkerLocationById(locationId);
            if (amrLocation == null)
            {
                // AMR Location이 없으면 생성
                amrLocation = new MarkerLocation(locationId, locationName);

                // LocationService에 등록 (실제 구현에서는 초기화 시점에 미리 등록되어야 함)
                _locationService.AddLocations(new[] { amrLocation });

                _logger.LogInformation("Created new AMR location: {LocationId}", locationId);
            }

            return amrLocation;
        }

        /// <summary>
        /// 스토커 Location을 조회하거나 생성합니다.
        /// </summary>
        /// <param name="locationId">스토커 Location ID (예: ST01.CP01)</param>
        /// <returns>스토커 CassetteLocation</returns>
        private CassetteLocation CreateOrGetStockerLocation(string locationId)
        {
            CassetteLocation? stockerLocation = _locationService.GetCassetteLocationById(locationId);
            if (stockerLocation == null)
            {
                // 스토커 Location이 없으면 생성
                stockerLocation = new CassetteLocation(locationId, locationId);

                // LocationService에 등록
                _locationService.AddLocations(new[] { stockerLocation });

                _logger.LogInformation("Created new stocker location: {LocationId}", locationId);
            }

            return stockerLocation;
        }

        /// <summary>
        /// 비어있는 Area를 우선적으로 찾습니다.
        /// </summary>
        /// <param name="requiredCassetteSlots">필요한 카세트 슬롯 수</param>
        /// <returns>비어있는 Area 또는 null</returns>
        private Area? GetEmptyAreaForCassettes(int requiredCassetteSlots)
        {
            try
            {
                // 모든 Area 중에서 Idle 상태인 것을 우선적으로 선택
                List<Area> emptyAreas = _areaService.Areas
                    .Where(area => area.Status == EAreaStatus.Idle)
                    .Where(area => GetAvailableCassettePortCount(area) >= requiredCassetteSlots)
                    .OrderBy(area => GetCassetteOccupancy(area)) // 가장 비어있는 것부터
                    .ToList();

                if (emptyAreas.Any())
                {
                    Area selectedArea = emptyAreas.First();
                    _logger.LogInformation("Found empty area {AreaId} with {AvailableSlots} available cassette slots",
                        selectedArea.Id, GetAvailableCassettePortCount(selectedArea));
                    return selectedArea;
                }

                _logger.LogWarning("No empty area found with {RequiredSlots} available slots", requiredCassetteSlots);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while searching for empty area");
                return null;
            }
        }

        /// <summary>
        /// Area의 사용 가능한 카세트 포트 수를 계산합니다.
        /// </summary>
        private int GetAvailableCassettePortCount(Area area)
        {
            return area.CassetteLocations.Count(cp => cp.CurrentItemId == null);
        }

        /// <summary>
        /// Area의 카세트 점유율을 계산합니다 (0.0 = 비어있음, 1.0 = 가득참).
        /// </summary>
        private double GetCassetteOccupancy(Area area)
        {
            if (area.CassetteLocations.Count == 0)
            {
                return 1.0; // 포트가 없으면 가득 찬 것으로 간주
            }

            int occupiedPorts = area.CassetteLocations.Count(cp => cp.CurrentItemId != null);
            return (double)occupiedPorts / area.CassetteLocations.Count;
        }

        #endregion
    }
}
