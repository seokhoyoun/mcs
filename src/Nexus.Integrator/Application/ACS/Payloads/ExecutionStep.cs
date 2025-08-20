using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    public class ExecutionStep
    {
        [JsonPropertyName("stepNo")]
        public int StepNo { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; } 

        [JsonPropertyName("position")]
        public string Position { get; set; } 

        [JsonPropertyName("jobs")]
        public List<ExecutionJob> Jobs { get; set; }
        [JsonPropertyName("carrierIds")]
        public List<string> CarrierIds { get; set; } // 추가
    }
}
