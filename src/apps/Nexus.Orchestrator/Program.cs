using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Areas.Services;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Locations.Services;
using Nexus.Core.Domain.Models.Lots.Events;
using Nexus.Core.Domain.Models.Lots.Interfaces;
using Nexus.Core.Domain.Models.Robots.Interfaces;
using Nexus.Core.Domain.Models.Stockers.Interfaces;
using Nexus.Core.Domain.Models.Stockers.Services;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Models.Transports.Services;
using Nexus.Core.Domain.Shared.Events;
using Nexus.Core.Messaging;
using Nexus.Infrastructure.Messaging;
using Nexus.Infrastructure.Messaging.Redis;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Orchestrator.Application.Acs;
using Nexus.Orchestrator.Application.Acs.Services;
using Nexus.Orchestrator.Application.Scheduler;
using Nexus.Orchestrator.Application.Scheduler.Services;
using Nexus.Orchestrator.Application.Scheduler.Services.EventHandlers;
using Prometheus;
using StackExchange.Redis;

namespace Nexus.Orchestrator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
                string? fromSection = configuration["Redis:ConnectionString"];
                string? fromEnv = configuration["Redis__ConnectionString"];
                string connStr;
                if (fromSection != null)
                {
                    connStr = fromSection;
                }
                else if (fromEnv != null)
                {
                    connStr = fromEnv;
                }
                else
                {
                    connStr = "localhost:6379";
                }
                return ConnectionMultiplexer.Connect(connStr);
            });
       
            builder.Services.AddSingleton<ILocationRepository, RedisLocationRepository>();
            builder.Services.AddSingleton<ITransportRepository, RedisTransportRepository>();
            builder.Services.AddSingleton<IAreaRepository, RedisAreaRepository>();
            builder.Services.AddSingleton<IStockerRepository, RedisStockerRepository>();
            builder.Services.AddSingleton<ILotRepository, RedisLotRepository>();
            builder.Services.AddSingleton<IRobotRepository, RedisRobotRepository>();

            builder.Services.AddSingleton<ILocationService, LocationService>();
            builder.Services.AddSingleton<ITransportService, TransportService>();
            builder.Services.AddSingleton<IAreaService, AreaService>();
            builder.Services.AddSingleton<IStockerService, StockerService>();

            //builder.Services.AddSingleton<IAcsService, AcsSimulationService>();
            builder.Services.AddSingleton<SchedulerService>();

            builder.Services.AddSingleton<IMessagePublisher, RedisPublisher>();
            builder.Services.AddSingleton<IMessageSubscriber, RedisSubscriber>();
            builder.Services.AddSingleton<IEventPublisher, DomainEventPublisher>();


            builder.Services.AddScoped<IEventHandler<LotCreatedEvent>, LotCreatedEventHandler>();

            builder.Services.AddHostedService<AcsWorker>();
            builder.Services.AddHostedService<SchedulerWorker>();

            // SignalR hub for robot position broadcast
            builder.Services.AddSignalR();

            // CORS to allow Portal (8080) to connect
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials()
                          .SetIsOriginAllowed(_ => true);
                });
            });

            builder.Services.AddMetricServer(options =>
            {
                options.Port = 9091;
            });

            WebApplication app = builder.Build();

            app.UseCors();
            app.MapHub<Nexus.Core.Domain.Models.Robots.Hubs.RobotPositionMessageHub>("/hubs/robotPosition");

            app.Run();
        }
    }
}
