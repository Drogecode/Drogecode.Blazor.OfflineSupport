using System.Diagnostics.CodeAnalysis;
using ExpireStorage.Demo.Enums;
using ExpireStorage.Demo.Models;
using ExpireStorage.Demo.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace ExpireStorage.Demo.Layout;

public sealed partial class Theming : IDisposable
{
    [Inject, NotNull] private ILocalUserSettingService? LocalUserSettingService { get; set; }
    [Parameter, EditorRequired, NotNull] public MudThemeProvider? MudThemeProvider { get; set; }
    [Parameter] public EventCallback<bool> IsDarkModeChanged { get; set; }
    [Parameter] public EventCallback<DarkLightMode> DarkModeToggleChanged { get; set; }

    private LocalUserSettings? _localUserSettings;
    private DotNetObjectReference<Theming>? _dotNetHelper;
    private readonly CancellationTokenSource _cls = new();
    private DateTime _lastVisibilityChange = DateTime.UtcNow;
    private bool _watchStarted;

    [Parameter]
    public bool IsDarkMode
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            if (IsDarkModeChanged.HasDelegate)
            {
                IsDarkModeChanged.InvokeAsync(value);
            }
        }
    }

    [Parameter]
    public DarkLightMode DarkModeToggle
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            RefreshMe();
            if (_localUserSettings == null)
            {
                return;
            }

            _localUserSettings.DarkLightMode = field;
            LocalUserSettingService.SetDarkLight(field);
            if (DarkModeToggleChanged.HasDelegate)
            {
                DarkModeToggleChanged.InvokeAsync(value);
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _localUserSettings = await LocalUserSettingService.GetLocalUserSettings();
            DarkModeToggle = _localUserSettings.DarkLightMode;
            switch (DarkModeToggle)
            {
                case DarkLightMode.System:
                    IsDarkMode = await MudThemeProvider.GetSystemDarkModeAsync();
                    await StartWatch();
                    break;
                case DarkLightMode.Light:
                    IsDarkMode = false;
                    break;
                case DarkLightMode.Dark:
                    IsDarkMode = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public async Task ToggleDarkLight()
    {
        var oldValue = IsDarkMode;
        switch (DarkModeToggle)
        {
            case DarkLightMode.System:
                DarkModeToggle = DarkLightMode.Light;
                IsDarkMode = false;
                break;
            case DarkLightMode.Light:
                DarkModeToggle = DarkLightMode.Dark;
                IsDarkMode = true;
                break;
            case DarkLightMode.Dark:
                DarkModeToggle = DarkLightMode.System;
                IsDarkMode = await MudThemeProvider.GetSystemDarkModeAsync();
                await StartWatch();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task StartWatch()
    {
        if (!_watchStarted)
        {
            _watchStarted = true;
            await MudThemeProvider.WatchSystemDarkModeAsync(OnSystemPreferenceChanged);
        }
    }

    [JSInvokable]
    public async Task VisibilityChange(string newState, bool isIos)
    {
        if (string.Compare(newState, "visible", StringComparison.InvariantCulture) != 0)
            return;
        await OnSystemPreferenceChanged(await MudThemeProvider.GetSystemDarkModeAsync()); // always check dark/light on reopen
        if (_lastVisibilityChange.AddMinutes(3).CompareTo(DateTime.UtcNow) > 0)
            return;
        _lastVisibilityChange = DateTime.UtcNow;
    }

    public async Task OnSystemPreferenceChanged(bool newValue)
    {
        if (DarkModeToggle == DarkLightMode.System)
        {
            if (newValue == IsDarkMode) return;
            IsDarkMode = newValue;
            RefreshMe();
        }
    }

    private void RefreshMe()
    {
        StateHasChanged();
    }

    public void Dispose()
    {
        _cls.Cancel();
    }
}