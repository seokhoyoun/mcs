using Nexus.Core.Domain.Models.Locations.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Locations
{
    internal class CassetteLocation : Location
    {
        public CassetteLocation(string id, string name, ELocationType locationType) : base(id, name, locationType)
        {
        }
    }
}
