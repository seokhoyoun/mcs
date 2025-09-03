using Nexus.Core.Domain.Shared.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Lots.Events
{

    public class LotCreatedEvent : IDomainEvent
    {
        public string LotId { get; }

        public DateTime OccurredOn => DateTime.UtcNow;

        public LotCreatedEvent(string lotId)
        {
            LotId = lotId;
        }
    }
}
