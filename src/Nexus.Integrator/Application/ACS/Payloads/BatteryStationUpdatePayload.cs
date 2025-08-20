using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nexus.Integrator.Application.ACS.Payloads
{
    public class BatteryStationUpdatePayload
    {
        [JsonPropertyName("stationId")]
        public string StationId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("chargingPorts")]
        public List<BatteryChargingPort> ChargingPorts { get; set; }
    }

    public class BatteryChargingPort
    {
        [JsonPropertyName("portId")]
        public string PortId { get; set; } = string.Empty;

        [JsonPropertyName("robotId")]
        public string RobotId { get; set; }

        [JsonPropertyName("batteryStatus")]
        public string BatteryStatus { get; set; } = string.Empty;

        [JsonPropertyName("packVoltage")]
        public double? PackVoltage { get; set; }

        [JsonPropertyName("packCurrent")]
        public double? PackCurrent { get; set; }

        [JsonPropertyName("packSOC")]
        public double? PackSOC { get; set; }

        [JsonPropertyName("packSOH")]
        public double? PackSOH { get; set; }

        [JsonPropertyName("packTemp")]
        public double? PackTemp { get; set; }
    }
}
