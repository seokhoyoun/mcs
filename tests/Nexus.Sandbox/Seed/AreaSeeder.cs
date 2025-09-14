using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Sandbox.Seed.Interfaces;
using Nexus.Core.Domain.Shared.Bases;
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
            List<Area> areas = LoadAreasFromLocalFile();

            await _repo.AddRangeAsync(areas);
        }

        private List<Area> LoadAreasFromLocalFile()
        {
            List<Area> areas = new List<Area>();
            string filePath = "tmp\\areas.json";


            // Configure distinct startX per area if needed
            Dictionary<string, int> areaStartYMap = new Dictionary<string, int>
                {
                    { "A01", 0 },
                    { "A02", 300 }
                };

            for (int areaIdx = 1; areaIdx <= 2; areaIdx++)
            {
                string areaId = $"A{areaIdx:00}";
                string areaName = $"area{areaIdx:00}";

                List<CassetteLocation> cassetteLocations = new List<CassetteLocation>();
                List<TrayLocation> trayLocations = new List<TrayLocation>();

                int cassetteColumns = 1;
                int cassetteSpacingX = 30;
                int cassetteSpacingY = 30;
                int areaBaseX = 300;
                int areaBaseY = 0;

                if (areaStartYMap.TryGetValue(areaId, out int configuredStartY))
                {
                    areaBaseY = configuredStartY;
                }
                else
                {
                    areaBaseY = (areaIdx - 1) * 1000; // fallback offset pattern
                }

                for (int cassetteIdx = 1; cassetteIdx <= 6; cassetteIdx++)
                {
                    string cassetteLocationId = $"{areaId}.CP{cassetteIdx:00}";
                    CassetteLocation cassette = new CassetteLocation(
                        id: cassetteLocationId,
                        name: $"{areaName}_cp{cassetteIdx:00}");

                    int cassetteZeroBased = cassetteIdx - 1;
                    int cassetteCol = cassetteZeroBased % cassetteColumns;
                    int cassetteRow = cassetteZeroBased / cassetteColumns;
                    uint cassetteX = (uint)(areaBaseX + cassetteCol * cassetteSpacingX);
                    uint cassetteY = (uint)(areaBaseY + cassetteRow * cassetteSpacingY);
                    uint cassetteZ = 0;
                    cassette.Position = new Position(cassetteX, cassetteY, cassetteZ);
                    cassette.Width = 30;
                    cassette.Height = 30;
                    cassetteLocations.Add(cassette);

                    for (int trayIdx = 1; trayIdx <= 6; trayIdx++)
                    {
                        string trayLocationId = $"{areaId}.CP{cassetteIdx:00}.TP{trayIdx:00}";
                        TrayLocation tray = new TrayLocation(
                            id: trayLocationId,
                            name: $"{areaName}_cp{cassetteIdx:00}_tp{trayIdx:00}");
                        // Align tray X/Y with its cassette; use Z as stack index
                        uint trayX = cassetteX;
                        uint trayY = cassetteY;
                        uint trayZ = (uint)trayIdx;
                        tray.Position = new Position(trayX, trayY, trayZ);
                        tray.Width = 20;
                        tray.Height = 20;
                        trayLocations.Add(tray);
                    }
                }

                List<Set> sets = new List<Set>();
                // Place sets fully within the intended area bounds
                int setColumns = 5; // 5 columns x 4 rows = 20 sets
                int setSpacingX = 100; // ensure each set block (≈95px wide) does not overlap
                int setSpacingY = 30;  // vertical separation between set blocks
                // Uniform start X for all areas; only Y differs by areaBaseY.
                int setsBaseX = areaBaseX + 40;
                int setsBaseY = areaBaseY + 10; // small top padding

                for (int i = 1; i <= 20; i++)
                {
                    List<MemoryLocation> memoryLocations = new List<MemoryLocation>();
                    int setZeroBased = i - 1;
                    int setCol = setZeroBased % setColumns;
                    int setRow = setZeroBased / setColumns;
                    int setOriginX = setsBaseX + setCol * setSpacingX;
                    int setOriginY = setsBaseY + setRow * setSpacingY;

                    int memColumns = 16;
                    int memSpacingX = 6;
                    int memSpacingY = 6;

                    for (int m = 1; m <= 32; m++)
                    {
                        MemoryLocation memory = new MemoryLocation(
                            id: $"{areaId}.SET{i:00}.MP{m:00}",
                            name: $"{areaName}_set{i:00}_mp{m:00}");

                        int memZeroBased = m - 1;
                        int memCol = memZeroBased % memColumns;
                        int memRow = memZeroBased / memColumns;
                        uint memX = (uint)(setOriginX + memCol * memSpacingX);
                        uint memY = (uint)(setOriginY + memRow * memSpacingY);
                        uint memZ = 0;
                        memory.Position = new Position(memX, memY, memZ);
                        memory.Width = 5;
                        memory.Height = 5;

                        memoryLocations.Add(memory);
                    }

                    sets.Add(new Set(
                        id: $"{areaId}.SET{i:00}",
                        name: $"{areaName}_set{i:00}",
                        memoryLocations: memoryLocations));
                }

                Area area = new Area(areaId, areaName, cassetteLocations, trayLocations, sets);
                areas.Add(area);
            }

            // 파일로 저장
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            string baseJson = JsonSerializer.Serialize(areas, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, baseJson);



            string json = File.ReadAllText(filePath);
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                //IncludeFields = true
            };
            List<Area>? fileAreas = JsonSerializer.Deserialize<List<Area>>(json, options);

            if (fileAreas != null)
            {
                areas.AddRange(fileAreas);
            }

            return areas;
        }
    }
}
