using System;
using System.Collections.Generic;
using System.Linq;

namespace Nexus.Core.Domain.Models.Locations.Graphs
{
    /// <summary>
    /// Location 간 이동 경로를 관리하는 그래프입니다.
    /// </summary>
    public sealed class LocationGraph
    {
        private readonly HashSet<string> _nodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<LocationEdge> _edges = new List<LocationEdge>();
        private readonly Dictionary<string, List<LocationEdge>> _adjacency = new Dictionary<string, List<LocationEdge>>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<string> Nodes => _nodes;

        public IReadOnlyCollection<LocationEdge> Edges => _edges.AsReadOnly();

        public void AddNode(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                throw new ArgumentException("Location id is required.", nameof(locationId));
            }

            if (_nodes.Contains(locationId))
            {
                return;
            }

            _nodes.Add(locationId);
            _adjacency[locationId] = new List<LocationEdge>();
        }

        public void AddEdge(LocationEdge edge)
        {
            if (edge == null)
            {
                throw new ArgumentNullException(nameof(edge));
            }

            AddNode(edge.FromLocationId);
            AddNode(edge.ToLocationId);

            _edges.Add(edge);

            if (!_adjacency.TryGetValue(edge.FromLocationId, out List<LocationEdge>? fromList))
            {
                fromList = new List<LocationEdge>();
                _adjacency[edge.FromLocationId] = fromList;
            }
            fromList.Add(edge);

            if (edge.IsBidirectional)
            {
                if (!_adjacency.TryGetValue(edge.ToLocationId, out List<LocationEdge>? toList))
                {
                    toList = new List<LocationEdge>();
                    _adjacency[edge.ToLocationId] = toList;
                }
                LocationEdge reverse = new LocationEdge(edge.ToLocationId, edge.FromLocationId, edge.Cost, true);
                _edges.Add(reverse);
                toList.Add(reverse);
            }
        }

        public IReadOnlyList<LocationEdge> GetNeighbors(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                throw new ArgumentException("Location id is required.", nameof(locationId));
            }

            if (_adjacency.TryGetValue(locationId, out List<LocationEdge>? neighbors))
            {
                return neighbors.AsReadOnly();
            }

            return Array.Empty<LocationEdge>();
        }
    }
}
