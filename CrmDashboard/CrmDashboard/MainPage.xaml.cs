using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using CrmDashboard.ViewModels;

namespace CrmDashboard;

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
