using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    public class ErrorReportPayload
    {
        [JsonPropertyName("robotId")]
        public string? RobotId { get; set; }

        [JsonPropertyName("planId")]
        public string? PlanId { get; set; }

        [JsonPropertyName("state")]
        public bool State { get; set; } // true: 발생, false: 해제

        [JsonPropertyName("stepNo")]
        public int? StepNo { get; set; }

        [JsonPropertyName("jobId")]
        public string? JobId { get; set; }

        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("level")]
        public string? Level { get; set; } // "heavy", "light"

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
