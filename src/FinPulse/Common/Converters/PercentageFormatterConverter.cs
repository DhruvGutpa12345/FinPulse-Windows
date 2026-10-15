using Microsoft.UI.Xaml.Data;

namespace FinPulse.Common.Converters;

/// <summary>
/// Formats percentage numbers with optional explicit sign and decimal places.
/// </summary>
public class PercentageFormatterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool showSign = parameter?.ToString() == "Sign";
        if (value is double dbl)
        {
            if (double.IsNaN(dbl) || double.IsInfinity(dbl)) return "N/A";
            return showSign ? $"{dbl:+0.00;-0.00;0.00}%" : $"{dbl:F2}%";
        }
        if (value is decimal dec)
        {
            return showSign ? $"{dec:+0.00;-0.00;0.00}%" : $"{dec:F2}%";
        }

        return "N/A";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
