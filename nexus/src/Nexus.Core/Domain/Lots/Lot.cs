using Nexus.Core.Domain.Cassettes;
using Nexus.Core.Domain.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Lots
{
    internal class Lot : IEntity
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public IReadOnlyList<Cassette> Cassettes => _cassettes.AsReadOnly();

        private List<Cassette> _cassettes = new List<Cassette>();
    }
}
