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
        public required string Id { get; set; }
        public required string Name { get; set; }

        public IReadOnlyList<Location<Memory>> MemoryPorts => _memoryPorts.AsReadOnly();

        private List<Location<Memory>> _memoryPorts = new List<Location<Memory>>();
    }
}
