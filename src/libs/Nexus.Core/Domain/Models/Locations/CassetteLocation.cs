using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Locations
{
    public class CassetteLocation : Location, IItemStorage
    {
        public CassetteLocation(string id, string name) : base(id, name, ELocationType.Cassette)
        {
        }

    }
}
