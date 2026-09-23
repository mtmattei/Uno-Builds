using FieldCheck.ViewModels;
using Microsoft.UI.Xaml.Navigation;

namespace FieldCheck.Views;

public sealed partial class HistoryPage : Page
{
    public HistoryPage()
    {
        ViewModel = App.Services.GetRequiredService<HistoryViewModel>();
        InitializeComponent();
    }

    public HistoryViewModel ViewModel { get; }

    protected override void OnNavigatedTo(NavigationEventArgs e) => _ = ViewModel.EnsureLoadedAsync();
}
