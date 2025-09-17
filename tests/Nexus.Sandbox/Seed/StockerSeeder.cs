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

namespace Nexus.Sandbox.Seed
{
    internal class StockerSeeder : IDataSeeder
    {
        private readonly RedisStockerRepository _repo;

        public StockerSeeder(RedisStockerRepository repo)
        {
            _repo = repo;
        }

        public async Task SeedAsync()
        {
            List<Stocker> stockers = LoadStockersFromLocalFile();
            await _repo.AddRangeAsync(stockers);
        }

        private List<Stocker> LoadStockersFromLocalFile()
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
            int cassetteDepth = 60; // length along Z (depth)
            int cassetteHeight = 60; // fixed vertical thickness (Y axis)
            int floorHeight = cassetteHeight; // Z-position offset per floor (vertical stacking)

            for (int i = 1; i <= itemsPerFloor * floors; i++)
            {
                string portId = $"{stockerId}.CP{i:00}";

                CassetteLocation cassette = new CassetteLocation(
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
                cassette.Position = new Position(x, y, z);
                cassette.Width = 30;
                cassette.Height = (uint)cassetteHeight; // fixed
                cassette.Depth = (uint)cassetteDepth; // length on Z
                cassette.ParentId = string.Empty;
                cassette.IsVisible = true;

                cassetteLocations.Add(cassette);

                // 각 CassetteLocation에 대해 TrayLocation 생성 (카세트 내부에 층층이 쌓이도록)
                for (int trayIdx = 1; trayIdx <= 6; trayIdx++)
                {
                    string trayLocationId = $"{stockerId}.CP{i:00}.TP{trayIdx:00}";
                    TrayLocation tray = new TrayLocation(
                        id: trayLocationId,
                        name: $"{stockerName}_cp{i:00}_tp{trayIdx:00}");

                    // 트레이 크기 설정 (카세트 내부 여유를 두고 중앙 정렬)
                    tray.Width = 30;
                    tray.Height = 4; // 얇은 트레이 높이로 레이어 시각화
                    tray.Depth = 30;

                    // 카세트 내부 중앙 정렬 (X/Y 평면)
                    tray.IsRelativePosition = true;
                    uint trayX = (uint)(((int)cassette.Width - (int)tray.Width) / 2);
                    uint trayY = (uint)(((int)cassette.Depth - (int)tray.Depth) / 2);

                    // 카세트 높이 범위 내에서 균등 분포 (Z=vertical)
                    int layers = 6;
                    int available = cassetteHeight - (int)tray.Height;
                    if (available < 0)
                    {
                        available = 0;
                    }
                    int step = layers > 1 ? (available / (layers - 1)) : 0;
                    int zeroBasedIndex = trayIdx - 1;
                    uint trayZ = (uint)(zeroBasedIndex * step);

                    tray.ParentId = portId;
                    tray.IsVisible = true;
                    tray.Position = new Position(trayX, trayY, trayZ);
                    trayLocations.Add(tray);
                }
            }

            Stocker stocker = new Stocker(stockerId, stockerName, cassetteLocations, trayLocations);
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
