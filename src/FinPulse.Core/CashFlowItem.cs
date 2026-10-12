using CommunityToolkit.Mvvm.ComponentModel;

namespace FinPulse.Models;

/// <summary>
/// Represents an individual cash flow transaction within an investment timeline.
/// Negative amounts represent investments/outflows; positive amounts represent payouts/inflows.
/// </summary>
public partial class CashFlowItem : ObservableObject
{
    private static long _idCounter;

    public long Id { get; } = Interlocked.Increment(ref _idCounter);

    [ObservableProperty]
    private DateTimeOffset _date = DateTimeOffset.Now.Date;

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private FlowType _flowType = FlowType.PremiumOutflow;

    [ObservableProperty]
    private string _notes = string.Empty;

    public CashFlowItem()
    {
    }

    public CashFlowItem(DateTimeOffset date, decimal amount, FlowType flowType, string notes = "")
    {
        Date = date;
        Amount = amount;
        FlowType = flowType;
        Notes = notes;
    }

    /// <summary>
    /// Auto-adjusts the mathematical sign of Amount based on the FlowType convention.
    /// Premium and TopUp are negative outflows; PartialWithdrawal and FinalMaturity are positive inflows.
    /// </summary>
    partial void OnFlowTypeChanged(FlowType value)
    {
        decimal abs = Math.Abs(Amount);
        if (value is FlowType.PremiumOutflow or FlowType.TopUp)
        {
            if (Amount > 0)
            {
                Amount = -abs;
            }
        }
        else
        {
            if (Amount < 0)
            {
                Amount = abs;
            }
        }
    }

    /// <summary>
    /// Convenience property indicating if this transaction is an outflow.
    /// </summary>
    public bool IsOutflow => Amount < 0;

    /// <summary>
    /// Convenience property indicating if this transaction is an inflow.
    /// </summary>
    public bool IsInflow => Amount > 0;
}
