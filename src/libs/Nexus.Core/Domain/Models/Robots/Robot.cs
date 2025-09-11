using Nexus.Core.Domain.Models.Robots.Enums;
using Nexus.Core.Domain.Shared.Bases;
using Nexus.Core.Domain.Models.Locations.Base;
using System.Collections.Generic;

namespace Nexus.Core.Domain.Models.Robots
{
    public class Robot : IEntity
    {
        public string Id { get; }
        public string Name { get; }
        public ERobotType RobotType { get; }
        public Position Position { get; set; } = new Position(0, 0, 0);
        public IReadOnlyList<Location> Locations => _locations.AsReadOnly();

        private readonly List<Location> _locations = new();

        public Robot(string id, string name, ERobotType robotType)
        {
            Id = id;
            Name = name;
            RobotType = robotType;
        }

        public Robot(
            string id,
            string name,
            ERobotType robotType,
            IReadOnlyList<Location> locations)
        {
            Id = id;
            Name = name;
            RobotType = robotType;
            _locations.AddRange(locations);
        }
    }
}
