using Nexus.Core.Messaging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Infrastructure.Messaging
{
    public class RedisSubscriber : IMessageSubscriber
    {
        private readonly ISubscriber _redisSubscriber;

        // 생성자를 통해 Redis 연결 정보를 주입받습니다.
        public RedisSubscriber(IConnectionMultiplexer connection)
        {
            _redisSubscriber = connection.GetSubscriber();
        }

        // IMessageSubscriber 인터페이스의 SubscribeAsync 메서드를 구현합니다.
        public async Task<string> SubscribeAsync(string channel, CancellationToken stoppingToken)
        {
            // Redis의 Pub/Sub 기능을 사용해 메시지를 구독하고 비동기로 결과를 기다립니다.
            var channelMessage = await _redisSubscriber.SubscribeAsync(RedisChannel.Literal(channel));

            // 메시지가 도착할 때까지 대기하는 로직을 추가할 수 있습니다.
            // 여기서는 예시를 위해 단순화합니다.
            ChannelMessage message = await channelMessage.ReadAsync(stoppingToken);

            return message.Message;
        }
    }
}
