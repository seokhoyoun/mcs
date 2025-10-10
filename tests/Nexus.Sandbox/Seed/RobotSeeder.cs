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
        private readonly RedisDimensionRepository _dimRepo;

        public RobotSeeder(RedisRobotRepository repo, RedisLocationRepository locationRepo, RedisDimensionRepository dimRepo)
        {
            _repo = repo;
            _locationRepo = locationRepo;
            _dimRepo = dimRepo;
        }

        public async Task SeedAsync()
        {
            // Create new locations for robots and save them
            List<Robot> robots = new List<Robot>();

            // Load tray dimension
            Nexus.Core.Domain.Standards.DimensionStandard? trayStd = await _dimRepo.GetByIdAsync("location:tray");
            uint trayW = trayStd != null ? trayStd.Width : 20u;
            uint trayH = trayStd != null ? trayStd.Height : 20u;
            uint trayD = trayStd != null ? trayStd.Depth : 20u;

            // CR01: 3 TrayLocations
            List<Location> cr01Locations = new List<Location>();
            for (int i = 1; i <= 3; i++)
            {
                string id = $"CR01.TP{i:00}";
                Nexus.Core.Domain.Models.Locations.TrayLocation trayLocation = new Nexus.Core.Domain.Models.Locations.TrayLocation(id, $"CR01_TRAY_{i:00}");
                trayLocation.Width = trayW;
                trayLocation.Height = trayH;
                trayLocation.Depth = trayD;
                trayLocation.ParentId = "CR01";
                trayLocation.IsVisible = true;
                trayLocation.IsRelativePosition = true;
                trayLocation.Position = new Nexus.Core.Domain.Shared.Bases.Position((uint)(i * 10), 0, 0);
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
                trayLocation.Width = trayW;
                trayLocation.Height = trayH;
                trayLocation.Depth = trayD;
                trayLocation.ParentId = "CR02";
                trayLocation.IsVisible = true;
                trayLocation.IsRelativePosition = true;
                trayLocation.Position = new Nexus.Core.Domain.Shared.Bases.Position((uint)(i * 10), 0, 0);
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
            cassetteLocation.ParentId = string.Empty;
            cassetteLocation.IsVisible = true;
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
