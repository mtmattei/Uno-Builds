using ClaudeDash.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeDash.Views;

public sealed partial class ProjectsPage : Page
{
    public ProjectsPage()
    {
        this.InitializeComponent();
        var host = App.Current.Host!.Services;
        DataContext = ActivatorUtilities.CreateInstance<BindableProjectsModel>(host);
    }
}
