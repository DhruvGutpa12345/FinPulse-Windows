using Microsoft.Extensions.DependencyInjection;

namespace FinPulse.ViewModels;

public static class ViewModelLocator
{
    private static IServiceProvider? _serviceProvider;

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static MainViewModel MainViewModel =>
        _serviceProvider?.GetRequiredService<MainViewModel>()
        ?? throw new InvalidOperationException("ServiceProvider not initialized.");

    public static UlipCalculatorViewModel UlipCalculatorViewModel =>
        _serviceProvider?.GetRequiredService<UlipCalculatorViewModel>()
        ?? throw new InvalidOperationException("ServiceProvider not initialized.");

    public static SipCalculatorViewModel SipCalculatorViewModel =>
        _serviceProvider?.GetRequiredService<SipCalculatorViewModel>()
        ?? throw new InvalidOperationException("ServiceProvider not initialized.");

    public static EmiCalculatorViewModel EmiCalculatorViewModel =>
        _serviceProvider?.GetRequiredService<EmiCalculatorViewModel>()
        ?? throw new InvalidOperationException("ServiceProvider not initialized.");

    public static CagrCalculatorViewModel CagrCalculatorViewModel =>
        _serviceProvider?.GetRequiredService<CagrCalculatorViewModel>()
        ?? throw new InvalidOperationException("ServiceProvider not initialized.");

    public static SettingsViewModel SettingsViewModel =>
        _serviceProvider?.GetRequiredService<SettingsViewModel>()
        ?? throw new InvalidOperationException("ServiceProvider not initialized.");
}
