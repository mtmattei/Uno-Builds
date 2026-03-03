using AdTokensIDE.ViewModels;

namespace AdTokensIDE;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel => App.ViewModel;

    public MainPage()
    {
        this.InitializeComponent();
    }
}
