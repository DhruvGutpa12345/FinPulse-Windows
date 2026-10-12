namespace FinPulse.Models;

/// <summary>
/// Summary KPI results calculated from a set of cash flow items.
/// </summary>
public record CalculationSummary
{
    public decimal TotalInvested { get; init; }
    public decimal TotalRealized { get; init; }
    public decimal NetProfit { get; init; }
    public double AbsoluteReturnPercentage { get; init; }
    public double? XirrPercentage { get; init; }
    public bool IsConverged { get; init; }
    public string ConvergenceMethod { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }

    public static CalculationSummary Empty => new()
    {
        TotalInvested = 0m,
        TotalRealized = 0m,
        NetProfit = 0m,
        AbsoluteReturnPercentage = 0.0,
        XirrPercentage = null,
        IsConverged = false,
        ConvergenceMethod = "None",
        ErrorMessage = "No data"
    };
}
