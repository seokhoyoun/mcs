using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Locations.Services;
using Nexus.Core.Domain.Shared.Bases;
using Nexus.Shared.Application.Interfaces;
using System.Text.Json;
using Nexus.Core.Domain.Models.Areas.Enums;
using Nexus.Core.Domain.Models.Locations.Interfaces;

namespace Nexus.Core.Domain.Models.Areas.Services
{
    public class AreaService : BaseDataService<Area, string>, IAreaService
    {
        public IReadOnlyList<Area> Areas => _areas.AsReadOnly();

        private readonly IAreaRepository _areaRepository;
        private readonly ILocationService _locationService;

        private readonly List<Area> _areas = new List<Area>();

        public AreaService(ILogger<AreaService> logger, IAreaRepository areaRepository, IEventPublisher eventPublisher, ILocationService locationService) : base(logger, areaRepository)
        {
            _areaRepository = areaRepository;
            _locationService = locationService;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            var areas = await _areaRepository.GetAllAsync();
            _areas.AddRange(areas);
        }

        /// <summary>
        /// 카세트 적재가 가능한 Area를 조회합니다.
        /// </summary>
        public Area? GetAvailableAreaForCassette()
        {
            var availableAreas = _areas
                .Where(area => area.Status == EAreaStatus.Idle) // 유휴 상태인 Area만 선택
                .ToList();

            // 사용 가능한 카세트 포트가 있는 Area 반환
            foreach (var area in availableAreas)
            {
                var availablePort = GetAvailableCassettePort(area);
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
        public CassetteLocation? GetAvailableCassettePort(Area area)
        {
            return area.CassetteLocations
                .FirstOrDefault(cp => cp.CurrentItem == null); // 비어있는 카세트 포트
        }

   
    }
}
