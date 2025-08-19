using System.Collections.Generic;
using System.Linq;
using Nexus.Core.Domain.Models.Plans.Enums;
using Nexus.Core.Domain.Models.Plans.Interfaces;

namespace Nexus.Core.Domain.Models.Plans
{
    public class PlanGroup
    {
        public string Id { get; }
        public string Name { get; }
        public EPlanGroupType GroupType { get; }             
        public List<Plan> Plans { get; } = new List<Plan>();

        public PlanGroup(string id,
                         string name,
                         EPlanGroupType groupType)
        {
            Id = id;
            Name = name;
            GroupType = groupType;                             

        }
    }
}
