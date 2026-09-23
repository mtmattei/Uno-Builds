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

    private void OnClearSearch(object sender, RoutedEventArgs e)
    {
        // Take focus off the search box first so the platform text input (Android IME) can't push the old text back.
        ((Control)sender).Focus(FocusState.Programmatic);
        HistorySearch.Text = string.Empty;
        ViewModel.ClearSearchCommand.Execute(null);
    }
}
