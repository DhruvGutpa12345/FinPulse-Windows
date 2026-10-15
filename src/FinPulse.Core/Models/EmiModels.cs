namespace FinPulse.Core.Models;

public record EmiInput(
    decimal Principal,
    decimal AnnualInterestRate,
    int TenureYears,
    decimal ExtraMonthlyPayment = 0m
);

public record EmiPeriodBreakdown(
    int PeriodNumber,
    int Year,
    int Month,
    decimal OpeningBalance,
    decimal Emi,
    decimal PrincipalPaid,
    decimal InterestPaid,
    decimal ClosingBalance
);

public record EmiYearlySummary(
    int Year,
    decimal PrincipalPaid,
    decimal InterestPaid,
    decimal TotalPaid,
    decimal EndingBalance
);

public record EmiResult(
    decimal MonthlyEmi,
    decimal TotalPrincipal,
    decimal TotalInterest,
    decimal TotalAmount,
    decimal InterestToPrincipalRatio,
    int ActualTenureMonths,
    int MonthsSaved,
    decimal InterestSaved,
    IReadOnlyList<EmiPeriodBreakdown> MonthlySchedule,
    IReadOnlyList<EmiYearlySummary> YearlySchedule
)
{
    public static EmiResult Empty => new(0m, 0m, 0m, 0m, 0m, 0, 0, 0m, Array.Empty<EmiPeriodBreakdown>(), Array.Empty<EmiYearlySummary>());
}
