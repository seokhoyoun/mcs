using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Models.Transports.Interfaces;

namespace Nexus.Core.Domain.Models.Locations
{
    public class MemoryLocation : Location
    {
 
        public MemoryLocation(string id, string name, ELocationType locationType) : base(id, name, locationType)
        {
        }
    }
}
