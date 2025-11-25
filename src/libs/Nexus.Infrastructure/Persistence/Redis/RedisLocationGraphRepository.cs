using Nexus.Core.Domain.Models.Locations.Graphs;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Infrastructure.Persistence.Redis
{
    /// <summary>
    /// LocationGraph를 Redis에 저장/조회하는 단순 리포지토리입니다.
    /// </summary>
    public sealed class RedisLocationGraphRepository : ILocationGraphRepository
    {
        private readonly IDatabase _database;

        private const string GRAPH_NODES_KEY = "location_graph:nodes";
        private const string GRAPH_EDGES_KEY = "location_graph:edges";

        public RedisLocationGraphRepository(IConnectionMultiplexer mux)
        {
            _database = mux.GetDatabase();
        }

        public async Task<LocationGraphSnapshot?> GetAsync(CancellationToken cancellationToken = default)
        {
            RedisValue[] nodeValues = await _database.SetMembersAsync(GRAPH_NODES_KEY);
            RedisValue[] edgeValues = await _database.ListRangeAsync(GRAPH_EDGES_KEY);

            List<string> nodes = nodeValues.Select(v => v.ToString()).Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
            List<LocationEdge> edges = new List<LocationEdge>();

            foreach (RedisValue edgeValue in edgeValues)
            {
                string raw = edgeValue.ToString();
                LocationEdge? parsed = ParseEdge(raw);
                if (parsed != null)
                {
                    edges.Add(parsed);
                }
            }

            if (nodes.Count == 0 && edges.Count == 0)
            {
                return null;
            }

            return new LocationGraphSnapshot(nodes, edges);
        }

        public async Task SaveAsync(LocationGraphSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            // 노드 저장
            foreach (string node in snapshot.Nodes)
            {
                await _database.SetAddAsync(GRAPH_NODES_KEY, node);
            }

            // 간선 저장 (리스트로)
            foreach (LocationEdge edge in snapshot.Edges)
            {
                string serialized = SerializeEdge(edge);
                await _database.ListRightPushAsync(GRAPH_EDGES_KEY, serialized);
            }
        }

        private static string SerializeEdge(LocationEdge edge)
        {
            return $"{edge.FromLocationId}|{edge.ToLocationId}|{edge.Cost}|{edge.IsBidirectional}";
        }

        private static LocationEdge? ParseEdge(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            string[] parts = raw.Split('|');
            if (parts.Length != 4)
            {
                return null;
            }

            string from = parts[0];
            string to = parts[1];
            if (!double.TryParse(parts[2], out double cost))
            {
                return null;
            }
            bool isBidirectional = bool.TryParse(parts[3], out bool b) && b;

            try
            {
                return new LocationEdge(from, to, cost, isBidirectional);
            }
            catch
            {
                return null;
            }
        }
    }
}
