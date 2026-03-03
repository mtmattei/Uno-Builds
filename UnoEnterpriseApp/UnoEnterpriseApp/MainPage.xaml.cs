using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using UnoEnterpriseApp.ViewModels;

namespace UnoEnterpriseApp;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        this.InitializeComponent();
        
        // Get ViewModel from DI container
        ViewModel = App.Current.Services.GetRequiredService<MainViewModel>();
        DataContext = ViewModel;
    }
}
