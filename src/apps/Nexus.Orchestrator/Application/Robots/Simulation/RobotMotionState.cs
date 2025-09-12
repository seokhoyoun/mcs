using System;

namespace Nexus.Orchestrator.Application.Robots.Simulation
{
    /// <summary>
    /// Holds target and speed for an active robot motion.
    /// </summary>
    public class RobotMotionState
    {
        public string RobotId { get; }
        public double TargetX { get; }
        public double TargetY { get; }
        public double Speed { get; } // units per second

        public RobotMotionState(string robotId, double targetX, double targetY, double speed)
        {
            RobotId = robotId;
            TargetX = targetX;
            TargetY = targetY;
            Speed = speed;
        }
    }
}

