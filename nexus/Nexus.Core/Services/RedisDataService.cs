using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Nexus.Core.Services 
{
    /// <summary>
    /// Redis 데이터베이스와의 상호작용을 처리하는 서비스입니다.
    /// 키-값 저장, 메시지 발행/구독, 리스트 및 해시 관리를 포함합니다.
    /// </summary>
    public class RedisDataService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly ISubscriber _subscriber;

        /// <summary>
        /// RedisDataService의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="redis">Redis 연결 멀티플렉서 인스턴스입니다.</param>
        public RedisDataService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = _redis.GetDatabase();
            _subscriber = _redis.GetSubscriber();
        }

        /// <summary>
        /// Redis에 키-값 쌍을 저장합니다. 객체는 JSON으로 직렬화됩니다.
        /// </summary>
        /// <typeparam name="T">저장할 값의 타입입니다.</typeparam>
        /// <param name="key">Redis 키입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        /// <param name="expiry">키의 만료 시간입니다 (선택 사항).</param>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, json, expiry);
        }

        /// <summary>
        /// Redis에서 키에 해당하는 값을 가져와 역직렬화합니다.
        /// </summary>
        /// <typeparam name="T">가져올 값의 타입입니다.</typeparam>
        /// <param name="key">Redis 키입니다.</param>
        /// <returns>역직렬화된 값 또는 키가 존재하지 않으면 기본값입니다.</returns>
        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(value!);
        }

        /// <summary>
        /// 지정된 채널에 메시지를 발행합니다.
        /// </summary>
        /// <param name="channel">메시지를 발행할 채널입니다.</param>
        /// <param name="message">발행할 메시지입니다.</param>
        public async Task PublishAsync(string channel, string message)
        {
            await _subscriber.PublishAsync(new RedisChannel(channel, RedisChannel.PatternMode.Literal), message);
        }

        /// <summary>
        /// Redis 구독자 인스턴스를 반환하여 외부에서 채널을 구독할 수 있도록 합니다.
        /// </summary>
        /// <returns>ISubscriber 인스턴스입니다.</returns>
        public ISubscriber GetSubscriber()
        {
            return _subscriber;
        }

        /// <summary>
        /// Redis 리스트의 오른쪽에 값을 추가합니다. 객체는 JSON으로 직렬화됩니다.
        /// </summary>
        /// <typeparam name="T">추가할 값의 타입입니다.</typeparam>
        /// <param name="key">리스트의 키입니다.</param>
        /// <param name="value">추가할 값입니다.</param>
        /// <returns>리스트의 새 길이입니다.</returns>
        public async Task<long> ListRightPushAsync<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value);
            return await _database.ListRightPushAsync(key, json);
        }

        /// <summary>
        /// Redis 리스트의 왼쪽에서 값을 제거하고 반환합니다. 값을 역직렬화합니다.
        /// </summary>
        /// <typeparam name="T">가져올 값의 타입입니다.</typeparam>
        /// <param name="key">리스트의 키입니다.</param>
        /// <returns>제거된 값 또는 리스트가 비어있으면 기본값입니다.</returns>
        public async Task<T?> ListLeftPopAsync<T>(string key)
        {
            var value = await _database.ListLeftPopAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(value!);
        }

        /// <summary>
        /// Redis 해시에서 모든 필드와 값을 가져와 역직렬화합니다.
        /// </summary>
        /// <typeparam name="T">해시 값의 타입입니다.</typeparam>
        /// <param name="key">해시의 키입니다.</param>
        /// <returns>필드-값 쌍의 딕셔너리입니다.</returns>
        public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string key)
        {
            var hashEntries = await _database.HashGetAllAsync(key);
            return hashEntries.ToDictionary(
                entry => entry.Name.ToString(),
                entry => JsonSerializer.Deserialize<T>(entry.Value!)!
            );
        }

        /// <summary>
        /// Redis 해시에 필드-값 쌍을 저장합니다. 객체는 JSON으로 직렬화됩니다.
        /// </summary>
        /// <typeparam name="T">저장할 값의 타입입니다.</typeparam>
        /// <param name="key">해시의 키입니다.</param>
        /// <param name="field">해시 필드입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public async Task HashSetAsync<T>(string key, string field, T value)
        {
            var json = JsonSerializer.Serialize(value);
            await _database.HashSetAsync(key, field, json);
        }

        /// <summary>
        /// Redis에 해당 키가 존재하는지 확인합니다.
        /// </summary>
        /// <param name="key">확인할 Redis 키입니다.</param>
        /// <returns>키가 존재하면 true, 아니면 false입니다.</returns>
        public async Task<bool> KeyExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        /// <summary>
        /// Redis에서 해당 키를 삭제합니다.
        /// </summary>
        /// <param name="key">삭제할 Redis 키입니다.</param>
        /// <returns>삭제 성공 여부입니다.</returns>
        public async Task<bool> KeyDeleteAsync(string key)
        {
            return await _database.KeyDeleteAsync(key);
        }

        /// <summary>
        /// Redis 키의 만료 시간을 조회합니다.
        /// </summary>
        /// <param name="key">만료 시간을 조회할 키입니다.</param>
        /// <returns>남은 만료 시간(TimeSpan) 또는 null(만료 없음/키 없음)입니다.</returns>
        public async Task<TimeSpan?> GetExpiryAsync(string key)
        {
            return await _database.KeyTimeToLiveAsync(key);
        }

        /// <summary>
        /// Redis 키의 만료 시간을 설정합니다.
        /// </summary>
        /// <param name="key">만료 시간을 설정할 키입니다.</param>
        /// <param name="expiry">설정할 만료 시간입니다.</param>
        /// <returns>설정 성공 여부입니다.</returns>
        public async Task<bool> SetExpiryAsync(string key, TimeSpan expiry)
        {
            return await _database.KeyExpireAsync(key, expiry);
        }

        /// <summary>
        /// Redis 리스트의 모든 값을 조회하여 역직렬화합니다.
        /// </summary>
        /// <typeparam name="T">리스트 값의 타입입니다.</typeparam>
        /// <param name="key">리스트의 키입니다.</param>
        /// <param name="start">조회 시작 인덱스(기본값: 0)</param>
        /// <param name="stop">조회 종료 인덱스(기본값: -1, 전체)</param>
        /// <returns>리스트의 모든 값(T) 컬렉션입니다.</returns>
        public async Task<List<T>> ListRangeAsync<T>(string key, long start = 0, long stop = -1)
        {
            var values = await _database.ListRangeAsync(key, start, stop);
            return values.Select(v => JsonSerializer.Deserialize<T>(v!)!).ToList();
        }

        /// <summary>
        /// Redis 해시에서 특정 필드를 삭제합니다.
        /// </summary>
        /// <param name="key">해시의 키입니다.</param>
        /// <param name="field">삭제할 해시 필드입니다.</param>
        /// <returns>삭제 성공 여부입니다.</returns>
        public async Task<bool> HashDeleteAsync(string key, string field)
        {
            return await _database.HashDeleteAsync(key, field);
        }

        /// <summary>
        /// 지정된 채널을 구독하고, 메시지 수신 시 핸들러를 실행합니다.
        /// </summary>
        /// <param name="channel">구독할 채널 이름입니다.</param>
        /// <param name="handler">메시지 수신 시 실행할 핸들러입니다.</param>
        public async Task SubscribeAsync(string channel, Action<RedisChannel, RedisValue> handler)
        {
            await _subscriber.SubscribeAsync(new RedisChannel(channel, RedisChannel.PatternMode.Literal), handler);
        }
    }
}
