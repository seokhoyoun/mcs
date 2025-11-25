using System.Collections.Generic;
using Nexus.Core.Domain.Models.Locations.ValueObjects;

namespace Nexus.Core.Domain.Models.Locations.Interfaces
{
    /// <summary>
    /// CarrierLocation을 포함하고 중첩 공간을 가질 수 있는 공간 단위를 나타냅니다.
    /// </summary>
    public interface ISpace
    {
        SpaceSpecification Specification { get; }

        IReadOnlyList<CarrierLocation> CarrierLocations { get; }

        IReadOnlyList<ISpace> Spaces { get; }
    }
}
