using FinPulse.Models;
using Windows.ApplicationModel.DataTransfer;

namespace FinPulse.Services;

/// <summary>
/// WinUI 3 desktop export service leveraging Windows DataPackage and Clipboard API.
/// </summary>
public class WindowsExportService : CoreExportService
{
    public override Task CopyToClipboardAsync(string text)
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(text);
        Clipboard.SetContent(dataPackage);
        return Task.CompletedTask;
    }
}
