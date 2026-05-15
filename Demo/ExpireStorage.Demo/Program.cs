using Drogecode.Blazor.ExpireStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ExpireStorage.Demo;
using ExpireStorage.Demo.Services;
using ExpireStorage.Demo.Services.Interfaces;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ILocalUserSettingService, LocalUserSettingService>();

// Adds expire storage services
builder.Services.AddExpireStorage();

await builder.Build().RunAsync();
