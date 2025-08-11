using Nexus.Core.Domain.Models.Plans;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Lots
{
    public class LotStep : IEntity
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public int LoadingType { get; set; }
        public string DpcType { get; set; } = string.Empty;

        public string PGM { get; set; } = string.Empty;
        public int PlanPercent { get; set; } = 100;

        public IReadOnlyList<Cassette> Cassettes => _cassettes.AsReadOnly();
        public IReadOnlyList<PlanGroup> PlanGroups => _planGroups.AsReadOnly();

        private List<Cassette> _cassettes = new List<Cassette>();
        private List<PlanGroup> _planGroups = new List<PlanGroup>();
    }
}
