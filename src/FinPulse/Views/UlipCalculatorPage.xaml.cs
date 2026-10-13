using FinPulse.Models;
using FinPulse.ViewModels;
using FinPulse.Views.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FinPulse.Views;

public sealed partial class UlipCalculatorPage : Page
{
    public UlipCalculatorViewModel ViewModel { get; }

    public UlipCalculatorPage()
    {
        ViewModel = ViewModelLocator.UlipCalculatorViewModel;
        this.InitializeComponent();
    }

    private void OnCashFlowValueChanged(object sender, object e)
    {
        ViewModel.Recalculate();
    }

    private void OnFlowTypeComboLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is ComboBox cb && cb.DataContext is CashFlowItem item)
        {
            cb.SelectedIndex = item.FlowType switch
            {
                FlowType.PremiumOutflow => 0,
                FlowType.TopUp => 1,
                FlowType.PartialWithdrawal => 2,
                FlowType.FinalMaturity => 3,
                _ => 0
            };
        }
    }

    private void OnFlowTypeComboSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox cb && cb.DataContext is CashFlowItem item)
        {
            item.FlowType = cb.SelectedIndex switch
            {
                0 => FlowType.PremiumOutflow,
                1 => FlowType.TopUp,
                2 => FlowType.PartialWithdrawal,
                3 => FlowType.FinalMaturity,
                _ => FlowType.PremiumOutflow
            };
            ViewModel.Recalculate();
        }
    }

    private void OnDeleteRowClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is CashFlowItem item)
        {
            ViewModel.RemoveTransaction(item);
        }
    }

    private async void OnQuickPrefillClicked(object sender, RoutedEventArgs e)
    {
        var dialogContent = new QuickGeneratorDialog();
        var dialog = new ContentDialog
        {
            Title = "Quick Policy Generator",
            Content = dialogContent,
            PrimaryButtonText = "Generate Cash Flows",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var parameters = dialogContent.GetParams();
            ViewModel.ApplyQuickGenerator(parameters);
        }
    }
}
