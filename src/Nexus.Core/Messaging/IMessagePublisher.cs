namespace Nexus.Core.Messaging
{
    /// <summary>
    /// �޽��� ���� ����� �߻�ȭ�ϴ� �������̽��Դϴ�.
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// ������ ä�η� �޽����� �񵿱������� �����մϴ�.
        /// </summary>
        Task PublishAsync(string channel, string message, CancellationToken cancellationToken = default);
      
    }
}