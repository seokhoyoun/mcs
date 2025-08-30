using Nexus.Gateway.Services.Commands;

namespace Nexus.Gateway.Services.Interfaces
{
    public interface ILotCreationService
    {
        Task<string> CreateLotAsync(CreateLotCommand command, CancellationToken cancellationToken = default);
    }
}