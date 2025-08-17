using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Stockers
{
    public class Stocker : IEntity
    {
        public string Id { get; }
        public string Name { get; }

        public IReadOnlyList<CassetteLocation> CassettePorts => _cassettePorts.AsReadOnly();

        private List<CassetteLocation> _cassettePorts = new List<CassetteLocation>();

        public Stocker(string id, string name, IReadOnlyList<CassetteLocation> cassettePorts)
        {
            Id = id;
            Name = name;

            _cassettePorts.AddRange(cassettePorts);
        }
    }
}
