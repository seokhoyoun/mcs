using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Shared.Events;
using Nexus.Shared.Application.DTO;

namespace Nexus.Core.Domain.Models.Locations.Events
{
    public class LocationStateChangedEvent : IDomainEvent
    {
        public LocationState State { get; set; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public LocationStateChangedEvent(LocationState state)
        {
            State = state;
        }
    }
}