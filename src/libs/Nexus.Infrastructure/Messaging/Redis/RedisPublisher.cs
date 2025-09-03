using Nexus.Core.Messaging;
using StackExchange.Redis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Infrastructure.Messaging.Redis
{
    public class RedisPublisher : IMessagePublisher
    {
        private readonly ISubscriber _redisPublisher;

        public RedisPublisher(IConnectionMultiplexer connection)
        {
            _redisPublisher = connection.GetSubscriber();
        }

        public async Task PublishAsync(string channel, string message, CancellationToken cancellationToken = default)
        {
            await _redisPublisher.PublishAsync(RedisChannel.Literal(channel), message);
        }

    }
}