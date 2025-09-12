namespace Nexus.Core.Domain.Models.Locations.DTO
{
    public class LocationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LocationType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
