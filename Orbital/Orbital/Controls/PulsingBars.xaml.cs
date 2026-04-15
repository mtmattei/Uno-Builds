using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
using Orbital.Helpers;

namespace Orbital.Controls;

public sealed partial class PulsingBars : UserControl
{
    private readonly Random _random = new();
    private readonly List<Storyboard> _storyboards = [];

    public static readonly DependencyProperty BarCountProperty =
        DependencyProperty.Register(nameof(BarCount), typeof(int), typeof(PulsingBars),
            new PropertyMetadata(4, OnPropertyChanged));

    public static readonly DependencyProperty BarColorProperty =
        DependencyProperty.Register(nameof(BarColor), typeof(string), typeof(PulsingBars),
            new PropertyMetadata("emerald", OnPropertyChanged));

    public PulsingBars()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) => BuildBars();
        this.Unloaded += (_, _) => StopAll();
    }

    public int BarCount
    {
        get => (int)GetValue(BarCountProperty);
        set => SetValue(BarCountProperty, value);
    }

    public string BarColor
    {
        get => (string)GetValue(BarColorProperty);
        set => SetValue(BarColorProperty, value);
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PulsingBars bars)
            bars.BuildBars();
    }

    private void StopAll()
    {
        foreach (var sb in _storyboards)
            sb.Stop();
        _storyboards.Clear();
    }

    private void BuildBars()
    {
        StopAll();
        BarsPanel.Children.Clear();

        var brush = GetBrush();

        for (var i = 0; i < BarCount; i++)
        {
            // Initial random height between 40-100% of max (16px)
            var initialScale = 0.4 + _random.NextDouble() * 0.6;

            var scaleTransform = new ScaleTransform
            {
                ScaleY = initialScale,
                ScaleX = 1,
            };

            var bar = new Rectangle
            {
                Name = $"Bar{i}",
                Width = 3,
                Height = 16,
                RadiusX = 1.5,
                RadiusY = 1.5,
                Fill = brush,
                VerticalAlignment = VerticalAlignment.Bottom,
                RenderTransform = scaleTransform,
                RenderTransformOrigin = new Windows.Foundation.Point(0.5, 1), // scale from bottom
            };

            BarsPanel.Children.Add(bar);

            // Each bar gets its own storyboard with unique duration (1.2-1.9s)
            var duration = TimeSpan.FromMilliseconds(1200 + _random.Next(700));
            var beginDelay = TimeSpan.FromMilliseconds(i * 150);

            // ScaleY keyframes: 0.3 -> 1.0 -> 0.3 (full ping-pong)
            var scaleAnim = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = beginDelay,
            };
            scaleAnim.KeyFrames.Add(new SplineDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 0.3 });
            scaleAnim.KeyFrames.Add(new SplineDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(duration / 2), Value = 1.0, KeySpline = new KeySpline { ControlPoint1 = new Windows.Foundation.Point(0.42, 0), ControlPoint2 = new Windows.Foundation.Point(0.58, 1) } });
            scaleAnim.KeyFrames.Add(new SplineDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(duration), Value = 0.3, KeySpline = new KeySpline { ControlPoint1 = new Windows.Foundation.Point(0.42, 0), ControlPoint2 = new Windows.Foundation.Point(0.58, 1) } });

            Storyboard.SetTarget(scaleAnim, scaleTransform);
            Storyboard.SetTargetProperty(scaleAnim, "ScaleY");

            // Opacity keyframes: 0.3 -> 1.0 -> 0.3
            var opacityAnim = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = beginDelay,
            };
            opacityAnim.KeyFrames.Add(new SplineDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 0.3 });
            opacityAnim.KeyFrames.Add(new SplineDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(duration / 2), Value = 1.0, KeySpline = new KeySpline { ControlPoint1 = new Windows.Foundation.Point(0.42, 0), ControlPoint2 = new Windows.Foundation.Point(0.58, 1) } });
            opacityAnim.KeyFrames.Add(new SplineDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(duration), Value = 0.3, KeySpline = new KeySpline { ControlPoint1 = new Windows.Foundation.Point(0.42, 0), ControlPoint2 = new Windows.Foundation.Point(0.58, 1) } });

            Storyboard.SetTarget(opacityAnim, bar);
            Storyboard.SetTargetProperty(opacityAnim, "Opacity");

            var sb = new Storyboard();
            sb.Children.Add(scaleAnim);
            sb.Children.Add(opacityAnim);

            _storyboards.Add(sb);
            sb.Begin();
        }
    }

    private SolidColorBrush GetBrush() =>
        new(OrbitalColors.AccentColor(BarColor));
}
