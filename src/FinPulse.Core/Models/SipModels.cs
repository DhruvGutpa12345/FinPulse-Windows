namespace FinPulse.Core.Models;

public record SipInput(
    decimal MonthlyInvestment,
    decimal ExpectedAnnualReturn,
    int TenureYears,
    decimal AnnualStepUpPercent = 0m
);

public record SipYearBreakdown(
    int Year,
    decimal YearlyInvestment,
    decimal CumulativeInvested,
    decimal FutureValue,
    decimal WealthGain
);

public record SipResult(
    decimal TotalInvested,
    decimal TotalGain,
    decimal FutureValue,
    decimal WealthMultiplier,
    IReadOnlyList<SipYearBreakdown> YearlySchedule
)
{
    public static SipResult Empty => new(0m, 0m, 0m, 1m, Array.Empty<SipYearBreakdown>());
}
