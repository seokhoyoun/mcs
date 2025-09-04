using Microsoft.Extensions.Hosting;
using MudBlazor.Services;
using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Areas.Services;
using Nexus.Core.Domain.Models.Stockers.Interfaces;
using Nexus.Core.Domain.Models.Stockers.Services;
using Nexus.Core.Domain.Shared.Bases;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Portal.Components;
using StackExchange.Redis;

namespace Nexus.Portal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis:6379,abortConnect=false"));

            builder.Services.AddScoped<ITransportsRepository, RedisTransportsRepository>();
            builder.Services.AddScoped<IStockerRepository, RedisStockerRepository>();
            builder.Services.AddScoped<IAreaRepository, RedisAreaRepository>();

            builder.Services.AddMudServices();

            var app = builder.Build();

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
