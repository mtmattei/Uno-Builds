using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Wellmetrix.Models;

namespace Wellmetrix.Controls;

public sealed partial class InsightCard : UserControl
{
    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(string), typeof(InsightCard),
            new PropertyMetadata(string.Empty, OnPropertyChanged));

    public static readonly DependencyProperty InsightTypeProperty =
        DependencyProperty.Register(nameof(InsightType), typeof(InsightType), typeof(InsightCard),
            new PropertyMetadata(Models.InsightType.Neutral, OnPropertyChanged));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public InsightType InsightType
    {
        get => (InsightType)GetValue(InsightTypeProperty);
        set => SetValue(InsightTypeProperty, value);
    }

    public InsightCard()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateDisplay();
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InsightCard control)
        {
            control.UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (MessageText != null)
            MessageText.Text = Message ?? string.Empty;

        if (InsightIcon != null)
        {
            string glyph;
            Brush foreground;

            switch (InsightType)
            {
                case Models.InsightType.Positive:
                    glyph = "\uE73E"; // Checkmark
                    foreground = (Brush)Application.Current.Resources["KidneysAccentBrush"];
                    break;
                case Models.InsightType.Suggestion:
                    glyph = "\uE82F"; // Lightbulb
                    foreground = (Brush)Application.Current.Resources["LiverAccentBrush"];
                    break;
                case Models.InsightType.Warning:
                    glyph = "\uE7BA"; // Warning
                    foreground = (Brush)Application.Current.Resources["DangerBrush"];
                    break;
                default:
                    glyph = "\uE946"; // Info
                    foreground = (Brush)Application.Current.Resources["LungsAccentBrush"];
                    break;
            }

            InsightIcon.Glyph = glyph;
            InsightIcon.Foreground = foreground;
        }
    }
}
