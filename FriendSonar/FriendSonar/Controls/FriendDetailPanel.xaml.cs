using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using FriendSonar.Models;

namespace FriendSonar.Controls;

public sealed partial class FriendDetailPanel : UserControl
{
    private bool _isOpen = false;
    private Friend? _currentFriend;
    private const double PanelHeight = 300;

    public event EventHandler<Friend>? NavigateRequested;
    public event EventHandler<Friend>? MessageRequested;
    public event EventHandler<Friend>? CallRequested;

    public FriendDetailPanel()
    {
        this.InitializeComponent();
        this.Visibility = Visibility.Collapsed;
    }

    public bool IsOpen => _isOpen;

    public void ShowFriend(Friend friend)
    {
        _currentFriend = friend;

        // Update UI with friend data
        AvatarInitials.Text = friend.Initials;
        FriendName.Text = friend.Name;
        StatusText.Text = friend.Status.ToString().ToUpperInvariant();
        DistanceText.Text = friend.DistanceMiles;
        BearingText.Text = friend.BearingDegrees;
        LastSeenText.Text = "NOW";

        // Update status dot color
        StatusDot.Fill = friend.Status switch
        {
            FriendStatus.Active => (Brush)Application.Current.Resources["StatusActiveBrush"],
            FriendStatus.Idle => (Brush)Application.Current.Resources["StatusIdleBrush"],
            FriendStatus.Away => (Brush)Application.Current.Resources["StatusAwayBrush"],
            _ => (Brush)Application.Current.Resources["PhosphorGreen100Brush"]
        };

        // Show and animate
        this.Visibility = Visibility.Visible;
        AnimateOpen();
    }

    public void Hide()
    {
        AnimateClose();
    }

    private void AnimateOpen()
    {
        _isOpen = true;

        var storyboard = new Storyboard();

        // Slide panel up
        var slideUp = new DoubleAnimation
        {
            From = PanelHeight,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(slideUp, PanelContainer);
        Storyboard.SetTargetProperty(slideUp, "(UIElement.RenderTransform).(TranslateTransform.Y)");

        // Fade in dim overlay
        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(200)
        };
        Storyboard.SetTarget(fadeIn, DimOverlay);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");

        storyboard.Children.Add(slideUp);
        storyboard.Children.Add(fadeIn);
        storyboard.Begin();
    }

    private void AnimateClose()
    {
        var storyboard = new Storyboard();

        // Slide panel down
        var slideDown = new DoubleAnimation
        {
            From = 0,
            To = PanelHeight,
            Duration = TimeSpan.FromMilliseconds(250),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        Storyboard.SetTarget(slideDown, PanelContainer);
        Storyboard.SetTargetProperty(slideDown, "(UIElement.RenderTransform).(TranslateTransform.Y)");

        // Fade out dim overlay
        var fadeOut = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(200)
        };
        Storyboard.SetTarget(fadeOut, DimOverlay);
        Storyboard.SetTargetProperty(fadeOut, "Opacity");

        storyboard.Completed += (s, e) =>
        {
            _isOpen = false;
            this.Visibility = Visibility.Collapsed;
        };

        storyboard.Children.Add(slideDown);
        storyboard.Children.Add(fadeOut);
        storyboard.Begin();
    }

    private void DimOverlay_Tapped(object sender, TappedRoutedEventArgs e)
    {
        Hide();
    }

    private void Panel_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
        // Only allow dragging down
        var newY = PanelTranslate.Y + e.Delta.Translation.Y;
        if (newY >= 0)
        {
            PanelTranslate.Y = newY;

            // Update dim overlay opacity based on panel position
            var progress = newY / PanelHeight;
            DimOverlay.Opacity = 1 - progress;
        }
    }

    private void Panel_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        // If dragged more than 1/3 of panel height or with velocity, close it
        if (PanelTranslate.Y > PanelHeight / 3 || e.Velocities.Linear.Y > 0.5)
        {
            AnimateClose();
        }
        else
        {
            // Snap back to open position
            AnimateOpen();
        }
    }

    private void NavigateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentFriend != null)
        {
            NavigateRequested?.Invoke(this, _currentFriend);
        }
    }

    private void MessageButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentFriend != null)
        {
            MessageRequested?.Invoke(this, _currentFriend);
        }
    }

    private void CallButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentFriend != null)
        {
            CallRequested?.Invoke(this, _currentFriend);
        }
    }
}
