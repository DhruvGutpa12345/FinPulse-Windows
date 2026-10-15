using FinPulse.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace FinPulse.Views;

public sealed partial class EmiCalculatorPage : Page
{
    public EmiCalculatorViewModel ViewModel { get; }

    public EmiCalculatorPage()
    {
        ViewModel = ViewModelLocator.EmiCalculatorViewModel;
        this.InitializeComponent();
    }

    private void OnInputValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        ViewModel?.Recalculate();
    }

    private void OnSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        ViewModel?.Recalculate();
    }

    private void OnHomeLoanPreset(object sender, RoutedEventArgs e) => ViewModel.ApplyPreset("Home");
    private void OnCarLoanPreset(object sender, RoutedEventArgs e) => ViewModel.ApplyPreset("Car");
    private void OnPersonalLoanPreset(object sender, RoutedEventArgs e) => ViewModel.ApplyPreset("Personal");
}
