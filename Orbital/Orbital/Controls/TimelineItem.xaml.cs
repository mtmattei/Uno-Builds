using Microsoft.UI.Xaml.Media;
using Orbital.Helpers;

namespace Orbital.Controls;

public sealed partial class TimelineItem : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(TimelineItem),
            new PropertyMetadata(string.Empty, (d, e) => ((TimelineItem)d).TitleText.Text = (string)e.NewValue));

    public static readonly DependencyProperty TimeProperty =
        DependencyProperty.Register(nameof(Time), typeof(string), typeof(TimelineItem),
            new PropertyMetadata(string.Empty, (d, e) => ((TimelineItem)d).TimeText.Text = (string)e.NewValue));

    public static readonly DependencyProperty DetailProperty =
        DependencyProperty.Register(nameof(Detail), typeof(string), typeof(TimelineItem),
            new PropertyMetadata(string.Empty, OnDetailChanged));

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(nameof(Status), typeof(string), typeof(TimelineItem),
            new PropertyMetadata(string.Empty, OnStatusChanged));

    public static readonly DependencyProperty IsLastProperty =
        DependencyProperty.Register(nameof(IsLast), typeof(bool), typeof(TimelineItem),
            new PropertyMetadata(false, (d, e) => ((TimelineItem)d).ConnectorLine.Visibility =
                (bool)e.NewValue ? Visibility.Collapsed : Visibility.Visible));

    public TimelineItem()
    {
        this.InitializeComponent();
        ApplyStatusColors("ok");
    }

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Time { get => (string)GetValue(TimeProperty); set => SetValue(TimeProperty, value); }
    public string Detail { get => (string)GetValue(DetailProperty); set => SetValue(DetailProperty, value); }
    public string Status { get => (string)GetValue(StatusProperty); set => SetValue(StatusProperty, value); }
    public bool IsLast { get => (bool)GetValue(IsLastProperty); set => SetValue(IsLastProperty, value); }

    private static void OnDetailChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var item = (TimelineItem)d;
        var text = (string)e.NewValue;
        item.DetailText.Text = text;
        item.DetailText.Visibility = string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;
    }

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((TimelineItem)d).ApplyStatusColors((string)e.NewValue);

    private void ApplyStatusColors(string status)
    {
        var stroke = OrbitalColors.StatusColor(status);
        var fill = Windows.UI.ColorHelper.FromArgb(51, stroke.R, stroke.G, stroke.B);
        DotOuter.Stroke = new SolidColorBrush(stroke);
        DotInner.Fill = new SolidColorBrush(fill);
    }
}
