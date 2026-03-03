using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using Zara.Models;

namespace Zara.Views.Controls;

public sealed partial class ProductCard : UserControl
{
    public Product Product
    {
        get => (Product)GetValue(ProductProperty);
        set => SetValue(ProductProperty, value);
    }

    public static readonly DependencyProperty ProductProperty =
        DependencyProperty.Register(
            nameof(Product),
            typeof(Product),
            typeof(ProductCard),
            new PropertyMetadata(null));

    // Available sizes for the product
    public List<string> AvailableSizes { get; } = new()
    {
        "XS",
        "S",
        "M",
        "L",
        "XL",
        "XXL"
    };

    public event EventHandler<Product>? ProductClicked;
    public event EventHandler<Product>? BookmarkClicked;
    public event EventHandler<string>? SizeSelected;

    public ProductCard()
    {
        this.InitializeComponent();
    }

    private void OnProductClick(object sender, RoutedEventArgs e)
    {
        if (Product != null)
        {
            ProductClicked?.Invoke(this, Product);
        }
    }

    private void OnBookmarkClick(object sender, RoutedEventArgs e)
    {
        if (Product != null)
        {
            Product.IsBookmarked = !Product.IsBookmarked;
            BookmarkClicked?.Invoke(this, Product);
        }
        // Note: Event bubbling prevention not needed in Uno Platform for this scenario
    }

    private void OnSizeButtonClick(object sender, RoutedEventArgs e)
    {
        // The flyout will open automatically due to Button.Flyout
        // This handler is here in case we need additional logic
    }

    private void OnSizeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView listView && listView.SelectedItem is string selectedSize)
        {
            // Notify that a size was selected
            SizeSelected?.Invoke(this, selectedSize);

            // Close the flyout
            SizeFlyout.Hide();

            // Clear selection for next time
            listView.SelectedItem = null;
        }
    }
}
