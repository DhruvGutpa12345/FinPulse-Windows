using FinPulse.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace FinPulse.Views;

public sealed partial class CagrCalculatorPage : Page
{
    public CagrCalculatorViewModel ViewModel { get; }

    public CagrCalculatorPage()
    {
        ViewModel = ViewModelLocator.CagrCalculatorViewModel;
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

    private void OnFrequencyChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel?.Recalculate();
    }

    private void OnEquityPreset(object sender, RoutedEventArgs e) => ViewModel.ApplyAssetPreset("Equity");
    private void OnRealEstatePreset(object sender, RoutedEventArgs e) => ViewModel.ApplyAssetPreset("RealEstate");
    private void OnGoldPreset(object sender, RoutedEventArgs e) => ViewModel.ApplyAssetPreset("Gold");
    private void OnFdPreset(object sender, RoutedEventArgs e) => ViewModel.ApplyAssetPreset("FixedDeposit");
}
