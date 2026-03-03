using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
using System;
using Windows.UI;
using Unoblueprint.Models;

namespace Unoblueprint.Controls;

public sealed partial class InstallButton : UserControl
{
    public static readonly DependencyProperty IsInstalledProperty =
        DependencyProperty.Register(
            nameof(IsInstalled),
            typeof(bool),
            typeof(InstallButton),
            new PropertyMetadata(false, OnIsInstalledChanged));

    public bool IsInstalled
    {
        get => (bool)GetValue(IsInstalledProperty);
        set => SetValue(IsInstalledProperty, value);
    }

    public event EventHandler<InstallStateChangedEventArgs>? InstallStateChanged;

    private InstallState _currentState = InstallState.NotInstalled;

    public InstallButton()
    {
        this.InitializeComponent();
        this.Loaded += (s, e) => UpdateVisualState(false);
    }

    private static void OnIsInstalledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InstallButton button)
        {
            button.UpdateVisualState(true);
        }
    }

    private void UpdateVisualState(bool useTransitions)
    {
        var newState = IsInstalled ? InstallState.Installed : InstallState.NotInstalled;
        var oldState = _currentState;
        _currentState = newState;

        VisualStateManager.GoToState(this, newState.ToString(), useTransitions);

        if (oldState != newState)
        {
            InstallStateChanged?.Invoke(this, new InstallStateChangedEventArgs
            {
                NewState = newState,
                OldState = oldState
            });
        }
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        // Always trigger the micro-interaction effects
        CreateRippleEffect();
        CreateParticleBurst();

        // Toggle the installed state
        IsInstalled = !IsInstalled;
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(this, "PointerOver", true);
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(this, "Normal", true);
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(this, "Pressed", true);
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(this, "Normal", true);
    }

    private void CreateRippleEffect()
    {
        // Create multiple ripples for a stronger effect
        for (int i = 0; i < 3; i++)
        {
            var ripple = new Ellipse
            {
                Width = 110,
                Height = 40,
                Fill = new SolidColorBrush(Color.FromArgb(102, 196, 255, 13)), // #66C4FF0D (40% opacity)
                RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5),
                RenderTransform = new ScaleTransform()
            };

            Canvas.SetLeft(ripple, (RippleCanvas.Width - 110) / 2);
            Canvas.SetTop(ripple, (RippleCanvas.Height - 40) / 2);

            RippleCanvas.Children.Add(ripple);

            var storyboard = new Storyboard();
            var delay = i * 100; // Stagger each ripple by 100ms

            var scaleXAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 2.5,
                Duration = TimeSpan.FromMilliseconds(800),
                BeginTime = TimeSpan.FromMilliseconds(delay),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleXAnimation, ripple);
            Storyboard.SetTargetProperty(scaleXAnimation, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");

            var scaleYAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 2.5,
                Duration = TimeSpan.FromMilliseconds(800),
                BeginTime = TimeSpan.FromMilliseconds(delay),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleYAnimation, ripple);
            Storyboard.SetTargetProperty(scaleYAnimation, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");

            var opacityAnimation = new DoubleAnimation
            {
                From = 0.8,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(800),
                BeginTime = TimeSpan.FromMilliseconds(delay),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(opacityAnimation, ripple);
            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");

            storyboard.Children.Add(scaleXAnimation);
            storyboard.Children.Add(scaleYAnimation);
            storyboard.Children.Add(opacityAnimation);

            storyboard.Completed += (s, e) =>
            {
                RippleCanvas.Children.Remove(ripple);
            };

            storyboard.Begin();
        }
    }

    private void CreateParticleBurst()
    {
        var colors = new[]
        {
            Color.FromArgb(255, 196, 255, 13),  // #C4FF0D
            Color.FromArgb(255, 255, 71, 87),   // #FF4757
            Color.FromArgb(255, 176, 232, 12)   // #B0E80C
        };

        var random = new Random();
        var centerX = ParticleCanvas.Width / 2;
        var centerY = ParticleCanvas.Height / 2;

        for (int i = 0; i < 8; i++)
        {
            var angle = (i * 45) * Math.PI / 180; // 8 particles evenly distributed
            var distance = 50;

            var particle = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = new SolidColorBrush(colors[random.Next(colors.Length)]),
                RenderTransform = new CompositeTransform()
            };

            Canvas.SetLeft(particle, centerX - 3);
            Canvas.SetTop(particle, centerY - 3);

            ParticleCanvas.Children.Add(particle);

            var storyboard = new Storyboard();

            var translateX = new DoubleAnimation
            {
                From = 0,
                To = Math.Cos(angle) * distance,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(translateX, particle);
            Storyboard.SetTargetProperty(translateX, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");

            var translateY = new DoubleAnimation
            {
                From = 0,
                To = Math.Sin(angle) * distance,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(translateY, particle);
            Storyboard.SetTargetProperty(translateY, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");

            var opacity = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                BeginTime = TimeSpan.FromMilliseconds(200),
                Duration = TimeSpan.FromMilliseconds(300)
            };
            Storyboard.SetTarget(opacity, particle);
            Storyboard.SetTargetProperty(opacity, "Opacity");

            storyboard.Children.Add(translateX);
            storyboard.Children.Add(translateY);
            storyboard.Children.Add(opacity);

            storyboard.Completed += (s, e) =>
            {
                ParticleCanvas.Children.Remove(particle);
            };

            storyboard.Begin();
        }
    }
}
