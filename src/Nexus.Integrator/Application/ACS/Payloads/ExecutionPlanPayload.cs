using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    public class ExecutionPlanPayload
    {
        [JsonPropertyName("planId")]
        public string? PlanId { get; set; }
        [JsonPropertyName("lotId")]
        public string? LotId { get; set; } 

        [JsonPropertyName("priority")]
        public int? Priority { get; set; }

        [JsonPropertyName("steps")]
        public List<ExecutionStep>? Steps { get; set; } 
    }
}
