using FieldCheck.ViewModels;
using Microsoft.UI.Xaml.Navigation;

namespace FieldCheck.Views;

public sealed partial class AssetDetailPage : Page
{
    public AssetDetailPage()
    {
        ViewModel = App.Services.GetRequiredService<AssetDetailViewModel>();
        InitializeComponent();
    }

    public AssetDetailViewModel ViewModel { get; }

    protected override void OnNavigatedTo(NavigationEventArgs e) => ViewModel.Load((string)e.Parameter);

    protected override void OnNavigatedFrom(NavigationEventArgs e) => ViewModel.Dispose();
}
