using System.Text.Json.Serialization;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    public class CancelPlanPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; } 

        [JsonPropertyName("reason")]
        public string Reason { get; set; }
    }

    public class AbortPlanPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }
    }

    public class PausePlanPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; } 

        [JsonPropertyName("reason")]
        public string Reason { get; set; }
    }

    public class ResumePlanPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; } 

    }

    public class CancelResultReportPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; }

        [JsonPropertyName("result")]
        public string Result { get; set; }  // 예: "Success", "Fail"

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class AbortResultReportPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; }

        [JsonPropertyName("result")]
        public string Result { get; set; }  // 예: "Success", "Fail"

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class PauseResultReportPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; }

        [JsonPropertyName("result")]
        public string Result { get; set; }  // 예: "Success", "Fail"

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class ResumeResultReportPayload
    {
        [JsonPropertyName("planId")]
        public string PlanId { get; set; }

        [JsonPropertyName("result")]
        public string Result { get; set; }  // 예: "Success", "Fail"

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
