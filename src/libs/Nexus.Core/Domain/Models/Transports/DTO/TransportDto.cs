using System;

namespace Nexus.Core.Domain.Models.Transports.DTO
{
    // Grid row DTO combining Cassette, Tray, and Memory
    public class TransportDto
    {
        public string Type { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Parent { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;
    }
}
