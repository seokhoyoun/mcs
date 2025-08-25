namespace Nexus.Core.Domain.Models.Stockers.Interfaces
{
    public interface IStockerService
    {
        Task AssignCassetteAsync(string stockerId, string cassetteId, string portId, CancellationToken cancellationToken = default);
    }
}
