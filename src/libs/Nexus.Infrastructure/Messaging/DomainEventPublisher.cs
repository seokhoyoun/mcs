using Nexus.Core.Messaging;
using Nexus.Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nexus.Infrastructure.Messaging
{
    public class DomainEventPublisher : IEventPublisher
    {
        private readonly IMessagePublisher _messagePublisher;

        public DomainEventPublisher(IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        {
            var eventType = typeof(TEvent).Name;
            var message = JsonSerializer.Serialize(@event);
            await _messagePublisher.PublishAsync(eventType, message, cancellationToken);
        }
    }
}
