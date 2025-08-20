using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    public class RequestAcsPlanHistoryAckPayload
    {
        [JsonPropertyName("plans")]
        public List<AcsPlanHistoryInfo>? Plans { get; set; }
    }

    public class AcsPlanHistoryInfo
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
