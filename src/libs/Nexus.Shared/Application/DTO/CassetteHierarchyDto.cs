using Nexus.Core.Domain.Models.Transports;

namespace Nexus.Shared.Application.DTO
{
    public class CassetteHierarchyDto
    {
        public IReadOnlyList<Cassette> Cassettes { get; set; } = new List<Cassette>();
        public Dictionary<string, IReadOnlyList<Tray>> CassetteTrays { get; set; } = new();
        public Dictionary<string, IReadOnlyList<Memory>> TrayMemories { get; set; } = new();
    }
}
