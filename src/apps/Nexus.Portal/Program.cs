using Microsoft.Extensions.Hosting;
using MudBlazor.Services;
using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Areas.Services;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Locations.Services;
using Nexus.Core.Domain.Models.Robots.Interfaces;
using Nexus.Core.Domain.Models.Lots.Interfaces;
using Nexus.Core.Domain.Models.Stockers.Interfaces;
using Nexus.Core.Domain.Models.Stockers.Services;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Models.Transports.Services;
using Nexus.Core.Domain.Shared.Bases;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Portal.Components;
using Nexus.Portal.Components.Layout;
using StackExchange.Redis;
using Nexus.Core.Domain.Standards.Interfaces;

namespace Nexus.Portal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

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

            builder.Services.AddScoped<ITransportRepository, RedisTransportRepository>();
            builder.Services.AddScoped<IStockerRepository, RedisStockerRepository>();
            builder.Services.AddScoped<IAreaRepository, RedisAreaRepository>();
            builder.Services.AddScoped<ILocationRepository, RedisLocationRepository>();
            builder.Services.AddScoped<IRobotRepository, RedisRobotRepository>();
            builder.Services.AddScoped<ILotRepository, RedisLotRepository>();
            builder.Services.AddScoped<IDimensionRepository, RedisDimensionRepository>();

            builder.Services.AddScoped<ILocationService, LocationService>();
            builder.Services.AddScoped<ITransportService, TransportService>();
            

            builder.Services.AddMudServices();
            builder.Services.AddScoped<DockService>();

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
