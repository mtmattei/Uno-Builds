using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
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

    public Brush FrameBrush => CreateFrameBrush();
    public Brush StatusBrush => CreateStatusBrush();

    public AvatarControl()
    {
        this.InitializeComponent();
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AvatarControl control)
        {
            control.OnPropertyChanged(nameof(FrameCornerRadius));
            control.OnPropertyChanged(nameof(AvatarCornerRadius));
            control.OnPropertyChanged(nameof(InitialsFontSize));
            control.OnPropertyChanged(nameof(StatusSize));
            control.OnPropertyChanged(nameof(StatusCornerRadius));
        }
    }

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AvatarControl control)
        {
            control.OnPropertyChanged(nameof(StatusBrush));
        }
    }

    private static void OnFrameColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AvatarControl control)
        {
            control.OnPropertyChanged(nameof(FrameBrush));
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        Bindings.Update();
    }

    private Brush CreateFrameBrush()
    {
        var startColor = ColorHelper.FromArgb(255, 0, 179, 119); // MsnGreen
        if (!string.IsNullOrEmpty(FrameColor))
        {
            try
            {
                var hex = FrameColor.TrimStart('#');
                if (hex.Length == 6)
                {
                    startColor = ColorHelper.FromArgb(255,
                        Convert.ToByte(hex.Substring(0, 2), 16),
                        Convert.ToByte(hex.Substring(2, 2), 16),
                        Convert.ToByte(hex.Substring(4, 2), 16));
                }
            }
            catch { }
        }

        var endColor = ColorHelper.FromArgb(255, 0, 120, 212); // MsnBlue

        return new LinearGradientBrush
        {
            StartPoint = new Windows.Foundation.Point(0, 0),
            EndPoint = new Windows.Foundation.Point(1, 1),
            GradientStops =
            {
                new GradientStop { Color = startColor, Offset = 0 },
                new GradientStop { Color = endColor, Offset = 1 }
            }
        };
    }

    private Brush CreateStatusBrush()
    {
        var color = Status switch
        {
            PresenceStatus.Online => ColorHelper.FromArgb(255, 0, 204, 102),
            PresenceStatus.Away => ColorHelper.FromArgb(255, 255, 184, 0),
            PresenceStatus.Busy => ColorHelper.FromArgb(255, 255, 59, 48),
            PresenceStatus.Offline => ColorHelper.FromArgb(255, 142, 142, 147),
            _ => Colors.Gray
        };
        return new SolidColorBrush(color);
    }
}
