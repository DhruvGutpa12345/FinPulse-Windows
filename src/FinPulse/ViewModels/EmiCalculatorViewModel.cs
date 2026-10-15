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

public partial class EmiCalculatorViewModel : ObservableObject
{
    private readonly IEmiCalculator _emiCalculator;
    private readonly IExportService _exportService;

    [ObservableProperty]
    private decimal _principal = 50_00_000m;

    [ObservableProperty]
    private decimal _annualInterestRate = 8.5m;

    [ObservableProperty]
    private int _tenureYears = 20;

    [ObservableProperty]
    private decimal _extraMonthlyPayment = 0m;

    [ObservableProperty]
    private bool _showYearlyView = true;

    [ObservableProperty]
    private EmiResult _result = EmiResult.Empty;

    [ObservableProperty]
    private ObservableCollection<EmiYearlySummary> _yearlySchedule = new();

    [ObservableProperty]
    private ObservableCollection<EmiPeriodBreakdown> _monthlySchedule = new();

    [ObservableProperty]
    private string _statusMessage = "Ready";

    // Charts
    [ObservableProperty]
    private ISeries[] _pieSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _amortizationSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private LiveChartsCore.Kernel.Sketches.ICartesianAxis[] _amortizationXAxes = Array.Empty<LiveChartsCore.Kernel.Sketches.ICartesianAxis>();

    [ObservableProperty]
    private LiveChartsCore.Kernel.Sketches.ICartesianAxis[] _amortizationYAxes = Array.Empty<LiveChartsCore.Kernel.Sketches.ICartesianAxis>();

    private readonly ISettingsService? _settingsService;

    public EmiCalculatorViewModel(IEmiCalculator emiCalculator, IExportService exportService, ISettingsService? settingsService = null)
    {
        _emiCalculator = emiCalculator;
        _exportService = exportService;
        _settingsService = settingsService;

        if (_settingsService != null && _settingsService.CurrentSettings != null)
        {
            _annualInterestRate = _settingsService.CurrentSettings.DefaultLoanRate;
        }

        Recalculate();
    }

    [RelayCommand]
    public void Recalculate()
    {
        try
        {
            var input = new EmiInput(Principal, AnnualInterestRate, TenureYears, ExtraMonthlyPayment);
            Result = _emiCalculator.Calculate(input);

            YearlySchedule.Clear();
            foreach (var y in Result.YearlySchedule)
            {
                YearlySchedule.Add(y);
            }

            MonthlySchedule.Clear();
            foreach (var m in Result.MonthlySchedule)
            {
                MonthlySchedule.Add(m);
            }

            UpdateCharts();
            StatusMessage = $"EMI amortized across {Result.ActualTenureMonths} months.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Calculation Error: {ex.Message}";
        }
    }

    [RelayCommand]
    public void ApplyPreset(string presetType)
    {
        switch (presetType)
        {
            case "Home":
                Principal = 50_00_000m;
                AnnualInterestRate = 8.5m;
                TenureYears = 20;
                ExtraMonthlyPayment = 0m;
                break;
            case "Car":
                Principal = 10_00_000m;
                AnnualInterestRate = 9.2m;
                TenureYears = 5;
                ExtraMonthlyPayment = 0m;
                break;
            case "Personal":
                Principal = 5_00_000m;
                AnnualInterestRate = 12.5m;
                TenureYears = 3;
                ExtraMonthlyPayment = 0m;
                break;
        }

        Recalculate();
    }

    [RelayCommand]
    public async Task CopySummaryAsync()
    {
        string summaryText =
            $"FinPulse Loan EMI & Amortization Report\n" +
            $"---------------------------------------\n" +
            $"Loan Principal:         {Principal:C0}\n" +
            $"Interest Rate:          {AnnualInterestRate:F2}%\n" +
            $"Loan Tenure:            {TenureYears} Years\n" +
            $"Extra Monthly Payment:  {ExtraMonthlyPayment:C0}\n" +
            $"---------------------------------------\n" +
            $"Monthly EMI:            {Result.MonthlyEmi:C0}\n" +
            $"Total Principal:        {Result.TotalPrincipal:C0}\n" +
            $"Total Interest:         {Result.TotalInterest:C0}\n" +
            $"Total Amount Payable:   {Result.TotalAmount:C0}\n" +
            $"Interest-to-Loan Ratio: {Result.InterestToPrincipalRatio:F1}%\n" +
            (Result.MonthsSaved > 0 ? $"Early Closure: Saved {Result.MonthsSaved} months & {Result.InterestSaved:C0} interest!" : "");

        await _exportService.CopyToClipboardAsync(summaryText);
        StatusMessage = "Loan summary copied to clipboard!";
    }

    [RelayCommand]
    public async Task ExportCsvAsync()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== FINPULSE LOAN AMORTIZATION REPORT ===");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Loan Principal,{Principal:F2}");
        sb.AppendLine($"Interest Rate,{AnnualInterestRate:F2}%");
        sb.AppendLine($"Tenure Years,{TenureYears}");
        sb.AppendLine($"Monthly EMI,{Result.MonthlyEmi:F2}");
        sb.AppendLine($"Total Interest,{Result.TotalInterest:F2}");
        sb.AppendLine($"Total Payable,{Result.TotalAmount:F2}");
        sb.AppendLine();

        sb.AppendLine("Year,Principal Paid,Interest Paid,Total Payment,Ending Balance");
        foreach (var row in Result.YearlySchedule)
        {
            sb.AppendLine($"{row.Year},{row.PrincipalPaid:F2},{row.InterestPaid:F2},{row.TotalPaid:F2},{row.EndingBalance:F2}");
        }

        await _exportService.CopyToClipboardAsync(sb.ToString());
        StatusMessage = "Loan amortization CSV copied to clipboard!";
    }

    private void UpdateCharts()
    {
        // 1. Doughnut: Principal vs Total Interest
        PieSeries = new ISeries[]
        {
            new PieSeries<decimal>
            {
                Name = "Principal Loan Amount",
                Values = new[] { Result.TotalPrincipal },
                Fill = new SolidColorPaint(new SKColor(59, 130, 246)), // Blue
                InnerRadius = 65,
                Stroke = new SolidColorPaint(SKColors.White, 2f)
            },
            new PieSeries<decimal>
            {
                Name = "Total Interest Payable",
                Values = new[] { Result.TotalInterest },
                Fill = new SolidColorPaint(new SKColor(239, 68, 68)), // Red/Coral
                InnerRadius = 65,
                Stroke = new SolidColorPaint(SKColors.White, 2f)
            }
        };

        // 2. Amortization Stacked Column Chart (Yearly Principal vs Interest)
        var principalPoints = new List<ObservablePoint>();
        var interestPoints = new List<ObservablePoint>();

        for (int i = 0; i < Result.YearlySchedule.Count; i++)
        {
            var item = Result.YearlySchedule[i];
            principalPoints.Add(new ObservablePoint(i, (double)item.PrincipalPaid));
            interestPoints.Add(new ObservablePoint(i, (double)item.InterestPaid));
        }

        AmortizationSeries = new ISeries[]
        {
            new StackedColumnSeries<ObservablePoint>
            {
                Name = "Principal Paid",
                Values = principalPoints,
                Fill = new SolidColorPaint(new SKColor(59, 130, 246)),
                Stroke = new SolidColorPaint(new SKColor(59, 130, 246), 1.5f),
                MaxBarWidth = 24,
                Rx = 3,
                Ry = 3
            },
            new StackedColumnSeries<ObservablePoint>
            {
                Name = "Interest Paid",
                Values = interestPoints,
                Fill = new SolidColorPaint(new SKColor(239, 68, 68, 210)),
                Stroke = new SolidColorPaint(new SKColor(239, 68, 68), 1.5f),
                MaxBarWidth = 24,
                Rx = 3,
                Ry = 3
            }
        };

        AmortizationXAxes = new Axis[]
        {
            new Axis
            {
                Labels = Result.YearlySchedule.Select(x => $"Yr {x.Year}").ToArray(),
                LabelsRotation = 25,
                TextSize = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(148, 163, 184))
            }
        };

        AmortizationYAxes = new Axis[]
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
