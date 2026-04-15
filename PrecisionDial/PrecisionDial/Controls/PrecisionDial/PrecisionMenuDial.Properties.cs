using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace PrecisionDial.Controls;

public sealed partial class PrecisionMenuDial
{
    public static readonly DependencyProperty MenuItemsProperty =
        DependencyProperty.Register(nameof(MenuItems), typeof(IList<string>), typeof(PrecisionMenuDial),
            new PropertyMetadata(null, (d, _) => ((PrecisionMenuDial)d).RebuildItems()));
    public IList<string> MenuItems
    {
        get => (IList<string>)GetValue(MenuItemsProperty);
        set => SetValue(MenuItemsProperty, value);
    }

    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(PrecisionMenuDial),
            new PropertyMetadata(0));
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        private set => SetValue(SelectedIndexProperty, value);
    }

    public static readonly DependencyProperty DialSizeProperty =
        DependencyProperty.Register(nameof(DialSize), typeof(double), typeof(PrecisionMenuDial),
            new PropertyMetadata(200.0, (d, _) => ((PrecisionMenuDial)d).OnDialSizeChanged()));
    public double DialSize
    {
        get => (double)GetValue(DialSizeProperty);
        set => SetValue(DialSizeProperty, value);
    }

    public static readonly DependencyProperty MenuIconsProperty =
        DependencyProperty.Register(nameof(MenuIcons), typeof(IList<string>), typeof(PrecisionMenuDial),
            new PropertyMetadata(null, (d, _) => ((PrecisionMenuDial)d).RebuildItems()));
    public IList<string> MenuIcons
    {
        get => (IList<string>)GetValue(MenuIconsProperty);
        set => SetValue(MenuIconsProperty, value);
    }

    public static readonly DependencyProperty AccentBrushProperty =
        DependencyProperty.Register(nameof(AccentBrush), typeof(Brush), typeof(PrecisionMenuDial),
            new PropertyMetadata(null, (d, _) => ((PrecisionMenuDial)d).OnAccentBrushChanged()));
    public Brush AccentBrush
    {
        get => (Brush)GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }
}
