using Microsoft.UI.Xaml.Media.Animation;
using MsnMessenger.Helpers;
using MsnMessenger.Models;

namespace MsnMessenger.Views;

public sealed partial class SignInPage : Page
{
    private readonly List<Storyboard> _ambientStoryboards = new();
    private bool _ambientStarted;

    public SignInPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        StartFloatingAnimations();

        await Task.Delay(200);
        MicroAnimations.AnimateEntrance(SignInButton, 0);
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

        StartBob(Circle1Transform, 10, TimeSpan.FromSeconds(4));
        StartBob(Circle2Transform, -8, TimeSpan.FromSeconds(3.2));
        StartBob(Circle3Transform, 12, TimeSpan.FromSeconds(3.8));
        StartBob(Circle4Transform, -10, TimeSpan.FromSeconds(4.5));
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

    private async void OnSignInClick(object sender, RoutedEventArgs e)
    {
        await MicroAnimations.AnimatePress(SignInButton);

        var selectedStatus = StatusComboBox.SelectedIndex switch
        {
            0 => PresenceStatus.Online,
            1 => PresenceStatus.Away,
            2 => PresenceStatus.Busy,
            3 => PresenceStatus.Offline,
            _ => PresenceStatus.Online,
        };

        Frame.Navigate(typeof(MainPage), selectedStatus);
    }
}
