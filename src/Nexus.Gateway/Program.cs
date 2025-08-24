using Nexus.Core.Domain.Models.Lots.Interfaces;
using Nexus.Core.Domain.Models.Lots.Services;
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

            // Repository ���� - LotStepRepository ����
            builder.Services.AddSingleton<ILotRepository, RedisLotRepository>();
            // builder.Services.AddSingleton<ILotStepRepository, RedisLotStepRepository>(); // ����

            // ����Ͻ� ����
            builder.Services.AddScoped<ILotCreationService, LotCreationService>();
            builder.Services.AddScoped<LotService>(); // LotStep ������
            // Redis ����
            var redisOptions = new RedisOptions();
            builder.Configuration.GetSection(RedisOptions.SECTION_NAME).Bind(redisOptions);

            builder.Services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
            {
                return ConnectionMultiplexer.Connect(redisOptions.GetConnectionString());
            });

            // �޽�¡ ����
            builder.Services.AddSingleton<IMessagePublisher, RedisPublisher>();
            builder.Services.AddSingleton<IEventPublisher, DomainEventPublisher>();

            // Repository ����
            builder.Services.AddSingleton<ILotRepository, RedisLotRepository>();

            // ����Ͻ� ����
            builder.Services.AddScoped<ILotCreationService, LotCreationService>();

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
