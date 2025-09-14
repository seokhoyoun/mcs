using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.DTO;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Robots;
using Nexus.Core.Domain.Models.Robots.DTO;

namespace Nexus.Portal.Components.Pages.Monitoring
{
    public partial class Realtime : IAsyncDisposable
    {
        private IReadOnlyList<Location> _locations = new List<Location>();
        private IReadOnlyList<Robot> _robots = new List<Robot>();
        private string _selectedRobotId = string.Empty;
        private string _selectedLocationId = string.Empty;
        private double _moveSpeed = 10;
        private string _loadItemId = string.Empty;
        private bool _threeInitialized = false;
        private HubConnection? _hubConnection;
        private Random _random = new Random();

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
            await JS.InvokeVoidAsync("pixiGame.init3D", "threeContainer", (object)locations);
            await JS.InvokeVoidAsync("pixiGame.loadRobots3D", (object)robots);

            _threeInitialized = true;

            try
            {
                await ConnectSignalRAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ConnectSignalRAsync failed: {ex}");
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
                        await JS.InvokeVoidAsync("pixiGame.updateRobot3D", update);
                    });
                }
            });

            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR StartAsync failed: {ex}");
            }
        }

        private async Task LoadLocationsAsync()
        {
            try
            {
                _locations = await LocationRepository.GetLocationsByTypeAsync(ELocationType.Marker);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"위치 정보 로드 중 오류 발생: {ex.Message}");
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
                Console.WriteLine($"로봇 정보 로드 중 오류 발생: {ex.Message}");
                _robots = new List<Robot>();
            }
        }

        // 테스트용 위치 추가
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

            await JS.InvokeVoidAsync("pixiGame.addLocation3D", testLocation);
        }

        private LocationDto MapLocationToDto(Location location)
        {    
            return new LocationDto
            {
                Id = location.Id,
                Name = location.Name,
                LocationType = location.LocationType.ToString(),
                Status = location.Status.ToString(),
                X = (int)location.Position.X,
                Y = (int)location.Position.Y,
                Z = (int)location.Position.Z,
                Width = (int)location.Width,
                Height = (int)location.Height
            };
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
                Console.WriteLine($"Move result: {(int)res.StatusCode}");

                // path drawing removed in 3D-only mode
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MoveSelectedRobot failed: {ex}");
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
                Console.WriteLine($"Load result: {(int)res.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadSelectedRobot failed: {ex}");
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
                Console.WriteLine($"Unload result: {(int)res.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UnloadSelectedRobot failed: {ex}");
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

