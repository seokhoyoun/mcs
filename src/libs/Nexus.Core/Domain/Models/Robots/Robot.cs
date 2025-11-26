using Nexus.Core.Domain.Shared.Bases;
using Nexus.Core.Domain.Models.Locations.Base;
using System.Collections.Generic;

namespace Nexus.Core.Domain.Models.Robots
{
    public class Robot : IEntity
    {
        public string Id { get; }
        public string Name { get; }
        public Position Position { get; set; } = new Position(0, 0, 0);
   

        public Robot(
            string id,
            string name)
        {
            Id = id;
            Name = name;
        }
    }
}
