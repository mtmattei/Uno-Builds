using SalesHeatmap.ViewModels;

namespace SalesHeatmap;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; } = new();

    public MainPage()
    {
        this.InitializeComponent();
    }
}
