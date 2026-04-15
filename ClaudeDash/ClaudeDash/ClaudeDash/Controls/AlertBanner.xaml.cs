using Microsoft.UI.Xaml.Media.Animation;

namespace ClaudeDash.Controls;

public sealed partial class AlertBanner : UserControl
{
    public static readonly DependencyProperty AlertProperty =
        DependencyProperty.Register(nameof(Alert), typeof(AlertItem), typeof(AlertBanner),
            new PropertyMetadata(null, OnAlertChanged));

    public AlertItem? Alert
    {
        get => (AlertItem?)GetValue(AlertProperty);
        set => SetValue(AlertProperty, value);
    }

    public AlertBanner()
    {
        this.InitializeComponent();
    }

    private static void OnAlertChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AlertBanner banner || e.NewValue is not AlertItem alert) return;

        banner.AlertMessage.Text = alert.Message;

        var (label, color, bgColor) = alert.Type switch
        {
            AlertType.Error => ("ERR",
                ColorHelper.FromArgb(255, 239, 68, 68),     // StatusRed
                ColorHelper.FromArgb(255, 42, 13, 13)),     // StatusRedBg
            AlertType.Warning => ("WARN",
                ColorHelper.FromArgb(255, 251, 191, 36),    // StatusYellow
                ColorHelper.FromArgb(255, 42, 31, 10)),     // StatusYellowBg
            _ => ("INFO",
                ColorHelper.FromArgb(255, 110, 110, 120),   // TextTertiary
                ColorHelper.FromArgb(0, 0, 0, 0))           // transparent
        };

        banner.LevelLabel.Text = label;
        banner.LevelLabel.Foreground = new SolidColorBrush(color);
        banner.AlertBorder.BorderBrush = new SolidColorBrush(color);
        banner.AlertBorder.Background = new SolidColorBrush(bgColor);

        // Pulsing dot + action link for critical alerts
        if (alert.Type == AlertType.Error)
        {
            banner.PulseDot.Fill = new SolidColorBrush(color);
            banner.PulseDot.Visibility = Visibility.Visible;

            // Start pulse animation
            if (banner.Resources["PulseDotStoryboard"] is Storyboard sb)
                sb.Begin();

            // Action link
            banner.ActionLink.Visibility = Visibility.Visible;
            banner.ActionText.Text = !string.IsNullOrEmpty(alert.NavigationTarget)
                ? "view details >"
                : "dismiss";
        }
        else
        {
            banner.PulseDot.Visibility = Visibility.Collapsed;
            banner.ActionLink.Visibility = Visibility.Collapsed;
        }
    }
}
