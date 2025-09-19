using System.Collections.Generic;

namespace Nexus.Core.Domain.Models.Locations.DTO
{
    public class LocationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LocationType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ParentId { get; set; } = string.Empty;
        public bool IsVisible { get; set; } = true;
        public bool IsRelativePosition { get; set; }
        public int RotateX { get; set; }
        public int RotateY { get; set; }
        public int RotateZ { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Depth { get; set; }
        public string MarkerRole { get; set; } = string.Empty;
        public string CurrentItemId { get; set; } = string.Empty;
        public List<string> Children { get; set; } = new List<string>();
    }
}
