using Microsoft.UI.Xaml.Input;
using Sanctum.Models;
using Sanctum.Services;
using Sanctum.ViewModels;

namespace Sanctum.Pages;

public sealed partial class DashboardPage : UserControl
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage()
    {
        var appState = App.Services!.GetRequiredService<IAppStateService>();
        var mockData = App.Services!.GetRequiredService<IMockDataService>();
        ViewModel = new DashboardViewModel(appState, mockData);

        this.InitializeComponent();
    }

    private void StatusPill_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border border && border.Tag is SourceControlItem item)
        {
            ViewModel.CycleSourceStatusCommand.Execute(item);
        }
    }
}
