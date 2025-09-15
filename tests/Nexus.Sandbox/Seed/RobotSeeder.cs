using Nexus.Core.Domain.Models.Robots;
using Nexus.Core.Domain.Models.Robots.Enums;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Sandbox.Seed.Interfaces;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Sandbox.Seed
{
    public class RobotSeeder : IDataSeeder
    {
        private readonly RedisRobotRepository _repo;
        private readonly RedisLocationRepository _locationRepo;

        public RobotSeeder(RedisRobotRepository repo, RedisLocationRepository locationRepo)
        {
            _repo = repo;
            _locationRepo = locationRepo;
        }

        public async Task SeedAsync()
        {
            // Create new locations for robots and save them
            List<Robot> robots = new List<Robot>();

            // CR01: 3 TrayLocations
            List<Location> cr01Locations = new List<Location>();
            for (int i = 1; i <= 3; i++)
            {
                string id = $"CR01.TP{i:00}";
                Nexus.Core.Domain.Models.Locations.TrayLocation trayLocation = new Nexus.Core.Domain.Models.Locations.TrayLocation(id, $"CR01_TRAY_{i:00}");
                trayLocation.Position = new Nexus.Core.Domain.Shared.Bases.Position((uint)(100 + i * 25), 100, 0);
                trayLocation.Width = 20;
                trayLocation.Height = 20;
                trayLocation.Depth = 20;
                await _locationRepo.AddAsync(trayLocation);
                cr01Locations.Add(trayLocation);
            }
            robots.Add(new Robot(
                id: "CR01",
                name: "Control Robot 01",
                robotType: ERobotType.Control,
                locations: cr01Locations));

            // CR02: 3 TrayLocations
            List<Location> cr02Locations = new List<Location>();
            for (int i = 1; i <= 3; i++)
            {
                string id = $"CR02.TP{i:00}";
                Nexus.Core.Domain.Models.Locations.TrayLocation trayLocation = new Nexus.Core.Domain.Models.Locations.TrayLocation(id, $"CR02_TRAY_{i:00}");
                trayLocation.Position = new Nexus.Core.Domain.Shared.Bases.Position((uint)(200 + i * 25), 120, 0);
                trayLocation.Width = 20;
                trayLocation.Height = 20;
                trayLocation.Depth = 20;
                await _locationRepo.AddAsync(trayLocation);
                cr02Locations.Add(trayLocation);
            }
            robots.Add(new Robot(
                id: "CR02",
                name: "Control Robot 02",
                robotType: ERobotType.Control,
                locations: cr02Locations));

            // LR01: 1 CassetteLocation
            List<Location> lrLocations = new List<Location>();
            Nexus.Core.Domain.Models.Locations.CassetteLocation cassetteLocation = new Nexus.Core.Domain.Models.Locations.CassetteLocation("LR01.CP01", "LR01_CASSETTE_01");
            cassetteLocation.Position = new Nexus.Core.Domain.Shared.Bases.Position(50, 100, 0);
            cassetteLocation.Width = 30;
            cassetteLocation.Height = 30;
            cassetteLocation.Depth = 30;
            await _locationRepo.AddAsync(cassetteLocation);
            lrLocations.Add(cassetteLocation);
            robots.Add(new Robot(
                id: "LR01",
                name: "Logistics Robot 01",
                robotType: ERobotType.Logistics,
                locations: lrLocations));

            await _repo.AddRangeAsync(robots);
        }
    }
}
