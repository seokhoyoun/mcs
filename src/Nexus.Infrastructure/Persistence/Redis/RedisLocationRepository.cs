using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nexus.Infrastructure.Persistence.Redis
{
    public class RedisLocationRepository : ILocationRepository
    {
        private readonly IDatabase _redisDb;
        private readonly ILogger<RedisLocationRepository> _logger;

        public RedisLocationRepository(IConnectionMultiplexer connection, ILogger<RedisLocationRepository> logger)
        {
            _redisDb = connection.GetDatabase();
            _logger = logger;
        }

        public async Task<IEnumerable<Location<ITransportable>>> GetAllAsync()
        {
            // 1. Redis에서 데이터 조회
            var redisHash = await _redisDb.HashGetAllAsync("locations");

            // 2. Redis에 데이터가 없는 경우
            if (redisHash.Length == 0)
            {
                // 2-1. 로컬 파일에서 데이터를 로드
                _logger.LogWarning("Redis에 위치 데이터가 없습니다. 로컬 파일에서 로드합니다.");
                var localLocations = LoadLocationsFromLocalFile(); // 로컬 파일 로딩 메서드

                // 2-2. Redis에 데이터 저장
                var hashEntries = localLocations.Select(loc => new HashEntry(loc.Id, JsonSerializer.Serialize(loc))).ToArray();
                await _redisDb.HashSetAsync("locations", hashEntries);

                _logger.LogInformation("로컬 파일 데이터를 Redis에 성공적으로 저장했습니다.");

                return localLocations;
            }


            // 3. Redis에 데이터가 있는 경우
            var locations = new List<Location<ITransportable>>();
            foreach (var hashEntry in redisHash)
            {
                var location = JsonSerializer.Deserialize<Location<ITransportable>>(hashEntry.Value);
                locations.Add(location);
            }
            return locations;
        }

        private List<Location<ITransportable>> LoadLocationsFromLocalFile()
        {
            // 예를 들어, JSON 파일에서 읽어오는 로직을 구현합니다.
            var json = File.ReadAllText("locations.json");
            return JsonSerializer.Deserialize<List<Location<ITransportable>>>(json);
        }
    }
}
