using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Drogecode.Blazor.OfflineSupport;

public static class BuilderHelper
{
    public static IServiceCollection AddOfflineSupport(this IServiceCollection services)
    {
        services.TryAddScoped<ILocalStorageExpireService, LocalStorageExpireService>();
        services.TryAddScoped<ISessionExpireService, SessionExpireService>();
        services.TryAddScoped<IOfflineSupportService, OfflineSupportService>();
        services.TryAddScoped<IJsStorageService, JsStorageService>();
        return services;
    }

    public static IServiceCollection AddOfflineSupportAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<ILocalStorageExpireService, LocalStorageExpireService>();
        services.TryAddSingleton<ISessionExpireService, SessionExpireService>();
        services.TryAddSingleton<IOfflineSupportService, OfflineSupportService>();
        services.TryAddSingleton<IJsStorageService, JsStorageService>();
        return services;
    }
}