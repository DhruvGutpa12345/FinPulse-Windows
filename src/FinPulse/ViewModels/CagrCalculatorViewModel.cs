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

public partial class CagrCalculatorViewModel : ObservableObject
{
    private readonly ICagrCalculator _cagrCalculator;
    private readonly IExportService _exportService;

    // CAGR Mode Inputs
    [ObservableProperty]
    private decimal _initialValue = 100_000m;

    [ObservableProperty]
    private decimal _finalValue = 250_000m;

    [ObservableProperty]
    private decimal _durationYears = 5m;

    [ObservableProperty]
    private CagrResult _cagrResult = CagrResult.Empty;

    // Compounding & Inflation Mode Inputs
    [ObservableProperty]
    private decimal _principal = 100_000m;

    [ObservableProperty]
    private decimal _annualRatePercent = 12.0m;

    [ObservableProperty]
    private int _compoundingTenureYears = 10;

    [ObservableProperty]
    private int _selectedFrequencyIndex = 0; // 0=Annual, 1=Semi, 2=Quarter, 3=Monthly, 4=Daily

    [ObservableProperty]
    private decimal _additionalMonthlyDeposit = 0m;

    [ObservableProperty]
    private decimal _annualInflationRatePercent = 6.0m;

    [ObservableProperty]
    private CompoundInterestResult _compoundResult = CompoundInterestResult.Empty;

    [ObservableProperty]
    private ObservableCollection<CompoundingYearBreakdown> _compoundingSchedule = new();

    [ObservableProperty]
    private string _statusMessage = "Ready";

    // Chart Series
    [ObservableProperty]
    private ISeries[] _trajectorySeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private LiveChartsCore.Kernel.Sketches.ICartesianAxis[] _trajectoryXAxes = Array.Empty<LiveChartsCore.Kernel.Sketches.ICartesianAxis>();

    [ObservableProperty]
    private LiveChartsCore.Kernel.Sketches.ICartesianAxis[] _trajectoryYAxes = Array.Empty<LiveChartsCore.Kernel.Sketches.ICartesianAxis>();

    public CagrCalculatorViewModel(ICagrCalculator cagrCalculator, IExportService exportService)
    {
        _cagrCalculator = cagrCalculator;
        _exportService = exportService;

        Recalculate();
    }

    [RelayCommand]
    public void Recalculate()
    {
        try
        {
            // 1. Recalculate CAGR
            var cagrInput = new CagrInput(InitialValue, FinalValue, DurationYears);
            CagrResult = _cagrCalculator.CalculateCagr(cagrInput);

            // 2. Recalculate Compounding & Inflation
            var frequency = SelectedFrequencyIndex switch
            {
                0 => CompoundingFrequency.Annually,
                1 => CompoundingFrequency.SemiAnnually,
                2 => CompoundingFrequency.Quarterly,
                3 => CompoundingFrequency.Monthly,
                4 => CompoundingFrequency.Daily,
                _ => CompoundingFrequency.Annually
            };

            var compInput = new CompoundInterestInput(
                Principal,
                AnnualRatePercent,
                CompoundingTenureYears,
                frequency,
                AdditionalMonthlyDeposit,
                AnnualInflationRatePercent
            );

            CompoundResult = _cagrCalculator.CalculateCompounding(compInput);

            CompoundingSchedule.Clear();
            foreach (var item in CompoundResult.YearlySchedule)
            {
                CompoundingSchedule.Add(item);
            }

            UpdateCharts();
            StatusMessage = $"CAGR: {CagrResult.CagrPercent}% | Real Return: {CompoundResult.RealRateOfReturnPercent}%";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Calculation Error: {ex.Message}";
        }
    }

    [RelayCommand]
    public void ApplyAssetPreset(string assetType)
    {
        switch (assetType)
        {
            case "Equity":
                AnnualRatePercent = 12.0m;
                AnnualInflationRatePercent = 6.0m;
                break;
            case "RealEstate":
                AnnualRatePercent = 9.0m;
                AnnualInflationRatePercent = 5.5m;
                break;
            case "Gold":
                AnnualRatePercent = 8.5m;
                AnnualInflationRatePercent = 5.5m;
                break;
            case "FixedDeposit":
                AnnualRatePercent = 7.0m;
                AnnualInflationRatePercent = 5.5m;
                break;
        }

        Recalculate();
    }

    [RelayCommand]
    public async Task CopySummaryAsync()
    {
        string summaryText =
            $"FinPulse CAGR & Compounding Report\n" +
            $"------------------------------------\n" +
            $"Initial Value:          {InitialValue:C0}\n" +
            $"Final Value:            {FinalValue:C0}\n" +
            $"Duration:               {DurationYears} Years\n" +
            $"Annualized CAGR:        {CagrResult.CagrPercent:F2}%\n" +
            $"Absolute Growth:        {CagrResult.AbsoluteReturnPercent:F2}%\n" +
            $"Rule of 72 Doubling:    {CagrResult.RuleOf72Years} Years\n" +
            $"------------------------------------\n" +
            $"Nominal 10Y Value:      {CompoundResult.NominalFutureValue:C0}\n" +
            $"Real Purchasing Power:  {CompoundResult.RealFutureValue:C0}\n" +
            $"Real Return (Post-Inf): {CompoundResult.RealRateOfReturnPercent:F2}%";

        await _exportService.CopyToClipboardAsync(summaryText);
        StatusMessage = "CAGR summary copied to clipboard!";
    }

    [RelayCommand]
    public async Task ExportCsvAsync()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== FINPULSE COMPOUNDING & INFLATION REPORT ===");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Principal,{Principal:F2}");
        sb.AppendLine($"Annual Rate,{AnnualRatePercent:F2}%");
        sb.AppendLine($"Inflation Rate,{AnnualInflationRatePercent:F2}%");
        sb.AppendLine($"Nominal Future Value,{CompoundResult.NominalFutureValue:F2}");
        sb.AppendLine($"Real Purchasing Power Value,{CompoundResult.RealFutureValue:F2}");
        sb.AppendLine($"Real Rate of Return,{CompoundResult.RealRateOfReturnPercent:F2}%");
        sb.AppendLine();

        sb.AppendLine("Year,Invested Capital,Nominal Value,Real Purchasing Power,Simple Interest");
        foreach (var row in CompoundResult.YearlySchedule)
        {
            sb.AppendLine($"{row.Year},{row.InvestedAmount:F2},{row.NominalFutureValue:F2},{row.RealInflationAdjustedValue:F2},{row.SimpleInterestValue:F2}");
        }

        await _exportService.CopyToClipboardAsync(sb.ToString());
        StatusMessage = "Compounding schedule CSV copied to clipboard!";
    }

    private void UpdateCharts()
    {
        var nominalPoints = new List<ObservablePoint>();
        var realPoints = new List<ObservablePoint>();
        var simplePoints = new List<ObservablePoint>();

        for (int i = 0; i < CompoundResult.YearlySchedule.Count; i++)
        {
            var item = CompoundResult.YearlySchedule[i];
            nominalPoints.Add(new ObservablePoint(i, (double)item.NominalFutureValue));
            realPoints.Add(new ObservablePoint(i, (double)item.RealInflationAdjustedValue));
            simplePoints.Add(new ObservablePoint(i, (double)item.SimpleInterestValue));
        }

        TrajectorySeries = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Name = "Nominal Compounded Wealth",
                Values = nominalPoints,
                Stroke = new SolidColorPaint(new SKColor(16, 185, 129), 3f), // Emerald Green
                Fill = new SolidColorPaint(new SKColor(16, 185, 129, 25)),
                GeometrySize = 7,
                GeometryFill = new SolidColorPaint(new SKColor(16, 185, 129)),
                GeometryStroke = new SolidColorPaint(SKColors.White, 2f)
            },
            new LineSeries<ObservablePoint>
            {
                Name = "Real Purchasing Power (Inflation Adjusted)",
                Values = realPoints,
                Stroke = new SolidColorPaint(new SKColor(59, 130, 246), 2.5f), // Royal Blue
                Fill = new SolidColorPaint(new SKColor(59, 130, 246, 20)),
                GeometrySize = 7,
                GeometryFill = new SolidColorPaint(new SKColor(59, 130, 246)),
                GeometryStroke = new SolidColorPaint(SKColors.White, 2f)
            },
            new LineSeries<ObservablePoint>
            {
                Name = "Simple Interest Baseline",
                Values = simplePoints,
                Stroke = new SolidColorPaint(new SKColor(245, 158, 11), 2f), // Amber
                Fill = null,
                GeometrySize = 6,
                GeometryFill = new SolidColorPaint(new SKColor(245, 158, 11)),
                GeometryStroke = new SolidColorPaint(SKColors.White, 2f)
            }
        };

        TrajectoryXAxes = new Axis[]
        {
            new Axis
            {
                Labels = CompoundResult.YearlySchedule.Select(x => $"Yr {x.Year}").ToArray(),
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
