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
        public PlanGroupType GroupType { get; }             
        public IPlanExecutionStrategy ExecutionStrategy { get; }
        public List<Plan> Plans { get; }

        public PlanGroup(string id,
                         string name,
                         PlanGroupType groupType,              
                         IPlanExecutionStrategy executionStrategy,
                         IEnumerable<Plan> plans)
        {
            Id = id;
            Name = name;
            GroupType = groupType;                             
            ExecutionStrategy = executionStrategy;
            Plans = plans?.ToList() ?? new List<Plan>();
        }
    }
}
