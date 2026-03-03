using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ZaraApp.Controls;

public sealed partial class ProductGridItem : UserControl
{
    public static readonly DependencyProperty FitNameProperty =
        DependencyProperty.Register(
            nameof(FitName),
            typeof(string),
            typeof(ProductGridItem),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(
            nameof(ImageSource),
            typeof(string),
            typeof(ProductGridItem),
            new PropertyMetadata(string.Empty));

    public string FitName
    {
        get => (string)GetValue(FitNameProperty);
        set => SetValue(FitNameProperty, value);
    }

    public string ImageSource
    {
        get => (string)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public ProductGridItem()
    {
        this.InitializeComponent();
    }
}
