using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Areas.Services;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Locations.Services;
using Nexus.Core.Domain.Models.Lots.Events;
using Nexus.Core.Domain.Models.Stockers.Interfaces;
using Nexus.Core.Domain.Models.Stockers.Services;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Models.Transports.Services;
using Nexus.Core.Messaging;
using Nexus.Infrastructure.Messaging;
using Nexus.Infrastructure.Messaging.Redis;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Orchestrator.Application.Acs;
using Nexus.Orchestrator.Application.Acs.Services;
using Nexus.Orchestrator.Application.Scheduler;
using Nexus.Orchestrator.Application.Scheduler.Services;
using Nexus.Orchestrator.Application.Scheduler.Services.EventHandlers;
using Nexus.Shared.Application.Interfaces;
using Prometheus;
using StackExchange.Redis;

namespace Nexus.Orchestrator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis:6379,abortConnect=false"));
       
            builder.Services.AddSingleton<ILocationRepository, RedisLocationRepository>();
            builder.Services.AddSingleton<ITransportsRepository, RedisTransportsRepository>();
            builder.Services.AddSingleton<IAreaRepository, RedisAreaRepository>();
            builder.Services.AddSingleton<IStockerRepository, RedisStockerRepository>();

            builder.Services.AddSingleton<ILocationService, LocationService>();
            builder.Services.AddSingleton<ITransportService, TransportService>();
            builder.Services.AddSingleton<IAreaService, AreaService>();
            builder.Services.AddSingleton<IStockerService, StockerService>();
            builder.Services.AddSingleton<SchedulerService>();


            builder.Services.AddSingleton<IMessagePublisher, RedisPublisher>();
            builder.Services.AddSingleton<IMessageSubscriber, RedisSubscriber>();
            builder.Services.AddSingleton<IEventPublisher, DomainEventPublisher>();


            builder.Services.AddScoped<IEventHandler<LotCreatedEvent>, LotCreatedEventHandler>();

            // ACS 서비스 및 워커 등록
            builder.Services.AddSingleton<AcsService>();
            builder.Services.AddHostedService<AcsWorker>();
            builder.Services.AddHostedService<SchedulerWorker>();

            builder.Services.AddMetricServer(options => { });

            var host = builder.Build();
            host.Run();
        }
    }
}