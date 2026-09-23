using FieldCheck.Models;
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
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(AssetsViewModel.SelectedAsset))
            {
                SyncSelection();
            }
        };
        List.SelectionChanged += (_, _) =>
        {
            if (ViewModel.IsWide && List.SelectedItem is Asset asset && asset != ViewModel.SelectedAsset)
            {
                ViewModel.SelectedAsset = asset;
            }
        };
    }

    public AssetsViewModel ViewModel { get; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        UpdateLayoutMode();
        _ = ViewModel.EnsureLoadedAsync();
    }

    private void UpdateLayoutMode()
    {
        var root = XamlRoot?.Size.Width ?? ActualWidth;
        ViewModel.IsWide = root >= ShellPage.MasterDetailMinWidth;
        SyncSelection();
    }

    private void SyncSelection()
    {
        // The list shows selection only in master/detail mode.
        var target = ViewModel.IsWide ? ViewModel.SelectedAsset : null;
        if (!Equals(List.SelectedItem, target))
        {
            List.SelectedItem = target is null ? null : ViewModel.Items.FirstOrDefault(a => a.Id == target.Id);
        }
    }

    private void OnAssetClick(object sender, ItemClickEventArgs e) => ViewModel.OpenAsset((Asset)e.ClickedItem);
}
