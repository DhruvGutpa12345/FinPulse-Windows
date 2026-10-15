using FinPulse.Core.Models;

namespace FinPulse.Core.Services;

public interface ICagrCalculator
{
    CagrResult CalculateCagr(CagrInput input);
    CompoundInterestResult CalculateCompounding(CompoundInterestInput input);
}

public class CagrCalculator : ICagrCalculator
{
    public CagrResult CalculateCagr(CagrInput input)
    {
        if (input.InitialValue <= 0m || input.FinalValue <= 0m || input.DurationYears <= 0m)
        {
            return CagrResult.Empty;
        }

        double v0 = (double)input.InitialValue;
        double vt = (double)input.FinalValue;
        double t = (double)input.DurationYears;

        double cagrDouble = Math.Pow(vt / v0, 1.0 / t) - 1.0;
        decimal cagrPercent = Math.Round((decimal)cagrDouble * 100m, 2);

        decimal absoluteReturn = Math.Round(((input.FinalValue - input.InitialValue) / input.InitialValue) * 100m, 2);
        decimal totalGrowth = input.FinalValue - input.InitialValue;

        decimal ruleOf72 = cagrPercent > 0m ? Math.Round(72m / cagrPercent, 1) : 0m;

        return new CagrResult(cagrPercent, absoluteReturn, totalGrowth, ruleOf72);
    }

    public CompoundInterestResult CalculateCompounding(CompoundInterestInput input)
    {
        if (input.Principal < 0m || input.TenureYears <= 0)
        {
            return CompoundInterestResult.Empty;
        }

        int periodsPerYear = (int)input.Frequency;
        decimal ratePerPeriod = (input.AnnualRatePercent / 100m) / periodsPerYear;
        decimal inflationRate = input.AnnualInflationRatePercent / 100m;

        // Fisher Equation: (1 + nominal) = (1 + real)(1 + inflation) => real = (1 + nominal)/(1 + inflation) - 1
        decimal realRateOfReturn = inflationRate > -1m
            ? Math.Round(((1m + (input.AnnualRatePercent / 100m)) / (1m + inflationRate) - 1m) * 100m, 2)
            : input.AnnualRatePercent;

        decimal currentBalance = input.Principal;
        decimal totalInvested = input.Principal;

        var yearlySchedule = new List<CompoundingYearBreakdown>();

        for (int y = 1; y <= input.TenureYears; y++)
        {
            // Monthly contributions if any
            for (int m = 1; m <= 12; m++)
            {
                if (input.AdditionalMonthlyDeposit > 0m)
                {
                    totalInvested += input.AdditionalMonthlyDeposit;
                    currentBalance += input.AdditionalMonthlyDeposit;
                }

                // If monthly or daily frequency aligns
                if (periodsPerYear >= 12)
                {
                    int subPeriods = periodsPerYear / 12;
                    for (int s = 0; s < subPeriods; s++)
                    {
                        currentBalance *= (1m + ratePerPeriod);
                    }
                }
            }

            // If compounding is annual, semi-annual, or quarterly
            if (periodsPerYear < 12)
            {
                for (int p = 0; p < periodsPerYear; p++)
                {
                    currentBalance *= (1m + ratePerPeriod);
                }
            }

            decimal nominalValue = Math.Round(currentBalance, 2);

            // Discount nominal value by cumulative inflation to find real purchasing power
            double inflationFactor = Math.Pow(1.0 + (double)inflationRate, y);
            decimal realValue = inflationFactor > 0.0
                ? Math.Round(nominalValue / (decimal)inflationFactor, 2)
                : nominalValue;

            // Simple Interest baseline: Principal * (1 + r * t)
            decimal simpleInterest = Math.Round(input.Principal * (1m + (input.AnnualRatePercent / 100m) * y), 2);

            yearlySchedule.Add(new CompoundingYearBreakdown(
                y,
                totalInvested,
                nominalValue,
                realValue,
                simpleInterest
            ));
        }

        decimal finalNominal = Math.Round(currentBalance, 2);
        decimal totalInterest = Math.Max(0m, finalNominal - totalInvested);
        double finalInflationFactor = Math.Pow(1.0 + (double)inflationRate, input.TenureYears);
        decimal finalReal = finalInflationFactor > 0.0
            ? Math.Round(finalNominal / (decimal)finalInflationFactor, 2)
            : finalNominal;

        return new CompoundInterestResult(
            totalInvested,
            finalNominal,
            totalInterest,
            finalReal,
            realRateOfReturn,
            yearlySchedule
        );
    }
}
