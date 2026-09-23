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
        // Take focus off the search box first so the platform text input (Android IME) can't push the old text back.
        ((Control)sender).Focus(FocusState.Programmatic);
        Search.Text = string.Empty;
        ViewModel.ClearSearchCommand.Execute(null);
    }
}
