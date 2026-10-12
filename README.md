# FinPulse Windows - Financial Desktop Suite

**FinPulse Windows** is a modern, high-performance financial desktop application designed for Windows 11 and Windows 10, built natively using **WinUI 3**, the **Windows App SDK**, and **.NET 8**.

It features Windows 11 **Fluent Design** (integrated **Mica Alt** backdrop, system typography, interactive dialogs, animated layout transitions) and **LiveChartsCore.SkiaSharpView.WinUI** for fluid financial data visualizations.

---

## 🌟 Key Features

### 1. ULIP & Irregular Cash Flow (XIRR) Analyzer
- **Dual-Phase Hybrid Mathematical Engine (`Services/XirrCalculator.cs`)**:
  - Solves the exact non-linear NPV equation:
    $$\sum_{i=0}^{n-1} \frac{C_i}{(1 + r)^{\frac{d_i - d_0}{365}}} = 0$$
  - **Phase 1: Analytical Newton-Raphson** with exact first-order derivative evaluation for ultra-fast quadratic convergence.
  - **Phase 2: Brent's Method / Bisection Fallback** ensuring guaranteed root finding across wide boundaries ($-99.999\%$ to $+10,000\%$) even for oscillating or flat cash flow polynomials.
  - Automatic handling of same-day transactions, chronological sorting, and leap-year day count conventions.
- **Interactive Cash Flow Entry Table**:
  - Full per-row controls: `CalendarDatePicker`, `NumberBox` with currency spin buttons, `ComboBox` for transaction type, note input, and instant row deletion.
  - **Quick Policy Generator Dialog**: Prefill recurring annual/quarterly/monthly premiums and projected maturities with a single click.
- **Real-Time KPI Metric Cards**:
  - Total Invested (Capital Outflow)
  - Total Realized (Capital Inflow)
  - Net Profit & Absolute Return (%)
  - **Annualized XIRR (%) Hero Badge** with computation method details.
- **Interactive Visualizations (LiveChartsCore)**:
  - **Timeline Distribution**: Outflows (red) vs Inflows (emerald green).
  - **Net Cumulative Curve**: Progression curve illustrating capital accumulation towards maturity.
- **Export & Portability**:
  - Instant one-click CSV report generation.
  - Formatted clipboard copying.

### 2. Architecture & Tech Stack
- **Target Framework:** `net8.0-windows10.0.19041.0`
- **Application Model:** WinUI 3 (Windows App SDK 1.6)
- **MVVM Framework:** `CommunityToolkit.Mvvm` (8.3.2) with source-generated observable properties and relay commands.
- **Charts:** `LiveChartsCore.SkiaSharpView.WinUI` (v2.0.0-rc2)
- **Dependency Injection:** `Microsoft.Extensions.DependencyInjection`
- **Unit Testing:** MSTest suite with `FluentAssertions` verifying calculations against standard Excel test vectors.

---

## 📁 Project Structure

```
FinPulse/
├── FinPulse.sln                              # Solution file
├── README.md                                 # Architecture & getting started guide
├── src/
│   └── FinPulse/
│       ├── FinPulse.csproj                   # WinUI 3 Net8 project
│       ├── App.xaml / App.xaml.cs            # Entry point & DI container
│       ├── MainWindow.xaml / .cs             # Shell with Mica Alt & NavigationView
│       ├── app.manifest                      # DPI awareness & OS compatibility
│       ├── Models/
│       │   ├── FlowType.cs                   # Transaction classification enum
│       │   ├── CashFlowItem.cs               # Observable cash flow item
│       │   ├── CalculationSummary.cs         # Calculated KPIs
│       │   └── QuickGeneratorParams.cs       # Quick prefill configuration
│       ├── Services/
│       │   ├── IXirrCalculator.cs            # Financial calculation contract
│       │   ├── XirrCalculator.cs             # Newton-Raphson & Brent's solver
│       │   └── ExportService.cs              # CSV & Clipboard service
│       ├── ViewModels/
│       │   ├── MainViewModel.cs              # Navigation coordinator
│       │   ├── UlipCalculatorViewModel.cs    # Cash flows, KPIs, and chart series
│       │   └── ViewModelLocator.cs           # Service locator bridge
│       ├── Views/
│       │   ├── UlipCalculatorPage.xaml / .cs # ULIP analyzer view & interaction
│       │   ├── Dialogs/
│       │   │   └── QuickGeneratorDialog.xaml # Quick prefill dialog
│       │   └── Placeholders/                 # Future calculators (SIP, EMI, CAGR)
│       └── Common/Converters/
│           ├── CurrencyFormatterConverter.cs
│           ├── PercentageFormatterConverter.cs
│           └── FlowTypeToBrushConverter.cs
└── tests/
    └── FinPulse.Tests/
        ├── FinPulse.Tests.csproj             # MSTest project
        └── XirrCalculatorTests.cs            # Math accuracy tests vs Excel vectors
```

---

## 🚀 Building & Running

### Prerequisites
1. Windows 10 (version 1809+) or Windows 11.
2. [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
3. Visual Studio 2022 / 2026 with the **.NET Desktop Development** and **Windows App SDK** workloads installed.

### Command Line
```powershell
# Restore and build the solution
dotnet build FinPulse.sln -c Release

# Run Unit Tests
dotnet test tests/FinPulse.Tests/FinPulse.Tests.csproj

# Run FinPulse Desktop App
dotnet run --project src/FinPulse/FinPulse.csproj
```
