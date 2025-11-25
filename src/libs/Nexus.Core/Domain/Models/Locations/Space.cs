using System.Collections.Generic;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Locations.ValueObjects;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Locations
{
    /// <summary>
    /// 도메인에 독립적인 공간 표현. 규격(Specification), 슬롯(CarrierLocation), 중첩 공간을 보유할 수 있습니다.
    /// </summary>
    public sealed class Space : IEntity, ISpace
    {
        private readonly List<CarrierLocation> _carrierLocations = new List<CarrierLocation>();
        private readonly List<ISpace> _spaces = new List<ISpace>();

        public Space(string id, string name, SpaceSpecification specification, IReadOnlyList<CarrierLocation> carrierLocations, IReadOnlyList<ISpace> spaces)
        {
            Id = id;
            Name = name;
            Specification = specification;
            _carrierLocations.AddRange(carrierLocations);
            _spaces.AddRange(spaces);
        }

        public string Id { get; }

        public string Name { get; }

        public SpaceSpecification Specification { get; }

        public IReadOnlyList<CarrierLocation> CarrierLocations => _carrierLocations.AsReadOnly();

        public IReadOnlyList<ISpace> Spaces => _spaces.AsReadOnly();
    }
}
