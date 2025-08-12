using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Locations
{
    public class LocationService
    {
        public IReadOnlyDictionary<string, Location<ITransportable>> Locations => _locations;

        private readonly Dictionary<string, Location<ITransportable>> _locations = new();
        private readonly ILocationRepository _locationRepository;

        public LocationService(ILocationRepository locationInfoRepository)
        {
            _locationRepository = locationInfoRepository;
        }

        /// <summary>
        /// Redis에서 Location 정보를 불러와 내부 컬렉션을 초기화합니다.
        /// </summary>
        public async Task InitializeAsync()
        {
            var locations = await _locationRepository.GetAllAsync();

            foreach (var location in locations)
            {
                _locations[location.Id] = location;
            }
        }

        public Location<ITransportable>? GetLocation(string id)
        {
            _locations.TryGetValue(id, out var location);
            return location;
        }
    }
}
