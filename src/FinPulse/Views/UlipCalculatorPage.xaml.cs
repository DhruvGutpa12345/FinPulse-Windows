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
        this.InitializeComponent();
        ViewModel = ViewModelLocator.UlipCalculatorViewModel;
    }

    private void OnCashFlowValueChanged(object sender, object e)
    {
        ViewModel.Recalculate();
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
