using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    public class RequestAcsPlanHistoryPayload
    {
        [JsonPropertyName("planIds")]
        public List<string> PlanIds { get; set; }
    }
}
