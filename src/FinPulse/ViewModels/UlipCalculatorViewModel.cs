using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinPulse.Models;
using FinPulse.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace FinPulse.ViewModels;

public partial class UlipCalculatorViewModel : ObservableObject
{
    private readonly IXirrCalculator _xirrCalculator;
    private readonly IExportService _exportService;

    [ObservableProperty]
    private ObservableCollection<CashFlowItem> _cashFlows = new();

    [ObservableProperty]
    private CalculationSummary _summary = CalculationSummary.Empty;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _hasError;

    // Chart Series
    [ObservableProperty]
    private ISeries[] _timelineSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private LiveChartsCore.Kernel.Sketches.ICartesianAxis[] _timelineXAxes = Array.Empty<LiveChartsCore.Kernel.Sketches.ICartesianAxis>();

    [ObservableProperty]
    private LiveChartsCore.Kernel.Sketches.ICartesianAxis[] _timelineYAxes = Array.Empty<LiveChartsCore.Kernel.Sketches.ICartesianAxis>();

    [ObservableProperty]
    private ISeries[] _cumulativeSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private LiveChartsCore.Kernel.Sketches.ICartesianAxis[] _cumulativeXAxes = Array.Empty<LiveChartsCore.Kernel.Sketches.ICartesianAxis>();

    [ObservableProperty]
    private LiveChartsCore.Kernel.Sketches.ICartesianAxis[] _cumulativeYAxes = Array.Empty<LiveChartsCore.Kernel.Sketches.ICartesianAxis>();

    public UlipCalculatorViewModel(IXirrCalculator xirrCalculator, IExportService exportService)
    {
        _xirrCalculator = xirrCalculator;
        _exportService = exportService;

        LoadDefaultSampleData();
        Recalculate();
    }

    /// <summary>
    /// Loads realistic 10-year ULIP policy with ₹1,00,000 annual premium and ₹20,50,000 maturity.
    /// </summary>
    private void LoadDefaultSampleData()
    {
        CashFlows.Clear();
        var baseDate = DateTimeOffset.Now.Date.AddYears(-10);

        // 10 annual premiums
        for (int i = 0; i < 10; i++)
        {
            CashFlows.Add(new CashFlowItem(
                baseDate.AddYears(i),
                -100_000m,
                FlowType.PremiumOutflow,
                $"Policy Year {i + 1} Premium"
            ));
        }

        // Partial withdrawal at year 7
        CashFlows.Add(new CashFlowItem(
            baseDate.AddYears(7).AddMonths(3),
            50_000m,
            FlowType.PartialWithdrawal,
            "Partial Child Education Withdrawal"
        ));

        // Final maturity at year 10
        CashFlows.Add(new CashFlowItem(
            baseDate.AddYears(10),
            20_50_000m,
            FlowType.FinalMaturity,
            "10-Year Final Policy Maturity"
        ));
    }

    [RelayCommand]
    public void AddTransaction()
    {
        var lastDate = CashFlows.Count > 0
            ? CashFlows.Max(x => x.Date).AddYears(1)
            : DateTimeOffset.Now.Date;

        var newItem = new CashFlowItem(lastDate, -100_000m, FlowType.PremiumOutflow, "New Premium Payment");
        CashFlows.Add(newItem);
        Recalculate();
    }

    [RelayCommand]
    public void AddMaturityPayout()
    {
        var lastDate = CashFlows.Count > 0
            ? CashFlows.Max(x => x.Date).AddYears(1)
            : DateTimeOffset.Now.Date;

        var newItem = new CashFlowItem(lastDate, 500_000m, FlowType.FinalMaturity, "Maturity / Fund Payout");
        CashFlows.Add(newItem);
        Recalculate();
    }

    [RelayCommand]
    public void RemoveTransaction(CashFlowItem item)
    {
        if (item != null && CashFlows.Contains(item))
        {
            CashFlows.Remove(item);
            Recalculate();
        }
    }

    [RelayCommand]
    public void ClearAll()
    {
        CashFlows.Clear();
        Recalculate();
    }

    [RelayCommand]
    public void ResetSample()
    {
        LoadDefaultSampleData();
        Recalculate();
    }

    [RelayCommand]
    public void Recalculate()
    {
        try
        {
            Summary = _xirrCalculator.CalculateSummary(CashFlows);
            HasError = !Summary.IsConverged && CashFlows.Count >= 2;
            StatusMessage = Summary.IsConverged
                ? $"XIRR calculated successfully via {Summary.ConvergenceMethod}."
                : (Summary.ErrorMessage ?? "Awaiting sufficient cash flows.");

            UpdateCharts();
        }
        catch (Exception ex)
        {
            HasError = true;
            StatusMessage = $"Calculation Error: {ex.Message}";
        }
    }

    [RelayCommand]
    public void ApplyQuickGenerator(QuickGeneratorParams parameters)
    {
        if (parameters == null) return;

        CashFlows.Clear();
        DateTimeOffset current = parameters.StartDate;

        for (int i = 0; i < parameters.NumberOfInstallments; i++)
        {
            decimal amt = -Math.Abs(parameters.InstallmentAmount);
            string note = $"{parameters.NotePrefix} #{i + 1}";
            CashFlows.Add(new CashFlowItem(current, amt, parameters.FlowType, note));

            current = parameters.Frequency switch
            {
                RecurrenceFrequency.Annual => current.AddYears(1),
                RecurrenceFrequency.SemiAnnual => current.AddMonths(6),
                RecurrenceFrequency.Quarterly => current.AddMonths(3),
                RecurrenceFrequency.Monthly => current.AddMonths(1),
                _ => current.AddYears(1)
            };
        }

        if (parameters.IncludeProjectedMaturity && parameters.ProjectedMaturityAmount > 0)
        {
            DateTimeOffset maturityDate = parameters.StartDate.AddYears(parameters.MaturityYearsAfterStart);
            CashFlows.Add(new CashFlowItem(
                maturityDate,
                Math.Abs(parameters.ProjectedMaturityAmount),
                FlowType.FinalMaturity,
                $"Projected Final Maturity ({parameters.MaturityYearsAfterStart} Yrs)"
            ));
        }

        Recalculate();
    }

    [RelayCommand]
    public async Task CopySummaryAsync()
    {
        string summaryText =
            $"FinPulse XIRR Report\n" +
            $"--------------------\n" +
            $"Total Invested: {Summary.TotalInvested:C2}\n" +
            $"Total Realized: {Summary.TotalRealized:C2}\n" +
            $"Net Profit:     {Summary.NetProfit:C2}\n" +
            $"Absolute Return: {Summary.AbsoluteReturnPercentage:F2}%\n" +
            $"Annualized XIRR: {(Summary.XirrPercentage.HasValue ? Summary.XirrPercentage.Value.ToString("F2") + "%" : "N/A")}\n" +
            $"Calculation Method: {Summary.ConvergenceMethod}";

        await _exportService.CopyToClipboardAsync(summaryText);
        StatusMessage = "Summary copied to clipboard!";
    }

    [RelayCommand]
    public async Task ExportCsvAsync()
    {
        string csv = _exportService.GenerateCsv(CashFlows, Summary);
        await _exportService.CopyToClipboardAsync(csv);
        StatusMessage = "Cash flow CSV copied to clipboard!";
    }

    private void UpdateCharts()
    {
        var sorted = CashFlows.OrderBy(x => x.Date).ToList();
        if (sorted.Count == 0)
        {
            TimelineSeries = Array.Empty<ISeries>();
            CumulativeSeries = Array.Empty<ISeries>();
            return;
        }

        // 1. Build Timeline Bar Chart Series
        var outflowPoints = new List<ObservablePoint>();
        var inflowPoints = new List<ObservablePoint>();

        for (int i = 0; i < sorted.Count; i++)
        {
            var item = sorted[i];
            if (item.Amount < 0)
            {
                outflowPoints.Add(new ObservablePoint(i, (double)item.Amount));
            }
            else
            {
                inflowPoints.Add(new ObservablePoint(i, (double)item.Amount));
            }
        }

        TimelineSeries = new ISeries[]
        {
            new ColumnSeries<ObservablePoint>
            {
                Name = "Outflows / Premiums",
                Values = outflowPoints,
                Fill = new SolidColorPaint(new SKColor(239, 68, 68, 220)), // Soft Red
                Stroke = new SolidColorPaint(new SKColor(239, 68, 68), 1.5f),
                MaxBarWidth = 28,
                Rx = 4,
                Ry = 4
            },
            new ColumnSeries<ObservablePoint>
            {
                Name = "Inflows / Maturity",
                Values = inflowPoints,
                Fill = new SolidColorPaint(new SKColor(16, 185, 129, 220)), // Emerald Green
                Stroke = new SolidColorPaint(new SKColor(16, 185, 129), 1.5f),
                MaxBarWidth = 28,
                Rx = 4,
                Ry = 4
            }
        };

        TimelineXAxes = new Axis[]
        {
            new Axis
            {
                Labels = sorted.Select(x => x.Date.ToString("MMM yyyy")).ToArray(),
                LabelsRotation = 35,
                TextSize = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(148, 163, 184))
            }
        };

        TimelineYAxes = new Axis[]
        {
            new Axis
            {
                Labeler = v => v.ToString("C0"),
                TextSize = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(148, 163, 184))
            }
        };

        // 2. Build Cumulative Progression Curve
        var cumulativePoints = new List<ObservablePoint>();
        double runningSum = 0;

        for (int i = 0; i < sorted.Count; i++)
        {
            runningSum += (double)sorted[i].Amount;
            cumulativePoints.Add(new ObservablePoint(i, runningSum));
        }

        CumulativeSeries = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Name = "Net Cumulative Position",
                Values = cumulativePoints,
                Stroke = new SolidColorPaint(new SKColor(59, 130, 246), 3f), // Royal Blue
                Fill = new SolidColorPaint(new SKColor(59, 130, 246, 35)),
                GeometrySize = 8,
                GeometryFill = new SolidColorPaint(new SKColor(59, 130, 246)),
                GeometryStroke = new SolidColorPaint(SKColors.White, 2f)
            }
        };

        CumulativeXAxes = new Axis[]
        {
            new Axis
            {
                Labels = sorted.Select(x => x.Date.ToString("yyyy")).ToArray(),
                LabelsRotation = 25,
                TextSize = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(148, 163, 184))
            }
        };

        CumulativeYAxes = new Axis[]
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
