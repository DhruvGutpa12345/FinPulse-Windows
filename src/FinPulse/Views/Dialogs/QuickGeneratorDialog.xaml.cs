using FinPulse.Models;
using Microsoft.UI.Xaml.Controls;

namespace FinPulse.Views.Dialogs;

public sealed partial class QuickGeneratorDialog : UserControl
{
    public QuickGeneratorDialog()
    {
        this.InitializeComponent();
        StartDatePicker.Date = DateTimeOffset.Now.Date;
        IncludeMaturityCheckBox.Checked += (s, e) => MaturityDetailsPanel.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        IncludeMaturityCheckBox.Unchecked += (s, e) => MaturityDetailsPanel.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
    }

    public QuickGeneratorParams GetParams()
    {
        var freq = FrequencyComboBox.SelectedIndex switch
        {
            1 => RecurrenceFrequency.SemiAnnual,
            2 => RecurrenceFrequency.Quarterly,
            3 => RecurrenceFrequency.Monthly,
            _ => RecurrenceFrequency.Annual
        };

        return new QuickGeneratorParams
        {
            StartDate = StartDatePicker.Date ?? DateTimeOffset.Now.Date,
            InstallmentAmount = (decimal)(double.IsNaN(AmountNumberBox.Value) ? 100000 : AmountNumberBox.Value),
            NumberOfInstallments = (int)(double.IsNaN(InstallmentsNumberBox.Value) ? 10 : InstallmentsNumberBox.Value),
            Frequency = freq,
            FlowType = FlowType.PremiumOutflow,
            IncludeProjectedMaturity = IncludeMaturityCheckBox.IsChecked ?? false,
            MaturityYearsAfterStart = (int)(double.IsNaN(MaturityYearsNumberBox.Value) ? 15 : MaturityYearsNumberBox.Value),
            ProjectedMaturityAmount = (decimal)(double.IsNaN(MaturityAmountNumberBox.Value) ? 2200000 : MaturityAmountNumberBox.Value)
        };
    }
}
