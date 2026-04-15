using Microsoft.UI.Xaml.Media.Animation;

namespace PhosphorProtocol.Views;

public sealed partial class BootPage : Page
{
    private readonly DispatcherTimer _cursorTimer;
    private bool _cursorVisible = true;

    public BootPage()
    {
        this.InitializeComponent();
        _cursorTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
        _cursorTimer.Tick += (s, e) =>
        {
            _cursorVisible = !_cursorVisible;
            Cursor.Opacity = _cursorVisible ? 1 : 0;
        };
        Loaded += BootPage_Loaded;
    }

    private async void BootPage_Loaded(object sender, RoutedEventArgs e)
    {
        // --- Phase 1: CRT warm-up scanline appears ---
        await Task.Delay(200);

        var lineAppear = new Storyboard();
        var lineOpacityIn = new DoubleAnimation
        {
            From = 0, To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(lineOpacityIn, WarmUpLine);
        Storyboard.SetTargetProperty(lineOpacityIn, "Opacity");
        lineAppear.Children.Add(lineOpacityIn);
        lineAppear.Begin();
        await Task.Delay(400);

        // --- Phase 2: Scanline expands vertically, CRT glass fades in ---
        var expandStoryboard = new Storyboard();

        var scaleExpand = new DoubleAnimation
        {
            From = 1, To = 200,
            Duration = new Duration(TimeSpan.FromMilliseconds(500)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(scaleExpand, WarmUpLineScale);
        Storyboard.SetTargetProperty(scaleExpand, "ScaleY");
        expandStoryboard.Children.Add(scaleExpand);

        var glassFadeIn = new DoubleAnimation
        {
            From = 0, To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(500)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(glassFadeIn, CRTGlass);
        Storyboard.SetTargetProperty(glassFadeIn, "Opacity");
        expandStoryboard.Children.Add(glassFadeIn);

        expandStoryboard.Begin();
        await Task.Delay(500);

        // --- Phase 3: Hide warm-up overlay ---
        var lineOut = new Storyboard();
        var lineOpacityOut = new DoubleAnimation
        {
            From = 1, To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        Storyboard.SetTarget(lineOpacityOut, WarmUpLine);
        Storyboard.SetTargetProperty(lineOpacityOut, "Opacity");
        lineOut.Children.Add(lineOpacityOut);
        lineOut.Begin();
        await Task.Delay(250);

        WarmUpLine.Visibility = Visibility.Collapsed;

        // --- Phase 4: Elegant Riviera title fade in ---
        TitlePanel.Opacity = 1;
        await AnimateElementIn(TitleText, 600);
        await Task.Delay(200);
        await AnimateElementIn(SubtitleText, 500);
        await Task.Delay(1200);

        // --- Phase 5: Fade out title, switch to vehicle status POST ---
        var titleFadeOut = new Storyboard();
        var titleOpacityOut = new DoubleAnimation
        {
            From = 1, To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(400)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        Storyboard.SetTarget(titleOpacityOut, TitlePanel);
        Storyboard.SetTargetProperty(titleOpacityOut, "Opacity");
        titleFadeOut.Children.Add(titleOpacityOut);
        titleFadeOut.Begin();
        await Task.Delay(450);

        TitlePanel.Visibility = Visibility.Collapsed;
        BootLines.Opacity = 1;

        // Start cursor blink and POST text sequence
        Cursor.Opacity = 1;
        _cursorTimer.Start();

        var lines = new[] { Line1, Line2, Line3, Line4, Line5, Line6 };
        var random = new Random();

        foreach (var line in lines)
        {
            await Task.Delay(220 + random.Next(100));
            await AnimateLineIn(line);
        }

        await Task.Delay(300);
        await AnimateLineIn(LineFinal);
        await Task.Delay(600);

        _cursorTimer.Stop();

        // Fade to black before navigating to avoid flash
        var fadeOut = new Storyboard();
        var glassOut = new DoubleAnimation
        {
            From = 1, To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(400)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        Storyboard.SetTarget(glassOut, CRTGlass);
        Storyboard.SetTargetProperty(glassOut, "Opacity");
        fadeOut.Children.Add(glassOut);
        fadeOut.Begin();
        await Task.Delay(450);

        // Navigate to DashboardShell
        if (this.Frame is { } frame)
        {
            frame.Navigate(typeof(DashboardShell));
        }
    }

    private static async Task AnimateElementIn(UIElement element, int durationMs)
    {
        var transform = new Microsoft.UI.Xaml.Media.TranslateTransform();
        element.RenderTransform = transform;

        var fadeIn = new DoubleAnimation
        {
            From = 0, To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(durationMs)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var slideUp = new DoubleAnimation
        {
            From = 12, To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(durationMs)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeIn);
        storyboard.Children.Add(slideUp);

        Storyboard.SetTarget(fadeIn, element);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");
        Storyboard.SetTarget(slideUp, transform);
        Storyboard.SetTargetProperty(slideUp, "Y");

        storyboard.Begin();
        await Task.Delay(durationMs + 50);
    }

    private static async Task AnimateLineIn(TextBlock line)
    {
        var transform = new Microsoft.UI.Xaml.Media.TranslateTransform();
        line.RenderTransform = transform;

        var fadeIn = new DoubleAnimation
        {
            From = 0, To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(250)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var slideUp = new DoubleAnimation
        {
            From = 5, To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(250)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeIn);
        storyboard.Children.Add(slideUp);

        Storyboard.SetTarget(fadeIn, line);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");
        Storyboard.SetTarget(slideUp, transform);
        Storyboard.SetTargetProperty(slideUp, "Y");

        storyboard.Begin();
        await Task.Delay(260);
    }
}
