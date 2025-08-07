namespace Nexus.Core.Domain.Shared.Events
{
    public class LocationStatusChangedEvent : IDomainEvent
    {
        public string LocationId { get; }
        public ELocationStatus OldStatus { get; }
        public ELocationStatus NewStatus { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public LocationStatusChangedEvent(string locationId, ELocationStatus oldStatus, ELocationStatus newStatus)
        {
            LocationId = locationId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }
}