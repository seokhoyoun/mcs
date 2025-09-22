using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Robots;
using Nexus.Core.Domain.Models.Robots.Interfaces;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Shared.Bases;
using Nexus.Orchestrator.Application.Robots.Simulation;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Orchestrator.Application.Robots
{
    public static class RobotCommandExtensions
    {
        private const string ROBOT_CARRY_KEY_PREFIX = "robot:";
        private const string ROBOT_CARRY_KEY_SUFFIX = ":carrying_item_id";

        public static async Task MoveToLocationAsync(this Robot robot,
                                                     string targetLocationId,
                                                     double speed,
                                                     ILocationRepository locationRepository,
                                                     RobotMotionService motionService,
                                                     CancellationToken cancellationToken = default)
        {
            if (robot == null)
            {
                throw new ArgumentNullException(nameof(robot));
            }
            if (string.IsNullOrEmpty(targetLocationId))
            {
                throw new ArgumentException("Target location id is required.", nameof(targetLocationId));
            }
            if (speed <= 0)
            {
                throw new ArgumentException("Speed must be greater than 0.", nameof(speed));
            }

            Location? target = await locationRepository.GetByIdAsync(targetLocationId, cancellationToken);
            if (target == null)
            {
                throw new InvalidOperationException($"Target location not found: {targetLocationId}");
            }

            motionService.ScheduleMove(robot.Id, target.Position.X, target.Position.Y, speed);
        }

        public static async Task LoadAsync(this Robot robot,
                                           string fromLocationId,
                                           string itemId,
                                           ILocationRepository locationRepository,
                                           ILocationService locationService,
                                           ITransportRepository transportRepository,
                                           IRobotRepository robotRepository,
                                           IConnectionMultiplexer redis,
                                           CancellationToken cancellationToken = default)
        {
            if (robot == null)
            {
                throw new ArgumentNullException(nameof(robot));
            }
            if (string.IsNullOrEmpty(fromLocationId) || string.IsNullOrEmpty(itemId))
            {
                throw new ArgumentException("FromLocationId and ItemId are required.");
            }

            Location? source = await locationRepository.GetByIdAsync(fromLocationId, cancellationToken);
            if (source == null)
            {
                throw new InvalidOperationException($"Source location not found: {fromLocationId}");
            }

            ITransportable? item = await transportRepository.GetByIdAsync(itemId, cancellationToken);
            if (item == null)
            {
                throw new InvalidOperationException($"Transport item not found: {itemId}");
            }

            Position? robotPos = await robotRepository.GetPositionAsync(robot.Id, cancellationToken);
            if (robotPos == null)
            {
                throw new InvalidOperationException($"Robot not found: {robot.Id}");
            }

            if (robotPos.X != source.Position.X || robotPos.Y != source.Position.Y || robotPos.Z != source.Position.Z)
            {
                throw new InvalidOperationException("Robot is not at the source location.");
            }

            if (!string.IsNullOrEmpty(source.CurrentItemId) && source.CurrentItemId != itemId)
            {
                throw new InvalidOperationException("A different item is present at the source location.");
            }

            bool cleared = await locationService.TryClearItemAsync(source.Id, cancellationToken);
            if (!cleared)
            {
                throw new InvalidOperationException("Failed to clear source location.");
            }

            IDatabase db = redis.GetDatabase();
            string carryKey = ROBOT_CARRY_KEY_PREFIX + robot.Id + ROBOT_CARRY_KEY_SUFFIX;
            await db.StringSetAsync(carryKey, itemId);
        }

        public static async Task UnloadAsync(this Robot robot,
                                             string toLocationId,
                                             ILocationRepository locationRepository,
                                             ILocationService locationService,
                                             IRobotRepository robotRepository,
                                             IConnectionMultiplexer redis,
                                             CancellationToken cancellationToken = default)
        {
            if (robot == null)
            {
                throw new ArgumentNullException(nameof(robot));
            }
            if (string.IsNullOrEmpty(toLocationId))
            {
                throw new ArgumentException("ToLocationId is required.", nameof(toLocationId));
            }

            IDatabase db = redis.GetDatabase();
            string carryKey = ROBOT_CARRY_KEY_PREFIX + robot.Id + ROBOT_CARRY_KEY_SUFFIX;
            string? itemId = await db.StringGetAsync(carryKey);
            if (string.IsNullOrEmpty(itemId))
            {
                throw new InvalidOperationException("Robot is not carrying any item.");
            }

            Location? dest = await locationRepository.GetByIdAsync(toLocationId, cancellationToken);
            if (dest == null)
            {
                throw new InvalidOperationException($"Destination location not found: {toLocationId}");
            }

            Position? robotPos = await robotRepository.GetPositionAsync(robot.Id, cancellationToken);
            if (robotPos == null)
            {
                throw new InvalidOperationException($"Robot not found: {robot.Id}");
            }

            if (robotPos.X != dest.Position.X || robotPos.Y != dest.Position.Y || robotPos.Z != dest.Position.Z)
            {
                throw new InvalidOperationException("Robot is not at the destination location.");
            }

            if (!string.IsNullOrEmpty(dest.CurrentItemId))
            {
                throw new InvalidOperationException("Destination location is occupied.");
            }

            bool assigned = await locationService.TryAssignItemAsync(dest.Id, itemId, cancellationToken);
            if (!assigned)
            {
                throw new InvalidOperationException("Failed to assign item to destination location.");
            }
            await db.KeyDeleteAsync(carryKey);
        }
    }
}
