using Microsoft.UI.Xaml.Data;

namespace FinPulse.Common.Converters;

/// <summary>
/// Formats percentage numbers with sign and two decimal places.
/// </summary>
public class PercentageFormatterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double dbl)
        {
            if (double.IsNaN(dbl) || double.IsInfinity(dbl)) return "N/A";
            return $"{dbl:+0.00;-0.00;0.00}%";
        }
        if (value is decimal dec)
        {
            return $"{dec:+0.00;-0.00;0.00}%";
        }

        return "N/A";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
