using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Shared.Application.DTO;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;

namespace Nexus.Infrastructure.Persistence.Redis
{
    public class RedisTransportsRepository : ITransportsRepository
    {
        private readonly IDatabase _redis;

        public RedisTransportsRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            _redis = connectionMultiplexer.GetDatabase();
        }

        #region Cassette

        public IEnumerable<CassetteState> GetAllCassettes()
        {
            var cassetteValues = _redis.SetMembers("cassettes");
            foreach (var x in cassetteValues)
            {
                if (x.IsNullOrEmpty) continue;
                var id = (string)x!;
                var hash = _redis.HashGetAll($"cassette:{id}");
                if (hash.Length == 0) continue;

                var cassetteId = hash.FirstOrDefault(x => x.Name == "Id").Value;
                if (!cassetteId.IsNullOrEmpty)
                {
                    var trayValues = _redis.SetMembers($"cassette:{cassetteId}:tray_ids");
                    var trayIds = new List<string>();
                    foreach (var t in trayValues)
                    {
                        if (!t.IsNullOrEmpty)
                            trayIds.Add((string)t!);
                    }

                    yield return new CassetteState
                    {
                        Id = cassetteId!,
                        TrayIds = trayIds
                    };
                }
            }
        }

        public CassetteState? GetCassetteById(string id)
        {
            var hash = _redis.HashGetAll($"cassette:{id}");
            if (hash.Length == 0) return null;

            var cassetteId = hash.FirstOrDefault(x => x.Name == "Id").Value;
            if (!cassetteId.IsNullOrEmpty)
            {
                var trayValues = _redis.SetMembers($"cassette:{cassetteId}:tray_ids");
                var trayIds = new List<string>();
                foreach (var t in trayValues)
                {
                    if (!t.IsNullOrEmpty)
                        trayIds.Add((string)t!);
                }

                return new CassetteState
                {
                    Id = cassetteId!,
                    TrayIds = trayIds
                };
            }
            return null;
        }

        public void SaveCassette(CassetteState cassette)
        {
            var key = $"cassette:{cassette.Id}";
            var entries = new HashEntry[]
            {
                new HashEntry("Id", cassette.Id)
            };
            _redis.HashSet(key, entries);

            // cassettes Set에 ID 추가
            _redis.SetAdd("cassettes", cassette.Id);

            var setKey = $"cassette:{cassette.Id}:tray_ids";
            _redis.KeyDelete(setKey);
            if (cassette.TrayIds != null && cassette.TrayIds.Count > 0)
            {
                _redis.SetAdd(setKey, cassette.TrayIds.Select(x => (RedisValue)x).ToArray());
            }
        }

        public void DeleteCassette(string id)
        {
            _redis.KeyDelete($"cassette:{id}");
            _redis.KeyDelete($"cassette:{id}:tray_ids");
            _redis.SetRemove("cassettes", id);
        }

        public void AddTrayToCassette(string cassetteId, string trayId)
        {
            _redis.SetAdd($"cassette:{cassetteId}:tray_ids", trayId);
        }

        public void RemoveTrayFromCassette(string cassetteId, string trayId)
        {
            _redis.SetRemove($"cassette:{cassetteId}:tray_ids", trayId);
        }

        #endregion

        #region Tray

        public IEnumerable<TrayState> GetAllTrays()
        {
            var trayValues = _redis.SetMembers("trays");
            foreach (var x in trayValues)
            {
                if (x.IsNullOrEmpty) continue;
                var id = (string)x!;
                var hash = _redis.HashGetAll($"tray:{id}");
                if (hash.Length == 0) continue;

                var trayId = hash.FirstOrDefault(x => x.Name == "Id").Value;
                if (!trayId.IsNullOrEmpty)
                {
                    var memoryValues = _redis.SetMembers($"tray:{trayId}:memory_ids");
                    var memoryIds = new List<string>();
                    foreach (var m in memoryValues)
                    {
                        if (!m.IsNullOrEmpty)
                            memoryIds.Add((string)m!);
                    }

                    yield return new TrayState
                    {
                        Id = trayId!,
                        MemoryIds = memoryIds
                    };
                }
            }
        }

        public TrayState? GetTrayById(string id)
        {
            var hash = _redis.HashGetAll($"tray:{id}");
            if (hash.Length == 0) return null;

            var trayId = hash.FirstOrDefault(x => x.Name == "Id").Value;
            if (!trayId.IsNullOrEmpty)
            {
                var memoryValues = _redis.SetMembers($"tray:{trayId}:memory_ids");
                var memoryIds = new List<string>();
                foreach (var m in memoryValues)
                {
                    if (!m.IsNullOrEmpty)
                        memoryIds.Add((string)m!);
                }

                return new TrayState
                {
                    Id = trayId!,
                    MemoryIds = memoryIds
                };
            }
            return null;
        }

        public void SaveTray(TrayState tray)
        {
            var key = $"tray:{tray.Id}";
            var entries = new HashEntry[]
            {
                new HashEntry("Id", tray.Id)
            };
            _redis.HashSet(key, entries);

            // trays Set에 ID 추가
            _redis.SetAdd("trays", tray.Id);

            var setKey = $"tray:{tray.Id}:memory_ids";
            _redis.KeyDelete(setKey);
            if (tray.MemoryIds != null && tray.MemoryIds.Count > 0)
            {
                _redis.SetAdd(setKey, tray.MemoryIds.Select(x => (RedisValue)x).ToArray());
            }
        }

        public void DeleteTray(string id)
        {
            _redis.KeyDelete($"tray:{id}");
            _redis.KeyDelete($"tray:{id}:memory_ids");
            _redis.SetRemove("trays", id);
        }

        public void AddMemoryToTray(string trayId, string memoryId)
        {
            _redis.SetAdd($"tray:{trayId}:memory_ids", memoryId);
        }

        public void RemoveMemoryFromTray(string trayId, string memoryId)
        {
            _redis.SetRemove($"tray:{trayId}:memory_ids", memoryId);
        }

        #endregion

        #region Memory

        public IEnumerable<MemoryState> GetAllMemories()
        {
            var redisValues = _redis.SetMembers("memories");
            foreach (var x in redisValues)
            {
                if (x.IsNullOrEmpty) continue;
                var id = (string)x!;

                yield return new MemoryState
                {
                    Id = id
                };
            }
        }

        public MemoryState? GetMemoryById(string id)
        {
            var hash = _redis.HashGetAll($"memory:{id}");
            if (hash.Length == 0) return null;

            var memoryId = hash.FirstOrDefault(x => x.Name == "Id").Value;
            if (!memoryId.IsNullOrEmpty)
            {
                return new MemoryState
                {
                    Id = memoryId!
                };
            }
            return null;
        }

        public void SaveMemory(MemoryState memory)
        {
            var key = $"memory:{memory.Id}";
            var entries = new HashEntry[]
            {
                new HashEntry("Id", memory.Id)
            };
            _redis.HashSet(key, entries);

            // memories Set에 ID 추가
            _redis.SetAdd("memories", memory.Id);
        }

        public void DeleteMemory(string id)
        {
            _redis.KeyDelete($"memory:{id}");
            _redis.SetRemove("memories", id);
        }

        #endregion
    }
}
