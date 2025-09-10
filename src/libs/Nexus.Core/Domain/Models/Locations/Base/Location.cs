using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Locations.Base
{

    public abstract class Location : IEntity
    {
        public string Id { get; protected set; }
        public string Name { get; protected set; }
        public ELocationType LocationType { get; protected set; }
        public ELocationStatus Status { get; set; }
        public string CurrentItemId { get; set; } = string.Empty;

        protected Location(string id, string name, ELocationType locationType) 
        {
            Id = id;
            Name = name;
            LocationType = locationType;
        }


    }
}
