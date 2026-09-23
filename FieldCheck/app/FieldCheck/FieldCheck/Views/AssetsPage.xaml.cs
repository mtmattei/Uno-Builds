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

        // Keep initial focus on the page instead of the search box: focusing a TextBox would open the
        // Android soft keyboard every time the user returns to this list.
        DispatcherQueue.TryEnqueue(() => Focus(FocusState.Programmatic));
    }

    private void UpdateLayoutMode()
    {
        var width = XamlRoot?.Size.Width ?? ActualWidth;
        ViewModel.IsWide = width >= ShellPage.MasterDetailMinWidth;
    }

    private void OnAssetClick(object sender, RoutedEventArgs e) => ViewModel.OpenAsset(((AssetRow)((FrameworkElement)sender).DataContext).Asset);
}
