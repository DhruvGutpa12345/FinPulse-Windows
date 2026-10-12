using FinPulse.Models;

namespace FinPulse.Services;

/// <summary>
/// Result returned by the XIRR calculation engine.
/// </summary>
public record XirrResult
{
    public double Rate { get; init; }
    public double Percentage => Rate * 100.0;
    public bool IsSuccess { get; init; }
    public string MethodUsed { get; init; } = string.Empty;
    public int Iterations { get; init; }
    public double NpvResidual { get; init; }
    public string? ErrorMessage { get; init; }

    public static XirrResult Failure(string message) => new()
    {
        Rate = double.NaN,
        IsSuccess = false,
        MethodUsed = "Failed",
        Iterations = 0,
        NpvResidual = double.NaN,
        ErrorMessage = message
    };

    public static XirrResult Success(double rate, string method, int iterations, double residual) => new()
    {
        Rate = rate,
        IsSuccess = true,
        MethodUsed = method,
        Iterations = iterations,
        NpvResidual = residual,
        ErrorMessage = null
    };
}

public interface IXirrCalculator
{
    /// <summary>
    /// Computes the exact annualized internal rate of return (XIRR) for irregular cash flows.
    /// Uses Newton-Raphson with an automatic robust Brent's method / bisection fallback.
    /// </summary>
    /// <param name="items">Sequence of cash flow transactions.</param>
    /// <param name="guess">Optional initial guess rate (e.g., 0.10 for 10%).</param>
    /// <returns>Calculation result containing rate, convergence status, and metadata.</returns>
    XirrResult Calculate(IEnumerable<CashFlowItem> items, double? guess = null);

    /// <summary>
    /// Computes aggregate financial summary (Invested, Realized, Absolute Return, XIRR).
    /// </summary>
    CalculationSummary CalculateSummary(IEnumerable<CashFlowItem> items);
}
