using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Robots.Interfaces;
using Nexus.Core.Domain.Models.Locations.Services;
using Nexus.Infrastructure.Persistence.Redis;
using StackExchange.Redis;
using Prometheus;

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
       
            builder.Services.AddSingleton<ISpaceRepository, RedisSpaceRepository>();
            builder.Services.AddSingleton<ILocationGraphRepository, RedisLocationGraphRepository>();
            builder.Services.AddSingleton<ILocationGraphService, LocationGraphService>();
            builder.Services.AddSingleton<IRobotRepository, RedisRobotRepository>();

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
            // Expose default HTTP metrics for incoming requests
            app.UseHttpMetrics();

            app.Run();
        }
    }
}
