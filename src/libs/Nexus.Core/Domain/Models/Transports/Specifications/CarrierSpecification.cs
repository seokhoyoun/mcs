using Nexus.Core.Domain.Models.Transports.Enums;
using Nexus.Core.Domain.Models.Transports.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nexus.Core.Domain.Models.Transports.Specifications
{
    /// <summary>
    /// Carrier가 적재할 수 있는 규칙(허용 종류/수량/규격/무게)을 정의합니다.
    /// </summary>
    public sealed class CarrierSpecification
    {
        public CarrierSpecification(string code, int? maxItemCount, IReadOnlyCollection<ECargoKind> allowedKinds, CargoDimension? maxSize, decimal? maxWeight)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Specification code is required.", nameof(code));
            }

            if (maxItemCount.HasValue && maxItemCount.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxItemCount), "Max item count cannot be negative.");
            }

            if (allowedKinds == null)
            {
                throw new ArgumentNullException(nameof(allowedKinds));
            }

            Code = code;
            MaxItemCount = maxItemCount;
            AllowedKinds = allowedKinds;
            MaxSize = maxSize;
            MaxWeight = maxWeight;
        }

        public string Code { get; }

        public int? MaxItemCount { get; }

        public IReadOnlyCollection<ECargoKind> AllowedKinds { get; }

        public CargoDimension? MaxSize { get; }

        public decimal? MaxWeight { get; }

        public bool IsKindAllowed(ECargoKind kind)
        {
            if (AllowedKinds.Count == 0)
            {
                return true;
            }

            return AllowedKinds.Contains(kind);
        }
    }
}
