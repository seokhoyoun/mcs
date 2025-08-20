namespace Nexus.Core.Domain.Models.Plans.Enums
{
    public enum EJobStatus
    {
        Pending = 0,      // 대기 중
        Instructed,   // 명령 전달됨
        InProgress,   // 진행 중
        Completed,    // 완료
        Failed        // 실패
    }
}