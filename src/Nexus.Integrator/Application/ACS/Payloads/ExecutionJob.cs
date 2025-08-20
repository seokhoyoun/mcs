using System.Text.Json.Serialization;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    public class ExecutionJob
    {
        [JsonPropertyName("jobId")]
        public string? JobId { get; set; } 

        [JsonPropertyName("from")]
        public string? From { get; set; } 

        [JsonPropertyName("to")]
        public string? To { get; set; } 
    }
}
