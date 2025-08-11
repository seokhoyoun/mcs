using Nexus.Core.Domain.Models.Plans.Interfaces;
using Nexus.Core.Domain.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Plans
{
    public class PlanGroup : IEntity
    {
        public required string Id { get; set; }
        public required string Name { get; set; }

        // 이 PlanGroup에 속한 Plan 목록
        public IReadOnlyList<Plan> Plans => _plans.AsReadOnly();

        private List<Plan> _plans = new List<Plan>();

        private readonly IPlanExecutionStrategy _executionStrategy;

        public PlanGroup(IPlanExecutionStrategy executionStrategy)
        {
        }

        public PlanGroup(string id, string name, IPlanExecutionStrategy executionStrategy, IEnumerable<Plan>? plans = null)
        {
            Id = id;
            Name = name;

            if (plans != null)
                _plans.AddRange(plans);
        }

        public void AddPlan(Plan plan)
        {
            _plans.Add(plan);
        }
    }
}
