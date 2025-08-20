using System.Text.Json.Serialization;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    // ExecutionPlanAck 응답 payload 예시
    public class ExecutionPlanAckPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; }
    }
}
