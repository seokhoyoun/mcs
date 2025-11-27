using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Runtime.InteropServices.JavaScript;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddMudServices();

await JSHost.ImportAsync("__threeBridge", "/three-bridge.js");
await builder.Build().RunAsync();
