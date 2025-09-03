using Nexus.Gateway.Services.Commands;

namespace Nexus.Gateway.Services.Interfaces
{
    public interface IAreaCreationService
    {
        Task<string> CreateAreaAsync(string jsonPayload, CancellationToken cancellationToken = default);    
    }
}