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
                Console.WriteLine($"?꾩튂 ?뺣낫 濡쒕뱶 以??ㅻ쪟 諛쒖깮: {ex.Message}");
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
                Console.WriteLine($"濡쒕큸 ?뺣낫 濡쒕뱶 以??ㅻ쪟 諛쒖깮: {ex.Message}");
                _robots = new List<Robot>();
            }
        }

        // ?뚯뒪?몄슜 ?꾩튂 異붽?
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


