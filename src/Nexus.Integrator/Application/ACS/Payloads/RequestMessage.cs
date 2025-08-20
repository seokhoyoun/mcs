using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    // 공통 요청 메시지 포맷
    public class RequestMessage<TPayload>
    {
        [JsonPropertyName("command")]
        public string Command { get; set; }

        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; } 

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } 

        [JsonPropertyName("payload")]
        public TPayload Payload { get; set; } 
    }
}
