using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Zara.Models;

namespace Zara.Views.Controls;

public sealed partial class ProductListSection : UserControl
{
    public string SectionTitle
    {
        get => (string)GetValue(SectionTitleProperty);
        set => SetValue(SectionTitleProperty, value);
    }

    public static readonly DependencyProperty SectionTitleProperty =
        DependencyProperty.Register(
            nameof(SectionTitle),
            typeof(string),
            typeof(ProductListSection),
            new PropertyMetadata(string.Empty));

    public ObservableCollection<Product> Products
    {
        get => (ObservableCollection<Product>)GetValue(ProductsProperty);
        set => SetValue(ProductsProperty, value);
    }

    public static readonly DependencyProperty ProductsProperty =
        DependencyProperty.Register(
            nameof(Products),
            typeof(ObservableCollection<Product>),
            typeof(ProductListSection),
            new PropertyMetadata(null));

    public ProductListSection()
    {
        this.InitializeComponent();
    }
}
