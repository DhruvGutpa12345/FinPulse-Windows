using System.IO;
using FinPulse.Core.Services;
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
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();
        ViewModelLocator.Initialize(Services);

        this.UnhandledException += (sender, e) =>
        {
            try
            {
                string logPath = Path.Combine(Path.GetTempPath(), "FinPulse_Crash.log");
                File.WriteAllText(logPath, $"[{DateTime.Now:O}] UnhandledException: {e.Message}\n{e.Exception}");
            }
            catch { }
            e.Handled = true;
        };

        this.InitializeComponent();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Core Mathematical & Infrastructure Services
        services.AddSingleton<IXirrCalculator, XirrCalculator>();
        services.AddSingleton<ISipCalculator, SipCalculator>();
        services.AddSingleton<IEmiCalculator, EmiCalculator>();
        services.AddSingleton<ICagrCalculator, CagrCalculator>();
        services.AddSingleton<IExportService, WindowsExportService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<UlipCalculatorViewModel>();
        services.AddTransient<SipCalculatorViewModel>();
        services.AddTransient<EmiCalculatorViewModel>();
        services.AddTransient<CagrCalculatorViewModel>();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow();
        _mainWindow.Activate();
    }
}
