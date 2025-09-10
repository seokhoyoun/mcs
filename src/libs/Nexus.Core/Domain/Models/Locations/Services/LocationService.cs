using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Areas.Services;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Models.Transports.Services; // TransportService using 추가
using Nexus.Core.Domain.Shared.Bases;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Locations.Services
{
    public class LocationService : BaseDataService<Location, string>, ILocationService
    {
        private readonly ILocationRepository _locationRepository;
        private readonly ITransportRepository _transportRepository; 

        private Dictionary<string, Location> _locations = new();

        private List<CassetteLocation> _cassetteLocations = new();
        private List<TrayLocation> _trayLocations = new();
        private List<MemoryLocation> _memoryLocations = new();
        private List<RobotLocation> _robotLocations = new();

        public LocationService(
            ILogger<LocationService> logger,
            ILocationRepository locationRepository,
            ITransportRepository transportRepository) : base(logger, locationRepository)
        {
            _locationRepository = locationRepository;
            _transportRepository = transportRepository;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Location> locations = await _locationRepository.GetAllAsync();

            if (locations == null || locations.Count == 0)
            {
                _logger.LogWarning("초기화된 Location 데이터가 없습니다.");

                return;
            }

            foreach (Location location in locations)
            {
                _locations.Add(location.Id, location);

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
                    case ELocationType.Robot:  
                        _robotLocations.Add((RobotLocation)location);
                        break;
                    default:
                        Debug.Assert(false, $"Unknown location type: {location.LocationType}");
                        _logger.LogError($"Unknown location type: {location.LocationType}");
                        break;
                }
            }
        }

        public void AddLocations(IEnumerable<Location> locations)
        {
            foreach (Location location in locations)
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
                    case ELocationType.Robot:  // 새로 추가
                        _robotLocations.Add((RobotLocation)location);
                        break;
                    default:
                        Debug.Assert(false, $"Unknown location type: {location.LocationType}");
                        _logger.LogError($"Unknown location type: {location.LocationType}");
                        break;
                }
            }
        }

 
        public CassetteLocation? GetCassetteLocationById(string id)
        {
            if (_locations.TryGetValue(id, out Location? location))
            {
                return location as CassetteLocation;
            }
            return null;
        }

    
        public TrayLocation? GetTrayLocationById(string id)
        {
            if (_locations.TryGetValue(id, out Location? location))
            {
                return location as TrayLocation;
            }
            return null;
        }

      
        public MemoryLocation? GetMemoryLocationById(string id)
        {
            if (_locations.TryGetValue(id, out Location? location))
            {
                return location as MemoryLocation;
            }
            return null;
        }

     
        public RobotLocation? GetRobotLocationById(string id)
        {
            if (_locations.TryGetValue(id, out Location? location))
            {
                return location as RobotLocation;
            }
            return null;
        }

    }
}
