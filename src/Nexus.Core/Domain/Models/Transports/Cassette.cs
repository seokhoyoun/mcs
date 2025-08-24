using Nexus.Core.Domain.Models.Transports.Enums;
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
        public string Id { get; }
        public string Name { get; }
        public ETransportType TransportType => ETransportType.Cassette;
        public List<Tray> Trays { get; private set; }

        public Cassette(string id, string name, List<Tray> trays)
        {
            Id = id;
            Name = name;
            Trays = trays;
        }
    }
}
