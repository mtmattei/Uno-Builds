using ClaudeDash.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeDash.Views;

public sealed partial class DevicesPage : Page
{
    public DevicesPage()
    {
        this.InitializeComponent();
        var host = App.Current.Host!.Services;
        DataContext = ActivatorUtilities.CreateInstance<BindableDevicesModel>(host);
    }
}
