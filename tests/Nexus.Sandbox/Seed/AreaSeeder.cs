using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Shared.Bases;
using Nexus.Core.Domain.Standards;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Sandbox.Seed.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nexus.Sandbox.Seed
{
    internal class AreaSeeder : IDataSeeder
    {
        private readonly RedisAreaRepository _repo;
        private readonly RedisDimensionRepository _dimRepo;

        public AreaSeeder(RedisAreaRepository repo, RedisDimensionRepository dimRepo)
        {
            _repo = repo;
            _dimRepo = dimRepo;
        }
        public async Task SeedAsync()
        {
            List<Area> areas = await LoadAreasFromLocalFileAsync();

            await _repo.AddRangeAsync(areas);
        }

        private async Task<List<Area>> LoadAreasFromLocalFileAsync()
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
                Dictionary<string, CassetteLocation> cassetteMap = new Dictionary<string, CassetteLocation>();
                Dictionary<string, TrayLocation> trayMap = new Dictionary<string, TrayLocation>();
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
                DimensionStandard? cassetteStd = await _dimRepo.GetByIdAsync("transport:cassette");
                DimensionStandard? cassetteLocationStd = await _dimRepo.GetByIdAsync("location:cassette");
                DimensionStandard? trayLocationStd = await _dimRepo.GetByIdAsync("location:tray");
                DimensionStandard? memoryLocationStd = await _dimRepo.GetByIdAsync("location:memory");

                uint cassetteLocationW = cassetteLocationStd != null ? cassetteLocationStd.Width : 30u;
                uint cassetteLocationH = cassetteLocationStd != null ? cassetteLocationStd.Height : 60u;
                uint cassetteLocationD = cassetteLocationStd != null ? cassetteLocationStd.Depth : 60u;
                uint trayLocationW = trayLocationStd != null ? trayLocationStd.Width : 30u;
                uint trayLocationH = trayLocationStd != null ? trayLocationStd.Height : 4u;
                uint trayLocationD = trayLocationStd != null ? trayLocationStd.Depth : 30u;
                uint memLocationW = memoryLocationStd != null ? memoryLocationStd.Width : 5u;
                uint memLocationH = memoryLocationStd != null ? memoryLocationStd.Height : 5u;
                uint memLocationD = memoryLocationStd != null ? memoryLocationStd.Depth : 5u;

                Debug.Assert(cassetteStd != null, "Cassette dimension standard not found");
                uint cassetteW = cassetteStd.Width;
                uint cassetteD = cassetteStd.Depth;
                uint cassetteH = cassetteStd.Height;

                for (int cassetteIdx = 1; cassetteIdx <= 6; cassetteIdx++)
                {
                    string cassetteLocationId = $"{areaId}.CP{cassetteIdx:00}";
                    CassetteLocation cassetteLocation = new CassetteLocation(
                        id: cassetteLocationId,
                        name: $"{areaName}_cp{cassetteIdx:00}");

                    cassetteLocation.Children = new List<string>();
                    int cassetteZeroBased = cassetteIdx - 1;
                    int cassetteCol = cassetteZeroBased % cassetteColumns;
                    int cassetteRow = cassetteZeroBased / cassetteColumns;
                    uint cassetteX = (uint)(areaBaseX + cassetteCol * cassetteSpacingX);
                    uint cassetteY = (uint)(areaBaseY + cassetteRow * cassetteSpacingY);
                    uint cassetteZ = 0;
                    cassetteLocation.Position = new Position(cassetteX, cassetteY, cassetteZ);
                    cassetteLocation.Width = cassetteLocationW;
                    cassetteLocation.Height = cassetteLocationH;
                    cassetteLocation.Depth = cassetteLocationD;
                    cassetteLocation.Rotation = new Rotation(0, 90, 0); // rotate area cassette orientation
                    cassetteLocation.ParentId = string.Empty;
                    cassetteLocation.IsVisible = true;
                    cassetteLocations.Add(cassetteLocation);
                    cassetteMap[cassetteLocationId] = cassetteLocation;

                    uint baseTrayLocationZ = (cassetteLocation.Height - cassetteStd.Height) / 2;

                    for (int trayIdx = 1; trayIdx <= 6; trayIdx++)
                    {
                        string trayLocationId = $"{areaId}.CP{cassetteIdx:00}.TP{trayIdx:00}";
                        TrayLocation tray = new TrayLocation(
                            id: trayLocationId,
                            name: $"{areaName}_cp{cassetteIdx:00}_tp{trayIdx:00}");
                        tray.Children = new List<string>();
                        // Center trays inside the cassette footprint and stack vertically within cassette height
                        // Size first (from dimension standard)
                        tray.Width = trayLocationW;
                        tray.Height = trayLocationH;
                        tray.Depth = trayLocationD;

                        // Center X/Y within cassette bounds (relative offset)
                        tray.IsRelativePosition = true;
                        uint trayX = (uint)(((int)cassetteW - (int)trayLocationW) / 2);
                        uint trayY = (uint)(((int)cassetteD - (int)trayLocationD) / 2);

                        // Evenly distribute Z within cassette height so all layers fit inside
                        int layers = 6;
                        int available = (int)cassetteH - (int)trayLocationH;
                        if (available < 0)
                        {
                            available = 0;
                        }
                        int step = layers > 1 ? (available / (layers - 1)) : 0;
                        int zeroBasedIndex = trayIdx - 1;
                        uint trayZ = (uint)(zeroBasedIndex * step) + baseTrayLocationZ;

                        tray.ParentId = cassetteLocationId;
                        tray.IsVisible = true;
                        tray.Position = new Position(trayX, trayY, trayZ);
                        trayLocations.Add(tray);
                        cassetteLocation.Children.Add(trayLocationId);
                        trayMap[trayLocationId] = tray;
                    }
                }

                List<Set> sets = new List<Set>();
                // Place sets fully within the intended area bounds
                int setColumns = 5; // 5 columns x 4 rows = 20 sets
                int setSpacingX = 100; // ensure each set block (≈95px wide) does not overlap
                int setSpacingY = 30;  // vertical separation between set blocks
                // Uniform start X for all areas; only Y differs by areaBaseY.
                int setsBaseX = areaBaseX + 100;
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
                        memory.Width = memLocationW;
                        memory.Height = memLocationH;
                        memory.Depth = memLocationD;
                        // Parent tray inference from id: map SET index to CP, MP index to TP (1..6 cyclic)
                        int inferredCpIndex = i % 6 == 0 ? 6 : i % 6;
                        int inferredTpIndex = ((m - 1) % 6) + 1;
                        string trayParentId = $"{areaId}.CP{inferredCpIndex:00}.TP{inferredTpIndex:00}";
                        memory.ParentId = trayParentId;
                        memory.IsVisible = true;

                        memoryLocations.Add(memory);
                        if (trayMap.TryGetValue(trayParentId, out TrayLocation parentTray))
                        {
                            parentTray.Children.Add(memory.Id);
                        }
                    }

                    sets.Add(new Set(
                        id: $"{areaId}.SET{i:00}",
                        name: $"{areaName}_set{i:00}",
                        memoryLocations: memoryLocations));
                }

                Area area = new Area(areaId, areaName, cassetteLocations, trayLocations, sets);
                areas.Add(area);
            }


            //// 초기 스토커 적재 상태: 두 개 카세트를 A01의 앞 슬롯 두 곳에 배치
            //if (areas.Count > 0 && areas[0].CassetteLocations.Count >= 2)
            //{
            //    areas[0].CassetteLocations[0].CurrentItemId = "CST01";
            //    areas[0].CassetteLocations[1].CurrentItemId = "CST02";
            //}

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
