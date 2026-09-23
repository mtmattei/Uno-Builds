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

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _ = ViewModel.EnsureLoadedAsync();

        // Keep initial focus on the page instead of the search box: focusing a TextBox would open the
        // Android soft keyboard every time the user returns to this list.
        DispatcherQueue.TryEnqueue(() => Focus(FocusState.Programmatic));
    }
}
