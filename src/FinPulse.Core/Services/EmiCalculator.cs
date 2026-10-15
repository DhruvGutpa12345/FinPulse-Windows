using FinPulse.Core.Models;

namespace FinPulse.Core.Services;

public interface IEmiCalculator
{
    EmiResult Calculate(EmiInput input);
}

public class EmiCalculator : IEmiCalculator
{
    public EmiResult Calculate(EmiInput input)
    {
        if (input.Principal <= 0 || input.TenureYears <= 0)
        {
            return EmiResult.Empty;
        }

        int originalMonths = input.TenureYears * 12;
        decimal monthlyRate = (input.AnnualInterestRate / 100m) / 12m;

        decimal standardEmi;
        if (monthlyRate == 0m)
        {
            standardEmi = Math.Round(input.Principal / originalMonths, 2);
        }
        else
        {
            double r = (double)monthlyRate;
            double factor = Math.Pow(1.0 + r, originalMonths);
            double emiDouble = (double)input.Principal * r * factor / (factor - 1.0);
            standardEmi = Math.Round((decimal)emiDouble, 2);
        }

        // Calculate baseline total interest without extra payment
        decimal baselineInterest = CalculateTotalInterest(input.Principal, monthlyRate, standardEmi, originalMonths, 0m);

        // Calculate actual amortization schedule with optional extra payment
        decimal balance = input.Principal;
        decimal totalInterestPaid = 0m;
        decimal totalPrincipalPaid = 0m;

        var monthlySchedule = new List<EmiPeriodBreakdown>();
        var yearlySummaries = new List<EmiYearlySummary>();

        decimal yearPrincipal = 0m;
        decimal yearInterest = 0m;

        int month = 0;
        while (balance > 0.01m && month < originalMonths * 2)
        {
            month++;
            decimal openingBalance = balance;
            decimal interest = Math.Round(openingBalance * monthlyRate, 2);

            // On the final scheduled month without prepayment, adjust principal to absorb penny/paise rounding
            decimal availablePrincipal = (input.ExtraMonthlyPayment == 0m && month == originalMonths)
                ? openingBalance
                : standardEmi - interest + input.ExtraMonthlyPayment;

            decimal principalPaid = Math.Min(openingBalance, Math.Max(0m, availablePrincipal));

            decimal actualEmiPaid = principalPaid + interest;
            balance = Math.Max(0m, openingBalance - principalPaid);

            totalInterestPaid += interest;
            totalPrincipalPaid += principalPaid;
            yearPrincipal += principalPaid;
            yearInterest += interest;

            int currentYear = (month - 1) / 12 + 1;
            int currentMonthInYear = (month - 1) % 12 + 1;

            monthlySchedule.Add(new EmiPeriodBreakdown(
                month,
                currentYear,
                currentMonthInYear,
                openingBalance,
                actualEmiPaid,
                principalPaid,
                interest,
                balance
            ));

            if (month % 12 == 0 || balance <= 0.01m)
            {
                yearlySummaries.Add(new EmiYearlySummary(
                    currentYear,
                    yearPrincipal,
                    yearInterest,
                    yearPrincipal + yearInterest,
                    balance
                ));
                yearPrincipal = 0m;
                yearInterest = 0m;
            }
        }

        int actualMonths = month;
        int monthsSaved = Math.Max(0, originalMonths - actualMonths);
        decimal interestSaved = Math.Max(0m, baselineInterest - totalInterestPaid);
        decimal totalAmount = totalPrincipalPaid + totalInterestPaid;
        decimal ratio = totalPrincipalPaid > 0m ? Math.Round((totalInterestPaid / totalPrincipalPaid) * 100m, 1) : 0m;

        return new EmiResult(
            standardEmi,
            totalPrincipalPaid,
            totalInterestPaid,
            totalAmount,
            ratio,
            actualMonths,
            monthsSaved,
            interestSaved,
            monthlySchedule,
            yearlySummaries
        );
    }

    private static decimal CalculateTotalInterest(decimal principal, decimal monthlyRate, decimal emi, int maxMonths, decimal extraPayment)
    {
        decimal balance = principal;
        decimal totalInterest = 0m;
        int m = 0;

        while (balance > 0.01m && m < maxMonths)
        {
            m++;
            decimal interest = Math.Round(balance * monthlyRate, 2);
            decimal availablePrincipal = emi - interest + extraPayment;
            decimal principalPaid = Math.Min(balance, Math.Max(0m, availablePrincipal));
            balance -= principalPaid;
            totalInterest += interest;
        }

        return totalInterest;
    }
}
