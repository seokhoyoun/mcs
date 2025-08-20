using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    public class TscStateUpdatePayload
    {
        [JsonPropertyName("state")]
        public string? State { get; set; }
    }
}
