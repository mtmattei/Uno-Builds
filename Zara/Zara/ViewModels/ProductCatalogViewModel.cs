using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Zara.Models;
using Zara.Services;

namespace Zara.ViewModels;

public partial class ProductCatalogViewModel : ObservableObject
{
    private readonly MockProductService _productService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<ProductFit> _fits = new();

    [ObservableProperty]
    private ObservableCollection<Product> _straightBasicProducts = new();

    [ObservableProperty]
    private ObservableCollection<Product> _flareBootCutProducts = new();

    [ObservableProperty]
    private ObservableCollection<Product> _cargoUtilityProducts = new();

    [ObservableProperty]
    private ObservableCollection<Product> _balloonContemporaryProducts = new();

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private Category? _selectedCategory;

    public ProductCatalogViewModel()
    {
        _productService = new MockProductService();
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            // Load fits
            var fits = await _productService.GetFitsAsync();
            Fits = new ObservableCollection<ProductFit>(fits);

            // Load categories
            var categories = await _productService.GetCategoriesAsync();
            Categories = new ObservableCollection<Category>(categories);
            SelectedCategory = Categories.FirstOrDefault();

            // Load product sections
            StraightBasicProducts = new ObservableCollection<Product>(
                _productService.GetStraightBasicProducts());

            FlareBootCutProducts = new ObservableCollection<Product>(
                _productService.GetFlareBootCutProducts());

            CargoUtilityProducts = new ObservableCollection<Product>(
                _productService.GetCargoUtilityProducts());

            BalloonContemporaryProducts = new ObservableCollection<Product>(
                _productService.GetBalloonContemporaryProducts());
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SelectCategory(Category category)
    {
        if (SelectedCategory != null)
            SelectedCategory.IsSelected = false;

        SelectedCategory = category;
        category.IsSelected = true;

        // Filter products based on category (if needed)
        await LoadProductsByCategory(category.Id);
    }

    private async Task LoadProductsByCategory(string categoryId)
    {
        IsLoading = true;

        try
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);

            // Update product lists based on filtered results
            // For simplicity, keeping static sections for now
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void NavigateToProduct(Product product)
    {
        // Navigation logic (simplified, no actual navigation service)
        // In full implementation, use Uno.Extensions.Navigation
    }

    [RelayCommand]
    private void NavigateToFit(ProductFit fit)
    {
        // Navigate to fit-specific product listing
    }

    [RelayCommand]
    private void ToggleBookmark(Product product)
    {
        product.IsBookmarked = !product.IsBookmarked;
    }

    [RelayCommand]
    private void OpenMenu()
    {
        // Open side menu
    }

    [RelayCommand]
    private void OpenFilters()
    {
        // Open filters dialog
    }

    [RelayCommand]
    private void OpenSearch()
    {
        // Navigate to search page
    }

    [RelayCommand]
    private void OpenCart()
    {
        // Navigate to cart (not implemented in sample)
    }
}
