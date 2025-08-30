using Nexus.Core.Domain.Models.Lots.Enums;
using Nexus.Core.Domain.Models.Plans;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Shared.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Lots
{
    public class LotStep : IEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LotId { get; set; }
        public int LoadingType { get; set; }
        public string DpcType { get; set; } = string.Empty;
        public string Chipset { get; set; } = string.Empty;
        public string PGM { get; set; } = string.Empty;
        public int PlanPercent { get; set; } = 100;
        public ELotStatus Status { get; set; } = ELotStatus.Waiting;
        public List<Cassette> Cassettes { get; set; } = new List<Cassette>();
        public List<PlanGroup> PlanGroups { get; set; } = new List<PlanGroup>();


        public LotStep(string id, string lotId, string name, int loadingType, string dpcType, string chipset, string pgm, int planPercent, ELotStatus status)
        {
            Id = id;
            LotId = lotId;
            Name = name;
            LoadingType = loadingType;
            DpcType = dpcType;
            Chipset = chipset;
            PGM = pgm;
            PlanPercent = planPercent;
            Status = status;

        }
    }
}
