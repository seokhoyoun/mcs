using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Stockers.Interfaces
{
    public interface IStockerService : IDataService<Stocker, string>
    {
        Task AssignCassetteAsync(string stockerId, string cassetteId, string portId, CancellationToken cancellationToken = default);
    }
}
