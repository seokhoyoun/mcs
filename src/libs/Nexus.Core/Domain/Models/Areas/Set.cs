using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Shared.Bases;
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
        public IReadOnlyList<MemoryLocation> MemoryPorts => _memoryPorts.AsReadOnly();

        private readonly List<MemoryLocation> _memoryPorts = new List<MemoryLocation>();

        public Set(string id, string name, IReadOnlyList<MemoryLocation> memoryPorts)
        {
            Id = id;
            Name = name;

            _memoryPorts.AddRange(memoryPorts);
        }
    }
}
