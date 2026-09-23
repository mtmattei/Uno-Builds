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
        // Take focus off the search box first. On Android the IME commits the box's text when it loses
        // focus, so the clear is queued behind that commit instead of racing it.
        ((Control)sender).Focus(FocusState.Programmatic);
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
        {
            HistorySearch.Text = string.Empty;
            ViewModel.ClearSearchCommand.Execute(null);
        });
    }
}
