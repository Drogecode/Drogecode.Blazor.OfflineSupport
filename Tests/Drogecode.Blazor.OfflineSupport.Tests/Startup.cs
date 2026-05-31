using Drogecode.Blazor.OfflineSupport.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Xunit.DependencyInjection.Logging;

namespace Drogecode.Blazor.OfflineSupport.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<ILocalStorageExpireService, LocalStorageExpireService>();
        services.AddScoped<ISessionExpireService, SessionExpireService>();
        services.AddScoped<IOfflineSupportService, OfflineSupportService>();
        
        services.AddScoped<IJSRuntime, JSRuntimeMock>();
        services.AddScoped<IJsStorageService, MockJsStorageService>();

        services.AddLogging(lb => lb.AddXunitOutput());
    }
}