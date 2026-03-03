using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace FriendSonar.Controls;

public sealed partial class CRTOverlay : UserControl
{
    private Storyboard? _scanlineStoryboard;

    public CRTOverlay()
    {
        this.InitializeComponent();
        this.Loaded += CRTOverlay_Loaded;
        this.Unloaded += CRTOverlay_Unloaded;
    }

    private void CRTOverlay_Loaded(object sender, RoutedEventArgs e)
    {
        StartScanlineAnimation();
    }

    private void CRTOverlay_Unloaded(object sender, RoutedEventArgs e)
    {
        StopScanlineAnimation();
    }

    private void StartScanlineAnimation()
    {
        _scanlineStoryboard = new Storyboard
        {
            RepeatBehavior = RepeatBehavior.Forever
        };

        var animation = new DoubleAnimation
        {
            From = -10,
            To = ActualHeight > 0 ? ActualHeight + 10 : 900, // Fallback height
            Duration = TimeSpan.FromSeconds(6)
        };

        Storyboard.SetTarget(animation, MovingScanline);
        Storyboard.SetTargetProperty(animation, "(UIElement.RenderTransform).(TranslateTransform.Y)");

        // Add transform if not present
        if (MovingScanline.RenderTransform == null)
        {
            MovingScanline.RenderTransform = new TranslateTransform();
        }

        _scanlineStoryboard.Children.Add(animation);
        _scanlineStoryboard.Begin();
    }

    private void StopScanlineAnimation()
    {
        _scanlineStoryboard?.Stop();
        _scanlineStoryboard = null;
    }
}
