using System.Threading;
using System.Threading.Tasks;
using Nexus.Core.Domain.Models.Locations.Graphs;
using Nexus.Core.Domain.Models.Locations.Interfaces;

namespace Nexus.Core.Domain.Models.Locations.Services
{
    /// <summary>
    /// LocationGraph를 로드/수정/저장하는 서비스입니다.
    /// </summary>
    public sealed class LocationGraphService : ILocationGraphService
    {
        private readonly ILocationGraphRepository _repository;

        public LocationGraphService(ILocationGraphRepository repository)
        {
            _repository = repository;
        }

        public async Task<LocationGraph> GetGraphAsync(CancellationToken cancellationToken = default)
        {
            LocationGraphSnapshot? snapshot = await _repository.GetAsync(cancellationToken);
            if (snapshot == null)
            {
                return new LocationGraph();
            }

            LocationGraph graph = new LocationGraph();
            foreach (string node in snapshot.Nodes)
            {
                graph.AddNode(node);
            }

            foreach (LocationEdge edge in snapshot.Edges)
            {
                graph.AddEdge(edge);
            }

            return graph;
        }

        public async Task AddNodeAsync(string locationId, CancellationToken cancellationToken = default)
        {
            LocationGraph graph = await GetGraphAsync(cancellationToken);
            graph.AddNode(locationId);
            await SaveAsync(graph, cancellationToken);
        }

        public async Task AddEdgeAsync(string fromLocationId, string toLocationId, double cost, bool isBidirectional, CancellationToken cancellationToken = default)
        {
            LocationGraph graph = await GetGraphAsync(cancellationToken);
            LocationEdge edge = new LocationEdge(fromLocationId, toLocationId, cost, isBidirectional);
            graph.AddEdge(edge);
            await SaveAsync(graph, cancellationToken);
        }

        private Task SaveAsync(LocationGraph graph, CancellationToken cancellationToken)
        {
            LocationGraphSnapshot snapshot = new LocationGraphSnapshot(graph.Nodes, graph.Edges);
            return _repository.SaveAsync(snapshot, cancellationToken);
        }
    }
}
