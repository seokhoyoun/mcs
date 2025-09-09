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
        private readonly RedisTransportRepository _repo;

        public CassetteSeeder(RedisTransportRepository repo)
        {
            _repo = repo;
        }

        public async Task SeedAsync()
        {
            for (int i = 1; i <= 10; i++) // 카세트 10개
            {
                List<Tray> trays = new List<Tray>();

                for (int j = 1; j <= 6; j++) // 트레이 6개
                {
                    List<Memory> memories = new List<Memory>();

                    for (int k = 1; k <= 25; k++) // 메모리 25개
                    {
                        string memoryId = $"CST{i:D2}-T{j:D2}-M{k:D2}";
                        Memory memory = new Memory(memoryId, $"Memory {i}-{j}-{k}");
                        memories.Add(memory);
                    }

                    string trayId = $"CST{i:D2}-T{j:D2}";
                    Tray tray = new Tray(trayId, $"Tray {i}-{j}", memories);
                    trays.Add(tray);
                }

                string cassetteId = $"CST{i:D2}";
                Cassette cassette = new Cassette(cassetteId, $"Cassette {i}", trays);

                await _repo.AddAsync(cassette);

                Console.WriteLine($"Seeded cassette: {cassette.Name}");
            }
        }
    }

}
