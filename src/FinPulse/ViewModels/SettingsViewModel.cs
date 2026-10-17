using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinPulse.Services;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace FinPulse.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private bool _isInitializing = false;

    [ObservableProperty]
    private int _themeIndex;

    partial void OnThemeIndexChanged(int value)
    {
        SaveAndApply();
    }

    [ObservableProperty]
    private int _backdropIndex;

    partial void OnBackdropIndexChanged(int value)
    {
        SaveAndApply();
    }

    [ObservableProperty]
    private int _selectedCurrencyIndex;

    partial void OnSelectedCurrencyIndexChanged(int value)
    {
        SaveAndApply();
    }

    [ObservableProperty]
    private decimal _defaultReturnRate;

    partial void OnDefaultReturnRateChanged(decimal value)
    {
        SaveAndApply();
    }

    [ObservableProperty]
    private decimal _defaultLoanRate;

    partial void OnDefaultLoanRateChanged(decimal value)
    {
        SaveAndApply();
    }

    [ObservableProperty]
    private bool _autoSortCashFlows;

    partial void OnAutoSortCashFlowsChanged(bool value)
    {
        SaveAndApply();
    }

    [ObservableProperty]
    private bool _includeCsvHeaders;

    partial void OnIncludeCsvHeadersChanged(bool value)
    {
        SaveAndApply();
    }

    [ObservableProperty]
    private string _formattedSampleCurrency = string.Empty;

    [ObservableProperty]
    private string _activeThemeDescription = string.Empty;

    [ObservableProperty]
    private string _activeBackdropDescription = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public string AppVersion => "1.2.3";
    public string RuntimeInfo => ".NET 8.0 • Windows App SDK 1.6 • WinUI 3";

    private static readonly string[] CurrencyCodes = { "INR", "USD", "EUR", "GBP", "JPY" };

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        LoadFromSettings();
    }

    public void LoadFromSettings()
    {
        _isInitializing = true;
        try
        {
            var settings = _settingsService.CurrentSettings;
            ThemeIndex = settings.ThemeIndex;
            BackdropIndex = settings.BackdropIndex;
            SelectedCurrencyIndex = Array.IndexOf(CurrencyCodes, settings.CurrencyCode) switch
            {
                -1 => 0,
                var idx => idx
            };
            DefaultReturnRate = settings.DefaultReturnRate;
            DefaultLoanRate = settings.DefaultLoanRate;
            AutoSortCashFlows = settings.AutoSortCashFlows;
            IncludeCsvHeaders = settings.IncludeCsvHeaders;
        }
        finally
        {
            _isInitializing = false;
        }

        UpdateLivePreview();
    }

    private void SaveAndApply()
    {
        if (_isInitializing) return;
        SaveSettings();
    }

    [RelayCommand]
    public void SaveSettings()
    {
        var settings = new AppSettings
        {
            ThemeIndex = ThemeIndex,
            BackdropIndex = BackdropIndex,
            CurrencyCode = SelectedCurrencyIndex >= 0 && SelectedCurrencyIndex < CurrencyCodes.Length
                ? CurrencyCodes[SelectedCurrencyIndex]
                : "INR",
            DefaultReturnRate = DefaultReturnRate,
            DefaultLoanRate = DefaultLoanRate,
            AutoSortCashFlows = AutoSortCashFlows,
            IncludeCsvHeaders = IncludeCsvHeaders
        };

        _settingsService.SaveSettings(settings);
        ApplyThemeAndBackdrop(ThemeIndex, BackdropIndex);
        UpdateLivePreview();

        StatusMessage = "Settings applied and saved!";
    }

    [RelayCommand]
    public void ResetDefaults()
    {
        _settingsService.ResetToDefaults();
        LoadFromSettings();
        ApplyThemeAndBackdrop(ThemeIndex, BackdropIndex);
        StatusMessage = "Settings restored to system defaults.";
    }

    [RelayCommand]
    public void OpenGitHub()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/DhruvGutpa12345/FinPulse-Windows",
                UseShellExecute = true
            });
        }
        catch { }
    }

    [RelayCommand]
    public void ClearLogs()
    {
        try
        {
            string crashLog = Path.Combine(Path.GetTempPath(), "FinPulse_Crash.log");
            if (File.Exists(crashLog))
            {
                File.Delete(crashLog);
            }
            StatusMessage = "Diagnostics cache and temporary logs cleared.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Unable to clear logs: {ex.Message}";
        }
    }

    public void UpdateLivePreview()
    {
        var culture = _settingsService.CurrentCulture;
        FormattedSampleCurrency = (1250000m).ToString("C2", culture);
        ActiveThemeDescription = ThemeIndex switch
        {
            1 => "Light Theme Active",
            2 => "Dark Theme Active",
            _ => "Windows Default Theme Active"
        };
        ActiveBackdropDescription = BackdropIndex switch
        {
            0 => "Mica Alt (Windows 11 Tabbed Material)",
            1 => "Mica (Windows 11 Subtle Translucent)",
            2 => "Desktop Acrylic (Glass Blur Effect)",
            _ => "Solid Background (No Translucency)"
        };
    }

    public static void ApplyThemeAndBackdrop(int themeIndex, int backdropIndex)
    {
        var window = App.MainWindowInstance;
        if (window == null) return;

        // 1. Apply Theme to Window Content
        if (window.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = themeIndex switch
            {
                1 => ElementTheme.Light,
                2 => ElementTheme.Dark,
                _ => ElementTheme.Default
            };
        }

        // 2. Update Title Bar Caption Button Colors
        try
        {
            if (AppWindowTitleBar.IsCustomizationSupported() && window.AppWindow?.TitleBar != null)
            {
                var titleBar = window.AppWindow.TitleBar;
                bool isDark = themeIndex == 2 || (themeIndex == 0 && Application.Current.RequestedTheme == ApplicationTheme.Dark);
                var fg = isDark
                    ? Windows.UI.Color.FromArgb(255, 255, 255, 255)
                    : Windows.UI.Color.FromArgb(255, 0, 0, 0);
                titleBar.ButtonForegroundColor = fg;
                titleBar.ButtonHoverForegroundColor = fg;
            }
        }
        catch { }

        // 3. Apply System Backdrop Material
        try
        {
            window.SystemBackdrop = null;
            window.SystemBackdrop = backdropIndex switch
            {
                0 => MicaController.IsSupported() ? new MicaBackdrop { Kind = MicaKind.BaseAlt } : null,
                1 => MicaController.IsSupported() ? new MicaBackdrop { Kind = MicaKind.Base } : null,
                2 => DesktopAcrylicController.IsSupported() ? new DesktopAcrylicBackdrop() : null,
                _ => null
            };
        }
        catch { }
    }
}
