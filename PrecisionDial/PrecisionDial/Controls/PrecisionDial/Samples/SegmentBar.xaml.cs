using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace PrecisionDial.Samples;

public sealed partial class SegmentBar : UserControl
{
    private const int SegCount = 24;
    private const double SegWidth = 6.0;
    private const double SegHeight = 2.5;
    private const double SegGap = 2.5;
    private readonly Border[] _segs = new Border[SegCount];

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(SegmentBar),
            new PropertyMetadata(0.0, static (d, e) => ((SegmentBar)d).UpdateDisplay()));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(SegmentBar),
            new PropertyMetadata(100.0, static (d, e) => ((SegmentBar)d).UpdateDisplay()));

    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public double Maximum { get => (double)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

    public SegmentBar()
    {
        InitializeComponent();

        for (int i = 0; i < SegCount; i++)
        {
            var border = new Border
            {
                Width = SegWidth,
                Height = SegHeight,
                CornerRadius = new CornerRadius(1),
                Background = new SolidColorBrush(Color.FromArgb(10, 255, 255, 255)),
            };
            if (i < SegCount - 1)
                border.Margin = new Thickness(0, 0, SegGap, 0);
            _segs[i] = border;
            SegmentsPanel.Children.Add(border);
        }
    }

    private void UpdateDisplay()
    {
        double max = Maximum > 0 ? Maximum : 100;
        double normalized = Math.Clamp(Value / max, 0, 1);
        int activeCount = (int)(normalized * SegCount);

        for (int i = 0; i < SegCount; i++)
        {
            if (i < activeCount)
            {
                float ramp = (float)i / (SegCount - 1);
                byte alpha = (byte)((0.4f + ramp * 0.6f) * 255);
                _segs[i].Background = new SolidColorBrush(Color.FromArgb(alpha, 212, 169, 89));
            }
            else
            {
                _segs[i].Background = new SolidColorBrush(Color.FromArgb(10, 255, 255, 255));
            }
        }
    }
}
