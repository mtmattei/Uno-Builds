using System.Collections.Generic;

namespace ClaudeDash.Controls;

public sealed partial class StatCard : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(int), typeof(StatCard),
            new PropertyMetadata(0, OnValueChanged));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(StatCard),
            new PropertyMetadata("", OnLabelChanged));

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(StatCard),
            new PropertyMetadata("", OnSubtitleChanged));

    public static readonly DependencyProperty DotColorProperty =
        DependencyProperty.Register(nameof(DotColor), typeof(Color), typeof(StatCard),
            new PropertyMetadata(ColorHelper.FromArgb(255, 74, 222, 128), OnDotColorChanged));

    public static readonly DependencyProperty AccentColorProperty =
        DependencyProperty.Register(nameof(AccentColor), typeof(Color), typeof(StatCard),
            new PropertyMetadata(ColorHelper.FromArgb(255, 74, 222, 128), OnAccentColorChanged));

    public static readonly DependencyProperty TrendProperty =
        DependencyProperty.Register(nameof(Trend), typeof(string), typeof(StatCard),
            new PropertyMetadata("", OnTrendChanged));

    public static readonly DependencyProperty IconGlyphProperty =
        DependencyProperty.Register(nameof(IconGlyph), typeof(string), typeof(StatCard),
            new PropertyMetadata("", OnIconGlyphChanged));

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public Color DotColor
    {
        get => (Color)GetValue(DotColorProperty);
        set => SetValue(DotColorProperty, value);
    }

    public Color AccentColor
    {
        get => (Color)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    public string Trend
    {
        get => (string)GetValue(TrendProperty);
        set => SetValue(TrendProperty, value);
    }

    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    public StatCard()
    {
        this.InitializeComponent();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatCard card) card.ValueText.Text = e.NewValue?.ToString() ?? "0";
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatCard card) card.LabelText.Text = e.NewValue as string ?? "";
    }

    private static void OnSubtitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatCard card) card.SubtitleText.Text = e.NewValue as string ?? "";
    }

    private static void OnDotColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatCard card && e.NewValue is Color c)
        {
            card.StatusDot.Fill = new SolidColorBrush(c);
        }
    }

    private static void OnAccentColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatCard card && e.NewValue is Color c)
        {
            card.AccentBar.Background = new LinearGradientBrush
            {
                StartPoint = new Windows.Foundation.Point(0, 0.5),
                EndPoint = new Windows.Foundation.Point(1, 0.5),
                GradientStops =
                {
                    new GradientStop { Color = c, Offset = 0 },
                    new GradientStop { Color = ColorHelper.FromArgb(0, 0, 0, 0), Offset = 1 }
                }
            };
        }
    }

    private static void OnTrendChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatCard card)
        {
            var trend = e.NewValue as string ?? "";
            if (string.IsNullOrWhiteSpace(trend))
            {
                card.TrendText.Visibility = Visibility.Collapsed;
                return;
            }

            card.TrendText.Text = trend;
            card.TrendText.Visibility = Visibility.Visible;

            // Color based on direction
            if (trend.StartsWith('+') || trend.Contains('\u2191')) // up arrow
                card.TrendText.Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 74, 222, 128)); // green
            else if (trend.StartsWith('-') || trend.Contains('\u2193')) // down arrow
                card.TrendText.Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 239, 68, 68)); // red
            else
                card.TrendText.Foreground = (Brush)Application.Current.Resources["TextTertiaryBrush"];
        }
    }

    private static void OnIconGlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatCard card)
        {
            var glyph = e.NewValue as string ?? "";
            if (string.IsNullOrWhiteSpace(glyph))
            {
                card.CardIcon.Visibility = Visibility.Collapsed;
            }
            else
            {
                card.CardIcon.Glyph = glyph;
                card.CardIcon.Visibility = Visibility.Visible;
            }
        }
    }

    // No-op: sparkline removed in redesign but callers still reference this
    public void SetSparkline(IList<double> data, Color? color = null) { }
}
