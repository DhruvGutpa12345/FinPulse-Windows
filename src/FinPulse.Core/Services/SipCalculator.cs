using FinPulse.Core.Models;

namespace FinPulse.Core.Services;

public interface ISipCalculator
{
    SipResult Calculate(SipInput input);
}

public class SipCalculator : ISipCalculator
{
    public SipResult Calculate(SipInput input)
    {
        if (input.MonthlyInvestment <= 0 || input.TenureYears <= 0)
        {
            return SipResult.Empty;
        }

        int totalMonths = input.TenureYears * 12;
        decimal monthlyRate = (input.ExpectedAnnualReturn / 100m) / 12m;
        decimal stepUpMultiplier = 1m + (input.AnnualStepUpPercent / 100m);

        decimal currentMonthlyAmount = input.MonthlyInvestment;
        decimal totalInvested = 0m;
        decimal accumulatedValue = 0m;

        var yearlySchedule = new List<SipYearBreakdown>();
        decimal yearInvested = 0m;

        for (int m = 1; m <= totalMonths; m++)
        {
            // Apply annual step up at the start of each year after Year 1
            if (m > 1 && (m - 1) % 12 == 0)
            {
                currentMonthlyAmount = Math.Round(currentMonthlyAmount * stepUpMultiplier, 2);
            }

            totalInvested += currentMonthlyAmount;
            yearInvested += currentMonthlyAmount;

            // Compound previous balance and add new monthly investment with month-end return
            accumulatedValue = (accumulatedValue + currentMonthlyAmount) * (1m + monthlyRate);

            if (m % 12 == 0)
            {
                int year = m / 12;
                decimal roundedAccumulated = Math.Round(accumulatedValue, 2);
                decimal gain = Math.Max(0m, roundedAccumulated - totalInvested);

                yearlySchedule.Add(new SipYearBreakdown(
                    year,
                    yearInvested,
                    totalInvested,
                    roundedAccumulated,
                    gain
                ));

                yearInvested = 0m;
            }
        }

        decimal finalFutureValue = Math.Round(accumulatedValue, 2);
        decimal finalTotalGain = Math.Max(0m, finalFutureValue - totalInvested);
        decimal multiplier = totalInvested > 0 ? Math.Round(finalFutureValue / totalInvested, 2) : 1m;

        return new SipResult(
            totalInvested,
            finalTotalGain,
            finalFutureValue,
            multiplier,
            yearlySchedule
        );
    }
}
