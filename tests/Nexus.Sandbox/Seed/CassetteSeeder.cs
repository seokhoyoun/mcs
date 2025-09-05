using Nexus.Core.Domain.Models.Transports;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Sandbox.Seed.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Sandbox.Seed
{
    public class CassetteSeeder : IDataSeeder
    {
        private readonly RedisTransportsRepository _repo;

        public CassetteSeeder(RedisTransportsRepository repo)
        {
            _repo = repo;
        }

        public async Task SeedAsync()
        {
            for (int i = 1; i <= 10; i++) // 카세트 10개
            {
                var trays = new List<Tray>();

                for (int j = 1; j <= 6; j++) // 트레이 6개
                {
                    var memories = new List<Memory>();

                    for (int k = 1; k <= 25; k++) // 메모리 25개
                    {
                        var memoryId = $"CST{i:D2}-T{j:D2}-M{k:D2}";
                        var memory = new Memory(memoryId, $"Memory {i}-{j}-{k}");
                        memories.Add(memory);
                    }

                    var trayId = $"CST{i:D2}-T{j:D2}";
                    var tray = new Tray(trayId, $"Tray {i}-{j}", memories);
                    trays.Add(tray);
                }

                var cassetteId = $"CST{i:D2}";
                var cassette = new Cassette(cassetteId, $"Cassette {i}", trays);

                await _repo.AddAsync(cassette);

                Console.WriteLine($"Seeded cassette: {cassette.Name}");
            }
        }
    }

}
