using Nexus.Core.Domain.Models.Locations.Graphs;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Locations.Interfaces
{
    public interface ILocationGraphService
    {
        Task<LocationGraph> GetGraphAsync(CancellationToken cancellationToken = default);

        Task AddNodeAsync(string locationId, CancellationToken cancellationToken = default);

        Task AddEdgeAsync(string fromLocationId, string toLocationId, double cost, bool isBidirectional, CancellationToken cancellationToken = default);
    }
}
