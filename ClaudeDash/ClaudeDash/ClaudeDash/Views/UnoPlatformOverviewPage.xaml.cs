using ClaudeDash.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeDash.Views;

public sealed partial class UnoPlatformOverviewPage : Page
{
    public UnoPlatformOverviewPage()
    {
        this.InitializeComponent();
        var host = App.Current.Host!.Services;
        DataContext = ActivatorUtilities.CreateInstance<BindableUnoPlatformOverviewModel>(host);
    }
}
