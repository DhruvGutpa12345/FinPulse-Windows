using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinPulse.Services;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace FinPulse.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private int _themeIndex;

    [ObservableProperty]
    private int _backdropIndex;

    [ObservableProperty]
    private int _selectedCurrencyIndex;

    [ObservableProperty]
    private decimal _defaultReturnRate;

    [ObservableProperty]
    private decimal _defaultLoanRate;

    [ObservableProperty]
    private bool _autoSortCashFlows;

    [ObservableProperty]
    private bool _includeCsvHeaders;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public string AppVersion => "1.2.0";
    public string RuntimeInfo => ".NET 8.0 • Windows App SDK 1.6 • WinUI 3";

    private static readonly string[] CurrencyCodes = { "INR", "USD", "EUR", "GBP", "JPY" };

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        LoadFromSettings();
    }

    private void LoadFromSettings()
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

        StatusMessage = "Settings saved successfully!";
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

    public static void ApplyThemeAndBackdrop(int themeIndex, int backdropIndex)
    {
        var window = App.MainWindowInstance;
        if (window == null) return;

        // Apply Theme to Window Content
        if (window.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = themeIndex switch
            {
                1 => ElementTheme.Light,
                2 => ElementTheme.Dark,
                _ => ElementTheme.Default
            };
        }

        // Apply System Backdrop
        try
        {
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
