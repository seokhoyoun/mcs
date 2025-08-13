using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Areas
{
    public class Set : IEntity
    {
        public string Id { get; private set; }
        public string Name { get; private set; }

        public IReadOnlyList<Location<Memory>> MemoryPorts => _memoryPorts.AsReadOnly();

        private List<Location<Memory>> _memoryPorts = new List<Location<Memory>>();

        public Set(string id, string name, IEnumerable<Location<Memory>> memoryPorts)
        {
            Id = id;
            Name = name;

            _memoryPorts.AddRange(memoryPorts);
        }
    }
}
