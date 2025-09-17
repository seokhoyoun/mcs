using Nexus.Core.Domain.Standards;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Sandbox.Seed.Interfaces;

namespace Nexus.Sandbox.Seed
{
    internal class DimensionSeeder : IDataSeeder
    {
        private readonly RedisDimensionRepository _repo;

        public DimensionSeeder(RedisDimensionRepository repo)
        {
            _repo = repo;
        }

        public async Task SeedAsync()
        {
            List<DimensionStandard> items = new List<DimensionStandard>
            {
                // Location types
                new DimensionStandard(id: "location:cassette", name: "CassetteLocation", category: "location", width: 30, height: 60, depth: 60),
                new DimensionStandard(id: "location:tray", name: "TrayLocation", category: "location", width: 30, height: 4, depth: 30),
                new DimensionStandard(id: "location:memory", name: "MemoryLocation", category: "location", width: 5, height: 5, depth: 5),

                // Transport (items placed at locations)
                new DimensionStandard(id: "transport:cassette", name: "Cassette", category: "transport", width: 25, height: 50, depth: 50),
                new DimensionStandard(id: "transport:tray", name: "Tray", category: "transport", width: 28, height: 3, depth: 28),
                new DimensionStandard(id: "transport:memory", name: "Memory", category: "transport", width: 4, height: 4, depth: 4),

                // Robots
                new DimensionStandard(id: "robot:logistics", name: "LogisticsRobot", category: "robot", width: 20, height: 18, depth: 20),
                new DimensionStandard(id: "robot:control", name: "ControlRobot", category: "robot", width: 20, height: 18, depth: 20),

                // Defaults / fallbacks
                new DimensionStandard(id: "default:location", name: "DefaultLocation", category: "default", width: 10, height: 10, depth: 10),
                new DimensionStandard(id: "default:robot", name: "DefaultRobot", category: "default", width: 20, height: 18, depth: 20)
            };

            await _repo.AddRangeAsync(items);
        }
    }
}
