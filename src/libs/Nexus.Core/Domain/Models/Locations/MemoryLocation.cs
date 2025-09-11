using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Models.Transports.Interfaces;

namespace Nexus.Core.Domain.Models.Locations
{
    public class MemoryLocation : Location, IItemStorage
    {
        public MemoryLocation(string id, string name) : base(id, name, ELocationType.Memory)
        {
        }
    }
}
