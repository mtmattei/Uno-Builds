using Microsoft.UI.Xaml.Media;
using ListHold.Models;

namespace ListHold.Controls;

public sealed partial class HoldProgressRing : UserControl
{
    private const double Radius = 16;
    private const double CenterX = 18;
    private const double CenterY = 18;

    public static readonly DependencyProperty ProgressProperty =
        DependencyProperty.Register(
            nameof(Progress),
            typeof(double),
            typeof(HoldProgressRing),
            new PropertyMetadata(0.0, OnProgressChanged));

    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(
            nameof(State),
            typeof(HoldState),
            typeof(HoldProgressRing),
            new PropertyMetadata(HoldState.Idle, OnStateChanged));

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public HoldState State
    {
        get => (HoldState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public HoldProgressRing()
    {
        this.InitializeComponent();
        UpdateArc(0);
    }

    private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoldProgressRing ring)
        {
            ring.UpdateArc((double)e.NewValue);
        }
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoldProgressRing ring)
        {
            ring.UpdateIcon((HoldState)e.NewValue);
        }
    }

    private void UpdateArc(double progress)
    {
        if (progress <= 0)
        {
            ProgressArc.Data = null;
            return;
        }

        progress = Math.Min(progress, 0.999);

        var angle = progress * 360;
        var startAngle = -90.0;
        var endAngle = startAngle + angle;

        var startRad = startAngle * Math.PI / 180;
        var endRad = endAngle * Math.PI / 180;

        var startX = CenterX + Radius * Math.Cos(startRad);
        var startY = CenterY + Radius * Math.Sin(startRad);
        var endX = CenterX + Radius * Math.Cos(endRad);
        var endY = CenterY + Radius * Math.Sin(endRad);

        var largeArc = angle > 180;

        var geometry = new PathGeometry();
        var figure = new PathFigure
        {
            StartPoint = new Windows.Foundation.Point(startX, startY),
            IsClosed = false
        };

        var arcSegment = new ArcSegment
        {
            Point = new Windows.Foundation.Point(endX, endY),
            Size = new Windows.Foundation.Size(Radius, Radius),
            SweepDirection = SweepDirection.Clockwise,
            IsLargeArc = largeArc
        };

        figure.Segments.Add(arcSegment);
        geometry.Figures.Add(figure);
        ProgressArc.Data = geometry;
    }

    private void UpdateIcon(HoldState state)
    {
        CenterIcon.Glyph = state == HoldState.Locked ? "\uE738" : "\uE710";
        IconRotation.Angle = state == HoldState.Locked ? 180 : 0;
    }
}
