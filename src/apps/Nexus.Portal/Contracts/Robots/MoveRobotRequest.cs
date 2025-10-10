namespace Nexus.Portal.Contracts.Robots
{
    public class MoveRobotRequest
    {
        public string LocationId { get; set; } = string.Empty;
        public double Speed { get; set; }
    }
}

