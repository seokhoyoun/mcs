using Nexus.Core.Domain.Shared.Events;
using Nexus.Core.Domain.Shared.Interfaces;
using Nexus.Core.Domain.Transports.Interfaces;

namespace Nexus.Core.Domain.Shared
{

    public class Location<T> : IEntity where T : ITransportable
    {
        public string Id { get; }
        public string Name { get; }
        public ELocationType LocationType { get; }
        public ELocationStatus Status { get; set; }
        public T? CurrentItem { get; private set; }

        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();


        public Location(string id, string name, ELocationType locationType)
        {
            Id = id;
            Name = name;
            LocationType = locationType;
        }

        public void ChangeStatus(ELocationStatus newStatus)
        {
            if (Status != newStatus)
            {
                var oldStatus = Status;
                Status = newStatus;
                // 이벤트 발행
                _domainEvents.Add(new LocationStatusChangedEvent(Id, oldStatus, newStatus));
            }
        }

        public void ClearDomainEvents() => _domainEvents.Clear();

        public void Load(T item)
        {
            CurrentItem = item;
        }

        public T? Unload()
        {
            var item = CurrentItem;
            CurrentItem = default;
            return item;
        }
    }
}