using FinPulse.Views;
using FinPulse.Views.Placeholders;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace FinPulse;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();

        // Integrate with custom AppTitleBar
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        // Apply Windows 11 Mica Alt Material via modern WinUI 3 Window.SystemBackdrop
        this.SystemBackdrop = new MicaBackdrop { Kind = MicaKind.BaseAlt };

        // Default navigation
        ContentFrame.Navigate(typeof(UlipCalculatorPage));
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
