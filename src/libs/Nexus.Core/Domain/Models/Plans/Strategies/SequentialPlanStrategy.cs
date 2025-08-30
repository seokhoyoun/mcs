// src/Nexus.Core/Domain/Models/Plans/Strategies/SequentialPlanStrategy.cs
using Nexus.Core.Domain.Models.Plans.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Nexus.Core.Domain.Models.Plans.Strategies
{
    public class SequentialPlanStrategy : IPlanExecutionStrategy
    {
        public IEnumerable<Plan> GetNextPlansToStart(PlanGroup planGroup, string? completedPlanId)
        {
            if (completedPlanId == null)
                return planGroup.Plans.Take(1); // 첫 1건부터

            var completedIndex = planGroup.Plans.FindIndex(p => p.Id == completedPlanId);
            if (completedIndex < 0 || completedIndex + 1 >= planGroup.Plans.Count)
                return Enumerable.Empty<Plan>();

            return new[] { planGroup.Plans[completedIndex + 1] };
        }
    }
}
