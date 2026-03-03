using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Zara.Models;

namespace Zara.Views.Controls;

public sealed partial class FitsGridSection : UserControl
{
    public ObservableCollection<ProductFit> Fits
    {
        get => (ObservableCollection<ProductFit>)GetValue(FitsProperty);
        set => SetValue(FitsProperty, value);
    }

    public static readonly DependencyProperty FitsProperty =
        DependencyProperty.Register(
            nameof(Fits),
            typeof(ObservableCollection<ProductFit>),
            typeof(FitsGridSection),
            new PropertyMetadata(null, OnFitsChanged));

    private static void OnFitsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FitsGridSection section && e.NewValue is ObservableCollection<ProductFit> fits)
        {
            section.UpdateFitTiles(fits);
        }
    }

    public FitsGridSection()
    {
        this.InitializeComponent();
    }

    private void UpdateFitTiles(ObservableCollection<ProductFit> fits)
    {
        if (fits.Count > 0) FitTile1.Fit = fits[0];
        if (fits.Count > 1) FitTile2.Fit = fits[1];
        if (fits.Count > 2) FitTile3.Fit = fits[2];
        if (fits.Count > 3) FitTile4.Fit = fits[3];
    }
}
