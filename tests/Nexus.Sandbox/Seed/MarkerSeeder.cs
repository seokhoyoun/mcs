using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
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

                // Create a single region marker that encloses the stocker's cassette ports
                uint minX;
                uint minY;
                uint maxX;
                uint maxY;
                uint z;
                List<Location> stockerPortsAsLocations = new List<Location>();
                foreach (Nexus.Core.Domain.Models.Locations.CassetteLocation cp in stocker.CassettePorts)
                {
                    stockerPortsAsLocations.Add(cp);
                }
                GetBounds(stockerPortsAsLocations, out minX, out minY, out maxX, out maxY, out z);
                uint margin = 20;
                uint left;
                if (minX > margin)
                {
                    left = minX - margin;
                }
                else
                {
                    left = 0;
                }
                uint top;
                if (minY > margin)
                {
                    top = minY - margin;
                }
                else
                {
                    top = 0;
                }
                uint width;
                if (maxX >= left)
                {
                    width = maxX - left + margin;
                }
                else
                {
                    width = 0;
                }
                uint height;
                if (maxY >= top)
                {
                    height = maxY - top + margin;
                }
                else
                {
                    height = 0;
                }

                MarkerLocation region = new MarkerLocation(stocker.Id, stocker.Name);
                region.MarkerRole = Nexus.Core.Domain.Models.Locations.Enums.EMarkerRole.Stocker;
                region.Position = new Position(left, top, z);
                region.Width = width;
                region.Height = 180; // thin vertical thickness
                region.Depth = height; // depth is Z length
                await _locationRepo.AddAsync(region);
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

                // Create one region marker that encloses all sub-locations in the area
                uint minX;
                uint minY;
                uint maxX;
                uint maxY;
                uint z;
                GetBounds(locs, out minX, out minY, out maxX, out maxY, out z);
                uint margin = 14;
                uint left;
                if (minX > margin)
                {
                    left = minX - margin;
                }
                else
                {
                    left = 0;
                }
                // Top margin intentionally not applied to keep consistent inner padding across areas
                uint top = minY;
                uint width;
                if (maxX >= left)
                {
                    width = maxX - left + margin; // left margin + right margin
                }
                else
                {
                    width = 0;
                }
                uint height;
                if (maxY >= top)
                {
                    height = maxY - top + margin; // bottom margin only
                }
                else
                {
                    height = 0;
                }

                MarkerLocation region = new MarkerLocation(area.Id, area.Name);
                region.MarkerRole = Nexus.Core.Domain.Models.Locations.Enums.EMarkerRole.Area;
                region.Position = new Position(left, top, z);
                region.Width = width;
                region.Height = 120; // thin vertical thickness
                region.Depth = height; // depth is Z length
                await _locationRepo.AddAsync(region);
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

                    uint minX;
                    uint minY;
                    uint maxX;
                    uint maxY;
                    uint z;

                    List<Location> memAsLocations = new List<Location>();
                    foreach (MemoryLocation mp in s.MemoryLocations)
                    {
                        memAsLocations.Add(mp);
                    }

                    GetBounds(memAsLocations, out minX, out minY, out maxX, out maxY, out z);
                    // Reduce margin so set regions don't collide with neighbors
                    uint margin = 2;
                    uint left;
                    if (minX > margin)
                    {
                        left = minX - margin;
                    }
                    else
                    {
                        left = 0;
                    }
                    uint top;
                    if (minY > margin)
                    {
                        top = minY - margin;
                    }
                    else
                    {
                        top = 0;
                    }
                    uint width;
                    if (maxX >= left)
                    {
                        width = maxX - left + margin;
                    }
                    else
                    {
                        width = 0;
                    }
                    uint height;
                    if (maxY >= top)
                    {
                        height = maxY - top + margin;
                    }
                    else
                    {
                        height = 0;
                    }

                    // Shrink width slightly to avoid inter-set collision after stroke rendering
                    uint shrinkX = 6;
                    if (width > shrinkX)
                    {
                        width = width - shrinkX;
                    }

                    MarkerLocation region = new MarkerLocation(s.Id, s.Name);
                    region.MarkerRole = Nexus.Core.Domain.Models.Locations.Enums.EMarkerRole.Set;
                    region.Position = new Position(left, top, z);
                    region.Width = width;
                    region.Height = 30; // thin vertical thickness
                    region.Depth = height; // depth is Z length
                    await _locationRepo.AddAsync(region);
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

        private void GetBounds(IEnumerable<Location> locations, out uint minX, out uint minY, out uint maxX, out uint maxY, out uint z)
        {
            bool hasAny = false;
            minX = 0;
            minY = 0;
            maxX = 0;
            maxY = 0;
            z = 0;

            foreach (Location l in locations)
            {
                if (!hasAny)
                {
                    minX = l.Position.X;
                    minY = l.Position.Y;
                    maxX = l.Position.X + l.Width;
                    maxY = l.Position.Y + l.Height;
                    z = l.Position.Z;
                    hasAny = true;
                }
                else
                {
                    if (l.Position.X < minX)
                    {
                        minX = l.Position.X;
                    }
                    if (l.Position.Y < minY)
                    {
                        minY = l.Position.Y;
                    }
                    uint lxMax = l.Position.X + l.Width;
                    uint lyMax = l.Position.Y + l.Height;
                    if (lxMax > maxX)
                    {
                        maxX = lxMax;
                    }
                    if (lyMax > maxY)
                    {
                        maxY = lyMax;
                    }
                }
            }

            if (!hasAny)
            {
                minX = 0;
                minY = 0;
                maxX = 0;
                maxY = 0;
                z = 0;
            }
        }
    }
}

