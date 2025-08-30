using Nexus.Core.Domain.Models.Plans.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Plans.Strategies
{
    public class ParallelPlanStrategy : IPlanExecutionStrategy
    {
        public IEnumerable<Plan> GetNextPlansToStart(PlanGroup planGroup, string? completedPlanId)
        {
            // 첫 시작 시에 모든 Plan을 반환
            if (completedPlanId == null)
            {
                return planGroup.Plans;
            }

            // Plan이 하나씩 완료될 때는 아무것도 시작하지 않음
            return Enumerable.Empty<Plan>();
        }
    }
}
