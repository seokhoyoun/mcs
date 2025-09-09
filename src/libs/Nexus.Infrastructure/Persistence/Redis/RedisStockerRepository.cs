using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Enums;
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

        private const string STOCKERS_ALL_KEY = "stockers:all";

        private const string STOCKER_KEY_PREFIX = "stocker:";
        private const string CASSETTE_LOCATION_KEY_PREFIX = "cassette_location:";

        private const string ID_SEPARATOR = ",";

        #endregion

        #region Constructor

        public RedisStockerRepository(IConnectionMultiplexer connection)
        {
            _database = connection.GetDatabase();
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
            string cassettePortIdsValue = Helper.GetHashValue(hashEntries, "cassette_port_ids");
            string[] cassettePortIds = cassettePortIdsValue.Split(ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            List<CassetteLocation> cassettePorts = new List<CassetteLocation>();
            foreach (string portId in cassettePortIds)
            {
                CassetteLocation? port = await GetCassetteLocationByIdAsync(portId);
                if (port != null)
                {
                    cassettePorts.Add(port);
                }
            }

            return new Stocker(id, name, cassettePorts.AsReadOnly());
        }

        public async Task<Stocker> AddAsync(Stocker entity, CancellationToken cancellationToken = default)
        {
            await SaveStockerAsync(entity);
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

        private async Task SaveStockerAsync(Stocker stocker)
        {
            string cassettePortIds = string.Join(ID_SEPARATOR, stocker.CassettePorts.Select(cp => cp.Id));

            HashEntry[] entries = new HashEntry[]
            {
                new HashEntry("id", stocker.Id),
                new HashEntry("name", stocker.Name),
                new HashEntry("cassette_port_ids", cassettePortIds)
            };

            // Save child cassette ports first (without registering to global location sets, matching Area behavior)
            foreach (CassetteLocation port in stocker.CassettePorts)
            {
                await SaveCassetteLocationAsync(port);
            }

            await _database.HashSetAsync($"{STOCKER_KEY_PREFIX}{stocker.Id}", entries);
            await _database.SetAddAsync(STOCKERS_ALL_KEY, stocker.Id);
        }

        private async Task<CassetteLocation?> GetCassetteLocationByIdAsync(string id)
        {
            HashEntry[] hashEntries = await _database.HashGetAllAsync($"{CASSETTE_LOCATION_KEY_PREFIX}{id}");
            if (hashEntries.Length == 0)
            {
                return null;
            }

            string name = Helper.GetHashValue(hashEntries, "name");
            ELocationType locationType = Helper.GetHashValueAsEnum<ELocationType>(hashEntries, "location_type");
            ELocationStatus status = Helper.GetHashValueAsEnum<ELocationStatus>(hashEntries, "status");

            CassetteLocation loc = new CassetteLocation(id, name, locationType)
            {
                Status = status
            };
            return loc;
        }

        private async Task SaveCassetteLocationAsync(CassetteLocation cassetteLocation)
        {
            HashEntry[] entries = new HashEntry[]
            {
                new HashEntry("id", cassetteLocation.Id),
                new HashEntry("name", cassetteLocation.Name),
                new HashEntry("location_type", cassetteLocation.LocationType.ToString()),
                new HashEntry("status", cassetteLocation.Status.ToString()),
                new HashEntry("current_item_id", cassetteLocation.CurrentItemId)
            };

            await _database.HashSetAsync($"{CASSETTE_LOCATION_KEY_PREFIX}{cassetteLocation.Id}", entries);
        }

        #endregion
    }
}
