using Microsoft.UI.Xaml.Data;

namespace FinPulse.Common.Converters;

/// <summary>
/// Converts int to double for WinUI 3 Slider and NumberBox two-way binding.
/// </summary>
public class IntToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int i)
        {
            return (double)i;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is double dbl)
        {
            if (double.IsNaN(dbl) || double.IsInfinity(dbl)) return 0;
            return (int)Math.Round(dbl);
        }
        return 0;
    }
}
