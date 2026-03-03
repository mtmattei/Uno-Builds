using Microsoft.UI.Xaml.Media.Animation;

namespace Worn.Controls;

public sealed partial class HeadlineTicker : UserControl
{
    private int _currentIndex;
    private IList<string>? _headlines;
    private bool _isAnimating;
    private Storyboard? _activeStoryboard;

    public static readonly DependencyProperty HeadlinesProperty =
        DependencyProperty.Register(
            nameof(Headlines),
            typeof(IList<string>),
            typeof(HeadlineTicker),
            new PropertyMetadata(null, OnHeadlinesChanged));

    public IList<string>? Headlines
    {
        get => (IList<string>?)GetValue(HeadlinesProperty);
        set => SetValue(HeadlinesProperty, value);
    }

    public static readonly DependencyProperty TextStyleProperty =
        DependencyProperty.Register(
            nameof(TextStyle),
            typeof(Style),
            typeof(HeadlineTicker),
            new PropertyMetadata(null, OnTextStyleChanged));

    public Style? TextStyle
    {
        get => (Style?)GetValue(TextStyleProperty);
        set => SetValue(TextStyleProperty, value);
    }

    public HeadlineTicker()
    {
        this.InitializeComponent();
        this.SizeChanged += OnSizeChanged;
        this.Tapped += OnTapped;
    }

    private static void OnHeadlinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HeadlineTicker ticker && e.NewValue is IList<string> list)
        {
            ticker.ApplyHeadlines(list);
        }
    }

    private static void OnTextStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HeadlineTicker ticker && e.NewValue is Style style)
        {
            ticker.CurrentText.Style = style;
            ticker.NextText.Style = style;
            ticker.CurrentText.Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["WornTerracottaBrush"];
            ticker.NextText.Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["WornTerracottaBrush"];
        }
    }

    private void ApplyHeadlines(IList<string> list)
    {
        _headlines = list;
        _currentIndex = 0;

        if (list.Count == 0) return;

        CurrentText.Text = list[0];
        NextText.Text = "";
        NextText.Opacity = 0;
    }

    private void OnTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (_isAnimating || _headlines is null || _headlines.Count < 2) return;
        _isAnimating = true;

        // Stop any previous storyboard to release animation holds
        _activeStoryboard?.Stop();

        var nextIndex = (_currentIndex + 1) % _headlines.Count;
        NextText.Text = _headlines[nextIndex];

        var slideHeight = CurrentText.ActualHeight > 0 ? CurrentText.ActualHeight : 40;

        CurrentTranslate.Y = 0;
        NextTranslate.Y = slideHeight;
        NextText.Opacity = 1;

        var duration = TimeSpan.FromMilliseconds(400);

        var currentSlide = new DoubleAnimation
        {
            From = 0,
            To = -slideHeight,
            Duration = new Duration(duration),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        var nextSlide = new DoubleAnimation
        {
            From = slideHeight,
            To = 0,
            Duration = new Duration(duration),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        var sb = new Storyboard();
        Storyboard.SetTarget(currentSlide, CurrentTranslate);
        Storyboard.SetTargetProperty(currentSlide, "Y");
        Storyboard.SetTarget(nextSlide, NextTranslate);
        Storyboard.SetTargetProperty(nextSlide, "Y");
        sb.Children.Add(currentSlide);
        sb.Children.Add(nextSlide);
        _activeStoryboard = sb;

        sb.Completed += (_, _) =>
        {
            // Set text and hide NextText BEFORE stopping to avoid a single-frame
            // flicker where Stop() resets Y to 0, briefly showing the old headline.
            CurrentText.Text = _headlines![nextIndex];
            NextText.Opacity = 0;
            sb.Stop();
            CurrentTranslate.Y = 0;
            NextTranslate.Y = 0;
            _currentIndex = nextIndex;
            _isAnimating = false;
            _activeStoryboard = null;
        };

        sb.Begin();
        e.Handled = true;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ClipRect.Rect = new Windows.Foundation.Rect(0, 0, e.NewSize.Width, e.NewSize.Height);
    }
}
