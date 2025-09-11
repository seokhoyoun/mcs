using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;

namespace Nexus.Core.Domain.Models.Locations
{
    // MarkerLocation: 공간상의 마커(포지션 전용). 아이템 적재 불가.
    public class MarkerLocation : Location
    {
        public MarkerLocation(string id, string name) : base(id, name, ELocationType.Marker)
        {
        }
    }
}

