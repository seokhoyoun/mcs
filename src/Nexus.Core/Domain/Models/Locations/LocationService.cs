using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Shared.Application.Interfaces;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Locations
{
    public class LocationService
    {
        private readonly IEventPublisher _eventPublisher;

        public LocationService(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        /// <summary>
        /// 특정 위치에 아이템을 적재하고 상태를 업데이트합니다.
        /// </summary>
        /// <param name="location">상태를 변경할 Location 객체입니다.</param>
        /// <param name="item">적재할 아이템입니다.</param>
        public async Task LoadItemAsync(Location<ITransportable> location, ITransportable item)
        {
            // Location 객체의 상태 변경 메서드 호출 (internal 접근)
            location.Load(item);
            location.ChangeStatus(ELocationStatus.Occupied);

            // DomainEvents 컬렉션을 순회하며 이벤트 발행
            foreach (var domainEvent in location.DomainEvents)
            {
                await _eventPublisher.PublishAsync(domainEvent);
            }
            // 이벤트 발행 후 DomainEvents 목록을 비웁니다.
            location.ClearDomainEvents();
        }

        /// <summary>
        /// 특정 위치에서 아이템을 언로드하고 상태를 업데이트합니다.
        /// </summary>
        /// <param name="location">상태를 변경할 Location 객체입니다.</param>
        /// <returns>언로드된 아이템입니다.</returns>
        public async Task<ITransportable?> UnloadItemAsync(Location<ITransportable> location)
        {
            var item = location.Unload();
            location.ChangeStatus(ELocationStatus.Available);

            // DomainEvents 컬렉션을 순회하며 이벤트 발행
            foreach (var domainEvent in location.DomainEvents)
            {
                await _eventPublisher.PublishAsync(domainEvent);
            }
            location.ClearDomainEvents();

            return item;
        }
    }
}