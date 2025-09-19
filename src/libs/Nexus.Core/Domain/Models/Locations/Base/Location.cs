using System.Collections.Generic;
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
        public string ParentId { get; set; } = string.Empty;
        public bool IsVisible { get; set; } = true;
        public bool IsRelativePosition { get; set; } = false;
        public List<string> Children { get; set; } = new List<string>();
        public Rotation Rotation { get; set; } = new Rotation(0, 0, 0);
        public Position Position { get; set; } = new Position(0, 0, 0);
        public uint Width { get; set; } = 0;
        public uint Height { get; set; } = 0;
        public uint Depth { get; set; } = 0;

        protected Location(string id, string name, ELocationType locationType)
        {
            Id = id;
            Name = name;
            LocationType = locationType;
        }
    }
}
