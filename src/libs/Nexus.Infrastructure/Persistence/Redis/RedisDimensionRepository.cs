using Nexus.Core.Domain.Shared.Bases;
using Nexus.Core.Domain.Standards;
using Nexus.Core.Domain.Standards.Interfaces;
using StackExchange.Redis;
using System.Linq.Expressions;

namespace Nexus.Infrastructure.Persistence.Redis
{
    public class RedisDimensionRepository : IDimensionRepository
    {
        private readonly IDatabase _database;

        private const string DIMENSION_KEY_PREFIX = "dimension:";
        private const string DIMENSIONS_ALL_KEY = "dimensions:all";

        public RedisDimensionRepository(IConnectionMultiplexer connection)
        {
            _database = connection.GetDatabase();
        }

        public async Task<IReadOnlyList<DimensionStandard>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            RedisValue[] ids = await _database.SetMembersAsync(DIMENSIONS_ALL_KEY);
            List<DimensionStandard> list = new List<DimensionStandard>();

            foreach (RedisValue id in ids)
            {
                DimensionStandard? std = await GetByIdAsync(id.ToString(), cancellationToken);
                if (std != null)
                {
                    list.Add(std);
                }
            }

            return list.AsReadOnly();
        }

        public async Task<IReadOnlyList<DimensionStandard>> GetAsync(Expression<Func<DimensionStandard, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<DimensionStandard> all = await GetAllAsync(cancellationToken);
            Func<DimensionStandard, bool> compiled = predicate.Compile();
            return all.Where(compiled).ToList().AsReadOnly();
        }

        public async Task<DimensionStandard?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            HashEntry[] hash = await _database.HashGetAllAsync($"{DIMENSION_KEY_PREFIX}{id}");
            if (hash.Length == 0)
            {
                return null;
            }

            string name = Helper.GetHashValue(hash, "name");
            string category = Helper.GetHashValue(hash, "category");
            uint width = (uint)Helper.GetHashValueAsInt(hash, "width");
            uint height = (uint)Helper.GetHashValueAsInt(hash, "height");
            uint depth = (uint)Helper.GetHashValueAsInt(hash, "depth");

            return new DimensionStandard(id, name, category, width, height, depth);
        }

        public async Task<DimensionStandard> AddAsync(DimensionStandard entity, CancellationToken cancellationToken = default)
        {
            await SaveAsync(entity, cancellationToken);
            return entity;
        }

        public async Task<IEnumerable<DimensionStandard>> AddRangeAsync(IEnumerable<DimensionStandard> entities, CancellationToken cancellationToken = default)
        {
            Task<DimensionStandard>[] tasks = entities.Select(e => AddAsync(e, cancellationToken)).ToArray();
            return await Task.WhenAll(tasks);
        }

        public async Task<DimensionStandard> UpdateAsync(DimensionStandard entity, CancellationToken cancellationToken = default)
        {
            return await AddAsync(entity, cancellationToken);
        }

        public async Task<bool> UpdateRangeAsync(IEnumerable<DimensionStandard> entities, CancellationToken cancellationToken = default)
        {
            Task<DimensionStandard>[] tasks = entities.Select(e => UpdateAsync(e, cancellationToken)).ToArray();
            await Task.WhenAll(tasks);
            return true;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            bool exists = await _database.KeyExistsAsync($"{DIMENSION_KEY_PREFIX}{id}");
            if (!exists)
            {
                return false;
            }
            await _database.KeyDeleteAsync($"{DIMENSION_KEY_PREFIX}{id}");
            await _database.SetRemoveAsync(DIMENSIONS_ALL_KEY, id);
            return true;
        }

        public async Task<bool> DeleteAsync(DimensionStandard entity, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync(entity.Id, cancellationToken);
        }

        public async Task<bool> DeleteRangeAsync(IEnumerable<DimensionStandard> entities, CancellationToken cancellationToken = default)
        {
            Task<bool>[] tasks = entities.Select(e => DeleteAsync(e, cancellationToken)).ToArray();
            bool[] results = await Task.WhenAll(tasks);
            return results.All(r => r);
        }

        public async Task<bool> ExistsAsync(Expression<Func<DimensionStandard, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<DimensionStandard> filtered = await GetAsync(predicate, cancellationToken);
            return filtered.Any();
        }

        public async Task<int> CountAsync(Expression<Func<DimensionStandard, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
            {
                long count = await _database.SetLengthAsync(DIMENSIONS_ALL_KEY);
                return (int)count;
            }
            IReadOnlyList<DimensionStandard> filtered = await GetAsync(predicate, cancellationToken);
            return filtered.Count;
        }

        private async Task SaveAsync(DimensionStandard d, CancellationToken cancellationToken = default)
        {
            HashEntry[] entries = new HashEntry[]
            {
                new HashEntry("id", d.Id),
                new HashEntry("name", d.Name),
                new HashEntry("category", d.Category),
                new HashEntry("width", d.Width.ToString()),
                new HashEntry("height", d.Height.ToString()),
                new HashEntry("depth", d.Depth.ToString())
            };
            await _database.HashSetAsync($"{DIMENSION_KEY_PREFIX}{d.Id}", entries);
            await _database.SetAddAsync(DIMENSIONS_ALL_KEY, d.Id);
        }
    }
}

