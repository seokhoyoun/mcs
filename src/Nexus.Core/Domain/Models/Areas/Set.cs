using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Areas
{
    public class Set : IEntity
    {
        public string Id { get; }
        public string Name { get; }
        public IReadOnlyList<Location<Memory>> MemoryPorts => _memoryPorts.AsReadOnly();

        private readonly List<Location<Memory>> _memoryPorts = new List<Location<Memory>>();

        public Set(string id, string name, IReadOnlyList<Location<Memory>> memoryPorts)
        {
            Id = id;
            Name = name;

            _memoryPorts.AddRange(memoryPorts);
        }
    }
}
