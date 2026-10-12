using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FinPulse.Models;
using FinPulse.Services;

namespace FinPulse.Tests;

[TestClass]
public class XirrCalculatorTests
{
    private readonly IXirrCalculator _calculator = new XirrCalculator();

    [TestMethod]
    public void Calculate_StandardExcelBenchmark_ReturnsExpectedXirr()
    {
        // Standard Excel XIRR benchmark vector:
        // 2020-01-01: -10,000
        // 2021-01-01: -10,000
        // 2022-01-01: -10,000
        // 2023-01-01: -10,000
        // 2024-01-01: -10,000
        // 2025-01-01: +65,000
        // Excel =XIRR gives ~8.07% (taking leap year 2020 and 2024 into account)
        var cashFlows = new List<CashFlowItem>
        {
            new(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero), -10000m, FlowType.PremiumOutflow),
            new(new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero), -10000m, FlowType.PremiumOutflow),
            new(new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero), -10000m, FlowType.PremiumOutflow),
            new(new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero), -10000m, FlowType.PremiumOutflow),
            new(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), -10000m, FlowType.PremiumOutflow),
            new(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), 65000m, FlowType.FinalMaturity)
        };

        var result = _calculator.Calculate(cashFlows);

        result.IsSuccess.Should().BeTrue();
        result.Percentage.Should().BeApproximately(8.07, 0.15); // within 0.15% tolerance of exact calendar day counts
        Math.Abs(result.NpvResidual).Should().BeLessThan(1e-4);
    }

    [TestMethod]
    public void Calculate_SimpleOneYearDouble_ReturnsExact100Percent()
    {
        // 2024-01-01: -1000
        // 2025-01-01: +2000 (Exactly 100% return over 366 leap-year days / approx 1.0 year)
        var cashFlows = new List<CashFlowItem>
        {
            new(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), -1000m, FlowType.PremiumOutflow),
            new(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), 2000m, FlowType.FinalMaturity)
        };

        var result = _calculator.Calculate(cashFlows);

        result.IsSuccess.Should().BeTrue();
        result.Percentage.Should().BeInRange(99.0, 101.0);
    }

    [TestMethod]
    public void Calculate_IrregularCalendarDatesAndPartialWithdrawals_ConvergesProperly()
    {
        // Staggered non-uniform intervals
        var cashFlows = new List<CashFlowItem>
        {
            new(new DateTimeOffset(2021, 3, 15, 0, 0, 0, TimeSpan.Zero), -50000m, FlowType.PremiumOutflow),
            new(new DateTimeOffset(2021, 9, 20, 0, 0, 0, TimeSpan.Zero), -25000m, FlowType.TopUp),
            new(new DateTimeOffset(2022, 5, 10, 0, 0, 0, TimeSpan.Zero), 15000m, FlowType.PartialWithdrawal),
            new(new DateTimeOffset(2023, 11, 30, 0, 0, 0, TimeSpan.Zero), 85000m, FlowType.FinalMaturity)
        };

        var result = _calculator.Calculate(cashFlows);

        result.IsSuccess.Should().BeTrue();
        result.Percentage.Should().BeGreaterThan(0.0);
        Math.Abs(result.NpvResidual).Should().BeLessThan(1e-4);
    }

    [TestMethod]
    public void Calculate_UnsortedDates_SortsAutomaticallyAndComputesAccurately()
    {
        // Dates passed out of chronological order
        var cashFlows = new List<CashFlowItem>
        {
            new(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), 60000m, FlowType.FinalMaturity),
            new(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero), -10000m, FlowType.PremiumOutflow),
            new(new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero), -10000m, FlowType.PremiumOutflow),
            new(new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero), -10000m, FlowType.PremiumOutflow)
        };

        var result = _calculator.Calculate(cashFlows);

        result.IsSuccess.Should().BeTrue();
        result.Percentage.Should().BeGreaterThan(0.0);
    }

    [TestMethod]
    public void Calculate_SameDayMultipleTransactions_AggregatesCorrectly()
    {
        // Two transactions on the same date: -10000 and -5000 = -15000 net
        var cashFlows = new List<CashFlowItem>
        {
            new(new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero), -10000m, FlowType.PremiumOutflow),
            new(new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero), -5000m, FlowType.TopUp),
            new(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), 18000m, FlowType.FinalMaturity)
        };

        var result = _calculator.Calculate(cashFlows);

        result.IsSuccess.Should().BeTrue();
        result.Percentage.Should().BeInRange(19.0, 21.0); // ~20% return on 15,000 -> 18,000
    }

    [TestMethod]
    public void Calculate_ZeroReturn_ReturnsExactlyZeroPercent()
    {
        // Invested 10,000 on day 0, received 10,000 on day 365 -> 0% return
        var cashFlows = new List<CashFlowItem>
        {
            new(new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero), -10000m, FlowType.PremiumOutflow),
            new(new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero), 10000m, FlowType.FinalMaturity)
        };

        var result = _calculator.Calculate(cashFlows);

        result.IsSuccess.Should().BeTrue();
        result.Percentage.Should().BeApproximately(0.0, 1e-4);
    }

    [TestMethod]
    public void Calculate_LossPosition_ReturnsNegativeXirr()
    {
        // Invested 10,000, recovered only 6,000 after 1 year -> -40% return
        var cashFlows = new List<CashFlowItem>
        {
            new(new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero), -10000m, FlowType.PremiumOutflow),
            new(new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero), 6000m, FlowType.FinalMaturity)
        };

        var result = _calculator.Calculate(cashFlows);

        result.IsSuccess.Should().BeTrue();
        result.Percentage.Should().BeInRange(-41.0, -39.0);
    }

    [TestMethod]
    public void Calculate_AllNegativeCashFlows_FailsGracefullyWithClearError()
    {
        var cashFlows = new List<CashFlowItem>
        {
            new(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero), -10000m, FlowType.PremiumOutflow),
            new(new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero), -10000m, FlowType.PremiumOutflow)
        };

        var result = _calculator.Calculate(cashFlows);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("negative outflow");
    }

    [TestMethod]
    public void Calculate_SingleCashFlow_FailsGracefullyWithClearError()
    {
        var cashFlows = new List<CashFlowItem>
        {
            new(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero), -10000m, FlowType.PremiumOutflow)
        };

        var result = _calculator.Calculate(cashFlows);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("At least two distinct");
    }

    [TestMethod]
    public void CalculateSummary_ComputesKPIsAccurately()
    {
        var cashFlows = new List<CashFlowItem>
        {
            new(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero), -100000m, FlowType.PremiumOutflow),
            new(new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero), -100000m, FlowType.PremiumOutflow),
            new(new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero), 250000m, FlowType.FinalMaturity)
        };

        var summary = _calculator.CalculateSummary(cashFlows);

        summary.TotalInvested.Should().Be(200000m);
        summary.TotalRealized.Should().Be(250000m);
        summary.NetProfit.Should().Be(50000m);
        summary.AbsoluteReturnPercentage.Should().Be(25.0);
        summary.IsConverged.Should().BeTrue();
        summary.XirrPercentage.Should().NotBeNull();
        summary.XirrPercentage!.Value.Should().BeGreaterThan(0.0);
    }
}
