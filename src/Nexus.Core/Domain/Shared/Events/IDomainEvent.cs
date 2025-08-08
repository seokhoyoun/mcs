namespace Nexus.Core.Domain.Shared.Events
{
    /// <summary>
    /// 모든 도메인 이벤트의 공통 인터페이스입니다.
    /// </summary>
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }
}