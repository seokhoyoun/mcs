using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var redisHash = await _redisDb.HashGetAllAsync("locations");

            if (redisHash.Length == 0)
            {
                _logger.LogWarning("Redis에 위치 데이터가 없습니다. 로컬 파일에서 로드합니다.");
                var localLocations = LoadLocationsFromLocalFile();

                var hashEntries = localLocations.Select(loc => new HashEntry(loc.Id, JsonSerializer.Serialize(loc))).ToArray();
                await _redisDb.HashSetAsync("locations", hashEntries);

                _logger.LogInformation("로컬 파일 데이터를 Redis에 성공적으로 저장했습니다.");
                return localLocations;
            }

            var locations = new List<Location<ITransportable>>();
            foreach (var hashEntry in redisHash)
            {
                // JsonSerializer.Deserialize 메서드는 제네릭 타입 Location<ITransportable>을 처리하기 위해
                // 기본 생성자가 필요합니다. 또는 적절한 DTO를 사용해야 합니다.
                var location = JsonSerializer.Deserialize<Location<ITransportable>>(hashEntry.Value);
                locations.Add(location);
            }
            return locations;
        }

        private List<Location<ITransportable>> LoadLocationsFromLocalFile()
        {
            var locations = new List<Location<ITransportable>>();
            var filePath = "/app/data/locations.csv"; // Docker 컨테이너 내부 경로

            _logger.LogInformation($"CSV 파일 경로: {filePath}");

            if (!File.Exists(filePath))
            {
                _logger.LogError($"CSV 파일을 찾을 수 없습니다: {filePath}");
                return locations;
            }

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines.Skip(1)) // 헤더 라인 건너뛰기
            {
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    var id = parts[0];
                    var name = parts[1];
                    // ELocationType으로 파싱
                    if (Enum.TryParse<ELocationType>(parts[2], true, out var locationType))
                    {
                        // Location<ITransportable> 객체 생성
                        locations.Add(new Location<ITransportable>(id, name, locationType));
                    }
                    else
                    {
                        _logger.LogError($"LocationType 파싱 오류: {parts[2]}");
                    }
                }
            }
            return locations;
        }
    }
}