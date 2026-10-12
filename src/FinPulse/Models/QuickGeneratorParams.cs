namespace FinPulse.Models;

public enum RecurrenceFrequency
{
    Annual,
    SemiAnnual,
    Quarterly,
    Monthly
}

/// <summary>
/// Parameters passed to the Quick Generator dialog for recurring investments.
/// </summary>
public class QuickGeneratorParams
{
    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.Now.Date;
    public decimal InstallmentAmount { get; set; } = 100_000m;
    public int NumberOfInstallments { get; set; } = 10;
    public RecurrenceFrequency Frequency { get; set; } = RecurrenceFrequency.Annual;
    public FlowType FlowType { get; set; } = FlowType.PremiumOutflow;
    public string NotePrefix { get; set; } = "Policy Year";

    // Optional projected maturity parameters
    public bool IncludeProjectedMaturity { get; set; } = true;
    public int MaturityYearsAfterStart { get; set; } = 15;
    public decimal ProjectedMaturityAmount { get; set; } = 2_200_000m;
}
