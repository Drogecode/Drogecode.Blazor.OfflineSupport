using Drogecode.Blazor.ExpireStorage;
using ExpireStorage.Demo.Enums;
using ExpireStorage.Demo.Models;
using ExpireStorage.Demo.Services.Interfaces;

namespace ExpireStorage.Demo.Services;

public class LocalUserSettingService : ILocalUserSettingService
{
    private const string KEY = "localUserSettings";
    private readonly IJsStorageService _jsStorageService;

    public LocalUserSettingService(IJsStorageService jsStorageService)
    {
        _jsStorageService = jsStorageService;
    }

    public async Task<LocalUserSettings> GetLocalUserSettings()
    {
        var defaultResponse = new LocalUserSettings(DarkLightMode.System);
        try
        {
            var settings = await _jsStorageService.RetrieveItem<LocalUserSettings?>(KEY, StorageLocation.BrowserLocal) ?? defaultResponse;
            return settings;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return defaultResponse;
        }
    }

    public async Task<LocalUserSettings> SetDarkLight(DarkLightMode darkLightMode)
    {
        var oldVersion = await GetLocalUserSettings();
        oldVersion.DarkLightMode = darkLightMode;
        await SetLocalUserSettings(oldVersion);
        return oldVersion;
    }

    public async Task<LocalUserSettings> SetRememberMe(bool rememberMe)
    {
        var oldVersion = await GetLocalUserSettings();
        await SetLocalUserSettings(oldVersion);
        return oldVersion;
    }

    private async Task SetLocalUserSettings(LocalUserSettings settings)
    {
        await _jsStorageService.StoreItem(KEY, StorageLocation.BrowserLocal, settings);
    }
}