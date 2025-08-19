using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Areas.Services;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Models.Transports.Services; // TransportService using 추가
using Nexus.Shared.Application.Interfaces;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Locations.Services
{
    public class LocationService
    {
        private readonly ILogger<AreaService> _logger;
        private readonly ILocationRepository _locationRepository;
        private readonly TransportService _transportService; // TransportService 주입

        private Dictionary<string, Location> _locations = new();

        private List<CassetteLocation> _cassetteLocations = new();
        private List<TrayLocation> _trayLocations = new();
        private List<MemoryLocation> _memoryLocations = new();

        public LocationService(
            ILogger<AreaService> logger,
            ILocationRepository locationRepository,
            TransportService transportService) // 생성자에 TransportService 추가
        {
            _logger = logger;
            _locationRepository = locationRepository;
            _transportService = transportService;
        }

        public void AddLocations(IEnumerable<Location> locations)
        {
            foreach (var location in locations)
            {
                if (_locations.ContainsKey(location.Id))
                {
                    continue;
                }

                _locations[location.Id] = location;

                switch (location.LocationType)
                {
                    case ELocationType.Cassette:
                        _cassetteLocations.Add((CassetteLocation)location);
                        break;
                    case ELocationType.Tray:
                        _trayLocations.Add((TrayLocation)location);
                        break;
                    case ELocationType.Memory:
                        _memoryLocations.Add((MemoryLocation)location);
                        break;
                    default:
                        Debug.Assert(false, $"Unknown location type: {location.LocationType}");
                        _logger.LogError($"Unknown location type: {location.LocationType}");
                        break;
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