using System.Text.Json;
using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Gateway.Services.Interfaces;

namespace Nexus.Gateway.Services
{
    public class AreaCreationService : IAreaCreationService
    {
        private readonly IAreaService _areaService;
        private readonly IAreaRepository _areaRepository;
        private readonly ILocationService _locationService;
 
        private readonly ILogger<AreaCreationService> _logger;

        public AreaCreationService(
            IAreaService areaService,
            IAreaRepository areaRepository,
            ILocationService locationService,
            ILogger<AreaCreationService> logger)
        {
            _areaService = areaService;
            _areaRepository = areaRepository;
            _locationService = locationService;
            _logger = logger;
        }

        public async Task<string> CreateAreaAsync(string jsonPayload, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting area creation process...");

                // JSON을 직접 파싱
                using JsonDocument document = JsonDocument.Parse(jsonPayload);
                JsonElement root = document.RootElement;

                if (!root.TryGetProperty("areas", out JsonElement areasElement))
                {
                    throw new ArgumentException("Missing 'areas' property in JSON payload");
                }

                List<Area> areas = new List<Area>();

                //foreach (var areaElement in areasElement.EnumerateArray())
                //{
                //    var area = ParseAreaFromJson(areaElement);
                //    areas.Add(area);
                //}

                // 각 Area의 위치 정보 등록
                int totalLocations = 0;
                foreach (Area area in areas)
                {
                    _logger.LogInformation("Processing area: {AreaId} - {AreaName}", area.Id, area.Name);

                    List<Location> locations = new List<Location>();

                    locations.AddRange(area.CassetteLocations);

                    locations.AddRange(area.TrayLocations);

                    // Memory 위치 추가 (Set에서 추출)
                    foreach (Set set in area.Sets)
                    {
                        locations.AddRange(set.MemoryLocations);
                    }

                    _locationService.AddLocations(locations);
                    totalLocations += locations.Count;

                    _logger.LogInformation("Added {LocationCount} locations for area {AreaId}",
                        locations.Count, area.Id);
                }

                // Redis에 Area 데이터 저장
                await _areaRepository.InitializeAreasAsync(areas);

                _logger.LogInformation("Area creation completed successfully. " +
                    "Created {AreaCount} areas with {LocationCount} total locations.",
                    areas.Count, totalLocations);

                return $"Successfully created {areas.Count} area(s) with {totalLocations} locations.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create areas");
                throw;
            }
        }
    }
}
