using System;

namespace Nexus.Shared.Application.DTO
{
    // Grid row DTO combining Cassette, Tray, and Memory
    public class CassetteGridItem
    {
        public string Type { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Parent { get; set; } = string.Empty;

        public string DeviceId { get; set; } = string.Empty;

        public string LocationId { get; set; } = string.Empty;
    }
}
