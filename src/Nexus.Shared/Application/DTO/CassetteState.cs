namespace Nexus.Shared.Application.DTO
{
    public class CassetteState
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public List<string> TrayIds { get; set; } = new();
    }
}