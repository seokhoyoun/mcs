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
using Nexus.Core.Messaging;
using Nexus.Gateway.Configuration;
using Nexus.Gateway.Services;
using Nexus.Gateway.Services.Interfaces;
using Nexus.Infrastructure.Messaging;
using Nexus.Infrastructure.Messaging.Redis;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Shared.Application.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Nexus.Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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

            // Redis 연결 설정
            var redisOptions = new RedisOptions();
            builder.Configuration.GetSection(RedisOptions.SECTION_NAME).Bind(redisOptions);

            builder.Services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
            {
                return ConnectionMultiplexer.Connect(redisOptions.GetConnectionString());
            });

            // 메시징 서비스 등록
            builder.Services.AddSingleton<IMessagePublisher, RedisPublisher>();
            builder.Services.AddSingleton<IMessageSubscriber, RedisSubscriber>();
            builder.Services.AddSingleton<IEventPublisher, DomainEventPublisher>();

            // Repository 서비스 등록
            builder.Services.AddSingleton<ILotRepository, RedisLotRepository>();

    
            builder.Services.AddSingleton<ILocationRepository, RedisLocationRepository>();
            builder.Services.AddSingleton<ILocationService, LocationService>();

            builder.Services.AddSingleton<ITransportsRepository, RedisTransportsRepository>();
            builder.Services.AddSingleton<ITransportService, TransportService>();

            builder.Services.AddSingleton<IAreaRepository, RedisAreaRepository>();
            builder.Services.AddSingleton<IAreaService, AreaService>();

            builder.Services.AddSingleton<IStockerRepository, RedisStockerRepository>();
            builder.Services.AddSingleton<IStockerService, StockerService>();

      
            builder.Services.AddScoped<LotService>();

            // Application 서비스 등록
            builder.Services.AddScoped<ILotCreationService, LotCreationService>();
            builder.Services.AddScoped<ICassetteCreationService, CassetteCreationService>();
            builder.Services.AddScoped<IAreaCreationService, AreaCreationService>();

            var app = builder.Build();

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