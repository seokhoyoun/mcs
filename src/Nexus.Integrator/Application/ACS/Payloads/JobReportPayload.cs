using System.Text.Json.Serialization;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    public class JobReportPayload
    {
        [JsonPropertyName("planId")]
        public string? PlanId { get; set; } 

        [JsonPropertyName("robotId")]
        public string? RobotId { get; set; } 

        [JsonPropertyName("stepNo")]
        public int StepNo { get; set; }

        [JsonPropertyName("jobId")]
        public string? JobId { get; set; } 

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
