using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Areas.Service;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Models.Transports.Service; // TransportService using 추가
using Nexus.Shared.Application.Interfaces;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Locations.Service
{
    public class LocationService
    {
        private readonly ILogger<AreaService> _logger;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILocationRepository _locationRepository;
        private readonly TransportService _transportService; // TransportService 주입

        private Dictionary<string, Location> _locations = new();

        public LocationService(
            ILogger<AreaService> logger,
            IEventPublisher eventPublisher,
            ILocationRepository locationRepository,
            TransportService transportService) // 생성자에 TransportService 추가
        {
            _logger = logger;
            _eventPublisher = eventPublisher;
            _locationRepository = locationRepository;
            _transportService = transportService;
        }

        public void AddLocations(IEnumerable<Location> locations)
        {
            foreach (var location in locations)
            {
                if (!_locations.ContainsKey(location.Id))
                {
                    _locations[location.Id] = location;
                }
            }
        }

        /// <summary>
        /// 저장소에서 LocationState를 조회하여 Location 객체의 상태를 동기화합니다.
        /// </summary>
        /// <param name="locationId">동기화할 Location의 ID</param>
        public async Task RefreshLocationStateAsync(string locationId)
        {
            var state = await _locationRepository.GetStateAsync(locationId);
            if (state == null) 
            {
                return;
            }

            var location = _locations[locationId];

            location.Status = (ELocationStatus)state.Status;

            // CurrentItemId가 있으면 TransportService에서 조회
            if (!string.IsNullOrEmpty(state.CurrentItemId))
            {
                var item = _transportService.GetItemById(state.CurrentItemId);
                location.CurrentItem = item;
            }
            else
            {
                location.CurrentItem = null;
            }
        }

    

      
    }
}