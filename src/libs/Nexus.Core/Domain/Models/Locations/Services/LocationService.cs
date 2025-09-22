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
        private List<MarkerLocation> _markerLocations = new();

        private bool _initialized = false;
        private readonly object _initLock = new object();
        private Task? _initTask;

        public LocationService(
            ILogger<LocationService> logger,
            ILocationRepository locationRepository,
            ITransportRepository transportRepository) : base(logger, locationRepository)
        {
            _locationRepository = locationRepository;
            _transportRepository = transportRepository;
        }

        private async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
        {
            if (_initialized)
            {
                return;
            }
            Task? startTask = null;
            lock (_initLock)
            {
                if (_initialized)
                {
                    return;
                }
                if (_initTask == null)
                {
                    _initTask = InitializeCoreAsync(cancellationToken);
                }
                startTask = _initTask;
            }
            await startTask;
        }

        private async Task InitializeCoreAsync(CancellationToken cancellationToken)
        {
            try
            {
                await ReloadAsync(cancellationToken);
                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LocationService 초기화 중 오류 발생");
                throw;
            }
        }

        private async Task ReloadAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<Location> locations = await _locationRepository.GetAllAsync(cancellationToken);

            _locations.Clear();
            _cassetteLocations.Clear();
            _trayLocations.Clear();
            _memoryLocations.Clear();
            _markerLocations.Clear();

            if (locations == null || locations.Count == 0)
            {
                _logger.LogWarning("초기화된 Location 데이터가 없습니다.");
                return;
            }

            foreach (Location location in locations)
            {
                _locations[location.Id] = location;

                if (location.LocationType == ELocationType.Cassette)
                {
                    _cassetteLocations.Add((CassetteLocation)location);
                }
                else if (location.LocationType == ELocationType.Tray)
                {
                    _trayLocations.Add((TrayLocation)location);
                }
                else if (location.LocationType == ELocationType.Memory)
                {
                    _memoryLocations.Add((MemoryLocation)location);
                }
                else if (location.LocationType == ELocationType.Marker)
                {
                    _markerLocations.Add((MarkerLocation)location);
                }
                else
                {
                    Debug.Assert(false, "Unknown location type");
                    _logger.LogError("Unknown location type: {Type}", location.LocationType);
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
                    case ELocationType.Marker:
                        _markerLocations.Add((MarkerLocation)location);
                        break;
                    default:
                        Debug.Assert(false, $"Unknown location type: {location.LocationType}");
                        _logger.LogError($"Unknown location type: {location.LocationType}");
                        break;
                }
            }
        }

 
        public async Task<CassetteLocation?> GetCassetteLocationByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken);
            if (_locations.TryGetValue(id, out Location? location))
            {
                return location as CassetteLocation;
            }
            return null;
        }

    
        public async Task<TrayLocation?> GetTrayLocationByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken);
            if (_locations.TryGetValue(id, out Location? location))
            {
                return location as TrayLocation;
            }
            return null;
        }

      
        public async Task<MemoryLocation?> GetMemoryLocationByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken);
            if (_locations.TryGetValue(id, out Location? location))
            {
                return location as MemoryLocation;
            }
            return null;
        }

     
        public async Task<MarkerLocation?> GetMarkerLocationByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken);
            if (_locations.TryGetValue(id, out Location? location))
            {
                return location as MarkerLocation;
            }
            return null;
        }

        public async Task<CassetteLocation?> FindCassetteLocationByItemIdAsync(string itemId, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken);
            foreach (CassetteLocation loc in _cassetteLocations)
            {
                string current = loc.CurrentItemId ?? string.Empty;
                if (!string.IsNullOrEmpty(current))
                {
                    if (string.Equals(current, itemId, StringComparison.OrdinalIgnoreCase))
                    {
                        return loc;
                    }
                }
            }
            // 캐시에 없으면 리로드 후 한 번 더 검색 (시드 타이밍 차이 대비)
            await ReloadAsync(cancellationToken);
            foreach (CassetteLocation loc in _cassetteLocations)
            {
                string current = loc.CurrentItemId ?? string.Empty;
                if (!string.IsNullOrEmpty(current))
                {
                    if (string.Equals(current, itemId, StringComparison.OrdinalIgnoreCase))
                    {
                        return loc;
                    }
                }
            }
            return null;
        }

        public async Task<bool> TryAssignItemAsync(string locationId, string itemId, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken);
            Location? location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
            if (location == null)
            {
                return false;
            }

            IItemStorage? storage = location as IItemStorage;
            if (storage == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(storage.CurrentItemId))
            {
                return false;
            }

            storage.CurrentItemId = itemId;
            await _locationRepository.UpdateAsync(location, cancellationToken);

            // 로컬 캐시 동기화
            if (_locations.TryGetValue(locationId, out Location? cached))
            {
                IItemStorage? cachedStorage = cached as IItemStorage;
                if (cachedStorage != null)
                {
                    cachedStorage.CurrentItemId = itemId;
                }
            }
            return true;
        }

        public async Task<bool> TryClearItemAsync(string locationId, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken);
            Location? location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
            if (location == null)
            {
                return false;
            }

            IItemStorage? storage = location as IItemStorage;
            if (storage == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(storage.CurrentItemId))
            {
                return false;
            }

            storage.CurrentItemId = string.Empty;
            await _locationRepository.UpdateAsync(location, cancellationToken);

            // 로컬 캐시 동기화
            if (_locations.TryGetValue(locationId, out Location? cached))
            {
                IItemStorage? cachedStorage = cached as IItemStorage;
                if (cachedStorage != null)
                {
                    cachedStorage.CurrentItemId = string.Empty;
                }
            }
            return true;
        }
    }
}
