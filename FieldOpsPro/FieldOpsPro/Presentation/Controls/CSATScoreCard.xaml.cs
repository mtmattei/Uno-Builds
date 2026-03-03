using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FieldOpsPro.Presentation.Controls;

public sealed partial class CSATScoreCard : UserControl
{
    public CSATScoreCard()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    public static readonly DependencyProperty ScoreProperty =
        DependencyProperty.Register(nameof(Score), typeof(double), typeof(CSATScoreCard),
            new PropertyMetadata(4.8, OnScoreChanged));

    public double Score
    {
        get => (double)GetValue(ScoreProperty);
        set => SetValue(ScoreProperty, value);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (ScoreText != null)
        {
            ScoreText.Text = Score.ToString("F1");
        }

        // Update stars based on score
        UpdateStars();
    }

    private void UpdateStars()
    {
        if (StarsPanel == null) return;

        var fullStars = (int)Math.Floor(Score);
        var hasHalfStar = Score - fullStars >= 0.5;

        var children = StarsPanel.Children.ToList();
        for (int i = 0; i < children.Count && i < 5; i++)
        {
            if (children[i] is FontIcon star)
            {
                if (i < fullStars)
                {
                    star.Glyph = "\uE735"; // Full star
                    star.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Windows.UI.Color.FromArgb(255, 255, 255, 255));
                }
                else if (i == fullStars && hasHalfStar)
                {
                    star.Glyph = "\uE7C6"; // Half star
                    star.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Windows.UI.Color.FromArgb(255, 192, 192, 192));
                }
                else
                {
                    star.Glyph = "\uE734"; // Empty star
                    star.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Windows.UI.Color.FromArgb(255, 96, 96, 96));
                }
            }
        }
    }

    private static void OnScoreChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CSATScoreCard card)
        {
            card.UpdateDisplay();
        }
    }
}
