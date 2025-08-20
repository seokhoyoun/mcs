using System.Text.Json.Serialization;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    // 공통 응답 메시지 포맷
    public class ResponseMessage<TPayload>
    {
        [JsonPropertyName("command")]
        public string Command { get; set; } 

        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; } 

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } 

        [JsonPropertyName("result")]
        public string Result { get; set; }  // "Success" or "Fail"

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("payload")]
        public TPayload Payload { get; set; } 
    }
}
