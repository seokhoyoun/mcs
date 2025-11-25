using System;

namespace Nexus.Core.Domain.Models.Locations.Graphs
{
    /// <summary>
    /// 위치 간 이동 가능 경로를 나타내는 간선입니다.
    /// </summary>
    public sealed class LocationEdge
    {
        public LocationEdge(string fromLocationId, string toLocationId, double cost, bool isBidirectional)
        {
            if (string.IsNullOrWhiteSpace(fromLocationId))
            {
                throw new ArgumentException("From location id is required.", nameof(fromLocationId));
            }

            if (string.IsNullOrWhiteSpace(toLocationId))
            {
                throw new ArgumentException("To location id is required.", nameof(toLocationId));
            }

            FromLocationId = fromLocationId;
            ToLocationId = toLocationId;
            Cost = cost;
            IsBidirectional = isBidirectional;
        }

        public string FromLocationId { get; }

        public string ToLocationId { get; }

        /// <summary>
        /// 이동 비용(거리, 시간 등)입니다.
        /// </summary>
        public double Cost { get; }

        /// <summary>
        /// 양방향 이동 가능 여부입니다.
        /// </summary>
        public bool IsBidirectional { get; }
    }
}
