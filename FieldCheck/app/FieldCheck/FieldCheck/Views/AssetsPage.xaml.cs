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
}
