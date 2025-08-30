namespace Nexus.Core.Domain.Models.Plans.Enums
{
    public enum EPlanStepStatus
    {
        Pending = 0,      // 대기 중
        Dispatched,   // 할당됨
        InProgress,   // 진행 중
        Completed,    // 완료
        Failed,       // 실패
        Skipped       // 건너뜀
    }
}