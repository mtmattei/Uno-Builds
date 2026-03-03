using Microsoft.UI.Xaml.Media.Animation;
using MsnMessenger.Helpers;
using MsnMessenger.Models;

namespace MsnMessenger.Views;

public sealed partial class SignInPage : Page
{
    public SignInPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        StartFloatingAnimations();

        // Animate entrance of sign in button
        await Task.Delay(200);
        MicroAnimations.AnimateEntrance(SignInButton, 0);
    }

    private void StartFloatingAnimations()
    {
        StartFloatingAnimation(Circle1Transform, 10, TimeSpan.FromSeconds(4));
        StartFloatingAnimation(Circle2Transform, -8, TimeSpan.FromSeconds(3.2));
        StartFloatingAnimation(Circle3Transform, 12, TimeSpan.FromSeconds(3.8));
        StartFloatingAnimation(Circle4Transform, -10, TimeSpan.FromSeconds(4.5));
    }

    private void StartFloatingAnimation(TranslateTransform transform, double targetY, TimeSpan duration)
    {
        AnimateToValue(transform, targetY, duration);
    }

    private void AnimateToValue(TranslateTransform transform, double targetY, TimeSpan duration)
    {
        var animation = new DoubleAnimation
        {
            To = targetY,
            Duration = new Duration(duration),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, transform);
        Storyboard.SetTargetProperty(animation, "Y");

        storyboard.Completed += (s, e) =>
        {
            // Reverse direction: if we went to targetY, go back to 0, and vice versa
            var nextTarget = transform.Y == 0 ? targetY : 0;
            AnimateToValue(transform, nextTarget, duration);
        };

        storyboard.Begin();
    }

    private async void OnSignInClick(object sender, RoutedEventArgs e)
    {
        // Animate button press
        await MicroAnimations.AnimatePress(SignInButton);

        // Get selected status
        var selectedStatus = StatusComboBox.SelectedIndex switch
        {
            0 => PresenceStatus.Online,
            1 => PresenceStatus.Away,
            2 => PresenceStatus.Busy,
            3 => PresenceStatus.Offline,
            _ => PresenceStatus.Online
        };

        // For now, just navigate to main page
        // In a real app, you'd validate credentials here
        Frame.Navigate(typeof(MainPage), selectedStatus);
    }
}
