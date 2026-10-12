using FinPulse.Services;
using FinPulse.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace FinPulse;

public partial class App : Application
{
    private Window? _mainWindow;

    public static IServiceProvider Services { get; private set; } = null!;

    public App()
    {
        this.InitializeComponent();

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        ViewModelLocator.Initialize(Services);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Core Mathematical & Infrastructure Services
        services.AddSingleton<IXirrCalculator, XirrCalculator>();
        services.AddSingleton<IExportService, ExportService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<UlipCalculatorViewModel>();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow();
        _mainWindow.Activate();
    }
}
