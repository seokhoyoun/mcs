using System.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.DTO;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Robots;
using Nexus.Core.Domain.Models.Robots.DTO;
using Nexus.Core.Domain.Standards;

namespace Nexus.Portal.Components.Pages.Monitoring
{
    public partial class Realtime : IAsyncDisposable
    {
        [Inject]
        private ILogger<Realtime> Logger { get; set; } = default!;

        private IReadOnlyList<Location> _locations = new List<Location>();
        private IReadOnlyList<Robot> _robots = new List<Robot>();
        private string _selectedRobotId = string.Empty;
        private string _selectedLocationId = string.Empty;
        private double _moveSpeed = 10;
        private bool _usePositionMode = false;
        public bool UsePositionMode
        {
            get { return _usePositionMode; }
            set
            {
                _usePositionMode = value;
                InvokeAsync(StateHasChanged);
            }
        }
        private double _targetX = 0;
        private double _targetY = 0;
        private string _loadItemId = string.Empty;
        private bool _threeInitialized = false;
        private HubConnection? _hubConnection;
        private Random _random = new Random();
        private bool _showTestPanel = true;

        
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                return;
            }

            await LoadLocationsAsync();
            await LoadRobotsAsync();

            if (_robots.Count > 0)
            {
                _selectedRobotId = _robots[0].Id;
            }
            if (_locations.Count > 0)
            {
                _selectedLocationId = _locations[0].Id;
            }

            await InvokeAsync(StateHasChanged);

            LocationDto[] locations = _locations.Select(MapLocationToDto).ToArray();
            RobotDto[] robots = _robots.Select(MapRobotToDto).ToArray();

            // Push dimension standards (transports) to 3D layer before init
            try
            {
                Dictionary<string, object> dims = await BuildDimensionPayloadAsync();
                await JS.InvokeVoidAsync("nexus3d.setDimensions", dims);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to push dimensions; using defaults in 3D layer");
            }

            await JS.InvokeVoidAsync("nexus3d.init3D", "threeContainer", (object)locations);
            await JS.InvokeVoidAsync("nexus3d.loadRobots3D", (object)robots);

            _threeInitialized = true;

            try
            {
                await ConnectSignalRAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ConnectSignalRAsync failed");
            }
        }

        private async Task<Dictionary<string, object>> BuildDimensionPayloadAsync()
        {
            Dictionary<string, object> transports = new Dictionary<string, object>();

            try
            {
                DimensionStandard? cassette = await DimensionRepository.GetByIdAsync("transport:cassette");
                DimensionStandard? tray = await DimensionRepository.GetByIdAsync("transport:tray");
                DimensionStandard? memory = await DimensionRepository.GetByIdAsync("transport:memory");

                if (cassette != null)
                {
                    transports["cassette"] = new { width = (int)cassette.Width, height = (int)cassette.Height, depth = (int)cassette.Depth };
                }
                if (tray != null)
                {
                    transports["tray"] = new { width = (int)tray.Width, height = (int)tray.Height, depth = (int)tray.Depth };
                }
                if (memory != null)
                {
                    transports["memory"] = new { width = (int)memory.Width, height = (int)memory.Height, depth = (int)memory.Depth };
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "DimensionRepository unavailable; building fallback dimensions");
            }

            // Fill any missing defaults
            if (!transports.ContainsKey("cassette"))
            {
                transports["cassette"] = new { width = 28, height = 58, depth = 58 };
            }
            if (!transports.ContainsKey("tray"))
            {
                transports["tray"] = new { width = 28, height = 3, depth = 28 };
            }
            if (!transports.ContainsKey("memory"))
            {
                transports["memory"] = new { width = 4, height = 4, depth = 4 };
            }

            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload["transports"] = transports;
            return payload;
        }

        

        private async Task ConnectSignalRAsync()
        {
            string? configuredUrl = Configuration["SignalR:RobotHubUrl"];
            string hubUrl;
            if (!string.IsNullOrEmpty(configuredUrl))
            {
                hubUrl = configuredUrl;
            }
            else
            {
                hubUrl = Navigation.ToAbsoluteUri("/hubs/robotPosition").ToString();
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<IEnumerable<RobotDto>>("ReceiveRobotPosition", async (updates) =>
            {
                if (!_threeInitialized)
                {
                    return;
                }

                foreach (RobotDto update in updates)
                {
                    await InvokeAsync(async () =>
                    {
                        await JS.InvokeVoidAsync("nexus3d.updateRobot3D", update);
                    });
                }
            });

            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SignalR StartAsync failed");
            }
        }

        private async Task LoadLocationsAsync()
        {
            try
            {
                IReadOnlyList<Location> markers = await LocationRepository.GetLocationsByTypeAsync(ELocationType.Marker);
                IReadOnlyList<Location> cassettes = await LocationRepository.GetLocationsByTypeAsync(ELocationType.Cassette);
                IReadOnlyList<Location> trays = await LocationRepository.GetLocationsByTypeAsync(ELocationType.Tray);

                List<Location> combined = new List<Location>();
                if (markers != null)
                {
                    combined.AddRange(markers);
                }
                if (cassettes != null)
                {
                    combined.AddRange(cassettes);
                }
                if (trays != null)
                {
                    combined.AddRange(trays);
                }

                _locations = combined
                    .GroupBy(l => l.Id)
                    .Select(g => g.First())
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Location data load error: {Message}", ex.Message);
                _locations = new List<Location>();
            }
        }

        private async Task LoadRobotsAsync()
        {
            try
            {
                _robots = await RobotRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Location data load error: {Message}", ex.Message);
                _robots = new List<Robot>();
            }
        }

        
        private async Task AddTestLocation()
        {
            if (!_threeInitialized)
            {
                return;
            }

            LocationDto testLocation = new LocationDto
            {
                Id = $"TEST_{DateTime.Now.Ticks}",
                Name = $"Test_{_random.Next(100, 999)}",
                LocationType = "Memory",
                Status = "Available",
                X = _random.Next(50, 800),
                Y = _random.Next(50, 600),
                Z = 0
            };

            await JS.InvokeVoidAsync("nexus3d.addLocation3D", testLocation);
        }

        private LocationDto MapLocationToDto(Location location)
        {
            LocationDto dto = new LocationDto();
            dto.Id = location.Id;
            dto.Name = location.Name;
            dto.LocationType = location.LocationType.ToString();
            dto.Status = location.Status.ToString();
            dto.ParentId = location.ParentId;
            dto.IsVisible = location.IsVisible;
            dto.IsRelativePosition = location.IsRelativePosition;
            dto.RotateX = location.Rotation != null ? location.Rotation.X : 0;
            dto.RotateY = location.Rotation != null ? location.Rotation.Y : 0;
            dto.RotateZ = location.Rotation != null ? location.Rotation.Z : 0;
            dto.X = (int)location.Position.X;
            dto.Y = (int)location.Position.Y;
            dto.Z = (int)location.Position.Z;
            dto.Width = (int)location.Width;
            dto.Height = (int)location.Height;
            dto.Depth = (int)location.Depth;
            dto.CurrentItemId = location.CurrentItemId;
            dto.Children = location.Children.ToList();

            Nexus.Core.Domain.Models.Locations.MarkerLocation? marker = location as Nexus.Core.Domain.Models.Locations.MarkerLocation;
            if (marker != null)
            {
                dto.MarkerRole = marker.MarkerRole.ToString();
            }

            return dto;
        }

        private RobotDto MapRobotToDto(Robot robot)
        {
            return new RobotDto
            {
                Id = robot.Id,
                Name = robot.Name,
                RobotType = robot.RobotType.ToString(),
                X = (int)robot.Position.X,
                Y = (int)robot.Position.Y,
                Z = (int)robot.Position.Z
            };
        }

        private string GetGatewayBaseUrl()
        {
            string? configured = Configuration["Gateway:BaseUrl"];
            if (!string.IsNullOrEmpty(configured))
            {
                return configured;
            }
            return "http://nexus.gateway:8082";
        }

        private void OnShowTestPanel()
        {
            _showTestPanel = !_showTestPanel;
        }

        private string GetTestPanelTooltip()
        {
            return _showTestPanel ? "Hide test panel" : "Show test panel";
        }

        private async Task MoveSelectedRobot()
        {
            if (string.IsNullOrEmpty(_selectedRobotId))
            {
                return;
            }
            try
            {
                HttpClient client = new HttpClient();
                string baseUrl = GetGatewayBaseUrl().TrimEnd('/');
   				Nexus.Portal.Contracts.Robots.MoveRobotRequest payload = new Nexus.Portal.Contracts.Robots.MoveRobotRequest
                {
                    LocationId = _selectedLocationId,
                    Speed = _moveSpeed
                };
                HttpResponseMessage res = await client.PostAsJsonAsync($"{baseUrl}/api/v1/robots/{_selectedRobotId}/move", payload);
                Logger.LogInformation("Move result: {StatusCode}", (int)res.StatusCode);
                if (_usePositionMode)
                {
                    var payloadPos = new { X = _targetX, Y = _targetY, Speed = _moveSpeed };
                    HttpResponseMessage resPos = await client.PostAsJsonAsync($"{baseUrl}/api/v1/robots/{_selectedRobotId}/move-to-position", payloadPos);
                    Logger.LogInformation("Move-to-position result: {StatusCode}", (int)resPos.StatusCode);
                }
                else
                {
                    if (string.IsNullOrEmpty(_selectedLocationId))
                    {
                        return;
                    }
                    var payloadLoc = new { LocationId = _selectedLocationId, Speed = _moveSpeed };
                    HttpResponseMessage resLoc = await client.PostAsJsonAsync($"{baseUrl}/api/v1/robots/{_selectedRobotId}/move", payloadLoc);
                    Logger.LogInformation("Move-to-location result: {StatusCode}", (int)resLoc.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "MoveSelectedRobot failed");
            }
        }

        private async Task LoadSelectedRobot()
        {
            if (string.IsNullOrEmpty(_selectedRobotId) || string.IsNullOrEmpty(_selectedLocationId) || string.IsNullOrEmpty(_loadItemId))
            {
                return;
            }

            try
            {
                HttpClient client = new HttpClient();
                string baseUrl = GetGatewayBaseUrl().TrimEnd('/');
                Nexus.Portal.Contracts.Robots.LoadRobotRequest payload = new Nexus.Portal.Contracts.Robots.LoadRobotRequest
                {
                    FromLocationId = _selectedLocationId,
                    ItemId = _loadItemId
                };
                HttpResponseMessage res = await client.PostAsJsonAsync($"{baseUrl}/api/v1/robots/{_selectedRobotId}/load", payload);
                Logger.LogInformation("Load result: {StatusCode}", (int)res.StatusCode);
                try
                {
                    await RefreshSelectedLocationAsync();
                }
                catch (Exception refreshEx)
                {
                    Logger.LogWarning(refreshEx, "Failed to refresh location after load");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "LoadSelectedRobot failed");
            }
        }

        private async Task UnloadSelectedRobot()
        {
            if (string.IsNullOrEmpty(_selectedRobotId) || string.IsNullOrEmpty(_selectedLocationId))
            {
                return;
            }
            try
            {
                HttpClient client = new HttpClient();
                string baseUrl = GetGatewayBaseUrl().TrimEnd('/');
                Nexus.Portal.Contracts.Robots.UnloadRobotRequest payload = new Nexus.Portal.Contracts.Robots.UnloadRobotRequest
                {
                    ToLocationId = _selectedLocationId
                };
                HttpResponseMessage res = await client.PostAsJsonAsync($"{baseUrl}/api/v1/robots/{_selectedRobotId}/unload", payload);
                Logger.LogInformation("Unload result: {StatusCode}", (int)res.StatusCode);
                try
                {
                    await RefreshSelectedLocationAsync();
                }
                catch (Exception refreshEx)
                {
                    Logger.LogWarning(refreshEx, "Failed to refresh location after unload");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "UnloadSelectedRobot failed");
            }
        }

        private async Task RefreshSelectedLocationAsync()
        {
            try
            {
                Location? updated = await LocationRepository.GetByIdAsync(_selectedLocationId);
                if (updated != null)
                {
                    LocationDto dto = MapLocationToDto(updated);
                    await JS.InvokeVoidAsync("nexus3d.updateLocation3D", dto);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "RefreshSelectedLocationAsync failed");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}



