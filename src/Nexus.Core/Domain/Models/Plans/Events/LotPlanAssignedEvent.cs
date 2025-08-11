using Nexus.Core.Domain.Shared.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Plans.Events
{
    public class LotPlanAssignedEvent : IDomainEvent
    {
        public string LotId { get; }
        public DateTime OccurredOn => DateTime.UtcNow;

        public LotPlanAssignedEvent(string lotId)
        {
            LotId = lotId;
        }
    }
}
