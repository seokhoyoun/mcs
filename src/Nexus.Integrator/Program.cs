using Nexus.Core.Messaging;
using Nexus.Infrastructure.Messaging.Redis;
using Nexus.Integrator.Application.Services;
using StackExchange.Redis;

namespace Nexus.Integrator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis:6379,abortConnect=false"));
            builder.Services.AddSingleton<IMessageSubscriber, RedisSubscriber>();

            builder.Services.AddSingleton<IntegratorService>();

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}