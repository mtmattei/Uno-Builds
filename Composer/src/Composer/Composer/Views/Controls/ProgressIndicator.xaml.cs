using System;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Composer.Views.Controls;

/// <summary>
/// Hairline + amber-fill progress indicator with mono eyebrow label and
/// counter beneath. Three DPs typed object so MVUX bindable wrappers around
/// <c>IFeed&lt;T&gt;</c> can be unwrapped via reflection. Per
/// <c>docs/ARCHITECTURE-BRIEF-from-scratch.md</c> §7.1 / Brief 03 §4.
///
/// Width animation: 480ms QuinticEase on Fraction changes. Re-measures
/// against the track's <c>ActualWidth</c> via <see cref="OnTrackSizeChanged"/>
/// so the fill stays accurate when the rails open / window resizes.
/// </summary>
public sealed partial class ProgressIndicator : UserControl
{
    public static readonly DependencyProperty FractionProperty =
        DependencyProperty.Register(
            nameof(Fraction), typeof(object), typeof(ProgressIndicator),
            new PropertyMetadata(null, OnFractionChanged));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label), typeof(object), typeof(ProgressIndicator),
            new PropertyMetadata(null, OnLabelChanged));

    public static readonly DependencyProperty CounterProperty =
        DependencyProperty.Register(
            nameof(Counter), typeof(object), typeof(ProgressIndicator),
            new PropertyMetadata(null, OnCounterChanged));

    public object? Fraction { get => GetValue(FractionProperty); set => SetValue(FractionProperty, value); }
    public object? Label    { get => GetValue(LabelProperty);    set => SetValue(LabelProperty, value); }
    public object? Counter  { get => GetValue(CounterProperty);  set => SetValue(CounterProperty, value); }

    public ProgressIndicator() => this.InitializeComponent();

    private static void OnFractionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressIndicator p) p.ApplyFraction();
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressIndicator p)
            p.LabelText.Text = AsString(e.NewValue) ?? string.Empty;
    }

    private static void OnCounterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressIndicator p)
            p.CounterText.Text = AsString(e.NewValue) ?? string.Empty;
    }

    private void OnTrackSizeChanged(object sender, SizeChangedEventArgs e) => ApplyFraction();

    private void ApplyFraction()
    {
        var f = AsDouble(Fraction);
        if (double.IsNaN(f) || f < 0) f = 0;
        if (f > 1) f = 1;

        var available = TrackRoot.ActualWidth;
        if (available <= 0) return; // wait for SizeChanged

        var target = available * f;

        if (Composer.Views.MotionPreferences.AnimationsEnabled)
        {
            var sb = new Storyboard();
            var anim = new DoubleAnimation
            {
                To = target,
                Duration = new Duration(TimeSpan.FromMilliseconds(480)),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut },
            };
            Storyboard.SetTarget(anim, ProgressFill);
            Storyboard.SetTargetProperty(anim, "Width");
            sb.Children.Add(anim);
            sb.Begin();
        }
        else
        {
            ProgressFill.Width = target;
        }
    }

    private static string? AsString(object? value) => value switch
    {
        string s => s,
        null     => null,
        _        => Composer.Views.MvuxValueReader.Unwrap<string>(value),
    };

    /// <summary>Unwrap a double from an MVUX bindable feed wrapper. Mirrors
    /// the bool helper on <see cref="AppTitleRow"/> — <see cref="MvuxValueReader.Unwrap{T}"/>
    /// is class-constrained, so primitives need reflection.</summary>
    private static double AsDouble(object? value)
    {
        if (value is double d) return d;
        if (value is int i)    return i;
        if (value is null)     return double.NaN;
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var t = value.GetType();
        foreach (var name in new[] { "Value", "Current", "Model", "_value" })
        {
            if (t.GetProperty(name, Flags)?.GetValue(value) is double pv) return pv;
            if (t.GetField(name, Flags)?.GetValue(value)    is double fv) return fv;
            if (t.GetProperty(name, Flags)?.GetValue(value) is int pi)    return pi;
        }
        foreach (var prop in t.GetProperties(Flags))
        {
            if (prop.PropertyType == typeof(double) && prop.GetValue(value) is double pv) return pv;
        }
        return double.NaN;
    }
}
