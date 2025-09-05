using Microsoft.AspNetCore.Hosting.Server;
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
    public class RedisTransportsRepository : ITransportRepository
    {
        private readonly IDatabase _database;


        private const string CASSETTES_ALL_KEY = "cassettes:all";
        private const string TRAYS_ALL_KEY = "trays:all";
        private const string MEMORIES_ALL_KEY = "memories:all";

        private const string CASSETTE_KEY_PREFIX = "cassette:";
        private const string TRAY_KEY_PREFIX = "tray:";
        private const string MEMORY_KEY_PREFIX = "memory:";

        private const string TRAY_ID_SEPARATOR = ",";
        private const string MEMORY_ID_SEPARATOR = ",";

        public RedisTransportsRepository(IConnectionMultiplexer connectionMultiplexer)
        {
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
                //var cassetteCount = await _database.SetLengthAsync(CASSETTES_SET_KEY);
                //var trayCount = await _database.SetLengthAsync(TRAYS_SET_KEY);
                //var memoryCount = await _database.SetLengthAsync(MEMORIES_SET_KEY);
                //return (int)(cassetteCount + trayCount + memoryCount);
                return 0;
            }

            var filteredTransportables = await GetAsync(predicate, cancellationToken);
            return filteredTransportables.Count;
        }

        #endregion

        #region ITransportsRepository Operations

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

        public async Task<IReadOnlyList<Cassette>> GetCassettesWithoutTraysAsync()
        {
            var ids = await _database.SetMembersAsync(CASSETTES_ALL_KEY);
            var cassettes = new List<Cassette>();
            foreach (RedisValue id in ids)
            {
                var hashEntries = await _database.HashGetAllAsync($"{CASSETTE_KEY_PREFIX}{id}");
                if (hashEntries.Length == 0)
                {
                    continue;
                }

                var cassetteName = GetHashValue(hashEntries, "name");

                var cassette = new Cassette(id.ToString(), cassetteName, new List<Tray>()); 
                cassettes.Add(cassette);
            }

            return cassettes;
        }
        public async Task<IReadOnlyList<Tray>> GetTraysWithoutMemoriesAsync(string cassetteId)
        {
            var trays = new List<Tray>();

            var hashEntries = await _database.HashGetAllAsync($"{CASSETTE_KEY_PREFIX}{cassetteId}");
            if (hashEntries.Length == 0)
                return trays;

            // tray_ids 필드에서 Tray Id 목록 파싱
            var trayIdsValue = GetHashValue(hashEntries, "tray_ids");
            string[] trayIds = trayIdsValue.Split(TRAY_ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);


            foreach (var trayId in trayIds)
            {
                var trayHash = await _database.HashGetAllAsync($"{TRAY_KEY_PREFIX}{trayId}");
                if (trayHash.Length == 0)
                    continue;

                string id = GetHashValue(trayHash, "id");
                string name = GetHashValue(trayHash, "name");

                var tray = new Tray(id, name, memories: new List<Memory>());

                trays.Add(tray);
            }

            return trays;
        }
        public async Task<IReadOnlyList<Memory>> GetMemoriesAsync(string trayId)
        {
            var trayHash = await _database.HashGetAllAsync($"{TRAY_KEY_PREFIX}{trayId}");
            if (trayHash.Length == 0)
                return Array.Empty<Memory>();

            var memoryIdsValue = GetHashValue(trayHash, "memory_ids");
            string[] memoryIds = memoryIdsValue.Split(TRAY_ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            var memories = new List<Memory>();

            foreach (var memoryId in memoryIds)
            {
                var memoryHash = await _database.HashGetAllAsync($"{MEMORY_KEY_PREFIX}{memoryId}");
                if (memoryHash.Length == 0)
                    continue;

                string id = GetHashValue(memoryHash, "id");
                string name = GetHashValue(memoryHash, "name");
                string deviceId = GetHashValue(memoryHash, "device_id");
                string locationId = GetHashValue(memoryHash, "location_id");

                var memory = new Memory(
                    id: id,
                    name: name
                );

                memory.DeviceId = deviceId;
                memories.Add(memory);
            }

            return memories;
        }


        #endregion

        #region Private Cassette Operations

        private async Task<List<Cassette>> GetAllCassettesAsync()
        {
            var ids = await _database.SetMembersAsync(CASSETTES_ALL_KEY);

            var cassettes = new List<Cassette>();
            foreach (RedisValue id in ids)
            {
                var cassette = await GetCassetteByIdAsync(id.ToString());
                if (cassette != null)
                    cassettes.Add(cassette);
            }

            return cassettes;
        }

        private async Task<Cassette?> GetCassetteByIdAsync(string id)
        {
            var hashEntries = await _database.HashGetAllAsync($"{CASSETTE_KEY_PREFIX}{id}");
            if (hashEntries.Length == 0)
            {
                return null;
            }

            var cassetteName = GetHashValue(hashEntries, "name");
            var trayIdsValue = GetHashValue(hashEntries, "tray_ids");

            string[] trayIds = trayIdsValue.Split(TRAY_ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            var trays = new List<Tray>();

            foreach (var trayId in trayIds)
            {
                var tray = await GetTrayByIdAsync(trayId);
                if (tray == null)
                {
                    continue;
                }
                trays.Add(tray);
            }

            return new Cassette(id, cassetteName, trays);
        }

        private async Task SaveCassetteAsync(Cassette cassette)
        {
            var trayIds = string.Join(TRAY_ID_SEPARATOR, cassette.Trays.Select(t => t.Id));

            var hashEntries = new HashEntry[]
            {
                new HashEntry("id", cassette.Id),
                new HashEntry("name", cassette.Name),
                new HashEntry("transport_type", cassette.TransportType.ToString()),
                new HashEntry("tray_ids", trayIds)
            };

            foreach (var tray in cassette.Trays)
            {
                await SaveTrayAsync(tray);
            }

            await _database.HashSetAsync($"{CASSETTE_KEY_PREFIX}{cassette.Id}", hashEntries);
            await _database.SetAddAsync(CASSETTES_ALL_KEY, cassette.Id);
        }

        private async Task DeleteCassetteAsync(string id)
        {
            await _database.KeyDeleteAsync($"{CASSETTE_KEY_PREFIX}{id}");
            await _database.SetRemoveAsync(CASSETTES_ALL_KEY, id);
        }

        #endregion

        #region Private Tray Operations

        private async Task<List<Tray>> GetAllTraysAsync()
        {
            var ids = await _database.SetMembersAsync(TRAYS_ALL_KEY);

            var trays = new List<Tray>();
            foreach (RedisValue id in ids)
            {
                var tray = await GetTrayByIdAsync(id.ToString());
                if (tray != null)
                    trays.Add(tray);
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
            string memoryIdsValue = GetHashValue(hashEntries, "memory_ids");

            string[] memoryIds = memoryIdsValue.Split(TRAY_ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            var memories = new List<Memory>();

            foreach (var memoryId in memoryIds)
            {
                var memory = await GetMemoryByIdAsync(memoryId);
                if (memory == null)
                {
                    continue;
                }
                memories.Add(memory);
            }

            return new Tray(id: trayId,
                            name: trayName,
                            memories: memories);
        }

        private async Task SaveTrayAsync(Tray tray)
        {
            var memoryIds = string.Join(MEMORY_ID_SEPARATOR, tray.Memories.Select(m => m.Id));

            var hashEntries = new HashEntry[]
            {
                new HashEntry("id", tray.Id),
                new HashEntry("name", tray.Name),
                new HashEntry("transport_type", tray.TransportType.ToString()),
                new HashEntry("memory_ids", memoryIds)
            };

            foreach (var memory in tray.Memories)
            {
                await SaveMemoryAsync(memory);
            }

            await _database.HashSetAsync($"{TRAY_KEY_PREFIX}{tray.Id}", hashEntries);
            await _database.SetAddAsync(TRAYS_ALL_KEY, tray.Id);
        }

        private async Task DeleteTrayAsync(string id)
        {
            await _database.KeyDeleteAsync($"{TRAY_KEY_PREFIX}{id}");
            await _database.SetRemoveAsync(TRAYS_ALL_KEY, id);
        }

        #endregion

        #region Private Memory Operations

        private async Task<List<Memory>> GetAllMemoriesAsync()
        {
            var ids = await _database.SetMembersAsync(MEMORIES_ALL_KEY);

            var memories = new List<Memory>();
            foreach (RedisValue id in ids)
            {
                var memory = await GetMemoryByIdAsync(id.ToString());
                if (memory != null)
                    memories.Add(memory);
            }

            return memories;
        }

        private async Task<Memory?> GetMemoryByIdAsync(string id)
        {
            var hashEntries = await _database.HashGetAllAsync($"{MEMORY_KEY_PREFIX}{id}");
            if (hashEntries.Length == 0)
            {
                return null;
            }

            string memoryId = GetHashValue(hashEntries, "id");
            string memoryName = GetHashValue(hashEntries, "name");
            string deviceId = GetHashValue(hashEntries, "device_id");

            var memory = new Memory(id: memoryId, name: memoryName);
            memory.DeviceId = deviceId;

            return memory;
        }

        private async Task SaveMemoryAsync(Memory memory)
        {
            var hashEntries = new HashEntry[]
            {
                new HashEntry("id", memory.Id),
                new HashEntry("name", memory.Name),
                new HashEntry("transport_type", memory.TransportType.ToString()),
                new HashEntry("device_id", memory.DeviceId)
            };

            await _database.HashSetAsync($"{MEMORY_KEY_PREFIX}{memory.Id}", hashEntries);
            await _database.SetAddAsync(MEMORIES_ALL_KEY, memory.Id);
        }

        private async Task DeleteMemoryAsync(string id)
        {
            await _database.KeyDeleteAsync($"{MEMORY_KEY_PREFIX}{id}");
            await _database.SetRemoveAsync(MEMORIES_ALL_KEY, id);
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