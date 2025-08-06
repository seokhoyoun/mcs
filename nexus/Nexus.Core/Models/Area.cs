using Nexus.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Models
{
    public class Area : IEntity
    {
        public required string Id { get; set; }
        public required string Name { get; set; }

        public IReadOnlyList<Location<Cassette>> CassettePorts => _casssettePorts.AsReadOnly();

        private List<Location<Cassette>> _casssettePorts = new List<Location<Cassette>>();
    }
}
