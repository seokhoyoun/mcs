namespace Nexus.Core.Domain.Shared.Events
{
    public interface IEventHandler<in TEvent>
    {
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
    }
}