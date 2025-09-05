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
            var stockers = LoadStockersFromLocalFile(); 
            await _repo.AddRangeAsync(stockers);
        }

        private List<Stocker> LoadStockersFromLocalFile()
        {
            var stockers = new List<Stocker>();
            var filePath = "stockers.json";

            if (!File.Exists(filePath))
            {
                // 기본 Stocker 하나 생성
                var stockerId = "ST01";
                var stockerName = "Main Stocker";

                var cassettePorts = new List<CassetteLocation>();
                for (int portIdx = 1; portIdx <= 12; portIdx++)
                {
                    var portId = $"{stockerId}.CP{portIdx:00}";
                    cassettePorts.Add(new CassetteLocation(
                        id: portId,
                        name: $"{stockerName}_CASSETTEPORT{portIdx:00}",
                        locationType: ELocationType.Cassette
                    ));
                }

                var stocker = new Stocker(stockerId, stockerName, cassettePorts);
                stockers.Add(stocker);

                // JSON 파일로 저장
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                var baseJson = JsonSerializer.Serialize(stockers, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, baseJson);

                return stockers;
            }

            // 파일이 이미 있는 경우 → 로딩
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var fileStockers = JsonSerializer.Deserialize<List<Stocker>>(json, options);

            if (fileStockers != null)
                stockers.AddRange(fileStockers);

            return stockers;
        }

    }
}
