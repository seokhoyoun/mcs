using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Transports
{
    public class Cassette : ITransportable
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
 

        private readonly List<Tray> _trays = new List<Tray>();
        public IReadOnlyList<IItem> Items => _trays.AsReadOnly();

        public Cassette()
        {
        }

        public Cassette(string id, string name, IEnumerable<Tray> trays)
        {
            Id = id;
            Name = name;

            if (trays != null)
                _trays.AddRange(trays);
        }
    }
}
