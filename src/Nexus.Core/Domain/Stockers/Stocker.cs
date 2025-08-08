using Nexus.Core.Domain.Cassettes;
using Nexus.Core.Domain.Shared;
using Nexus.Core.Domain.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Stockers
{
    public class Stocker : IEntity
    {
        public required string Id { get; set; }
        public required string Name { get; set; }

        public IReadOnlyList<Location<Cassette>> Ports => _ports.AsReadOnly();

        private List<Location<Cassette>> _ports = new List<Location<Cassette>>();
    }
}
