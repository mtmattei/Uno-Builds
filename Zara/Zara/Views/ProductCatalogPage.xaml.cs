using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Zara.Models;

namespace Zara.Views;

public sealed partial class ProductCatalogPage : Page
{
    public ProductCatalogPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync();
    }

    private void OnMenuClicked(object? sender, EventArgs e)
    {
        // Handle menu click
    }

    private void OnFiltersClicked(object? sender, EventArgs e)
    {
        // Handle filters click
    }

    private void OnSearchClicked(object? sender, EventArgs e)
    {
        // Handle search click
    }

    private void OnCartClicked(object? sender, EventArgs e)
    {
        // Handle cart click
    }

    private async void OnCategorySelected(object? sender, Category e)
    {
        await ViewModel.SelectCategoryCommand.ExecuteAsync(e);
    }
}
