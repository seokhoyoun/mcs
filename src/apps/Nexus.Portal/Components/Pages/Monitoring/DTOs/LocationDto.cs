namespace Nexus.Portal.Components.Pages.Monitoring.DTOs
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
    }
}
