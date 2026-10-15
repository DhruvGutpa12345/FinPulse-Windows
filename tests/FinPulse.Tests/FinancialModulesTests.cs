using FinPulse.Core.Models;
using FinPulse.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FinPulse.Tests;

[TestClass]
public class FinancialModulesTests
{
    private readonly ISipCalculator _sipCalculator = new SipCalculator();
    private readonly IEmiCalculator _emiCalculator = new EmiCalculator();
    private readonly ICagrCalculator _cagrCalculator = new CagrCalculator();

    #region SIP Tests

    [TestMethod]
    public void Sip_Standard10Year_CalculatesAccurateWealth()
    {
        // ₹10,000 / month, 12% annual return, 10 years
        var input = new SipInput(10_000m, 12.0m, 10, 0m);
        var result = _sipCalculator.Calculate(input);

        Assert.AreEqual(1200000m, result.TotalInvested);
        // Formula: 10000 * ((1+0.01)^120 - 1) / 0.01 * 1.01 = ~2323391
        Assert.IsTrue(result.FutureValue >= 2300000m && result.FutureValue <= 2350000m,
            $"Expected ~23.23L, but got {result.FutureValue}");
        Assert.AreEqual(result.TotalGain, result.FutureValue - result.TotalInvested);
        Assert.AreEqual(10, result.YearlySchedule.Count);
    }

    [TestMethod]
    public void Sip_WithAnnualStepUp_IncreasesTotalInvestmentAndCorpus()
    {
        var regularInput = new SipInput(10_000m, 12.0m, 5, 0m);
        var stepUpInput = new SipInput(10_000m, 12.0m, 5, 10m); // 10% annual step-up

        var regularResult = _sipCalculator.Calculate(regularInput);
        var stepUpResult = _sipCalculator.Calculate(stepUpInput);

        Assert.IsTrue(stepUpResult.TotalInvested > regularResult.TotalInvested);
        Assert.IsTrue(stepUpResult.FutureValue > regularResult.FutureValue);
    }

    [TestMethod]
    public void Sip_ZeroReturn_YieldsZeroGain()
    {
        var input = new SipInput(5_000m, 0m, 3, 0m);
        var result = _sipCalculator.Calculate(input);

        Assert.AreEqual(180000m, result.TotalInvested);
        Assert.AreEqual(180000m, result.FutureValue);
        Assert.AreEqual(0m, result.TotalGain);
    }

    #endregion

    #region EMI Tests

    [TestMethod]
    public void Emi_StandardHomeLoan_CalculatesAccurateEmiAndAmortization()
    {
        // ₹50,00,000 at 8.5% for 20 years
        var input = new EmiInput(50_00_000m, 8.5m, 20, 0m);
        var result = _emiCalculator.Calculate(input);

        // Standard Home Loan EMI for 50L @ 8.5% 20Y is ~₹43,391
        Assert.IsTrue(result.MonthlyEmi >= 43300m && result.MonthlyEmi <= 43500m,
            $"Expected EMI ~43391, but got {result.MonthlyEmi}");

        Assert.AreEqual(240, result.MonthlySchedule.Count);
        Assert.AreEqual(20, result.YearlySchedule.Count);

        // Ending balance of the last month must be 0
        Assert.AreEqual(0m, result.MonthlySchedule[^1].ClosingBalance);
        Assert.AreEqual(50_00_000m, result.TotalPrincipal);
        Assert.IsTrue(result.TotalInterest > 0m);
    }

    [TestMethod]
    public void Emi_WithPrepayment_ReducesTenureAndSavesInterest()
    {
        var standard = _emiCalculator.Calculate(new EmiInput(30_00_000m, 9.0m, 15, 0m));
        var withExtra = _emiCalculator.Calculate(new EmiInput(30_00_000m, 9.0m, 15, 5_000m)); // ₹5000 extra per month

        Assert.IsTrue(withExtra.ActualTenureMonths < standard.ActualTenureMonths);
        Assert.IsTrue(withExtra.MonthsSaved > 0);
        Assert.IsTrue(withExtra.InterestSaved > 0m);
    }

    [TestMethod]
    public void Emi_ZeroInterest_DividesPrincipalEqually()
    {
        var input = new EmiInput(120_000m, 0m, 1, 0m);
        var result = _emiCalculator.Calculate(input);

        Assert.AreEqual(10_000m, result.MonthlyEmi);
        Assert.AreEqual(0m, result.TotalInterest);
        Assert.AreEqual(120_000m, result.TotalAmount);
    }

    #endregion

    #region CAGR & Compounding Tests

    [TestMethod]
    public void Cagr_StandardDoubling_MatchesTheoreticalRate()
    {
        // ₹100,000 doubling to ₹200,000 in 5 years -> CAGR is (2)^(1/5) - 1 = 14.87%
        var input = new CagrInput(100_000m, 200_000m, 5m);
        var result = _cagrCalculator.CalculateCagr(input);

        Assert.AreEqual(14.87m, result.CagrPercent);
        Assert.AreEqual(100.0m, result.AbsoluteReturnPercent);
        Assert.AreEqual(100_000m, result.TotalGrowth);
        Assert.IsTrue(result.RuleOf72Years > 4.5m && result.RuleOf72Years < 5.0m);
    }

    [TestMethod]
    public void Compounding_HigherFrequency_YieldsHigherFutureValue()
    {
        var annualInput = new CompoundInterestInput(100_000m, 10.0m, 5, CompoundingFrequency.Annually);
        var monthlyInput = new CompoundInterestInput(100_000m, 10.0m, 5, CompoundingFrequency.Monthly);

        var annualResult = _cagrCalculator.CalculateCompounding(annualInput);
        var monthlyResult = _cagrCalculator.CalculateCompounding(monthlyInput);

        Assert.IsTrue(monthlyResult.NominalFutureValue > annualResult.NominalFutureValue);
    }

    [TestMethod]
    public void Compounding_RealReturn_AdjustsForInflationViaFisherEquation()
    {
        // 12% nominal return with 6% inflation -> real return is (1.12 / 1.06) - 1 = 5.66%
        var input = new CompoundInterestInput(100_000m, 12.0m, 10, CompoundingFrequency.Annually, 0m, 6.0m);
        var result = _cagrCalculator.CalculateCompounding(input);

        Assert.AreEqual(5.66m, result.RealRateOfReturnPercent);
        Assert.IsTrue(result.RealFutureValue < result.NominalFutureValue);
        Assert.AreEqual(10, result.YearlySchedule.Count);
    }

    #endregion
}
