using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Messaging
{
    // 메시지 구독 기능을 추상화하는 인터페이스
    public interface IMessageSubscriber
    {
        Task SubscribeAsync(string channel, Action<string> handler, CancellationToken stoppingToken);
    }
}
