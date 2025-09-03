namespace Nexus.Core.Domain.Models.Areas.Enums
{
    public enum EAreaStatus
    {
        Idle = 0,        // 유휴
        Reserved,    // 예약됨
        InUse,       // 사용 중
        Maintenance, // 점검/정비 중
        Disabled     // 비활성화
    }
}