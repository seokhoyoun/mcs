using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Areas.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nexus.Infrastructure.Persistence.Redis
{
    public class RedisAreaRepository : IAreaRepository
    {
        private readonly IDatabase _redisDb;
        private readonly ILogger<RedisAreaRepository> _logger;

        public RedisAreaRepository(IConnectionMultiplexer connection, ILogger<RedisAreaRepository> logger)
        {
            _redisDb = connection.GetDatabase();
            _logger = logger;
        }

        public async Task<IEnumerable<Area>> GetAllAreasAsync()
        {
            var redisHash = await _redisDb.HashGetAllAsync("areas");

            if (redisHash.Length == 0)
            {
                _logger.LogWarning("Redis에 Area 데이터가 없습니다. 로컬 파일에서 로드합니다.");
                var localAreas = LoadAreasFromLocalFile();

                var hashEntries = localAreas.Select(a => new HashEntry(a.Id, JsonSerializer.Serialize(a))).ToArray();
                await _redisDb.HashSetAsync("areas", hashEntries);

                _logger.LogInformation("로컬 파일 데이터를 Redis에 성공적으로 저장했습니다.");
                return localAreas;
            }

            var areas = new List<Area>();
            foreach (var hashEntry in redisHash)
            {
                if (hashEntry.Value.IsNullOrEmpty)
                {
                    _logger.LogWarning($"Redis에 저장된 Area 데이터가 비어 있습니다: {hashEntry.Name}");
                    continue;
                }

                var area = JsonSerializer.Deserialize<Area>(hashEntry.Value.ToString());
                if (area == null)
                {
                    _logger.LogWarning($"Area 데이터가 유효하지 않습니다: {hashEntry.Name}");
                    continue;
                }

                areas.Add(area);
            }
            return areas;
        }

        private List<Area> LoadAreasFromLocalFile()
        {
            var areas = new List<Area>();
            var filePath = "/app/data/areas.json";
            _logger.LogInformation($"JSON 파일 경로: {filePath}");

            if (!File.Exists(filePath))
            {
                _logger.LogError($"JSON 파일을 찾을 수 없습니다: {filePath}");
                return areas;
            }

            var json = File.ReadAllText(filePath);
            var fileAreas = JsonSerializer.Deserialize<List<Area>>(json);

            if (fileAreas != null)
            {
                areas.AddRange(fileAreas);
            }
            return areas;
        }
    }
}