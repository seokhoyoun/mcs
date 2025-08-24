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
    public class Tray : ITransportable
    {
        public string Id { get; }
        public string Name { get;  }
        public ETransportType TransportType => ETransportType.Tray;
        public List<Memory> Memories { get; }

        public Tray(string id, string name, List<Memory> memories)
        {
            Id = id;
            Name = name;
            Memories = memories;
        }

    }
}
