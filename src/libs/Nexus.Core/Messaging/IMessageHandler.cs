using System.Threading.Tasks;

namespace Nexus.Core.Messaging
{
    public interface IMessageHandler<in TMessage>
    {
        Task HandleAsync(TMessage message, CancellationToken cancellationToken = default);
    }
}