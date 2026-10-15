using FinPulse.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace FinPulse.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public bool HasStatusMessage => !string.IsNullOrEmpty(ViewModel.StatusMessage);

    public SettingsPage()
    {
        ViewModel = ViewModelLocator.SettingsViewModel;
        ViewModel.LoadFromSettings();
        this.InitializeComponent();

        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.StatusMessage))
            {
                Bindings.Update();
            }
        };
    }
}
