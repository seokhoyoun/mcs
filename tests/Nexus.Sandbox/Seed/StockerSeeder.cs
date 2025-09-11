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
            int columns = 6; // 6 x 2 grid for 12 ports
            int spacingX = 30;
            int spacingY = 30;

            for (int i = 1; i <= 12; i++)
            {
                string portId = $"{stockerId}.CP{i:00}";

                CassetteLocation cassette = new CassetteLocation(
                    id: portId,
                    name: $"{stockerName}_cp{i:00}"
                );

                int zeroBased = i - 1;
                int col = zeroBased % columns;
                int row = zeroBased / columns;
                uint x = (uint)(col * spacingX);
                uint y = (uint)(row * spacingY);
                uint z = 0;
                cassette.Position = new Position(x, y, z);

                cassetteLocations.Add(cassette);
            }

            Stocker stocker = new Stocker(stockerId, stockerName, cassetteLocations);
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
