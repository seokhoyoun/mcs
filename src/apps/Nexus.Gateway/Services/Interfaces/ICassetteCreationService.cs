using Nexus.Gateway.Services.Commands;

namespace Nexus.Gateway.Services.Interfaces
{
    public interface ICassetteCreationService
    {
        Task<string> CreateCassetteAsync(CreateCassetteCommand command, CancellationToken cancellationToken = default);
    }
}