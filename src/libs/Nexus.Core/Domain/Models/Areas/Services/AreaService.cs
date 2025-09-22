using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Areas.Enums;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Areas.Services
{
    public class AreaService : BaseDataService<Area, string>, IAreaService
    {
        

        private readonly IAreaRepository _areaRepository;
        private readonly ILocationService _locationService;

        private readonly List<Area> _areas = new List<Area>();
        private bool _initialized = false;
        private readonly object _initLock = new object();
        private Task? _initTask;

        public AreaService(ILogger<AreaService> logger, IAreaRepository areaRepository, ILocationService locationService) : base(logger, areaRepository)
        {
            _areaRepository = areaRepository;
            _locationService = locationService;
        }

        private async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
        {
            if (_initialized)
            {
                return;
            }
            Task? startTask = null;
            lock (_initLock)
            {
                if (_initialized)
                {
                    return;
                }
                if (_initTask == null)
                {
                    _initTask = InitializeCoreAsync(cancellationToken);
                }
                startTask = _initTask;
            }
            await startTask;
        }

        private async Task InitializeCoreAsync(CancellationToken cancellationToken)
        {
            try
            {
                IReadOnlyList<Area> areas = await _areaRepository.GetAllAsync(cancellationToken);
                _areas.Clear();
                if (areas != null && areas.Count > 0)
                {
                    _areas.AddRange(areas);
                }
                else
                {
                    _logger.LogWarning("초기화된 Area 데이터가 없습니다.");
                }
                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AreaService 초기화 중 오류 발생");
                throw;
            }
        }
        

        /// <summary>
        /// 카세트 적재가 가능한 Area를 조회합니다.
        /// </summary>
        public async Task<Area?> GetAvailableAreaForCassetteAsync(CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken);
            List<Area> availableAreas = _areas
                .Where(area => area.Status == EAreaStatus.Idle) // 유휴 상태인 Area만 선택
                .ToList();

            // 사용 가능한 카세트 포트가 있는 Area 반환
            foreach (Area area in availableAreas)
            {
                CassetteLocation? availablePort = await GetAvailableCassetteLocationAsync(area);
                if (availablePort != null)
                {
                    return area;
                }
            }

            return null;
        }

        /// <summary>
        /// 지정된 Area에서 사용 가능한 카세트 포트를 조회합니다.
        /// </summary>
        public async Task<CassetteLocation?> GetAvailableCassetteLocationAsync(Area area, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken);
            // Location.CurrentItemId 는 기본값이 빈 문자열이므로 null 또는 빈 문자열 모두를 비어있음으로 간주합니다.
            return area.CassetteLocations
                .FirstOrDefault(cp => string.IsNullOrEmpty(cp.CurrentItemId));
        }

        public async Task<Area?> GetEmptyAreaForCassettesAsync(int requiredCassetteSlots, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken);
            try
            {
                List<Area> emptyAreas = _areas
                    .Where(area => area.Status == EAreaStatus.Idle)
                    .Where(area => area.CassetteLocations.Count(cp => string.IsNullOrEmpty(cp.CurrentItemId)) >= requiredCassetteSlots)
                    .OrderBy(area =>
                        area.CassetteLocations.Count == 0
                            ? 1.0
                            : (double)area.CassetteLocations.Count(cp => !string.IsNullOrEmpty(cp.CurrentItemId)) / area.CassetteLocations.Count)
                    .ToList();

                if (emptyAreas.Any())
                {
                    Area selectedArea = emptyAreas.First();
                    _logger.LogInformation("Found empty area {AreaId} with {AvailableSlots} available cassette slots",
                        selectedArea.Id,
                        selectedArea.CassetteLocations.Count(cp => string.IsNullOrEmpty(cp.CurrentItemId)));
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

   
    }
}
