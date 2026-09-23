using FieldCheck.ViewModels;
using Microsoft.UI.Xaml.Navigation;

namespace FieldCheck.Views;

public sealed partial class AssetsPage : Page
{
    public AssetsPage()
    {
        ViewModel = App.Services.GetRequiredService<AssetsViewModel>();
        InitializeComponent();
        SizeChanged += (_, _) => UpdateLayoutMode();
    }

    public AssetsViewModel ViewModel { get; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        UpdateLayoutMode();
        _ = ViewModel.EnsureLoadedAsync();
    }

    private void UpdateLayoutMode()
    {
        var width = XamlRoot?.Size.Width ?? ActualWidth;
        ViewModel.IsWide = width >= ShellPage.MasterDetailMinWidth;
    }

    private void OnAssetClick(object sender, RoutedEventArgs e) => ViewModel.OpenAsset(((AssetRow)((FrameworkElement)sender).DataContext).Asset);

    private void OnClearSearch(object sender, RoutedEventArgs e)
    {
        // Take focus off the search box first. On Android the IME commits the box's text when it loses
        // focus, so the clear is queued behind that commit instead of racing it.
        ((Control)sender).Focus(FocusState.Programmatic);
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
        {
            Search.Text = string.Empty;
            ViewModel.ClearSearchCommand.Execute(null);
        });
    }
}
