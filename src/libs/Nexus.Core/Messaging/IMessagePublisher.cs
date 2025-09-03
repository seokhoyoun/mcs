namespace Nexus.Core.Messaging
{
    /// <summary>
    /// 메시지 발행 기능을 추상화하는 인터페이스입니다.
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// 지정된 채널로 메시지를 비동기적으로 발행합니다.
        /// </summary>
        Task PublishAsync(string channel, string message, CancellationToken cancellationToken = default);
      
    }
}