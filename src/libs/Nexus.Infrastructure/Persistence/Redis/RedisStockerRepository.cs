using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Stockers;
using Nexus.Core.Domain.Models.Stockers.Interfaces;
using StackExchange.Redis;
using System.Linq.Expressions;

namespace Nexus.Infrastructure.Persistence.Redis
{
    public class RedisStockerRepository : IStockerRepository
    {
        #region Fields

        private readonly IDatabase _database;
        private readonly ILocationRepository _locationRepository;

        private const string STOCKERS_ALL_KEY = "stockers:all";

        private const string STOCKER_KEY_PREFIX = "stocker:";

        private const string ID_SEPARATOR = ",";

        #endregion

        #region Constructor

        public RedisStockerRepository(IConnectionMultiplexer connection, ILocationRepository locationRepository)
        {
            _database = connection.GetDatabase();
            _locationRepository = locationRepository;
        }

        #endregion

        #region IRepository<Stocker,string> Implementation

        public async Task<IReadOnlyList<Stocker>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            RedisValue[] ids = await _database.SetMembersAsync(STOCKERS_ALL_KEY);
            List<Stocker> stockers = new List<Stocker>();

            foreach (RedisValue id in ids)
            {
                Stocker? stocker = await GetByIdAsync(id.ToString(), cancellationToken);
                if (stocker != null)
                {
                    stockers.Add(stocker);
                }
            }

            return stockers.AsReadOnly();
        }

        public async Task<IReadOnlyList<Stocker>> GetAsync(Expression<Func<Stocker, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Stocker> all = await GetAllAsync(cancellationToken);
            Func<Stocker, bool> compiled = predicate.Compile();
            return all.Where(compiled).ToList().AsReadOnly();
        }

        public async Task<Stocker?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            HashEntry[] hashEntries = await _database.HashGetAllAsync($"{STOCKER_KEY_PREFIX}{id}");
            if (hashEntries.Length == 0)
            {
                return null;
            }

            string name = Helper.GetHashValue(hashEntries, "name");

            string cassetteIdsValue = Helper.GetHashValue(hashEntries, "cassette_location_ids");
            string trayIdsValue = Helper.GetHashValue(hashEntries, "tray_location_ids");
         

            string[] cassetteLocationIds = cassetteIdsValue.Split(ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            string[] trayLocationIds = trayIdsValue.Split(ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            List<CassetteLocation> cassetteLocations = new List<CassetteLocation>();
            List<TrayLocation> trayLocations = new List<TrayLocation>();

            foreach (string cassetteLocationId in cassetteLocationIds)
            {
                Location? location = await _locationRepository.GetByIdAsync(cassetteLocationId, cancellationToken);
                if (location is CassetteLocation cassetteLocation)
                {
                    cassetteLocations.Add(cassetteLocation);
                }
            }

            foreach (string trayLocationId in trayLocationIds)
            {
                Location? location = await _locationRepository.GetByIdAsync(trayLocationId, cancellationToken);
                if (location is TrayLocation trayLocation)
                {
                    trayLocations.Add(trayLocation);
                }
            }

            return new Stocker(id, name, cassetteLocations.AsReadOnly(), trayLocations.AsReadOnly());
        }

        public async Task<Stocker> AddAsync(Stocker entity, CancellationToken cancellationToken = default)
        {
            await SaveStockerAsync(entity, cancellationToken);
            return entity;
        }

        public async Task<IEnumerable<Stocker>> AddRangeAsync(IEnumerable<Stocker> entities, CancellationToken cancellationToken = default)
        {
            Task<Stocker>[] tasks = entities.Select(e => AddAsync(e, cancellationToken)).ToArray();
            return await Task.WhenAll(tasks);
        }

        public async Task<Stocker> UpdateAsync(Stocker entity, CancellationToken cancellationToken = default)
        {
            // HSET behaves as upsert
            return await AddAsync(entity, cancellationToken);
        }

        public async Task<bool> UpdateRangeAsync(IEnumerable<Stocker> entities, CancellationToken cancellationToken = default)
        {
            Task<Stocker>[] tasks = entities.Select(e => UpdateAsync(e, cancellationToken)).ToArray();
            await Task.WhenAll(tasks);
            return true;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            bool exists = await _database.KeyExistsAsync($"{STOCKER_KEY_PREFIX}{id}");
            if (!exists)
            {
                return false;
            }

            await _database.KeyDeleteAsync($"{STOCKER_KEY_PREFIX}{id}");
            await _database.SetRemoveAsync(STOCKERS_ALL_KEY, id);
            return true;
        }

        public Task<bool> DeleteAsync(Stocker entity, CancellationToken cancellationToken = default)
        {
            return DeleteAsync(entity.Id, cancellationToken);
        }

        public async Task<bool> DeleteRangeAsync(IEnumerable<Stocker> entities, CancellationToken cancellationToken = default)
        {
            Task<bool>[] tasks = entities.Select(e => DeleteAsync(e, cancellationToken)).ToArray();
            bool[] results = await Task.WhenAll(tasks);
            return results.All(r => r);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Stocker, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Stocker> filtered = await GetAsync(predicate, cancellationToken);
            return filtered.Any();
        }

        public async Task<int> CountAsync(Expression<Func<Stocker, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
            {
                long count = await _database.SetLengthAsync(STOCKERS_ALL_KEY);
                return (int)count;
            }

            IReadOnlyList<Stocker> filtered = await GetAsync(predicate, cancellationToken);
            return filtered.Count;
        }

        public Task<IReadOnlyList<Stocker>> GetAllStockersAsync()
        {
            // Convenience wrapper if used by callers not via interface
            return GetAllAsync();
        }

        #endregion

        #region Private Helpers

        private async Task SaveStockerAsync(Stocker stocker, CancellationToken cancellationToken = default)
        {
            string cassetteLocationIds = string.Join(ID_SEPARATOR, stocker.CassetteLocations.Select(cp => cp.Id));
            string trayLocationIds = string.Join(ID_SEPARATOR, stocker.TrayLocations.Select(tp => tp.Id));

            HashEntry[] entries = new HashEntry[]
            {
                new HashEntry("id", stocker.Id),
                new HashEntry("name", stocker.Name),
                // New canonical fields
                new HashEntry("cassette_location_ids", cassetteLocationIds),
                new HashEntry("tray_location_ids", trayLocationIds),
            };

            // Persist locations via location repository
            foreach (CassetteLocation cassetteLocation in stocker.CassetteLocations)
            {
                await _locationRepository.AddAsync(cassetteLocation, cancellationToken);
            }

            foreach (TrayLocation trayLocation in stocker.TrayLocations)
            {
                await _locationRepository.AddAsync(trayLocation, cancellationToken);
            }

            await _database.HashSetAsync($"{STOCKER_KEY_PREFIX}{stocker.Id}", entries);
            await _database.SetAddAsync(STOCKERS_ALL_KEY, stocker.Id);
        }

        #endregion
    }
}
