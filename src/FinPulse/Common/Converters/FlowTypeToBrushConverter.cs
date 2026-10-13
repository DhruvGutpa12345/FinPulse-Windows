using FinPulse.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace FinPulse.Common.Converters;

/// <summary>
/// Returns intuitive Fluent accent colors corresponding to cash flow types:
/// Coral/Red for Outflows (Premium/TopUp), Emerald Green for Inflows (Withdrawal/Maturity).
/// </summary>
public class FlowTypeToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is FlowType flowType)
        {
            return flowType switch
            {
                FlowType.PremiumOutflow => new SolidColorBrush(Color.FromArgb(255, 239, 68, 68)),
                FlowType.TopUp => new SolidColorBrush(Color.FromArgb(255, 249, 115, 22)),
                FlowType.PartialWithdrawal => new SolidColorBrush(Color.FromArgb(255, 14, 165, 233)),
                FlowType.FinalMaturity => new SolidColorBrush(Color.FromArgb(255, 16, 185, 129)),
                _ => new SolidColorBrush(Color.FromArgb(255, 148, 163, 184))
            };
        }

        return new SolidColorBrush(Color.FromArgb(255, 148, 163, 184));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
