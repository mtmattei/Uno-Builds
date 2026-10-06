using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using MsnMessenger.Converters;
using MsnMessenger.Models;

namespace MsnMessenger.Controls;

public sealed partial class AvatarControl : UserControl
{
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(double), typeof(AvatarControl), new PropertyMetadata(44.0, OnSizeChanged));

    public static readonly DependencyProperty InitialsProperty =
        DependencyProperty.Register(nameof(Initials), typeof(string), typeof(AvatarControl), new PropertyMetadata("?"));

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(nameof(Status), typeof(PresenceStatus), typeof(AvatarControl), new PropertyMetadata(PresenceStatus.Offline, OnStatusChanged));

    public static readonly DependencyProperty FrameColorProperty =
        DependencyProperty.Register(nameof(FrameColor), typeof(string), typeof(AvatarControl), new PropertyMetadata(null, OnFrameColorChanged));

    public static readonly DependencyProperty ShowStatusProperty =
        DependencyProperty.Register(nameof(ShowStatus), typeof(Visibility), typeof(AvatarControl), new PropertyMetadata(Visibility.Visible));

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public string Initials
    {
        get => (string)GetValue(InitialsProperty);
        set => SetValue(InitialsProperty, value);
    }

    public PresenceStatus Status
    {
        get => (PresenceStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public string? FrameColor
    {
        get => (string?)GetValue(FrameColorProperty);
        set => SetValue(FrameColorProperty, value);
    }

    public Visibility ShowStatus
    {
        get => (Visibility)GetValue(ShowStatusProperty);
        set => SetValue(ShowStatusProperty, value);
    }

    public double FrameCornerRadius => Size * 0.27;
    public double AvatarCornerRadius => (Size - 6) * 0.25;
    public double InitialsFontSize => Size * 0.4;
    public double StatusSize => Size * 0.3;
    public double StatusCornerRadius => StatusSize / 2;

    private Brush _frameBrush;
    private Brush _statusBrush;

    public Brush FrameBrush => _frameBrush;
    public Brush StatusBrush => _statusBrush;

    public AvatarControl()
    {
        _frameBrush = BuildFrameBrush(null);
        _statusBrush = StatusBrushes.ForStatus(PresenceStatus.Offline);
        this.InitializeComponent();
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AvatarControl control)
        {
            control.Bindings.Update();
        }
    }

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AvatarControl control)
        {
            control._statusBrush = StatusBrushes.ForStatus(control.Status);
            control.Bindings.Update();
        }
    }

    private static void OnFrameColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AvatarControl control)
        {
            control._frameBrush = BuildFrameBrush(control.FrameColor);
            control.Bindings.Update();
        }
    }

    private static Brush BuildFrameBrush(string? frameColor)
    {
        var startColor = ColorHelper.FromArgb(255, 0, 179, 119);
        if (TryParseHex(frameColor, out var parsed))
        {
            startColor = parsed;
        }

        var endColor = ColorHelper.FromArgb(255, 0, 120, 212);

        return new LinearGradientBrush
        {
            StartPoint = new Windows.Foundation.Point(0, 0),
            EndPoint = new Windows.Foundation.Point(1, 1),
            GradientStops =
            {
                new GradientStop { Color = startColor, Offset = 0 },
                new GradientStop { Color = endColor, Offset = 1 },
            },
        };
    }

    private static bool TryParseHex(string? value, out Windows.UI.Color color)
    {
        color = default;
        if (string.IsNullOrEmpty(value)) return false;

        var hex = value.AsSpan().TrimStart('#');
        if (hex.Length != 6) return false;

        if (byte.TryParse(hex.Slice(0, 2), System.Globalization.NumberStyles.HexNumber, null, out var r)
            && byte.TryParse(hex.Slice(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var g)
            && byte.TryParse(hex.Slice(4, 2), System.Globalization.NumberStyles.HexNumber, null, out var b))
        {
            color = ColorHelper.FromArgb(255, r, g, b);
            return true;
        }
        return false;
    }
}
