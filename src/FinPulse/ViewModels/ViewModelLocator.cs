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
}
