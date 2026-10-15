using System.Globalization;

namespace FinPulse.Services;

public class AppSettings
{
    public int ThemeIndex { get; set; } = 0; // 0: System Default, 1: Light, 2: Dark
    public int BackdropIndex { get; set; } = 0; // 0: Mica Alt, 1: Mica, 2: Acrylic, 3: None
    public string CurrencyCode { get; set; } = "INR"; // INR, USD, EUR, GBP, JPY
    public decimal DefaultReturnRate { get; set; } = 12.0m;
    public decimal DefaultLoanRate { get; set; } = 8.5m;
    public bool AutoSortCashFlows { get; set; } = true;
    public bool IncludeCsvHeaders { get; set; } = true;
    public int DecimalPrecision { get; set; } = 2;
}

public interface ISettingsService
{
    AppSettings CurrentSettings { get; }
    CultureInfo CurrentCulture { get; }
    void SaveSettings(AppSettings settings);
    void ResetToDefaults();
    event Action<AppSettings>? SettingsChanged;
}
