using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.DTO;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Robots;
using Nexus.Core.Domain.Models.Robots.DTO;

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

                List<Location> combined = new List<Location>();
                if (markers != null)
                {
                    combined.AddRange(markers);
                }
                if (cassettes != null)
                {
                    combined.AddRange(cassettes);
                }

                _locations = combined
                    .GroupBy(l => l.Id)
                    .Select(g => g.First())
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "위치 데이터 로드 중 오류 발생: {Message}", ex.Message);
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
                Logger.LogError(ex, "로봇 데이터 로드 중 오류 발생: {Message}", ex.Message);
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
            dto.X = (int)location.Position.X;
            dto.Y = (int)location.Position.Y;
            dto.Z = (int)location.Position.Z;
            dto.Width = (int)location.Width;
            dto.Height = (int)location.Height;
            dto.Depth = (int)location.Depth;
            dto.CurrentItemId = location.CurrentItemId;

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
            if (string.IsNullOrEmpty(_selectedRobotId) || string.IsNullOrEmpty(_selectedLocationId))
            {
                return;
            }
            try
            {
                HttpClient client = new HttpClient();
                string baseUrl = GetGatewayBaseUrl().TrimEnd('/');
                var payload = new { LocationId = _selectedLocationId, Speed = _moveSpeed };
                HttpResponseMessage res = await client.PostAsJsonAsync($"{baseUrl}/api/v1/robots/{_selectedRobotId}/move", payload);
                Logger.LogInformation("Move result: {StatusCode}", (int)res.StatusCode);

                // path drawing removed in 3D-only mode
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
                var payload = new { FromLocationId = _selectedLocationId, ItemId = _loadItemId };
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
                var payload = new { ToLocationId = _selectedLocationId };
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


