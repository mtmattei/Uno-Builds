using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace PrecisionDial.Controls;

public sealed partial class PrecisionSquareDial
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(PrecisionSquareDial),
            new PropertyMetadata(0.0, OnValuePropertyChanged));
    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    { if (d is PrecisionSquareDial dial) dial.OnValueChanged((double)e.OldValue, (double)e.NewValue); }

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(PrecisionSquareDial), new PropertyMetadata(0.0));
    public double Minimum { get => (double)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(PrecisionSquareDial), new PropertyMetadata(100.0));
    public double Maximum { get => (double)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

    public static readonly DependencyProperty DetentCountProperty =
        DependencyProperty.Register(nameof(DetentCount), typeof(int), typeof(PrecisionSquareDial), new PropertyMetadata(20));
    public int DetentCount { get => (int)GetValue(DetentCountProperty); set => SetValue(DetentCountProperty, value); }

    public static readonly DependencyProperty IsHapticEnabledProperty =
        DependencyProperty.Register(nameof(IsHapticEnabled), typeof(bool), typeof(PrecisionSquareDial), new PropertyMetadata(true));
    public bool IsHapticEnabled { get => (bool)GetValue(IsHapticEnabledProperty); set => SetValue(IsHapticEnabledProperty, value); }

    public static readonly DependencyProperty ArcSweepDegreesProperty =
        DependencyProperty.Register(nameof(ArcSweepDegrees), typeof(double), typeof(PrecisionSquareDial), new PropertyMetadata(270.0));
    public double ArcSweepDegrees { get => (double)GetValue(ArcSweepDegreesProperty); set => SetValue(ArcSweepDegreesProperty, value); }

    public static readonly DependencyProperty AccentBrushProperty =
        DependencyProperty.Register(nameof(AccentBrush), typeof(Brush), typeof(PrecisionSquareDial), new PropertyMetadata(null));
    public Brush AccentBrush { get => (Brush)GetValue(AccentBrushProperty); set => SetValue(AccentBrushProperty, value); }

    public static readonly DependencyProperty SensitivityProperty =
        DependencyProperty.Register(nameof(Sensitivity), typeof(double), typeof(PrecisionSquareDial), new PropertyMetadata(0.4));
    public double Sensitivity { get => (double)GetValue(SensitivityProperty); set => SetValue(SensitivityProperty, value); }

    public static readonly DependencyProperty IsInertiaEnabledProperty =
        DependencyProperty.Register(nameof(IsInertiaEnabled), typeof(bool), typeof(PrecisionSquareDial), new PropertyMetadata(true));
    public bool IsInertiaEnabled { get => (bool)GetValue(IsInertiaEnabledProperty); set => SetValue(IsInertiaEnabledProperty, value); }

    public static readonly DependencyProperty InertiaDecayRateProperty =
        DependencyProperty.Register(nameof(InertiaDecayRate), typeof(double), typeof(PrecisionSquareDial), new PropertyMetadata(0.92));
    public double InertiaDecayRate { get => (double)GetValue(InertiaDecayRateProperty); set => SetValue(InertiaDecayRateProperty, value); }

    public event EventHandler<DialValueChangedEventArgs>? ValueChanged;
    public event EventHandler<DetentCrossedEventArgs>? DetentCrossed;
    public event RoutedEventHandler? DragStarted;
    public event RoutedEventHandler? DragCompleted;
    public event RoutedEventHandler? InertiaCompleted;
}
