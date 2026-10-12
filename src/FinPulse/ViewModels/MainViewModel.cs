using CommunityToolkit.Mvvm.ComponentModel;

namespace FinPulse.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _selectedPageTag = "Ulip";

    [ObservableProperty]
    private string _pageTitle = "ULIP & Irregular Cash Flow (XIRR) Analyzer";

    public void OnNavigationChanged(string tag)
    {
        SelectedPageTag = tag;
        PageTitle = tag switch
        {
            "Ulip" => "ULIP & Irregular Cash Flow (XIRR) Analyzer",
            "Sip" => "SIP (Systematic Investment Plan) Wealth Calculator",
            "Emi" => "Loan & EMI Amortization Calculator",
            "Cagr" => "CAGR & Lump-Sum Compounding Calculator",
            _ => "Financial Analyzer"
        };
    }
}
