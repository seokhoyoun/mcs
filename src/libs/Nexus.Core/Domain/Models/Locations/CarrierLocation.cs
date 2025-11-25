using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Transports.Enums;
using System;

namespace Nexus.Core.Domain.Models.Locations
{
    /// <summary>
    /// Carrier를 적재할 수 있는 단일 슬롯을 나타냅니다.
    /// </summary>
    public class CarrierLocation : Location, IItemStorage
    {
        public CarrierLocation(string id, string name, string specificationCode, ECargoKind allowedKind) : base(id, name)
        {
            if (string.IsNullOrWhiteSpace(specificationCode))
            {
                throw new ArgumentException("Specification code is required.", nameof(specificationCode));
            }

            SpecificationCode = specificationCode;
            AllowedKind = allowedKind;
        }

        /// <summary>
        /// 적재가 허용된 Carrier 규격 코드입니다.
        /// </summary>
        public string SpecificationCode { get; }

        /// <summary>
        /// 적재가 허용된 Cargo 종류입니다.
        /// </summary>
        public ECargoKind AllowedKind { get; }
    }
}
