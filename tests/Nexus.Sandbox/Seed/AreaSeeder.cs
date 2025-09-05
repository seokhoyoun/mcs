using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Sandbox.Seed.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nexus.Sandbox.Seed
{
    internal class AreaSeeder : IDataSeeder
    {
        private readonly RedisAreaRepository _repo;

        public AreaSeeder(RedisAreaRepository repo)
        {
            _repo = repo;
        }
        public async Task SeedAsync()
        {
            var areas = LoadAreasFromLocalFile();

            await _repo.AddRangeAsync(areas);
        }

        private List<Area> LoadAreasFromLocalFile()
        {
            var areas = new List<Area>();
            var filePath = "areas.json";

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

                return areas;
            }

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

            return areas;
        }
    }
}
