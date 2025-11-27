using MudBlazor.Services;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Locations.Services;
using Nexus.Core.Domain.Models.Robots.Interfaces;
using Nexus.Core.Domain.Standards.Interfaces;
using Nexus.Infrastructure.Persistence.Redis;
using Nexus.Playground.Client.Pages;
using Nexus.Playground.Components;
using Nexus.Playground.Components.Layout;
using StackExchange.Redis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

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

builder.Services.AddScoped<ISpaceRepository, RedisSpaceRepository>();
builder.Services.AddScoped<ILocationGraphRepository, RedisLocationGraphRepository>();
builder.Services.AddScoped<ILocationGraphService, LocationGraphService>();
builder.Services.AddScoped<IRobotRepository, RedisRobotRepository>();
builder.Services.AddScoped<IDimensionRepository, RedisDimensionRepository>();


builder.Services.AddMudServices();
builder.Services.AddScoped<DockService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Nexus.Playground.Client._Imports).Assembly);

app.Run();
