using Nexus.Core.Domain.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Shared.Events
{
    public abstract class EventEntity : IEntity
    {
        public string Id { get; protected set; }
        public string Name { get; protected set; }

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        private readonly List<IDomainEvent> _domainEvents = new();

        protected EventEntity(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
        protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    }
}
