using ClaudeDash.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeDash.Views;

public sealed partial class TerminalPage : Page
{
    public TerminalPage()
    {
        this.InitializeComponent();
        var host = App.Current.Host!.Services;
        DataContext = ActivatorUtilities.CreateInstance<BindableTerminalModel>(host);
    }
}
