using ClaudeDash.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeDash.Views;

public sealed partial class RalphLoopsPage : Page
{
    public RalphLoopsPage()
    {
        this.InitializeComponent();
        var host = App.Current.Host!.Services;
        DataContext = ActivatorUtilities.CreateInstance<BindableRalphLoopsModel>(host);
    }
}
