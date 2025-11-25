using System.Linq.Expressions;
using Nexus.Core.Domain.Models.Robots;
using Nexus.Core.Domain.Models.Robots.Enums;
using Nexus.Core.Domain.Models.Robots.Interfaces;
using Nexus.Core.Domain.Shared.Bases;
using StackExchange.Redis;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Interfaces;

namespace Nexus.Infrastructure.Persistence.Redis
{
    public class RedisRobotRepository : IRobotRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        private const string ROBOT_KEY_PREFIX = "robot:";
        private const string ROBOTS_ALL_KEY = "robots:all";
        private const string ID_SEPARATOR = ",";

        public RedisRobotRepository(IConnectionMultiplexer connection)
        {
            _redis = connection;
            _database = connection.GetDatabase();
        }

        public async Task<IReadOnlyList<Robot>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            List<Robot> robots = new List<Robot>();
            RedisValue[] ids = await _database.SetMembersAsync(ROBOTS_ALL_KEY);
            foreach (RedisValue id in ids)
            {
                Robot? robot = await GetByIdAsync(id.ToString(), cancellationToken);
                if (robot != null)
                {
                    robots.Add(robot);
                }
            }
            return robots.AsReadOnly();
        }

        public async Task<IReadOnlyList<Robot>> GetAsync(Expression<Func<Robot, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Robot> all = await GetAllAsync(cancellationToken);
            Func<Robot, bool> compiled = predicate.Compile();
            return all.Where(compiled).ToList().AsReadOnly();
        }

        public async Task<Robot?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            HashEntry[] hashEntries = await _database.HashGetAllAsync($"{ROBOT_KEY_PREFIX}{id}");
            if (hashEntries.Length == 0)
            {
                return null;
            }

            string name = Helper.GetHashValue(hashEntries, "name");
            ERobotType robotType = Helper.GetHashValueAsEnum<ERobotType>(hashEntries, "robot_type");

            // Position 정보 조회
            uint positionX = (uint)Helper.GetHashValueAsInt(hashEntries, "x");
            uint positionY = (uint)Helper.GetHashValueAsInt(hashEntries, "y");
            uint positionZ = (uint)Helper.GetHashValueAsInt(hashEntries, "z");

         

            Robot robot = new Robot(id, name, robotType)
            {
                Position = new Position(positionX, positionY, positionZ)
            };

            return robot;
        }

        public async Task<Robot> AddAsync(Robot entity, CancellationToken cancellationToken = default)
        {
          
            HashEntry[] entries = new HashEntry[]
            {
                new HashEntry("id", entity.Id),
                new HashEntry("name", entity.Name),
                new HashEntry("robot_type", entity.RobotType.ToString()),
                new HashEntry("x", entity.Position.X),
                new HashEntry("y", entity.Position.Y),
                new HashEntry("z", entity.Position.Z)
            };

            await _database.HashSetAsync($"{ROBOT_KEY_PREFIX}{entity.Id}", entries);
            await _database.SetAddAsync(ROBOTS_ALL_KEY, entity.Id);
            return entity;
        }

        public async Task<IEnumerable<Robot>> AddRangeAsync(IEnumerable<Robot> entities, CancellationToken cancellationToken = default)
        {
            IEnumerable<Task<Robot>> tasks = entities.Select(e => AddAsync(e, cancellationToken));
            return await Task.WhenAll(tasks);
        }

        public async Task<Robot> UpdateAsync(Robot entity, CancellationToken cancellationToken = default)
        {
            return await AddAsync(entity, cancellationToken);
        }

        public async Task<bool> UpdateRangeAsync(IEnumerable<Robot> entities, CancellationToken cancellationToken = default)
        {
            IEnumerable<Task<Robot>> tasks = entities.Select(e => UpdateAsync(e, cancellationToken));
            await Task.WhenAll(tasks);
            return true;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            if (await _database.KeyExistsAsync($"{ROBOT_KEY_PREFIX}{id}"))
            {
                await _database.KeyDeleteAsync($"{ROBOT_KEY_PREFIX}{id}");
                await _database.SetRemoveAsync(ROBOTS_ALL_KEY, id);
                return true;
            }
            return false;
        }

        public Task<bool> DeleteAsync(Robot entity, CancellationToken cancellationToken = default)
        {
            return DeleteAsync(entity.Id, cancellationToken);
        }

        public async Task<bool> DeleteRangeAsync(IEnumerable<Robot> entities, CancellationToken cancellationToken = default)
        {
            IEnumerable<Task<bool>> tasks = entities.Select(e => DeleteAsync(e, cancellationToken));
            bool[] results = await Task.WhenAll(tasks);
            return results.All(result => result);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Robot, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Robot> all = await GetAsync(predicate, cancellationToken);
            return all.Any();
        }

        public async Task<int> CountAsync(Expression<Func<Robot, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
            {
                long count = await _database.SetLengthAsync(ROBOTS_ALL_KEY);
                return (int)count;
            }
            IReadOnlyList<Robot> filtered = await GetAsync(predicate, cancellationToken);
            return filtered.Count;
        }

        /// <summary>
        /// 로봇의 위치 정보만 업데이트합니다.
        /// </summary>
        /// <param name="robotId">로봇 ID</param>
        /// <param name="position">새로운 위치</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>업데이트 성공 여부</returns>
        public async Task<bool> UpdatePositionAsync(string robotId, Position position, CancellationToken cancellationToken = default)
        {
            string key = $"{ROBOT_KEY_PREFIX}{robotId}";

            if (!await _database.KeyExistsAsync(key))
            {
                return false;
            }

            HashEntry[] positionEntries = new HashEntry[]
            {
                new HashEntry("x", position.X),
                new HashEntry("y", position.Y),
                new HashEntry("z", position.Z)
            };

            await _database.HashSetAsync(key, positionEntries);
            return true;
        }

        /// <summary>
        /// 로봇의 현재 위치를 조회합니다.
        /// </summary>
        /// <param name="robotId">로봇 ID</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>로봇의 위치 정보</returns>
        public async Task<Position?> GetPositionAsync(string robotId, CancellationToken cancellationToken = default)
        {
            string key = $"{ROBOT_KEY_PREFIX}{robotId}";
            HashEntry[] hashEntries = await _database.HashGetAllAsync(key);

            if (hashEntries.Length == 0)
            {
                return null;
            }

            uint positionX = (uint)Helper.GetHashValueAsInt(hashEntries, "x");
            uint positionY = (uint)Helper.GetHashValueAsInt(hashEntries, "y");
            uint positionZ = (uint)Helper.GetHashValueAsInt(hashEntries, "z");

            return new Position(positionX, positionY, positionZ);
        }
    }
}
