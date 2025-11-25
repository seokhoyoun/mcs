using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Nexus.Orchestrator.Application.Robots.Simulation
{
    /// <summary>
    /// Stores and manages active robot motions.
    /// </summary>
    public class RobotMotionService
    {
        private readonly ConcurrentDictionary<string, RobotMotionState> _motions = new ConcurrentDictionary<string, RobotMotionState>();

        public void ScheduleMove(string robotId, double targetX, double targetY, double speed)
        {
            RobotMotionState state = new RobotMotionState(robotId, targetX, targetY, speed);
            _motions.AddOrUpdate(robotId, state, (key, old) => state);
        }

        public void Complete(string robotId)
        {
            _motions.TryRemove(robotId, out RobotMotionState? _);
        }

        public IReadOnlyList<RobotMotionState> Snapshot()
        {
            List<RobotMotionState> list = new List<RobotMotionState>(_motions.Values);
            return list.AsReadOnly();
        }
    }
}

