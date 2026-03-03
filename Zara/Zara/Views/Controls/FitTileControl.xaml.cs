using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Zara.Models;

namespace Zara.Views.Controls;

public sealed partial class FitTileControl : UserControl
{
    public ProductFit Fit
    {
        get => (ProductFit)GetValue(FitProperty);
        set => SetValue(FitProperty, value);
    }

    public static readonly DependencyProperty FitProperty =
        DependencyProperty.Register(
            nameof(Fit),
            typeof(ProductFit),
            typeof(FitTileControl),
            new PropertyMetadata(null));

    public event EventHandler<ProductFit>? FitClicked;

    public FitTileControl()
    {
        this.InitializeComponent();
    }

    private void OnFitClick(object sender, RoutedEventArgs e)
    {
        if (Fit != null)
        {
            FitClicked?.Invoke(this, Fit);
        }
    }
}
