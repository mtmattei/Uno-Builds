using ListHold.ViewModels;

namespace ListHold;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        this.InitializeComponent();
        ViewModel = new MainViewModel();
        ItemsRepeater.ItemsSource = ViewModel.Items;
    }
}
