using System.Buffers.Text;
using System.Text;
using System.Text.Json;
using Drogecode.Blazor.OfflineSupport.Helpers;
using Drogecode.Blazor.OfflineSupport.Models;
using Microsoft.JSInterop;

namespace Drogecode.Blazor.OfflineSupport;

public class LocalStorageExpireService : ILocalStorageExpireService
{
    private readonly IJsStorageService _jsStorageService;
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _accessorJsRef;

    public LocalStorageExpireService(IJsStorageService jsStorageService, IJSRuntime jsRuntime)
    {
        _jsStorageService = jsStorageService;
        _jsRuntime = jsRuntime;

        //Fire and forget
        Task.Run(DeleteExpiredCache);
    }

    private async Task DeleteExpiredCache()
    {
        try
        {
            //Wait 60 seconds before starting to delete, do not slow down page loading for cleanup.
            for (var i = 0; i < 60; i++)
            {
                await Task.Delay(1000);
            }

            var ttl = DateTime.UtcNow.Ticks;
            var packages = await GetAllLocalStorageItemsAsync();
            if (packages is null)
            {
                ConsoleHelper.WriteLine("No packages found");
                return;
            }

            ConsoleHelper.WriteLine($"Found {packages.Count} packages, current tick {ttl}");
            var count = 0;
            foreach (var package in packages)
            {
                ExpiryStorageModel<object>? expiryStorageModel = null;
                try
                {
                    if (Base64.IsValid(package.Value))
                    {
                        var utf8Byes = Convert.FromBase64String(package.Value);
                        var jsonString = Encoding.UTF8.GetString(utf8Byes);
                        expiryStorageModel = JsonSerializer.Deserialize<ExpiryStorageModel<object>>(jsonString);
                    }
                    else
                    {
                        expiryStorageModel = JsonSerializer.Deserialize<ExpiryStorageModel<object>>(package.Value);
                    }

                    if (expiryStorageModel == null || expiryStorageModel.Ttl == 0) continue;
                }
                catch (JsonException)
                {
                    // Ignore json exceptions
                    continue;
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteLine("Inner exception", ex);
                    continue;
                }

                if (expiryStorageModel.Ttl >= ttl) continue;
                ConsoleHelper.WriteLine($"localstorage deleting {package.Key}, expired {new DateTime(expiryStorageModel.Ttl)}");
                await _jsStorageService.RemoveItem(package.Key, StorageLocation.BrowserLocal);
                count++;
            }

            ConsoleHelper.WriteLine($"{count}/{packages.Count} items where expired and deleted.");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteLine("Outer exception", ex);
        }
        finally
        {
            if (_accessorJsRef is not null)
            {
                try
                {
                    await _accessorJsRef.DisposeAsync();
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteLine("Failed to dispose JS accessor module", ex);
                }
            }
        }
    }

    private async Task<Dictionary<string, string>?> GetAllLocalStorageItemsAsync()
    {
        // Try importing JS module first
        try
        {
            if (_accessorJsRef is null)
            {
                _accessorJsRef = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Drogecode.Blazor.OfflineSupport/js/LocalStorageAccessor.js");
            }

            if (_accessorJsRef is not null)
            {
                return await _accessorJsRef.InvokeAsync<Dictionary<string, string>?>("getAll");
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteLine("Failed to load or execute LocalStorageAccessor.js module, falling back to direct eval", ex);
        }

        // Fallback: direct eval in case module loading was blocked/corrupted by service worker or network issue
        try
        {
            return await _jsRuntime.InvokeAsync<Dictionary<string, string>?>("eval", "Object.assign({}, localStorage)");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteLine("Failed to retrieve localStorage items via fallback eval", ex);
            return null;
        }
    }

    public async ValueTask<T?> GetItemAsync<T>(string key, CancellationToken clt = default)
    {
        var value = await _jsStorageService.RetrieveItem<ExpiryStorageModel<T?>>(key, StorageLocation.BrowserLocal, clt);
        if (value is null || value.Data is null)
            return default(T);
        if (value.Ttl < DateTime.UtcNow.Ticks)
        {
            ConsoleHelper.WriteLine($"localstorage deleting {key}, expired {new DateTime(value.Ttl)} on trying to get");
            await _jsStorageService.RemoveItem(key, StorageLocation.BrowserLocal, clt);
            return default(T);
        }

        var result = value.Data;
        return result;
    }

    public async ValueTask SetItemAsync<T>(string key, T data, DateTime expire, CancellationToken clt = default)
    {
        var value = new ExpiryStorageModel<T>
        {
            Data = data,
            Ttl = expire.Ticks,
        };
        await _jsStorageService.StoreItem(key, StorageLocation.BrowserLocal, value, clt);
    }

    public async Task DeleteItemAsync(string key, CancellationToken clt)
    {
        await _jsStorageService.RemoveItem(key, StorageLocation.BrowserLocal, clt);
    }
}