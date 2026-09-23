using FieldCheck.Models;
using FieldCheck.ViewModels;
using Microsoft.UI.Xaml.Navigation;

namespace FieldCheck.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        ViewModel = App.Services.GetRequiredService<DashboardViewModel>();
        InitializeComponent();
    }

    public DashboardViewModel ViewModel { get; }

    protected override void OnNavigatedTo(NavigationEventArgs e) => _ = ViewModel.EnsureLoadedAsync();

    private void OnAssetClick(object sender, ItemClickEventArgs e) => ViewModel.OpenAsset((Asset)e.ClickedItem);
}
