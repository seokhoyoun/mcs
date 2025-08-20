using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    public class RobotStatusUpdatePayload
    {
        [JsonPropertyName("robotId")]
        public string? RobotId { get; set; }
        [JsonPropertyName("robotType")]
        public string? RobotType { get; set; }

        [JsonPropertyName("robotStatus")]
        public string? RobotStatus { get; set; }

        [JsonPropertyName("position")]
        public string? Position { get; set; }

        [JsonPropertyName("carrierIds")]
        public List<string>? CarrierIds { get; set; } 

        [JsonPropertyName("planId")]
        public string? PlanId { get; set; }

        [JsonPropertyName("stepNo")]
        public int? StepNo { get; set; }

        [JsonPropertyName("jobId")]
        public string? JobId { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
