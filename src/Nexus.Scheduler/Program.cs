using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Lots.Events;
using Nexus.Core.Domain.Models.Stockers;
using Nexus.Core.Messaging;
using Nexus.Infrastructure.Messaging;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Scheduler.Application.Services;
using Nexus.Scheduler.Application.Services.EventHandlers;
using Nexus.Shared.Application.Interfaces;
using Prometheus;
using StackExchange.Redis;

namespace Nexus.Scheduler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis:6379,abortConnect=false"));
            builder.Services.AddSingleton<IAreaRepository, RedisAreaRepository>();

            builder.Services.AddSingleton<AreaService>();
            builder.Services.AddSingleton<SchedulerService>();

            builder.Services.AddSingleton<StockerService>();

            builder.Services.AddSingleton<IMessagePublisher, RedisPublisher>();
            builder.Services.AddSingleton<IEventPublisher, DomainEventPublisher>();


            builder.Services.AddScoped<IEventHandler<LotCreatedEvent>, LotCreatedEventHandler>();

            builder.Services.AddHostedService<Worker>();


            builder.Services.AddMetricServer(options => { });

            var host = builder.Build();
            host.Run();
        }
    }
}