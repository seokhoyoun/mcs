using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    public class StepReportPayload
    {
        [JsonPropertyName("planId")]
        public string? PlanId { get; set; }

        [JsonPropertyName("robotId")]
        public string? RobotId { get; set; }

        [JsonPropertyName("stepNo")]
        public int StepNo { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
