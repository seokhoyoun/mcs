using Nexus.Infrastructure.Persistence.Redis;
using StackExchange.Redis;
using System.Text.Json;
using Nexus.Core.Domain.Models.Robots.Interfaces;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Locations.Services;

namespace Nexus.Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // JSON 설정
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.PropertyNameCaseInsensitive = true;
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            // Redis 연결 설정 (환경변수/설정파일에서 로드)
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
                    connStr = "redis:6379";
                }
                return ConnectionMultiplexer.Connect(connStr);
            });

            // Repository/Service 등록 (Space 기반 최소 구성)
            builder.Services.AddSingleton<ISpaceRepository, RedisSpaceRepository>();
            builder.Services.AddSingleton<ILocationGraphRepository, RedisLocationGraphRepository>();
            builder.Services.AddSingleton<ILocationGraphService, LocationGraphService>();
            builder.Services.AddSingleton<IRobotRepository, RedisRobotRepository>();

            // Prometheus metrics server for Gateway (separate port 9092)
            //builder.Services.AddMetricServer(options =>
            //{
            //    options.Port = 9092;
            //});

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            // HTTP request metrics instrumentation
            //app.UseHttpMetrics();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
