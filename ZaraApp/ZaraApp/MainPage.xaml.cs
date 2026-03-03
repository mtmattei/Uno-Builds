using ZaraApp.ViewModels;

namespace ZaraApp;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        this.InitializeComponent();
        ViewModel = new MainViewModel();
    }
}
