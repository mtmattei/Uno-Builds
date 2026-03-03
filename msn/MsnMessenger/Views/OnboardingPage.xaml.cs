using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
using MsnMessenger.Helpers;

namespace MsnMessenger.Views;

public sealed partial class OnboardingPage : Page
{
    public OnboardingPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        StartFloatingAnimations();

        // Animate entrance of continue button
        await Task.Delay(300);
        MicroAnimations.AnimateEntrance(ContinueButton, 0);
    }

    private void StartFloatingAnimations()
    {
        StartFloatingAnimation(Circle1Transform, 12, TimeSpan.FromSeconds(4));
        StartFloatingAnimation(Circle2Transform, -8, TimeSpan.FromSeconds(3));
        StartFloatingAnimation(Circle3Transform, 10, TimeSpan.FromSeconds(3.5));
        StartFloatingAnimation(Circle4Transform, -14, TimeSpan.FromSeconds(5));
        StartFloatingAnimation(Circle5Transform, 6, TimeSpan.FromSeconds(2.5));
        StartFloatingAnimation(Circle6Transform, -5, TimeSpan.FromSeconds(2));
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

    private async void OnContinueClick(object sender, RoutedEventArgs e)
    {
        // Animate button press
        await MicroAnimations.AnimatePress(ContinueButton);

        // Navigate directly to sign in
        Frame.Navigate(typeof(SignInPage));
    }
}
