using SalesHeatmap.ViewModels;

namespace SalesHeatmap;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; } = new();

    public DashboardPage()
    {
        this.InitializeComponent();
    }
}
