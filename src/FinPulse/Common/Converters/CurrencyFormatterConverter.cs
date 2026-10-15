using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace FinPulse.Common.Converters;

/// <summary>
/// Formats numerical amounts into localized currency format (e.g. ₹1,00,000.00 or $100,000.00).
/// </summary>
public class CurrencyFormatterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var culture = FinPulse.Services.SettingsService.SharedCurrentCulture ?? CultureInfo.CurrentCulture;
        if (value is decimal dec)
        {
            return dec.ToString("C2", culture);
        }
        if (value is double dbl)
        {
            return dbl.ToString("C2", culture);
        }
        if (value is float flt)
        {
            return flt.ToString("C2", culture);
        }

        return "—";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
