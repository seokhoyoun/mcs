using Nexus.Core.Messaging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Infrastructure.Messaging.Redis
{
    public class RedisSubscriber : IMessageSubscriber
    {
        private readonly ISubscriber _redisSubscriber;

        // 생성자를 통해 Redis 연결 정보를 주입받습니다.
        public RedisSubscriber(IConnectionMultiplexer connection)
        {
            _redisSubscriber = connection.GetSubscriber();
        }

        public async Task SubscribeAsync(string channel, Action<string> handler, CancellationToken stoppingToken)
        {
            Debug.Assert(!string.IsNullOrEmpty(channel), "Channel name cannot be null or empty.");
            Debug.Assert(handler != null, "Handler cannot be null.");

            await _redisSubscriber.SubscribeAsync(RedisChannel.Literal(channel), (ch, message) =>
            {
                handler(message.ToString());
            });

            // 구독은 비동기적으로 계속 유지됨. 필요시 stoppingToken 활용해 구독 해제 가능
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
