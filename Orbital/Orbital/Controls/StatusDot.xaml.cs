using Microsoft.UI.Xaml.Media;
using Orbital.Helpers;

namespace Orbital.Controls;

public sealed partial class StatusDot : UserControl
{
    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(nameof(Status), typeof(string), typeof(StatusDot),
            new PropertyMetadata("ok", OnStatusChanged));

    public static readonly DependencyProperty DotSizeProperty =
        DependencyProperty.Register(nameof(DotSize), typeof(double), typeof(StatusDot),
            new PropertyMetadata(8.0, OnDotSizeChanged));

    public StatusDot()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    public string Status
    {
        get => (string)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public double DotSize
    {
        get => (double)GetValue(DotSizeProperty);
        set => SetValue(DotSizeProperty, value);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateVisuals();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        PulseAnimation.Stop();
    }

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusDot dot)
            dot.UpdateVisuals();
    }

    private static void OnDotSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusDot dot)
            dot.ApplySize();
    }

    private void ApplySize()
    {
        var size = DotSize;
        Dot.Width = size;
        Dot.Height = size;
        PulseRing.Width = size;
        PulseRing.Height = size;
    }

    private void UpdateVisuals()
    {
        var brush = new SolidColorBrush(OrbitalColors.StatusColor(Status));
        Dot.Fill = brush;
        PulseRing.Stroke = brush;
        ApplySize();

        if (Status == "ok")
        {
            PulseRing.Visibility = Visibility.Visible;
            PulseAnimation.Begin();
        }
        else
        {
            PulseAnimation.Stop();
            PulseRing.Visibility = Visibility.Collapsed;
            DotScale.ScaleX = 1;
            DotScale.ScaleY = 1;
        }
    }
}
