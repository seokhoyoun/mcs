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
        // 지정된 채널을 구독하고 메시지를 비동기적으로 받습니다.
        Task<string> SubscribeAsync(string channel, CancellationToken stoppingToken);
    }
}
