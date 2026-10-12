using System.Text;
using FinPulse.Models;

namespace FinPulse.Services;

public interface IExportService
{
    string GenerateCsv(IEnumerable<CashFlowItem> items, CalculationSummary summary);
    Task CopyToClipboardAsync(string text);
}

public class CoreExportService : IExportService
{
    public string GenerateCsv(IEnumerable<CashFlowItem> items, CalculationSummary summary)
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== FINPULSE XIRR CALCULATION REPORT ===");
        sb.AppendLine($"Generated Date: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Total Invested,{summary.TotalInvested:F2}");
        sb.AppendLine($"Total Realized,{summary.TotalRealized:F2}");
        sb.AppendLine($"Net Profit / Gain,{summary.NetProfit:F2}");
        sb.AppendLine($"Absolute Return (%),{summary.AbsoluteReturnPercentage:F2}%");
        sb.AppendLine($"Annualized XIRR (%),{(summary.XirrPercentage.HasValue ? summary.XirrPercentage.Value.ToString("F2") + "%" : "N/A")}");
        sb.AppendLine($"Calculation Engine,{summary.ConvergenceMethod}");
        sb.AppendLine();

        sb.AppendLine("Id,Date,Transaction Type,Amount,Notes");
        foreach (var item in items.OrderBy(x => x.Date))
        {
            string cleanNotes = (item.Notes ?? string.Empty).Replace("\"", "\"\"");
            sb.AppendLine($"{item.Id},{item.Date:yyyy-MM-dd},{item.FlowType},{item.Amount:F2},\"{cleanNotes}\"");
        }

        return sb.ToString();
    }

    public virtual Task CopyToClipboardAsync(string text)
    {
        return Task.CompletedTask;
    }
}
