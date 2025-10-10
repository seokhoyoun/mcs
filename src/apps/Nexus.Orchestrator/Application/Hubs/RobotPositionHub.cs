using Microsoft.AspNetCore.SignalR;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Orchestrator.Application.Robots.Simulation;
using System.Threading.Tasks;

namespace Nexus.Orchestrator.Application.Hubs
{
    public class RobotPositionHub : Hub
    {
        private readonly ILocationRepository _locationRepository;
        private readonly RobotMotionService _motionService;

        public RobotPositionHub(ILocationRepository locationRepository,
                                RobotMotionService motionService)
        {
            _locationRepository = locationRepository;
            _motionService = motionService;
        }

        public async Task ScheduleMoveToLocation(string robotId, string targetLocationId, double speed)
        {
            if (string.IsNullOrEmpty(robotId))
            {
                throw new HubException("RobotId is required.");
            }
            if (string.IsNullOrEmpty(targetLocationId))
            {
                throw new HubException("TargetLocationId is required.");
            }
            if (speed <= 0)
            {
                throw new HubException("Speed must be greater than 0.");
            }

            Location? location = await _locationRepository.GetByIdAsync(targetLocationId);
            if (location == null)
            {
                throw new HubException("Target location not found.");
            }

            _motionService.ScheduleMove(robotId, location.Position.X, location.Position.Y, speed);
        }

        public async Task ScheduleMoveToPosition(string robotId, double targetX, double targetY, double speed)
        {
            if (string.IsNullOrEmpty(robotId))
            {
                throw new HubException("RobotId is required.");
            }
            if (speed <= 0)
            {
                throw new HubException("Speed must be greater than 0.");
            }

            _motionService.ScheduleMove(robotId, targetX, targetY, speed);
            await Task.CompletedTask;
        }
    }
}

