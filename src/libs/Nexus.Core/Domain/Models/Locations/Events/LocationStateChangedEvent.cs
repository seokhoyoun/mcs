using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Shared.Events;

namespace Nexus.Core.Domain.Models.Locations.Events
{
    public class LocationStateChangedEvent : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public LocationStateChangedEvent()
        {
        }
    }
}
