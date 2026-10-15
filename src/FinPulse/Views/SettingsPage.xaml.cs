using FinPulse.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace FinPulse.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel => ViewModelLocator.SettingsViewModel;

    public bool HasStatusMessage => !string.IsNullOrEmpty(ViewModel.StatusMessage);

    public SettingsPage()
    {
        this.InitializeComponent();
        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.StatusMessage))
            {
                Bindings.Update();
            }
        };
    }

    private void OnSettingChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SaveSettingsCommand.Execute(null);
    }

    private void OnNumberBoxChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        ViewModel.SaveSettingsCommand.Execute(null);
    }

    private void OnToggleChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.SaveSettingsCommand.Execute(null);
    }
}
