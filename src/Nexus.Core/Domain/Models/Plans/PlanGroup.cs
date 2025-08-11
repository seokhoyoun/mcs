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
        public string Id { get;  }
        public string Name { get;  }

        public IReadOnlyList<Plan> Plans => _plans.AsReadOnly();

        private readonly IPlanExecutionStrategy _executionStrategy;
        private List<Plan> _plans = new List<Plan>();

        public PlanGroup(string id, string name, IPlanExecutionStrategy executionStrategy, List<Plan> plans)
        {
            Id = id;
            Name = name;
            _executionStrategy = executionStrategy;
            _plans = plans;
        }

    }
}
