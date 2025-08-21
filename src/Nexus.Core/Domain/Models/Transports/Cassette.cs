using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Shared.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Transports
{
    public class Cassette : ITransportable
    {
        public string Id { get;  }
        public string Name { get;  }

        public IReadOnlyList<IItem> Items => _trays.AsReadOnly();
        private readonly List<Tray> _trays = new List<Tray>();

        public Cassette(string id, string name, IReadOnlyList<Tray> trays)
        {
            Id = id;
            Name = name;

            if (trays != null)
                _trays.AddRange(trays);
        }
    }
}
