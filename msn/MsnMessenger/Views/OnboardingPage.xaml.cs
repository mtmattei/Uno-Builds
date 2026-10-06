using Microsoft.UI.Xaml.Media.Animation;
using MsnMessenger.Helpers;

namespace MsnMessenger.Views;

public sealed partial class OnboardingPage : Page
{
    private readonly List<Storyboard> _ambientStoryboards = new();
    private bool _ambientStarted;

    public OnboardingPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        StartFloatingAnimations();

        await Task.Delay(300);
        MicroAnimations.AnimateEntrance(ContinueButton, 0);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        foreach (var sb in _ambientStoryboards)
        {
            sb.Stop();
        }
        _ambientStoryboards.Clear();
        _ambientStarted = false;
    }

    private void StartFloatingAnimations()
    {
        if (_ambientStarted) return;
        _ambientStarted = true;

        StartBob(Circle1Transform, 12, TimeSpan.FromSeconds(4));
        StartBob(Circle2Transform, -8, TimeSpan.FromSeconds(3));
        StartBob(Circle3Transform, 10, TimeSpan.FromSeconds(3.5));
        StartBob(Circle4Transform, -14, TimeSpan.FromSeconds(5));
        StartBob(Circle5Transform, 6, TimeSpan.FromSeconds(2.5));
        StartBob(Circle6Transform, -5, TimeSpan.FromSeconds(2));
    }

    private void StartBob(TranslateTransform transform, double targetY, TimeSpan halfDuration)
    {
        var ease = new SineEase { EasingMode = EasingMode.EaseInOut };
        var anim = new DoubleAnimationUsingKeyFrames();
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.Zero, Value = 0, EasingFunction = ease });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = halfDuration, Value = targetY, EasingFunction = ease });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = halfDuration + halfDuration, Value = 0, EasingFunction = ease });
        Storyboard.SetTarget(anim, transform);
        Storyboard.SetTargetProperty(anim, "Y");

        var sb = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };
        sb.Children.Add(anim);
        _ambientStoryboards.Add(sb);
        sb.Begin();
    }

    private async void OnContinueClick(object sender, RoutedEventArgs e)
    {
        await MicroAnimations.AnimatePress(ContinueButton);
        Frame.Navigate(typeof(SignInPage));
    }
}
