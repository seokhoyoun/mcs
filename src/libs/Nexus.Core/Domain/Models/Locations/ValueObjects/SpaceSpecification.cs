using Nexus.Core.Domain.Models.Transports.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nexus.Core.Domain.Models.Locations.ValueObjects
{
    /// <summary>
    /// 공간(Space)의 규격/허용 규칙을 정의합니다.
    /// </summary>
    public sealed class SpaceSpecification
    {
        public SpaceSpecification(string code, IReadOnlyCollection<string> allowedCarrierSpecifications, IReadOnlyCollection<ECargoKind> allowedCargoKinds)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Specification code is required.", nameof(code));
            }

            if (allowedCarrierSpecifications == null)
            {
                throw new ArgumentNullException(nameof(allowedCarrierSpecifications));
            }

            if (allowedCargoKinds == null)
            {
                throw new ArgumentNullException(nameof(allowedCargoKinds));
            }

            Code = code;
            AllowedCarrierSpecifications = allowedCarrierSpecifications;
            AllowedCargoKinds = allowedCargoKinds;
        }

        public string Code { get; }

        /// <summary>
        /// 이 공간에 적재 가능한 Carrier 규격 코드 목록입니다.
        /// </summary>
        public IReadOnlyCollection<string> AllowedCarrierSpecifications { get; }

        /// <summary>
        /// 이 공간에 적재 가능한 Cargo 종류 목록입니다.
        /// </summary>
        public IReadOnlyCollection<ECargoKind> AllowedCargoKinds { get; }
    }
}
