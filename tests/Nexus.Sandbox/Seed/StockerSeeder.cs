using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Stockers;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Sandbox.Seed.Interfaces;
using Nexus.Core.Domain.Shared.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Nexus.Core.Domain.Standards;
using System.Diagnostics;

namespace Nexus.Sandbox.Seed
{
    internal class StockerSeeder : IDataSeeder
    {
        private readonly RedisStockerRepository _repo;
        private readonly RedisDimensionRepository _dimRepo;

        public StockerSeeder(RedisStockerRepository repo, RedisDimensionRepository dimRepo)
        {
            _repo = repo;
            _dimRepo = dimRepo;
        }

        public async Task SeedAsync()
        {
            List<Stocker> stockers = await LoadStockersFromLocalFileAsync();
            await _repo.AddRangeAsync(stockers);
        }

        private async Task<List<Stocker>> LoadStockersFromLocalFileAsync()
        {
            List<Stocker> stockers = new List<Stocker>();
            string filePath = "tmp\\stockers.json";

            // 기본 Stocker 하나 생성
            string stockerId = "ST01";
            string stockerName = "Main Stocker";

            List<CassetteLocation> cassetteLocations = new List<CassetteLocation>();
            List<TrayLocation> trayLocations = new List<TrayLocation>();

            int columns = 6; // 6 per floor
            int spacingX = 30;
            int spacingY = 30;
            int itemsPerFloor = 6; // total 12 -> 6 ports x 2 floors
            int floors = 2;
            //int cassetteDepth = 60; // length along Z (depth)
            int cassetteHeight = 60; // fixed vertical thickness (Y axis)
            int floorHeight = cassetteHeight; // Z-position offset per floor (vertical stacking)
            DimensionStandard? cassetteStd = await _dimRepo.GetByIdAsync("transport:cassette");
            DimensionStandard? cassetteLocationStd = await _dimRepo.GetByIdAsync("location:cassette");
            DimensionStandard? trayLocationStd = await _dimRepo.GetByIdAsync("location:tray");
            uint cassetteLocationW = cassetteLocationStd != null ? cassetteLocationStd.Width : 30u;
            uint cassetteLocationD = cassetteLocationStd != null ? cassetteLocationStd.Depth : 60u;
            uint trayLocationW = trayLocationStd != null ? trayLocationStd.Width : 30u;
            uint trayLocationH = trayLocationStd != null ? trayLocationStd.Height : 4u;
            uint trayLocationD = trayLocationStd != null ? trayLocationStd.Depth : 30u;

            Debug.Assert(cassetteStd != null, "Cassette dimension standard not found");
            uint cassetteW = cassetteStd.Width;
            uint cassetteD = cassetteStd.Depth;
            uint cassetteH = cassetteStd.Height;
            for (int i = 1; i <= itemsPerFloor * floors; i++)
            {
                string portId = $"{stockerId}.CP{i:00}";

                CassetteLocation cassetteLocation = new CassetteLocation(
                    id: portId,
                    name: $"{stockerName}_cp{i:00}"
                );

                int zeroBased = i - 1;
                int floorIndex = zeroBased / itemsPerFloor; // 0 or 1
                int indexInFloor = zeroBased % itemsPerFloor;
                int col = indexInFloor % columns; // 0..5
                int rowInFloor = indexInFloor / columns; // always 0 when columns==6

                uint x = (uint)(col * spacingX);
                uint y = (uint)(rowInFloor * spacingY);
                // Use Position.Z to stack floors vertically (0 = 1F, cassetteHeight = 2F)
                uint z = (uint)(floorIndex * floorHeight);
                cassetteLocation.Position = new Position(x, y, z);
                cassetteLocation.Width = cassetteLocationW;
                cassetteLocation.Height = (uint)cassetteHeight; // keep spacing
                cassetteLocation.Depth = cassetteLocationD;
                cassetteLocation.ParentId = string.Empty;
                cassetteLocation.IsVisible = true;

                cassetteLocations.Add(cassetteLocation);

                uint baseTrayLocationZ = (cassetteLocation.Height - cassetteStd.Height) / 2;

                // 각 CassetteLocation에 대해 TrayLocation 생성 (카세트 내부에 층층이 쌓이도록)
                for (int trayIdx = 1; trayIdx <= 6; trayIdx++)
                {
                    string trayLocationId = $"{stockerId}.CP{i:00}.TP{trayIdx:00}";
                    TrayLocation trayLocation = new TrayLocation(
                        id: trayLocationId,
                        name: $"{stockerName}_cp{i:00}_tp{trayIdx:00}");

                    // 트레이 크기 설정 (dimension)
                    trayLocation.Width = trayLocationW;
                    trayLocation.Height = trayLocationH;
                    trayLocation.Depth = trayLocationD;

                    // 카세트 내부 중앙 정렬 (X/Y 평면)
                    trayLocation.IsRelativePosition = true;
                    uint trayX = (uint)(((int)cassetteW - (int)trayLocationW) / 2);
                    uint trayY = (uint)(((int)cassetteD - (int)trayLocationD) / 2);

                    // 카세트 높이 범위 내에서 균등 분포 (Z=vertical)
                    int layers = 6;
                    int available = (int)cassetteH - (int)trayLocationH;
                    if (available < 0)
                    {
                        available = 0;
                    }
                    int step = layers > 1 ? (available / (layers - 1)) : 0;
                    int zeroBasedIndex = trayIdx - 1;
                    uint trayZ = (uint)(zeroBasedIndex * step) + baseTrayLocationZ;

                    trayLocation.ParentId = portId;
                    trayLocation.IsVisible = true;
                    trayLocation.Position = new Position(trayX, trayY, trayZ);
                    trayLocations.Add(trayLocation);
                }
            }

            Stocker stocker = new Stocker(stockerId, stockerName, cassetteLocations, trayLocations);

            CassetteLocation? sample = cassetteLocations.LastOrDefault();
            Debug.Assert(sample != null);
            sample.CurrentItemId = "CST01";

            stockers.Add(stocker);

            // JSON 파일로 저장
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            string baseJson = JsonSerializer.Serialize(stockers, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, baseJson);

            // 파일이 이미 있는 경우 → 로딩
            string json = File.ReadAllText(filePath);
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            List<Stocker>? fileStockers = JsonSerializer.Deserialize<List<Stocker>>(json, options);

            if (fileStockers != null)
            {
                stockers.AddRange(fileStockers);
            }

            return stockers;
        }
    }
}
