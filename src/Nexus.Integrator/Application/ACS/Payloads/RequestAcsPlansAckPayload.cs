using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    public class RequestAcsPlansAckPayload
    {
        [JsonPropertyName("plans")]
        public List<AcsPlanStatus> Plans { get; set; } = new List<AcsPlanStatus>();
    }

    public class AcsPlanStatus
    {
        [JsonPropertyName("planId")]
        public string? PlanId { get; set; }

        [JsonPropertyName("robotId")]
        public string? RobotId { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("stepNo")]
        public int StepNo { get; set; }

        [JsonPropertyName("currentAction")]
        public string? CurrentAction { get; set; }
        [JsonPropertyName("jobId")]
        public string? JobId { get; set; }
        [JsonPropertyName("startTime")]
        public string? StartTime { get; set; }  // ISO 8601 string
        [JsonPropertyName("endTime")]
        public string? EndTime { get; set; }  // ISO 8601 string
    }
}
