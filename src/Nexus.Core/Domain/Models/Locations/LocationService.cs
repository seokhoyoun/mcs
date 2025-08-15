using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Shared.Application.Interfaces;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Locations
{
    public class LocationService
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly ILocationRepository _locationRepository;

        private Dictionary<string, Location<ITransportable>> _locations = new();

        public LocationService(IEventPublisher eventPublisher, ILocationRepository locationRepository)
        {
            _eventPublisher = eventPublisher;
            _locationRepository = locationRepository;
        }

        public void AddLocations(IEnumerable<Location<ITransportable>> locations)
        {
            foreach (var location in locations)
            {
                if (!_locations.ContainsKey(location.Id))
                {
                    _locations[location.Id] = location;
                }
            }
        }

        /// <summary>
        /// 저장소에서 LocationState를 조회하여 Location 객체의 상태를 동기화합니다.
        /// </summary>
        /// <param name="locationId">동기화할 Location의 ID</param>
        /// <param name="location">상태를 업데이트할 Location 객체</param>
        public async Task RefreshLocationStateAsync(string locationId, Location<ITransportable> location)
        {
            var state = await _locationRepository.GetStateAsync(locationId);
            if (state == null) 
            {
                return;
            }
            location.Status = (ELocationStatus)state.Status;
            //location.CurrentItem = state.CurrentItemId;
            // 필요시 CurrentItem 등 추가 동기화
            // location.CurrentItem = ... (state에 해당 정보가 있다면 매핑)
        }

        /// <summary>
        /// 특정 위치에 아이템을 적재하고 상태를 업데이트합니다.
        /// </summary>
        /// <param name="location">상태를 변경할 Location 객체입니다.</param>
        /// <param name="item">적재할 아이템입니다.</param>
        public async Task LoadItemAsync(Location<ITransportable> location, ITransportable item)
        {
            // Location 객체의 상태 변경 메서드 호출 (internal 접근)
            //location.Load(item);
            //location.ChangeStatus(ELocationStatus.Occupied);

            //// DomainEvents 컬렉션을 순회하며 이벤트 발행
            //foreach (var domainEvent in location.DomainEvents)
            //{
            //    await _eventPublisher.PublishAsync(domainEvent);
            //}
            //// 이벤트 발행 후 DomainEvents 목록을 비웁니다.
            //location.ClearDomainEvents();
        }

        /// <summary>
        /// 특정 위치에서 아이템을 언로드하고 상태를 업데이트합니다.
        /// </summary>
        /// <param name="location">상태를 변경할 Location 객체입니다.</param>
        /// <returns>언로드된 아이템입니다.</returns>
        public async Task<ITransportable?> UnloadItemAsync(Location<ITransportable> location)
        {
            //var item = location.Unload();
            //location.ChangeStatus(ELocationStatus.Available);

            //// DomainEvents 컬렉션을 순회하며 이벤트 발행
            //foreach (var domainEvent in location.DomainEvents)
            //{
            //    await _eventPublisher.PublishAsync(domainEvent);
            //}
            //location.ClearDomainEvents();

            return null;
        }
    }
}