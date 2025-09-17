using System;

namespace Nexus.Core.Domain.Models.Lots.DTO
{
    public class LotPublishedEventDto
    {
        public string Event { get; set; } = string.Empty;
        public string LotId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}

