using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Areas.Enums;
using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Nexus.Infrastructure.Persistence.Redis
{
    public class RedisAreaRepository : IAreaRepository
    {
        #region Fields

        private readonly IDatabase _database;
        private readonly ILocationRepository _locationRepository;

        private const string AREAS_ALL_KEY = "areas:all";
        private const string SETS_ALL_KEY = "sets:all";

        private const string AREA_KEY_PREFIX = "area:";
        private const string SET_KEY_PREFIX = "set:";

        private const string ID_SEPARATOR = ",";

        #endregion

        #region Constructor

        public RedisAreaRepository(IConnectionMultiplexer connection, ILocationRepository locationRepository)
        {
            _database = connection.GetDatabase();
            _locationRepository = locationRepository;
        }

        #endregion

        #region IRepository<Area, string> Implementation

        public async Task<IReadOnlyList<Area>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            RedisValue[] ids = await _database.SetMembersAsync(AREAS_ALL_KEY);
            List<Area> areas = new List<Area>();

            foreach (RedisValue id in ids)
            {
                Area? area = await GetAreaByIdAsync(id.ToString());
                if (area != null)
                {
                    areas.Add(area);
                }
            }

            return areas.AsReadOnly();
        }

        public async Task<IReadOnlyList<Area>> GetAsync(Expression<Func<Area, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Area> allAreas = await GetAllAsync(cancellationToken);
            Func<Area, bool> compiledPredicate = predicate.Compile();
            return allAreas.Where(compiledPredicate).ToList().AsReadOnly();
        }

        public async Task<Area?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await GetAreaByIdAsync(id);
        }

        public async Task<Area> AddAsync(Area entity, CancellationToken cancellationToken = default)
        {
            await SaveAreaAsync(entity, cancellationToken);
            return entity;
        }

        public async Task<IEnumerable<Area>> AddRangeAsync(IEnumerable<Area> entities, CancellationToken cancellationToken = default)
        {
            Task<Area>[] tasks = entities.Select(entity => AddAsync(entity, cancellationToken)).ToArray();
            return await Task.WhenAll(tasks);
        }

        public async Task<Area> UpdateAsync(Area entity, CancellationToken cancellationToken = default)
        {
            return await AddAsync(entity, cancellationToken);
        }

        public async Task<bool> UpdateRangeAsync(IEnumerable<Area> entities, CancellationToken cancellationToken = default)
        {
            Task<Area>[] tasks = entities.Select(entity => UpdateAsync(entity, cancellationToken)).ToArray();
            await Task.WhenAll(tasks);
            return true;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            bool exists = await _database.KeyExistsAsync($"{AREA_KEY_PREFIX}{id}");
            if (exists)
            {
                await DeleteAreaAsync(id);
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteAsync(Area entity, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync(entity.Id, cancellationToken);
        }

        public async Task<bool> DeleteRangeAsync(IEnumerable<Area> entities, CancellationToken cancellationToken = default)
        {
            Task<bool>[] tasks = entities.Select(entity => DeleteAsync(entity, cancellationToken)).ToArray();
            bool[] results = await Task.WhenAll(tasks);
            return results.All(result => result);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Area, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Area> areas = await GetAsync(predicate, cancellationToken);
            return areas.Any();
        }

        public async Task<int> CountAsync(Expression<Func<Area, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
            {
                long areaCount = await _database.SetLengthAsync(AREAS_ALL_KEY);
                return (int)areaCount;
            }

            IReadOnlyList<Area> filteredAreas = await GetAsync(predicate, cancellationToken);
            return filteredAreas.Count;
        }

        #endregion

        #region IAreaRepository Implementation

        public async Task<IReadOnlyList<Set>> GetSetsByAreaIdAsync(string areaId, CancellationToken cancellationToken = default)
        {
            HashEntry[] areaHash = await _database.HashGetAllAsync($"{AREA_KEY_PREFIX}{areaId}");
            if (areaHash.Length == 0)
            {
                return Array.Empty<Set>();
            }

            string setIdsValue = Helper.GetHashValue(areaHash, "set_ids");
            string[] setIds = setIdsValue.Split(ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            List<Set> sets = new List<Set>();

            foreach (string setId in setIds)
            {
                Set? set = await GetSetByIdAsync(setId);
                if (set != null)
                {
                    sets.Add(set);
                }
            }

            return sets.AsReadOnly();
        }

        public async Task InitializeAreasAsync(IEnumerable<Area> areas, CancellationToken cancellationToken = default)
        {
            Task[] tasks = areas.Select(area => SaveAreaAsync(area, cancellationToken)).ToArray();
            await Task.WhenAll(tasks);
        }

        #endregion

        #region Private Area Operations

        private async Task<Area?> GetAreaByIdAsync(string id)
        {
            HashEntry[] hashEntries = await _database.HashGetAllAsync($"{AREA_KEY_PREFIX}{id}");
            if (hashEntries.Length == 0)
            {
                return null;
            }

            string areaName = Helper.GetHashValue(hashEntries, "name");
            EAreaStatus status = Helper.GetHashValueAsEnum<EAreaStatus>(hashEntries, "status");

            string cassetteLocationIdsValue = Helper.GetHashValue(hashEntries, "cassette_location_ids");
            string trayLocationIdsValue = Helper.GetHashValue(hashEntries, "tray_location_ids");
            string setIdsValue = Helper.GetHashValue(hashEntries, "set_ids");

            string[] cassetteLocationIds = cassetteLocationIdsValue.Split(ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            string[] trayLocationIds = trayLocationIdsValue.Split(ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            string[] setIds = setIdsValue.Split(ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            List<CassetteLocation> cassetteLocations = new List<CassetteLocation>();
            List<TrayLocation> trayLocations = new List<TrayLocation>();
            List<Set> sets = new List<Set>();

            // locationRepository를 통해 location 조회
            foreach (string cassetteLocationId in cassetteLocationIds)
            {
                Location? location = await _locationRepository.GetByIdAsync(cassetteLocationId);
                if (location is CassetteLocation cassetteLocation)
                {
                    cassetteLocations.Add(cassetteLocation);
                }
            }

            foreach (string trayLocationId in trayLocationIds)
            {
                Location? location = await _locationRepository.GetByIdAsync(trayLocationId);
                if (location is TrayLocation trayLocation)
                {
                    trayLocations.Add(trayLocation);
                }
            }

            foreach (string setId in setIds)
            {
                Set? set = await GetSetByIdAsync(setId);
                if (set != null)
                {
                    sets.Add(set);
                }
            }

            Area area = new Area(id, areaName, cassetteLocations.AsReadOnly(), trayLocations.AsReadOnly(), sets.AsReadOnly())
            {
                Status = status
            };

            return area;
        }

        private async Task SaveAreaAsync(Area area, CancellationToken cancellationToken = default)
        {
            string cassetteLocationIds = string.Join(ID_SEPARATOR, area.CassetteLocations.Select(cl => cl.Id));
            string trayLocationIds = string.Join(ID_SEPARATOR, area.TrayLocations.Select(tl => tl.Id));
            string setIds = string.Join(ID_SEPARATOR, area.Sets.Select(s => s.Id));

            HashEntry[] hashEntries = new HashEntry[]
            {
                new HashEntry("id", area.Id),
                new HashEntry("name", area.Name),
                new HashEntry("status", area.Status.ToString()),
                new HashEntry("cassette_location_ids", cassetteLocationIds),
                new HashEntry("tray_location_ids", trayLocationIds),
                new HashEntry("set_ids", setIds)
            };

            // locationRepository를 통해 location 저장
            foreach (CassetteLocation cassetteLocation in area.CassetteLocations)
            {
                await _locationRepository.AddAsync(cassetteLocation, cancellationToken);
            }

            foreach (TrayLocation trayLocation in area.TrayLocations)
            {
                await _locationRepository.AddAsync(trayLocation, cancellationToken);
            }

            foreach (Set set in area.Sets)
            {
                await SaveSetAsync(set, cancellationToken);
            }

            await _database.HashSetAsync($"{AREA_KEY_PREFIX}{area.Id}", hashEntries);
            await _database.SetAddAsync(AREAS_ALL_KEY, area.Id);
        }

        private async Task DeleteAreaAsync(string id)
        {
            await _database.KeyDeleteAsync($"{AREA_KEY_PREFIX}{id}");
            await _database.SetRemoveAsync(AREAS_ALL_KEY, id);
        }

        #endregion

        #region Private Set Operations

        private async Task<Set?> GetSetByIdAsync(string id)
        {
            HashEntry[] hashEntries = await _database.HashGetAllAsync($"{SET_KEY_PREFIX}{id}");
            if (hashEntries.Length == 0)
            {
                return null;
            }

            string setName = Helper.GetHashValue(hashEntries, "name");
            string memoryLocationIdsValue = Helper.GetHashValue(hashEntries, "memory_location_ids");

            string[] memoryLocationIds = memoryLocationIdsValue.Split(ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            List<MemoryLocation> memoryLocations = new List<MemoryLocation>();

            foreach (string memoryLocationId in memoryLocationIds)
            {
                Location? location = await _locationRepository.GetByIdAsync(memoryLocationId);
                if (location != null && location.LocationType == ELocationType.Memory)
                {
                    memoryLocations.Add((MemoryLocation)location);
                }
            }

            return new Set(id, setName, memoryLocations.AsReadOnly());
        }

        private async Task SaveSetAsync(Set set, CancellationToken cancellationToken = default)
        {
            string memoryLocationIds = string.Join(ID_SEPARATOR, set.MemoryLocations.Select(mp => mp.Id));

            HashEntry[] hashEntries = new HashEntry[]
            {
                new HashEntry("id", set.Id),
                new HashEntry("name", set.Name),
                new HashEntry("memory_location_ids", memoryLocationIds)
            };

            // locationRepository를 통해 memory location 저장
            foreach (MemoryLocation memoryLocation in set.MemoryLocations)
            {
                await _locationRepository.AddAsync(memoryLocation, cancellationToken);
            }

            await _database.HashSetAsync($"{SET_KEY_PREFIX}{set.Id}", hashEntries);
            await _database.SetAddAsync(SETS_ALL_KEY, set.Id);
        }

        #endregion
    }
}
