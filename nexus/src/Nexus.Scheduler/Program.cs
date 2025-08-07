using Nexus.Core.Infrastructure.Redis;
using StackExchange.Redis;

namespace Nexus.Scheduler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));
            builder.Services.AddSingleton<RedisDataService>();

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}