using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Plans.Interfaces
{

    public interface IPlanExecutionStrategy
    {
        /// <summary>
        /// 다음 Plan을 시작할 조건 결정 전략
        /// </summary>
        /// <param name="planGroup">현재 처리 중인 PlanGroup 인스턴스입니다. 이 객체의 상태(Plan 목록, 타입 등)를 기반으로 다음 행동을 결정합니다.</param>
        /// <param name="completedPlanId">방금 완료된 Plan의 ID입니다. PlanGroup이 처음 시작될 때는 null입니다. 이 ID를 통해 전략은 특정 Plan의 완료에 기반한 결정을 내릴 수 있습니다.</param>
        /// <returns></returns>
        IEnumerable<Plan> GetNextPlansToStart(PlanGroup planGroup, string? completedPlanId);
    }
}
