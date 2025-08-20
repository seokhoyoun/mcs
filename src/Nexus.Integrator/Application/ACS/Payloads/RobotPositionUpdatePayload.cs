using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    public class RobotPositionUpdatePayload
    {
        [JsonPropertyName("robots")]
        public List<RobotPositionInfo> Robots { get; set; }
    }

    public class RobotPositionInfo
    {
        [JsonPropertyName("robotId")]
        public string RobotId { get; set; } = string.Empty;

        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }

        [JsonPropertyName("angle")]
        public double Angle { get; set; }

        [JsonPropertyName("battery")]
        public double Battery { get; set; }
    }
}
