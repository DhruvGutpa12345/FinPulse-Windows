using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinPulse.Core.Models;
using FinPulse.Core.Services;
using FinPulse.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace FinPulse.ViewModels;

public partial class SipCalculatorViewModel : ObservableObject
{
    private readonly ISipCalculator _sipCalculator;
    private readonly IExportService _exportService;

    [ObservableProperty]
    private decimal _monthlyInvestment = 10_000m;

    [ObservableProperty]
    private decimal _expectedAnnualReturn = 12.0m;

    [ObservableProperty]
    private int _tenureYears = 10;

    [ObservableProperty]
    private decimal _annualStepUpPercent = 0m;

    [ObservableProperty]
    private SipResult _result = SipResult.Empty;

    [ObservableProperty]
    private ObservableCollection<SipYearBreakdown> _schedule = new();

    [ObservableProperty]
    private string _statusMessage = "Ready";

    // Chart Series
    [ObservableProperty]
    private ISeries[] _pieSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _trajectorySeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private LiveChartsCore.Kernel.Sketches.ICartesianAxis[] _trajectoryXAxes = Array.Empty<LiveChartsCore.Kernel.Sketches.ICartesianAxis>();

    [ObservableProperty]
    private LiveChartsCore.Kernel.Sketches.ICartesianAxis[] _trajectoryYAxes = Array.Empty<LiveChartsCore.Kernel.Sketches.ICartesianAxis>();

    public SipCalculatorViewModel(ISipCalculator sipCalculator, IExportService exportService)
    {
        _sipCalculator = sipCalculator;
        _exportService = exportService;

        Recalculate();
    }

    [RelayCommand]
    public void Recalculate()
    {
        try
        {
            var input = new SipInput(MonthlyInvestment, ExpectedAnnualReturn, TenureYears, AnnualStepUpPercent);
            Result = _sipCalculator.Calculate(input);

            Schedule.Clear();
            foreach (var item in Result.YearlySchedule)
            {
                Schedule.Add(item);
            }

            UpdateCharts();
            StatusMessage = $"SIP projected successfully for {TenureYears} years.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Calculation Error: {ex.Message}";
        }
    }

    [RelayCommand]
    public void SetTenure(int years)
    {
        TenureYears = years;
        Recalculate();
    }

    [RelayCommand]
    public async Task CopySummaryAsync()
    {
        string summaryText =
            $"FinPulse SIP Wealth Report\n" +
            $"-------------------------\n" +
            $"Monthly Investment:    {MonthlyInvestment:C0}\n" +
            $"Expected Return:       {ExpectedAnnualReturn:F1}%\n" +
            $"Investment Tenure:     {TenureYears} Years\n" +
            $"Annual Step-up:        {AnnualStepUpPercent:F1}%\n" +
            $"-------------------------\n" +
            $"Total Invested:        {Result.TotalInvested:C0}\n" +
            $"Estimated Wealth Gain: {Result.TotalGain:C0}\n" +
            $"Future Value:          {Result.FutureValue:C0}\n" +
            $"Wealth Multiplier:     {Result.WealthMultiplier}x";

        await _exportService.CopyToClipboardAsync(summaryText);
        StatusMessage = "SIP summary copied to clipboard!";
    }

    [RelayCommand]
    public async Task ExportCsvAsync()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== FINPULSE SIP WEALTH REPORT ===");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Monthly Investment,{MonthlyInvestment:F2}");
        sb.AppendLine($"Expected Annual Return,{ExpectedAnnualReturn:F2}%");
        sb.AppendLine($"Tenure Years,{TenureYears}");
        sb.AppendLine($"Annual Step-Up,{AnnualStepUpPercent:F2}%");
        sb.AppendLine($"Total Invested,{Result.TotalInvested:F2}");
        sb.AppendLine($"Wealth Gain,{Result.TotalGain:F2}");
        sb.AppendLine($"Future Value,{Result.FutureValue:F2}");
        sb.AppendLine($"Multiplier,{Result.WealthMultiplier}x");
        sb.AppendLine();
        sb.AppendLine("Year,Yearly Investment,Total Invested,Future Value,Wealth Gain");

        foreach (var row in Result.YearlySchedule)
        {
            sb.AppendLine($"{row.Year},{row.YearlyInvestment:F2},{row.CumulativeInvested:F2},{row.FutureValue:F2},{row.WealthGain:F2}");
        }

        await _exportService.CopyToClipboardAsync(sb.ToString());
        StatusMessage = "SIP schedule CSV copied to clipboard!";
    }

    private void UpdateCharts()
    {
        // 1. Doughnut Breakdown: Invested vs Wealth Gain
        PieSeries = new ISeries[]
        {
            new PieSeries<decimal>
            {
                Name = "Total Invested",
                Values = new[] { Result.TotalInvested },
                Fill = new SolidColorPaint(new SKColor(59, 130, 246)), // Royal Blue
                InnerRadius = 65,
                Stroke = new SolidColorPaint(SKColors.White, 2f)
            },
            new PieSeries<decimal>
            {
                Name = "Estimated Wealth Gain",
                Values = new[] { Result.TotalGain },
                Fill = new SolidColorPaint(new SKColor(16, 185, 129)), // Emerald Green
                InnerRadius = 65,
                Stroke = new SolidColorPaint(SKColors.White, 2f)
            }
        };

        // 2. Trajectory Chart
        var investedPoints = new List<ObservablePoint>();
        var valuePoints = new List<ObservablePoint>();

        for (int i = 0; i < Result.YearlySchedule.Count; i++)
        {
            var item = Result.YearlySchedule[i];
            investedPoints.Add(new ObservablePoint(i, (double)item.CumulativeInvested));
            valuePoints.Add(new ObservablePoint(i, (double)item.FutureValue));
        }

        TrajectorySeries = new ISeries[]
        {
            new ColumnSeries<ObservablePoint>
            {
                Name = "Total Invested",
                Values = investedPoints,
                Fill = new SolidColorPaint(new SKColor(59, 130, 246, 180)),
                Stroke = new SolidColorPaint(new SKColor(59, 130, 246), 1.5f),
                MaxBarWidth = 24,
                Rx = 4,
                Ry = 4
            },
            new LineSeries<ObservablePoint>
            {
                Name = "Future Value",
                Values = valuePoints,
                Stroke = new SolidColorPaint(new SKColor(16, 185, 129), 3f),
                Fill = new SolidColorPaint(new SKColor(16, 185, 129, 30)),
                GeometrySize = 7,
                GeometryFill = new SolidColorPaint(new SKColor(16, 185, 129)),
                GeometryStroke = new SolidColorPaint(SKColors.White, 2f)
            }
        };

        TrajectoryXAxes = new Axis[]
        {
            new Axis
            {
                Labels = Result.YearlySchedule.Select(x => $"Yr {x.Year}").ToArray(),
                LabelsRotation = 25,
                TextSize = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(148, 163, 184))
            }
        };

        TrajectoryYAxes = new Axis[]
        {
            new Axis
            {
                Labeler = v => v.ToString("C0"),
                TextSize = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(148, 163, 184))
            }
        };
    }
}
