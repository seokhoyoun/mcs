using Nexus.Core.Domain.Models.Lots.Enums;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Shared.Interfaces;

namespace Nexus.Core.Domain.Models.Lots
{
    public class Lot : IEntity
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public ELotStatus Status { get; set; } = ELotStatus.Waiting;
        public int Priority { get; set; } = 0;
        public DateTime ReceivedTime { get; set; } = DateTime.MinValue;
        public string Chipset { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string EvalNo { get; set; } = string.Empty;
        public string PartNo { get; set; } = string.Empty;
        public int Qty { get; set; } = 0;
        public string Option { get; set; } = string.Empty;
        public string Line { get; set; } = string.Empty;
        public IReadOnlyList<Cassette> Cassettes => _cassettes.AsReadOnly();
        public IReadOnlyList<LotStep> LotSteps => _lotSteps.AsReadOnly();

        private List<Cassette> _cassettes = new List<Cassette>();
        private List<LotStep> _lotSteps = new List<LotStep>();

    }
}
