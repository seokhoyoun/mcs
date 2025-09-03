namespace Nexus.Shared.Application.DTO
{
    public class TrayState
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public List<string> MemoryIds { get; set; } = new();
    }
}