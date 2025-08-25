using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Gateway.Services.Commands;
using Nexus.Gateway.Services.Interfaces;
using Nexus.Shared.Application.Interfaces;
using System.Text.Json;

namespace Nexus.Gateway.Services
{
    public class AreaCreationService : IAreaCreationService
    {
        private readonly IAreaService _areaService;
        private readonly IAreaRepository _areaRepository;
        private readonly ILocationService _locationService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<AreaCreationService> _logger;

        public AreaCreationService(
            IAreaService areaService,
            IAreaRepository areaRepository,
            ILocationService locationService,
            IEventPublisher eventPublisher,
            ILogger<AreaCreationService> logger)
        {
            _areaService = areaService;
            _areaRepository = areaRepository;
            _locationService = locationService;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<string> CreateAreaAsync(string jsonPayload, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting area creation process...");

                // JSON�� ���� �Ľ�
                using var document = JsonDocument.Parse(jsonPayload);
                var root = document.RootElement;

                if (!root.TryGetProperty("areas", out var areasElement))
                {
                    throw new ArgumentException("Missing 'areas' property in JSON payload");
                }

                var areas = new List<Area>();

                //foreach (var areaElement in areasElement.EnumerateArray())
                //{
                //    var area = ParseAreaFromJson(areaElement);
                //    areas.Add(area);
                //}

                // �� Area�� ��ġ ���� ���
                var totalLocations = 0;
                foreach (var area in areas)
                {
                    _logger.LogInformation("Processing area: {AreaId} - {AreaName}", area.Id, area.Name);

                    var locations = new List<Location>();

                    // Cassette ��ġ �߰�
                    locations.AddRange(area.CassetteLocations);

                    // Tray ��ġ �߰�
                    locations.AddRange(area.TrayLocations);

                    // Memory ��ġ �߰� (Set���� ����)
                    foreach (var set in area.Sets)
                    {
                        locations.AddRange(set.MemoryPorts);
                    }

                    _locationService.AddLocations(locations);
                    totalLocations += locations.Count;

                    _logger.LogInformation("Added {LocationCount} locations for area {AreaId}",
                        locations.Count, area.Id);
                }

                // Redis�� Area ������ ����
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