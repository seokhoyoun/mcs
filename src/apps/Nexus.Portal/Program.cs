using Microsoft.Extensions.Hosting;
using MudBlazor.Services;
using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Areas.Services;
using Nexus.Core.Domain.Models.Stockers.Interfaces;
using Nexus.Core.Domain.Models.Stockers.Services;
using Nexus.Core.Domain.Shared.Bases;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Portal.Components;

namespace Nexus.Portal
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddSingleton<IStockerRepository, RedisStockerRepository>();
            builder.Services.AddSingleton<IAreaRepository, RedisAreaRepository>();

            builder.Services.AddSingleton<IStockerService, StockerService>();
            builder.Services.AddSingleton<IService>(sp => sp.GetRequiredService<IStockerService>());
            builder.Services.AddSingleton<IAreaService, AreaService>();
            builder.Services.AddSingleton<IService>(sp => sp.GetRequiredService<IAreaService>());

            builder.Services.AddMudServices();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                IEnumerable<IService> services = scope.ServiceProvider.GetServices<IService>();

                List<Task> initializationTasks = new List<Task>();
                foreach (var service in services)
                {
                    initializationTasks.Add(service.InitializeAsync());
                }
                
                await Task.WhenAll(initializationTasks);
            }

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
