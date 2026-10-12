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
    private static readonly SolidColorBrush OutflowBrush = new(Color.FromArgb(255, 239, 68, 68));       // Crimson / Red #EF4444
    private static readonly SolidColorBrush TopUpBrush = new(Color.FromArgb(255, 249, 115, 22));        // Amber / Orange #F97316
    private static readonly SolidColorBrush WithdrawalBrush = new(Color.FromArgb(255, 14, 165, 233));   // Sky Blue #0EA5E9
    private static readonly SolidColorBrush MaturityBrush = new(Color.FromArgb(255, 16, 185, 129));     // Emerald Green #10B981
    private static readonly SolidColorBrush DefaultBrush = new(Color.FromArgb(255, 148, 163, 184));    // Slate Gray #94A3B8

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is FlowType flowType)
        {
            return flowType switch
            {
                FlowType.PremiumOutflow => OutflowBrush,
                FlowType.TopUp => TopUpBrush,
                FlowType.PartialWithdrawal => WithdrawalBrush,
                FlowType.FinalMaturity => MaturityBrush,
                _ => DefaultBrush
            };
        }

        return DefaultBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
