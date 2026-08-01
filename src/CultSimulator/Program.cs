using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using CultSimulator;
using CultSimulator.Game;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<CloudSaveService>(sp => new CloudSaveService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<IJSRuntime>()));
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<WorldLocationService>();
builder.Services.AddScoped<ConversionDataService>();

await builder.Build().RunAsync();
