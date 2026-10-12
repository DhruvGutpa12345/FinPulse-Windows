using FinPulse.Views;
using FinPulse.Views.Placeholders;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace FinPulse;

public sealed partial class MainWindow : Window
{
    private MicaController? _micaController;
    private SystemBackdropConfiguration? _configurationSource;

    public MainWindow()
    {
        this.InitializeComponent();

        // Integrate with custom AppTitleBar
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        // Apply Windows 11 Mica Alt Material
        TrySetMicaAltBackdrop();

        // Default navigation
        ContentFrame.Navigate(typeof(UlipCalculatorPage));
    }

    private bool TrySetMicaAltBackdrop()
    {
        if (!MicaController.IsSupported())
        {
            return false;
        }

        _configurationSource = new SystemBackdropConfiguration();
        this.Activated += OnWindowActivated;
        this.Closed += OnWindowClosed;

        if (Content is FrameworkElement rootElement)
        {
            rootElement.ActualThemeChanged += OnThemeChanged;
            _configurationSource.Theme = rootElement.ActualTheme switch
            {
                ElementTheme.Dark => SystemBackdropTheme.Dark,
                ElementTheme.Light => SystemBackdropTheme.Light,
                _ => SystemBackdropTheme.Default
            };
        }

        _micaController = new MicaController
        {
            Kind = MicaKind.BaseAlt
        };

        // Enable Mica backdrop for WinUI 3 Window
        this.SystemBackdrop = new MicaBackdrop { Kind = MicaKind.BaseAlt };
        return true;
    }

    private void OnWindowActivated(object sender, WindowActivatedEventArgs args)
    {
        if (_configurationSource != null)
        {
            _configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        if (_micaController != null)
        {
            _micaController.Dispose();
            _micaController = null;
        }
        this.Activated -= OnWindowActivated;
        _configurationSource = null;
    }

    private void OnThemeChanged(FrameworkElement sender, object args)
    {
        if (_configurationSource != null)
        {
            _configurationSource.Theme = sender.ActualTheme switch
            {
                ElementTheme.Dark => SystemBackdropTheme.Dark,
                ElementTheme.Light => SystemBackdropTheme.Light,
                _ => SystemBackdropTheme.Default
            };
        }
    }

    private void OnNavViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem selectedItem)
        {
            string tag = selectedItem.Tag?.ToString() ?? "Ulip";
            switch (tag)
            {
                case "Ulip":
                    ContentFrame.Navigate(typeof(UlipCalculatorPage));
                    break;
                case "Sip":
                    ContentFrame.Navigate(typeof(SipCalculatorPage));
                    break;
                case "Emi":
                    ContentFrame.Navigate(typeof(EmiCalculatorPage));
                    break;
                case "Cagr":
                    ContentFrame.Navigate(typeof(CagrCalculatorPage));
                    break;
            }
        }
    }
}
