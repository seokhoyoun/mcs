using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Base;
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
    public class Memory : ITransportable
    {
        public string Id { get; }
        public string Name { get; }
        public string DeviceId { get; set; } = string.Empty;

        public ETransportType TransportType => ETransportType.Memory;

        public Memory(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
   
}
