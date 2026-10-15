using System.Globalization;
using System.IO;
using System.Text.Json;

namespace FinPulse.Services;

public class SettingsService : ISettingsService
{
    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FinPulse");

    private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "settings.json");

    private AppSettings _currentSettings;

    public event Action<AppSettings>? SettingsChanged;

    public AppSettings CurrentSettings => _currentSettings;

    public static CultureInfo SharedCurrentCulture { get; private set; } = CultureInfo.GetCultureInfo("en-IN");

    public CultureInfo CurrentCulture => SharedCurrentCulture;

    public SettingsService()
    {
        _currentSettings = LoadSettingsInternal();
        UpdateSharedCulture(_currentSettings.CurrencyCode);
    }

    public void SaveSettings(AppSettings settings)
    {
        _currentSettings = settings;
        UpdateSharedCulture(settings.CurrencyCode);

        try
        {
            if (!Directory.Exists(SettingsDirectory))
            {
                Directory.CreateDirectory(SettingsDirectory);
            }

            string json = JsonSerializer.Serialize(_currentSettings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }
        catch { }

        SettingsChanged?.Invoke(_currentSettings);
    }

    public void ResetToDefaults()
    {
        SaveSettings(new AppSettings());
    }

    private static AppSettings LoadSettingsInternal()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null)
                {
                    return settings;
                }
            }
        }
        catch { }

        return new AppSettings();
    }

    private static void UpdateSharedCulture(string currencyCode)
    {
        SharedCurrentCulture = currencyCode switch
        {
            "USD" => CultureInfo.GetCultureInfo("en-US"),
            "EUR" => CultureInfo.GetCultureInfo("de-DE"),
            "GBP" => CultureInfo.GetCultureInfo("en-GB"),
            "JPY" => CultureInfo.GetCultureInfo("ja-JP"),
            _ => CultureInfo.GetCultureInfo("en-IN")
        };

        try
        {
            CultureInfo.DefaultThreadCurrentCulture = SharedCurrentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = SharedCurrentCulture;
            Thread.CurrentThread.CurrentCulture = SharedCurrentCulture;
            Thread.CurrentThread.CurrentUICulture = SharedCurrentCulture;
        }
        catch { }
    }
}
