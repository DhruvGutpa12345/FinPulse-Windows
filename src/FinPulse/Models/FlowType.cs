namespace FinPulse.Models;

/// <summary>
/// Identifies the financial transaction flow type within a ULIP or irregular cash flow timeline.
/// </summary>
public enum FlowType
{
    /// <summary>
    /// Regular premium payment or initial investment outflow (negative cash flow).
    /// </summary>
    PremiumOutflow,

    /// <summary>
    /// Additional lump-sum investment or top-up payment (negative cash flow).
    /// </summary>
    TopUp,

    /// <summary>
    /// Partial withdrawal, dividend, or interim cash payout (positive cash flow).
    /// </summary>
    PartialWithdrawal,

    /// <summary>
    /// Final maturity settlement, fund value surrender, or exit redemption (positive cash flow).
    /// </summary>
    FinalMaturity
}
