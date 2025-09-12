using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Areas.Services;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Locations.Services;
using Nexus.Core.Domain.Models.Lots.Interfaces;
using Nexus.Core.Domain.Models.Lots.Services;
using Nexus.Core.Domain.Models.Stockers.Interfaces;
using Nexus.Core.Domain.Models.Stockers.Services;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Models.Transports.Services;
using Nexus.Gateway.Services;
using Nexus.Gateway.Services.Interfaces;
using Nexus.Infrastructure.Persistence.Redis;
using StackExchange.Redis;
using System.Text.Json;
using Nexus.Core.Domain.Models.Robots.Interfaces;

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

            // Repository 서비스 등록
            builder.Services.AddSingleton<ILotRepository, RedisLotRepository>();

    
            builder.Services.AddSingleton<ILocationRepository, RedisLocationRepository>();
            builder.Services.AddSingleton<ILocationService, LocationService>();

            builder.Services.AddSingleton<ITransportRepository, RedisTransportRepository>();
            builder.Services.AddSingleton<ITransportService, TransportService>();

            builder.Services.AddSingleton<IAreaRepository, RedisAreaRepository>();
            builder.Services.AddSingleton<IAreaService, AreaService>();

            builder.Services.AddSingleton<IStockerRepository, RedisStockerRepository>();
            builder.Services.AddSingleton<IStockerService, StockerService>();

            // Robots
            builder.Services.AddSingleton<IRobotRepository, RedisRobotRepository>();

            builder.Services.AddScoped<LotService>();

            // Application 서비스 등록
            builder.Services.AddScoped<ILotCreationService, LotCreationService>();
            builder.Services.AddScoped<ICassetteCreationService, CassetteCreationService>();
            builder.Services.AddScoped<IAreaCreationService, AreaCreationService>();

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
