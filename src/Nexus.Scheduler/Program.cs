using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Lots.Events;
using Nexus.Core.Messaging;
using Nexus.Infrastructure.Messaging;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Scheduler.Application.Services;
using Nexus.Scheduler.Application.Services.EventHandlers;
using Nexus.Shared.Application.Interfaces;
using StackExchange.Redis;

namespace Nexus.Scheduler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis:6379,abortConnect=false"));
            builder.Services.AddSingleton<ILocationRepository, RedisLocationRepository>();

            builder.Services.AddSingleton<LocationService>();
            builder.Services.AddSingleton<SchedulerService>();

            builder.Services.AddSingleton<IMessagePublisher, RedisPublisher>();
            builder.Services.AddSingleton<IEventPublisher, DomainEventPublisher>();


            builder.Services.AddScoped<IEventHandler<LotCreatedEvent>, LotCreatedEventHandler>();

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}