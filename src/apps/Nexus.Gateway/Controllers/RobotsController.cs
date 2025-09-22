using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Robots;
using Nexus.Core.Domain.Models.Robots.Interfaces;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Shared.Bases;
using System.Linq;

namespace Nexus.Gateway.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class RobotsController : ControllerBase
    {
        private readonly ILocationRepository _locationRepository;
        private readonly ILocationService _locationService;
        private readonly IRobotRepository _robotRepository;
        private readonly ITransportRepository _transportRepository;
        private readonly IConfiguration _configuration;

        public RobotsController(ILocationRepository locationRepository,
                                IRobotRepository robotRepository,
                                ITransportRepository transportRepository,
                                IConfiguration configuration,
                                ILocationService locationService)
        {
            _locationRepository = locationRepository;
            _robotRepository = robotRepository;
            _transportRepository = transportRepository;
            _configuration = configuration;
            _locationService = locationService;
        }

        public class MoveRequest
        {
            public string LocationId { get; set; } = string.Empty;
            public double Speed { get; set; }
        }

        [HttpPost("{id}/move")]
        public async Task<ActionResult> MoveToLocation(string id, [FromBody] MoveRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }
            if (string.IsNullOrEmpty(request.LocationId))
            {
                return BadRequest("LocationId is required.");
            }
            if (request.Speed <= 0)
            {
                return BadRequest("Speed must be greater than 0.");
            }

            string? configuredUrl = _configuration["SignalR:RobotHubUrl"];
            string hubUrl = !string.IsNullOrEmpty(configuredUrl)
                ? configuredUrl
                : "http://nexus.orchestrator:8081/hubs/robotPosition";

            HubConnection connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            await connection.StartAsync(cancellationToken);
            await connection.InvokeCoreAsync(
                "ScheduleMoveToLocation",
                args: new object?[] { id, request.LocationId, request.Speed },
                cancellationToken: cancellationToken);
            await connection.DisposeAsync();

            return Accepted();
        }

        public class LoadRequest
        {
            public string FromLocationId { get; set; } = string.Empty;
            public string ItemId { get; set; } = string.Empty;
        }

        [HttpPost("{id}/load")]
        public async Task<ActionResult> Load(string id, [FromBody] LoadRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }
            if (string.IsNullOrEmpty(request.FromLocationId) || string.IsNullOrEmpty(request.ItemId))
            {
                return BadRequest("FromLocationId and ItemId are required.");
            }

            Location? source = await _locationRepository.GetByIdAsync(request.FromLocationId, cancellationToken);
            if (source == null)
            {
                return NotFound($"Source location not found: {request.FromLocationId}");
            }

            ITransportable? item = await _transportRepository.GetByIdAsync(request.ItemId, cancellationToken);
            if (item == null)
            {
                return NotFound($"Transport item not found: {request.ItemId}");
            }

            Position? robotPos = await _robotRepository.GetPositionAsync(id, cancellationToken);
            if (robotPos == null)
            {
                return NotFound($"Robot not found: {id}");
            }

            if (robotPos.X != source.Position.X || robotPos.Y != source.Position.Y || robotPos.Z != source.Position.Z)
            {
                return Conflict("Robot is not at the source location.");
            }

            if (!string.IsNullOrEmpty(source.CurrentItemId) && source.CurrentItemId != request.ItemId)
            {
                return Conflict("A different item is present at the source location.");
            }

            // Use robot's own storage-capable location (IItemStorage) for carrying state
            Robot? robot = await _robotRepository.GetByIdAsync(id, cancellationToken);
            if (robot == null)
            {
                return NotFound($"Robot not found: {id}");
            }

            Location? carryLocation = robot.Locations.FirstOrDefault(l => l is Nexus.Core.Domain.Models.Locations.Interfaces.IItemStorage);
            if (carryLocation == null)
            {
                return Conflict("Robot has no storage-capable location.");
            }

            if (!string.IsNullOrEmpty(carryLocation.CurrentItemId))
            {
                return Conflict("Robot storage location is already occupied.");
            }

            bool cleared = await _locationService.TryClearItemAsync(source.Id, cancellationToken);
            if (!cleared)
            {
                return Conflict("Failed to clear source location.");
            }

            bool assigned = await _locationService.TryAssignItemAsync(carryLocation.Id, request.ItemId, cancellationToken);
            if (!assigned)
            {
                return Conflict("Failed to assign item to robot storage location.");
            }

            return Ok();
        }

        public class UnloadRequest
        {
            public string ToLocationId { get; set; } = string.Empty;
        }

        [HttpPost("{id}/unload")]
        public async Task<ActionResult> Unload(string id, [FromBody] UnloadRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }
            if (string.IsNullOrEmpty(request.ToLocationId))
            {
                return BadRequest("ToLocationId is required.");
            }

            // Fetch carried item from robot's storage-capable location
            Robot? robot = await _robotRepository.GetByIdAsync(id, cancellationToken);
            if (robot == null)
            {
                return NotFound($"Robot not found: {id}");
            }

            Location? carryLocation = robot.Locations.FirstOrDefault(l => l is Nexus.Core.Domain.Models.Locations.Interfaces.IItemStorage);
            if (carryLocation == null)
            {
                return Conflict("Robot has no storage-capable location.");
            }

            string itemId = carryLocation.CurrentItemId;
            if (string.IsNullOrEmpty(itemId))
            {
                return Conflict("Robot is not carrying any item.");
            }

            Location? dest = await _locationRepository.GetByIdAsync(request.ToLocationId, cancellationToken);
            if (dest == null)
            {
                return NotFound($"Destination location not found: {request.ToLocationId}");
            }

            Position? robotPos = await _robotRepository.GetPositionAsync(id, cancellationToken);
            if (robotPos == null)
            {
                return NotFound($"Robot not found: {id}");
            }

            if (robotPos.X != dest.Position.X || robotPos.Y != dest.Position.Y || robotPos.Z != dest.Position.Z)
            {
                return Conflict("Robot is not at the destination location.");
            }

            if (!string.IsNullOrEmpty(dest.CurrentItemId))
            {
                return Conflict("Destination location is occupied.");
            }

            bool assigned = await _locationService.TryAssignItemAsync(dest.Id, itemId, cancellationToken);
            if (!assigned)
            {
                return Conflict("Failed to assign item to destination location.");
            }

            bool cleared = await _locationService.TryClearItemAsync(carryLocation.Id, cancellationToken);
            if (!cleared)
            {
                return Conflict("Failed to clear robot storage location.");
            }
            return Ok();
        }
    }
}
