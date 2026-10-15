using FinPulse.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace FinPulse.Views;

public sealed partial class SipCalculatorPage : Page
{
    public SipCalculatorViewModel ViewModel { get; }

    public SipCalculatorPage()
    {
        ViewModel = ViewModelLocator.SipCalculatorViewModel;
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

    private void OnPreset5Years(object sender, RoutedEventArgs e) => ViewModel.SetTenure(5);
    private void OnPreset10Years(object sender, RoutedEventArgs e) => ViewModel.SetTenure(10);
    private void OnPreset15Years(object sender, RoutedEventArgs e) => ViewModel.SetTenure(15);
    private void OnPreset20Years(object sender, RoutedEventArgs e) => ViewModel.SetTenure(20);
    private void OnPreset25Years(object sender, RoutedEventArgs e) => ViewModel.SetTenure(25);
}
