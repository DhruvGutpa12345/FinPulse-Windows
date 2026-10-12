using Microsoft.UI.Xaml.Data;

namespace FinPulse.Common.Converters;

/// <summary>
/// Converts decimal amount to double for WinUI 3 NumberBox.Value two-way binding.
/// </summary>
public class DecimalToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal dec)
        {
            return (double)dec;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is double dbl)
        {
            if (double.IsNaN(dbl) || double.IsInfinity(dbl)) return 0m;
            return (decimal)dbl;
        }
        return 0m;
    }
}
