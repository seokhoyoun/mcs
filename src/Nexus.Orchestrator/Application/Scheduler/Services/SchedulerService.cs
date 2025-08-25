using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Areas.Services;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Lots;
using Nexus.Core.Domain.Models.Lots.Events;
using Nexus.Core.Domain.Models.Plans;
using Nexus.Core.Domain.Models.Plans.Enums;
using Nexus.Core.Domain.Models.Plans.Interfaces;
using Nexus.Core.Domain.Models.Plans.Strategies;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Messaging;
using Nexus.Shared.Application.Interfaces;
using System.Collections.Generic;

namespace Nexus.Orchestrator.Application.Scheduler.Services
{
    public class SchedulerService
    {
        private readonly IAreaService _areaService;

        public SchedulerService(IAreaService areaService)
        {
            _areaService = areaService;
        }

        public async Task RunAsync(CancellationToken stoppingToken)
        {
            
        }

        public void CreatePlanGroup(Lot lot)
        {
            foreach (var lotStep in lot.LotSteps)
            {
                string id = Guid.NewGuid().ToString();
                string name = lotStep.Name;
                EPlanGroupType groupType = EPlanGroupType.StockerToArea; 

                var pg = new PlanGroup(id, name, groupType);

                foreach (var cassette in lotStep.Cassettes)
                {
                    // AreaService에서 사용 가능한 Area 조회
                    var availableArea = _areaService.GetAvailableAreaForCassette();
                    
                    if (availableArea == null)
                    {
                        // 사용 가능한 Area가 없는 경우 처리
                        throw new InvalidOperationException($"No available area found for cassette {cassette.Id}");
                    }

                    // 사용 가능한 카세트 포트 찾기
                    var availableCassettePort = _areaService.GetAvailableCassettePort(availableArea);
                    
                    if (availableCassettePort == null)
                    {
                        throw new InvalidOperationException($"No available cassette port found in area {availableArea.Id}");
                    }

                    Plan plan = new Plan("PID", $"{pg.Id}_{cassette.Id}");
                    
                    // 스토커에서 에어리어로 이동하는 PlanStep 생성
                    PlanStep cassetteLoadStep = new PlanStep("ps", "ps", 1, EPlanStepAction.CassetteLoad, availableCassettePort.Id);
                    PlanStep cassetteUnloadStep = new PlanStep("ps", "ps", 2, EPlanStepAction.CassetteUnload, availableCassettePort.Id);

                    plan.PlanSteps.Add(cassetteLoadStep);
                    plan.PlanSteps.Add(cassetteUnloadStep);

                    pg.Plans.Add(plan);
                }
            }
        }
    }
}