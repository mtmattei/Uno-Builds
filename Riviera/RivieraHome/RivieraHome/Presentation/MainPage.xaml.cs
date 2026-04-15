using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;

namespace RivieraHome.Presentation;

public sealed partial class MainPage : Page
{
    private readonly DispatcherTimer _clockTimer;

    public MainPage()
    {
        this.InitializeComponent();

        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => UpdateClock();
        _clockTimer.Start();
        UpdateClock();

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Fade in the page content
        this.Opacity = 0;
        var storyboard = new Storyboard();
        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(400)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(fadeIn, this);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");
        storyboard.Children.Add(fadeIn);
        storyboard.Begin();
    }

    private void UpdateClock()
    {
        ClockText.Text = DateTime.Now.ToString("HH:mm:ss");
    }
}
