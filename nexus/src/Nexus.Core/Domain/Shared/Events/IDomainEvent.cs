namespace Nexus.Core.Domain.Shared.Events
{
    /// <summary>
    /// ��� ������ �̺�Ʈ�� ���� �������̽��Դϴ�.
    /// </summary>
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }
}