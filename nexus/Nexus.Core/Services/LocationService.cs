using Nexus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Services
{
    public class LocationService 
    {
        private readonly RedisDataService _redisDataService;
        private const string LOCATION_KEY_PREFIX = "location:state:"; // Redis 키 접두사

        /// <summary>
        /// LocationService의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="redisDataService">Redis 데이터 상호작용을 위한 서비스입니다.</param>
        public LocationService(RedisDataService redisDataService)
        {
            _redisDataService = redisDataService;
        }

        /// <summary>
        /// 특정 위치의 현재 상태를 Redis에서 가져옵니다.
        /// </summary>
        /// <param name="locationId">위치 ID</param>
        /// <returns>LocationState 객체 또는 null</returns>
        public async Task<LocationState?> GetLocationStateAsync(string locationId)
        {
            return await _redisDataService.GetAsync<LocationState>($"{LOCATION_KEY_PREFIX}{locationId}");
        }

        /// <summary>
        /// 새로운 위치를 Redis에 저장합니다.
        /// </summary>
        /// <param name="locationState">저장할 LocationState 객체</param>
        /// <exception cref="InvalidOperationException">동일 ID의 위치가 이미 존재할 경우 발생합니다.</exception>
        public async Task CreateLocationAsync(LocationState locationState)
        {
            // 이미 존재하는지 확인하여 중복 생성을 방지합니다.
            var existingLocation = await GetLocationStateAsync(locationState.Id);
            if (existingLocation != null)
            {
                throw new InvalidOperationException($"Location with ID '{locationState.Id}' already exists.");
            }
            await _redisDataService.SetAsync($"{LOCATION_KEY_PREFIX}{locationState.Id}", locationState);
        }

        /// <summary>
        /// 특정 위치에 아이템을 적재하고 Redis에 상태를 업데이트합니다.
        /// </summary>
        /// <param name="locationId">위치 ID</param>
        /// <param name="itemId">적재할 아이템의 ID</param>
        /// <exception cref="InvalidOperationException">위치를 찾을 수 없거나 이미 점유 중일 때 발생합니다.</exception>
        public async Task LoadItemIntoLocationAsync(string locationId, string itemId)
        {
            var locationState = await GetLocationStateAsync(locationId);
            if (locationState == null)
            {
                throw new InvalidOperationException($"Location with ID '{locationId}' not found.");
            }
            if (locationState.CurrentItemId != null)
            {
                throw new InvalidOperationException($"Location with ID '{locationId}' is already occupied by item '{locationState.CurrentItemId}'.");
            }

            locationState.CurrentItemId = itemId;
            // TODO: LocationStatus (예: Occupied)를 LocationState에 추가하고 업데이트하는 로직을 여기에 반영해야 합니다.
            await _redisDataService.SetAsync($"{LOCATION_KEY_PREFIX}{locationId}", locationState);
        }

        /// <summary>
        /// 특정 위치에서 아이템을 언로드하고 Redis에 상태를 업데이트합니다.
        /// </summary>
        /// <param name="locationId">위치 ID</param>
        /// <returns>언로드된 아이템의 ID (없으면 null)</returns>
        /// <exception cref="InvalidOperationException">위치를 찾을 수 없을 때 발생합니다.</exception>
        public async Task<string?> UnloadItemFromLocationAsync(string locationId)
        {
            var locationState = await GetLocationStateAsync(locationId);
            if (locationState == null)
            {
                throw new InvalidOperationException($"Location with ID '{locationId}' not found.");
            }

            var unloadedItemId = locationState.CurrentItemId;
            locationState.CurrentItemId = null;
            // TODO: LocationStatus (예: Available)를 LocationState에 추가하고 업데이트하는 로직을 여기에 반영해야 합니다.
            await _redisDataService.SetAsync($"{LOCATION_KEY_PREFIX}{locationId}", locationState);

            return unloadedItemId;
        }
    }
}
