using System.Collections.Generic;

namespace Nexus.Core.Domain.Models.Locations.Graphs
{
    /// <summary>
    /// 직렬화/저장을 위한 LocationGraph 스냅샷입니다.
    /// </summary>
    public sealed class LocationGraphSnapshot
    {
        public LocationGraphSnapshot(IReadOnlyCollection<string> nodes, IReadOnlyCollection<LocationEdge> edges)
        {
            Nodes = nodes;
            Edges = edges;
        }

        public IReadOnlyCollection<string> Nodes { get; }
        public IReadOnlyCollection<LocationEdge> Edges { get; }
    }
}
