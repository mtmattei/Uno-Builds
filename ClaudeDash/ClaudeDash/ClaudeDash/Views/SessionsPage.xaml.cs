using ClaudeDash.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeDash.Views;

public sealed partial class SessionsPage : Page
{
    public SessionsPage()
    {
        this.InitializeComponent();
        var host = App.Current.Host!.Services;
        DataContext = ActivatorUtilities.CreateInstance<BindableSessionsModel>(host);
    }
}
