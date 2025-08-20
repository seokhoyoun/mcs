using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    public class AcsCommStateUpdatePayload
    {   
        [JsonPropertyName("isConnected")]
        public bool IsConnected { get; set; }
    }
}
