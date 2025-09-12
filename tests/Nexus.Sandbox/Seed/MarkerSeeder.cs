using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Shared.Bases;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Sandbox.Seed.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nexus.Sandbox.Seed
{
    internal class MarkerSeeder : IDataSeeder
    {
        private readonly RedisLocationRepository _locationRepo;
        private readonly RedisAreaRepository _areaRepo;
        private readonly RedisStockerRepository _stockerRepo;

        public MarkerSeeder(
            RedisLocationRepository locationRepo,
            RedisAreaRepository areaRepo,
            RedisStockerRepository stockerRepo)
        {
            _locationRepo = locationRepo;
            _areaRepo = areaRepo;
            _stockerRepo = stockerRepo;
        }

        public async Task SeedAsync()
        {
            await SeedStockerMarkersAsync();
            await SeedAreaMarkersAsync();
            await SeedSetMarkersAsync();
        }

        private async Task SeedStockerMarkersAsync()
        {
            IReadOnlyList<Nexus.Core.Domain.Models.Stockers.Stocker> stockers = await _stockerRepo.GetAllAsync();
            foreach (Nexus.Core.Domain.Models.Stockers.Stocker stocker in stockers)
            {
                if (stocker.CassettePorts == null || stocker.CassettePorts.Count == 0)
                {
                    continue;
                }

                Position center = GetCenter(stocker.CassettePorts.Select(cp => cp.Position));

                // Two markers around stocker center
                MarkerLocation mk1 = new MarkerLocation($"{stocker.Id}.MK01", $"{stocker.Name}_marker_01");
                mk1.Position = new Position(center.X + 10, center.Y, center.Z);
                await _locationRepo.AddAsync(mk1);

                MarkerLocation mk2 = new MarkerLocation($"{stocker.Id}.MK02", $"{stocker.Name}_marker_02");
                mk2.Position = new Position(center.X, center.Y + 10, center.Z);
                await _locationRepo.AddAsync(mk2);
            }
        }

        private async Task SeedAreaMarkersAsync()
        {
            IReadOnlyList<Area> areas = await _areaRepo.GetAllAsync();
            foreach (Area area in areas)
            {
                List<Location> locs = new List<Location>();
                locs.AddRange(area.CassetteLocations);
                locs.AddRange(area.TrayLocations);
                foreach (Set s in area.Sets)
                {
                    locs.AddRange(s.MemoryLocations);
                }

                if (locs.Count == 0)
                {
                    continue;
                }

                Position center = GetCenter(locs.Select(l => l.Position));

                MarkerLocation mk1 = new MarkerLocation($"{area.Id}.MK01", $"{area.Name}_marker_01");
                mk1.Position = new Position(center.X + 12, center.Y + 12, center.Z);
                await _locationRepo.AddAsync(mk1);

                MarkerLocation mk2 = new MarkerLocation($"{area.Id}.MK02", $"{area.Name}_marker_02");
                mk2.Position = new Position(center.X - 12, center.Y - 12, center.Z);
                await _locationRepo.AddAsync(mk2);
            }
        }

        private async Task SeedSetMarkersAsync()
        {
            IReadOnlyList<Area> areas = await _areaRepo.GetAllAsync();
            foreach (Area area in areas)
            {
                foreach (Set s in area.Sets)
                {
                    if (s.MemoryLocations == null || s.MemoryLocations.Count == 0)
                    {
                        continue;
                    }

                    Position center = GetCenter(s.MemoryLocations.Select(mp => mp.Position));

                    MarkerLocation mk1 = new MarkerLocation($"{s.Id}.MK01", $"{s.Name}_marker_01");
                    mk1.Position = new Position(center.X + 6, center.Y, center.Z);
                    await _locationRepo.AddAsync(mk1);

                    MarkerLocation mk2 = new MarkerLocation($"{s.Id}.MK02", $"{s.Name}_marker_02");
                    mk2.Position = new Position(center.X, center.Y + 6, center.Z);
                    await _locationRepo.AddAsync(mk2);
                }
            }
        }

        private Position GetCenter(IEnumerable<Position> positions)
        {
            uint count = 0;
            double sumX = 0.0;
            double sumY = 0.0;
            uint z = 0;
            foreach (Position p in positions)
            {
                sumX += p.X;
                sumY += p.Y;
                z = p.Z;
                count++;
            }
            if (count == 0)
            {
                return new Position(0, 0, 0);
            }
            uint cx = (uint)Math.Round(sumX / count);
            uint cy = (uint)Math.Round(sumY / count);
            return new Position(cx, cy, z);
        }
    }
}

