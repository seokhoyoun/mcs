
using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Services;

namespace Nexus.Core.Domain.Models.Areas.Services
{
    public class AreaService
    {
        public IReadOnlyList<Area> Areas => _areas.AsReadOnly();

        private readonly ILogger<AreaService> _logger;
        private readonly IAreaRepository _areaRepository;
        private readonly LocationService _locationService;

        private readonly List<Area> _areas = new List<Area>();

        public AreaService(ILogger<AreaService> logger, IAreaRepository areaRepository, LocationService locationService)
        {
            _logger = logger;
            _areaRepository = areaRepository;
            _locationService = locationService;
        }

        public async Task InitializeAreaService()
        {
            var areas = LoadAreasFromLocalFile();
            _areas.AddRange(areas);

            foreach (var area in _areas)
            {
                var locations = new List<Location>();
                
                _locationService.AddLocations(locations);
            }
            
            await _areaRepository.InitializeAreasAsync(areas);
        }

        private List<Area> LoadAreasFromLocalFile()
        {
            var areas = new List<Area>();
            var filePath = "/app/data/areas.json";
            _logger.LogInformation($"JSON 파일 경로: {filePath}");

            if (!File.Exists(filePath))
            {
                for (int areaIdx = 1; areaIdx <= 2; areaIdx++)
                {
                    var areaId = $"A{areaIdx:00}";
                    var areaName = $"AREA{areaIdx:00}";

                    var cassetteLocations = new List<CassetteLocation>();
                    var trayLocations = new List<TrayLocation>();

                    for (int cassetteIdx = 1; cassetteIdx <= 6; cassetteIdx++)
                    {
                        var cassetteLocationId = $"{areaId}.CP{cassetteIdx:00}";
                        cassetteLocations.Add(new CassetteLocation(
                            id: cassetteLocationId,
                            name: $"{areaName}_CASSETTEPORT{cassetteIdx:00}",
                            locationType: ELocationType.Cassette));

                        for (int trayIdx = 1; trayIdx <= 6; trayIdx++)
                        {
                            var trayLocationId = $"{areaId}.CP{cassetteIdx:00}.TP{trayIdx:00}";
                            trayLocations.Add(new TrayLocation(
                                id: trayLocationId,
                                name: $"{areaName}_CASSETTEPORT{cassetteIdx:00}_TRAYPORT{trayIdx:00}",
                                locationType: ELocationType.Tray));
                        }
                    }

                    var sets = new List<Set>();
                    for (int i = 1; i <= 20; i++)
                    {
                        var memoryLocations = new List<MemoryLocation>();
                        for (int m = 1; m <= 32; m++)
                        {
                            memoryLocations.Add(new MemoryLocation(
                                id: $"{areaId}.SET{i:00}.MP{m:00}",
                                name: $"{areaName}_SET{i:00}_MEMORYPORT{m:00}",
                                locationType: ELocationType.Memory));
                        }

                        sets.Add(new Set(
                            id: $"{areaId}.SET{i:00}",
                            name: $"{areaName}_SET{i:00}",
                            memoryPorts: memoryLocations));
                    }

                    var area = new Area(areaId, areaName, cassetteLocations, trayLocations, sets);
                    areas.Add(area);
                }

                // 파일로 저장
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                var baseJson = JsonSerializer.Serialize(areas, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, baseJson);

                _logger.LogWarning($"JSON 파일을 찾을 수 없습니다: {filePath}");
                _logger.LogInformation($"기본 Area 데이터를 {filePath}에 저장했습니다.");
                return areas;
            }
            try
            {
                var json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    //IncludeFields = true
                };
                var fileAreas = JsonSerializer.Deserialize<List<Area>>(json, options);

                if (fileAreas != null)
                {
                    areas.AddRange(fileAreas);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "로컬 파일에서 Area 데이터를 로드하는 중 오류가 발생했습니다.");
                throw;
            }
            return areas;
        }
    }
}
