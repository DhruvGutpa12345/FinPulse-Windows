namespace FinPulse.Core.Models;

public enum CompoundingFrequency
{
    Annually = 1,
    SemiAnnually = 2,
    Quarterly = 4,
    Monthly = 12,
    Daily = 365
}

public record CagrInput(
    decimal InitialValue,
    decimal FinalValue,
    decimal DurationYears
);

public record CagrResult(
    decimal CagrPercent,
    decimal AbsoluteReturnPercent,
    decimal TotalGrowth,
    decimal RuleOf72Years
)
{
    public static CagrResult Empty => new(0m, 0m, 0m, 0m);
}

public record CompoundInterestInput(
    decimal Principal,
    decimal AnnualRatePercent,
    int TenureYears,
    CompoundingFrequency Frequency = CompoundingFrequency.Annually,
    decimal AdditionalMonthlyDeposit = 0m,
    decimal AnnualInflationRatePercent = 0m
);

public record CompoundingYearBreakdown(
    int Year,
    decimal InvestedAmount,
    decimal NominalFutureValue,
    decimal RealInflationAdjustedValue,
    decimal SimpleInterestValue
);

public record CompoundInterestResult(
    decimal TotalInvested,
    decimal NominalFutureValue,
    decimal TotalNominalInterest,
    decimal RealFutureValue,
    decimal RealRateOfReturnPercent,
    IReadOnlyList<CompoundingYearBreakdown> YearlySchedule
)
{
    public static CompoundInterestResult Empty => new(0m, 0m, 0m, 0m, 0m, Array.Empty<CompoundingYearBreakdown>());
}
