using Nexus.Core.Domain.Models.Lots.Enums;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Lots
{
    public class Lot : IEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ELotStatus Status { get; set; } = ELotStatus.Waiting;
        public int Priority { get; set; } = 0;
        public DateTime ReceivedTime { get; set; } = DateTime.MinValue;
        public string Purpose { get; set; } = string.Empty;
        public string EvalNo { get; set; } = string.Empty;
        public string PartNo { get; set; } = string.Empty;
        public int Qty { get; set; } = 0;
        public string Option { get; set; } = string.Empty;
        public string Line { get; set; } = string.Empty;
        public List<string> CassetteIds { get; set; }
        public List<LotStep> LotSteps { get; set; } = new List<LotStep>();


        public Lot(string id, string name, ELotStatus status, int priority, DateTime receivedTime, string purpose, string evalNo, string partNo, int qty, string option, string line, List<string> cassetteIds)
        {
            Id = id;
            Name = name;
            Status = status;
            Priority = priority;
            ReceivedTime = receivedTime;
            Purpose = purpose;
            EvalNo = evalNo;
            PartNo = partNo;
            Qty = qty;
            Option = option;
            Line = line;
            CassetteIds = cassetteIds;
        }
    }
}
