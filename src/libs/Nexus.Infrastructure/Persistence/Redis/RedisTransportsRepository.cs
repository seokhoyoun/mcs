using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Models.Transports.Enums;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Shared.Application.DTO;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;

namespace Nexus.Infrastructure.Persistence.Redis
{
    public class RedisTransportsRepository : ITransportsRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        // Redis 키 상수
        private const string CASSETTE_KEY_PREFIX = "cassette:";
        private const string TRAY_KEY_PREFIX = "tray:";
        private const string MEMORY_KEY_PREFIX = "memory:";
        private const string CASSETTES_SET_KEY = "cassettes";
        private const string TRAYS_SET_KEY = "trays";
        private const string MEMORIES_SET_KEY = "memories";

        public RedisTransportsRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            _redis = connectionMultiplexer;
            _database = connectionMultiplexer.GetDatabase();
        }

        #region IRepository<ITransportable, string> Implementation

        public async Task<IReadOnlyList<ITransportable>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var transportables = new List<ITransportable>();

            // 모든 Cassette 조회
            var cassettes = await GetAllCassettesAsync();
            transportables.AddRange(cassettes);

            // 모든 Tray 조회
            var trays = await GetAllTraysAsync();
            transportables.AddRange(trays);

            // 모든 Memory 조회
            var memories = await GetAllMemoriesAsync();
            transportables.AddRange(memories);

            return transportables.AsReadOnly();
        }

        public async Task<IReadOnlyList<ITransportable>> GetAsync(Expression<Func<ITransportable, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var allTransportables = await GetAllAsync(cancellationToken);
            var compiledPredicate = predicate.Compile();
            return allTransportables.Where(compiledPredicate).ToList().AsReadOnly();
        }

        public async Task<ITransportable?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            // Cassette 먼저 확인
            var cassette = await GetCassetteByIdAsync(id);
            if (cassette != null)
                return cassette;

            // Tray 확인
            var tray = await GetTrayByIdAsync(id);
            if (tray != null)
                return tray;

            // Memory 확인
            var memory = await GetMemoryByIdAsync(id);
            if (memory != null)
                return memory;

            return null;
        }

        public async Task<ITransportable> AddAsync(ITransportable entity, CancellationToken cancellationToken = default)
        {
            switch (entity.TransportType)
            {
                case ETransportType.Cassette:
                    await SaveCassetteAsync((Cassette)entity);
                    break;

                case ETransportType.Tray:
                    await SaveTrayAsync((Tray)entity);
                    break;

                case ETransportType.Memory:
                    await SaveMemoryAsync((Memory)entity);
                    break;

                default:
                    throw new ArgumentException($"Unsupported transportable type: {entity.GetType()}");
            }

            return entity;
        }

        public async Task<IEnumerable<ITransportable>> AddRangeAsync(IEnumerable<ITransportable> entities, CancellationToken cancellationToken = default)
        {
            var tasks = entities.Select(entity => AddAsync(entity, cancellationToken));
            return await Task.WhenAll(tasks);
        }

        public async Task<ITransportable> UpdateAsync(ITransportable entity, CancellationToken cancellationToken = default)
        {
            // HSet은 업데이트와 추가가 동일
            return await AddAsync(entity, cancellationToken);
        }

        public async Task<bool> UpdateRangeAsync(IEnumerable<ITransportable> entities, CancellationToken cancellationToken = default)
        {
            var tasks = entities.Select(entity => UpdateAsync(entity, cancellationToken));
            await Task.WhenAll(tasks);
            return true;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            // 어떤 타입인지 확인 후 삭제
            if (await _database.KeyExistsAsync($"{CASSETTE_KEY_PREFIX}{id}"))
            {
                await DeleteCassetteAsync(id);
                return true;
            }

            if (await _database.KeyExistsAsync($"{TRAY_KEY_PREFIX}{id}"))
            {
                await DeleteTrayAsync(id);
                return true;
            }

            if (await _database.KeyExistsAsync($"{MEMORY_KEY_PREFIX}{id}"))
            {
                await DeleteMemoryAsync(id);
                return true;
            }

            return false;
        }

        public async Task<bool> DeleteAsync(ITransportable entity, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync(entity.Id, cancellationToken);
        }

        public async Task<bool> DeleteRangeAsync(IEnumerable<ITransportable> entities, CancellationToken cancellationToken = default)
        {
            var tasks = entities.Select(entity => DeleteAsync(entity, cancellationToken));
            var results = await Task.WhenAll(tasks);
            return results.All(result => result);
        }

        public async Task<bool> ExistsAsync(Expression<Func<ITransportable, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var transportables = await GetAsync(predicate, cancellationToken);
            return transportables.Any();
        }

        public async Task<int> CountAsync(Expression<Func<ITransportable, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
            {
                var cassetteCount = await _database.SetLengthAsync(CASSETTES_SET_KEY);
                var trayCount = await _database.SetLengthAsync(TRAYS_SET_KEY);
                var memoryCount = await _database.SetLengthAsync(MEMORIES_SET_KEY);
                return (int)(cassetteCount + trayCount + memoryCount);
            }

            var filteredTransportables = await GetAsync(predicate, cancellationToken);
            return filteredTransportables.Count;
        }

        #endregion

        #region ITransportsRepository Set Operations

        public void AddTrayToCassette(string cassetteId, string trayId)
        {
            _database.SetAdd($"{CASSETTE_KEY_PREFIX}{cassetteId}:tray_ids", trayId);
        }

        public void RemoveTrayFromCassette(string cassetteId, string trayId)
        {
            _database.SetRemove($"{CASSETTE_KEY_PREFIX}{cassetteId}:tray_ids", trayId);
        }

        public void AddMemoryToTray(string trayId, string memoryId)
        {
            _database.SetAdd($"{TRAY_KEY_PREFIX}{trayId}:memory_ids", memoryId);
        }

        public void RemoveMemoryFromTray(string trayId, string memoryId)
        {
            _database.SetRemove($"{TRAY_KEY_PREFIX}{trayId}:memory_ids", memoryId);
        }

        #endregion

        #region Private Cassette Operations

        private async Task<List<Cassette>> GetAllCassettesAsync()
        {
            var cassetteIds = await _database.SetMembersAsync(CASSETTES_SET_KEY);
            var cassettes = new List<Cassette>();

            foreach (var cassetteId in cassetteIds)
            {
                if (!cassetteId.IsNull)
                {
                    var cassette = await GetCassetteByIdAsync(cassetteId!);
                    if (cassette != null)
                        cassettes.Add(cassette);
                }
            }

            return cassettes;
        }

        private async Task<Cassette?> GetCassetteByIdAsync(string id)
        {
            var hashEntries = await _database.HashGetAllAsync($"{CASSETTE_KEY_PREFIX}{id}");
            if (hashEntries.Length == 0)
                return null;

            var cassetteName = GetHashValue(hashEntries, "name");

            // Tray ID 목록 조회
            var trayIds = await _database.SetMembersAsync($"{CASSETTE_KEY_PREFIX}{id}:tray_ids");
            var trays = new List<Tray>();

            foreach (var trayId in trayIds)
            {
                if (!trayId.IsNull)
                {
                    var tray = await GetTrayByIdAsync(trayId!);
                    if (tray != null)
                        trays.Add(tray);
                }
            }

            return new Cassette(id, cassetteName, trays);
        }

        private async Task SaveCassetteAsync(Cassette cassette)
        {
            var hashEntries = new HashEntry[]
            {
                new HashEntry("id", cassette.Id),
                new HashEntry("name", cassette.Name),
                new HashEntry("transport_type", cassette.TransportType.ToString())
            };

            await _database.HashSetAsync($"{CASSETTE_KEY_PREFIX}{cassette.Id}", hashEntries);
            await _database.SetAddAsync(CASSETTES_SET_KEY, cassette.Id);

            // Tray ID 목록 저장
            var traySetKey = $"{CASSETTE_KEY_PREFIX}{cassette.Id}:tray_ids";
            await _database.KeyDeleteAsync(traySetKey);

            if (cassette.Trays.Any())
            {
                var trayIds = cassette.Trays.Select(t => (RedisValue)t.Id).ToArray();
                await _database.SetAddAsync(traySetKey, trayIds);
            }
        }

        private async Task DeleteCassetteAsync(string id)
        {
            await _database.KeyDeleteAsync($"{CASSETTE_KEY_PREFIX}{id}");
            await _database.KeyDeleteAsync($"{CASSETTE_KEY_PREFIX}{id}:tray_ids");
            await _database.SetRemoveAsync(CASSETTES_SET_KEY, id);
        }

        #endregion

        #region Private Tray Operations

        private async Task<List<Tray>> GetAllTraysAsync()
        {
            var trayIds = await _database.SetMembersAsync(TRAYS_SET_KEY);
            var trays = new List<Tray>();

            foreach (var trayId in trayIds)
            {
                if (!trayId.IsNull)
                {
                    var tray = await GetTrayByIdAsync(trayId!);
                    if (tray != null)
                        trays.Add(tray);
                }
            }

            return trays;
        }

        private async Task<Tray?> GetTrayByIdAsync(string id)
        {
            var hashEntries = await _database.HashGetAllAsync($"{TRAY_KEY_PREFIX}{id}");
            if (hashEntries.Length == 0)
                return null;

            string trayId = GetHashValue(hashEntries, "id");
            string trayName = GetHashValue(hashEntries, "name");

            // Memory ID 목록 조회
            var memoryIds = await _database.SetMembersAsync($"{TRAY_KEY_PREFIX}{id}:memory_ids");
            var memories = new List<Memory>();

            foreach (var memoryId in memoryIds)
            {
                if (!memoryId.IsNull)
                {
                    var memory = await GetMemoryByIdAsync(memoryId!);
                    if (memory != null)
                        memories.Add(memory);
                }
            }


            return new Tray(id: trayId, name: trayName, memories: memories);
        }

        private async Task SaveTrayAsync(Tray tray)
        {
            // Tray 기본 정보 저장
            var hashEntries = new HashEntry[]
            {
                new HashEntry("id", tray.Id),
                new HashEntry("name", tray.Name),
                new HashEntry("transport_type", tray.TransportType.ToString())
            };

            await _database.HashSetAsync($"{TRAY_KEY_PREFIX}{tray.Id}", hashEntries);
            await _database.SetAddAsync(TRAYS_SET_KEY, tray.Id);

            // Memory ID 목록 저장
            var memorySetKey = $"{TRAY_KEY_PREFIX}{tray.Id}:memory_ids";
            await _database.KeyDeleteAsync(memorySetKey);

            if (tray.Memories != null && tray.Memories.Any())
            {
                // 각 Memory 객체를 개별적으로 저장
                foreach (Memory memory in tray.Memories)
                {
                    await SaveMemoryAsync(memory);
                }

                // Memory ID 목록을 Set으로 저장
                var memoryIds = tray.Memories.Select(m => (RedisValue)m.Id).ToArray();
                await _database.SetAddAsync(memorySetKey, memoryIds);
            }
        }

        private async Task DeleteTrayAsync(string id)
        {
            await _database.KeyDeleteAsync($"{TRAY_KEY_PREFIX}{id}");
            await _database.KeyDeleteAsync($"{TRAY_KEY_PREFIX}{id}:memory_ids");
            await _database.SetRemoveAsync(TRAYS_SET_KEY, id);
        }

        #endregion

        #region Private Memory Operations

        private async Task<List<Memory>> GetAllMemoriesAsync()
        {
            var memoryIds = await _database.SetMembersAsync(MEMORIES_SET_KEY);
            var memories = new List<Memory>();

            foreach (var memoryId in memoryIds)
            {
                if (!memoryId.IsNull)
                {
                    var memory = await GetMemoryByIdAsync(memoryId!);
                    if (memory != null)
                        memories.Add(memory);
                }
            }

            return memories;
        }

        private async Task<Memory?> GetMemoryByIdAsync(string id)
        {
            var hashEntries = await _database.HashGetAllAsync($"{MEMORY_KEY_PREFIX}{id}");
            if (hashEntries.Length == 0)
                return null;


            string memoryId = GetHashValue(hashEntries, "id");
            string memoryName = GetHashValue(hashEntries, "name");
            return new Memory(id: memoryId, name: memoryName);
        }

        private async Task SaveMemoryAsync(Memory memory)
        {
            var hashEntries = new HashEntry[]
            {
                new HashEntry("id", memory.Id),
                new HashEntry("name", memory.Name),
                new HashEntry("transport_type", memory.TransportType.ToString())
            };

            await _database.HashSetAsync($"{MEMORY_KEY_PREFIX}{memory.Id}", hashEntries);
            await _database.SetAddAsync(MEMORIES_SET_KEY, memory.Id);
        }

        private async Task DeleteMemoryAsync(string id)
        {
            await _database.KeyDeleteAsync($"{MEMORY_KEY_PREFIX}{id}");
            await _database.SetRemoveAsync(MEMORIES_SET_KEY, id);
        }

        #endregion

        #region Helper Methods

        private static string GetHashValue(HashEntry[] hashEntries, string fieldName)
        {
            return hashEntries.FirstOrDefault(e => e.Name == fieldName).Value.ToString();
        }

        private static int GetHashValueAsInt(HashEntry[] hashEntries, string fieldName)
        {
            var value = GetHashValue(hashEntries, fieldName);
            return int.TryParse(value, out var result) ? result : 0;
        }

        #endregion
    }
}