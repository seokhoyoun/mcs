using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Stockers;
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
            string filePath = "stockers.json";

            if (!File.Exists(filePath))
            {
                // 기본 Stocker 하나 생성
                string stockerId = "ST01";
                string stockerName = "Main Stocker";

                List<CassetteLocation> cassetteLocations = new List<CassetteLocation>();
                for (int i = 1; i <= 12; i++)
                {
                    string portId = $"{stockerId}.CP{i:00}";
                    cassetteLocations.Add(new CassetteLocation(
                        id: portId,
                        name: $"{stockerName}_cp{i:00}",
                        locationType: ELocationType.Cassette
                    ));
                }

                Stocker stocker = new Stocker(stockerId, stockerName, cassetteLocations);
                stockers.Add(stocker);

                // JSON 파일로 저장
                //Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                string baseJson = JsonSerializer.Serialize(stockers, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, baseJson);

                return stockers;
            }

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
