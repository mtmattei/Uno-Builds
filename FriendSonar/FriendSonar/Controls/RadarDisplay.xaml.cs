using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using FriendSonar.Models;

namespace FriendSonar.Controls;

public sealed partial class RadarDisplay : UserControl
{
    public event EventHandler<Friend>? BlipTapped;
    public event EventHandler<Friend>? NavigateRequested;
    public event EventHandler<Friend>? MessageRequested;
    public event EventHandler<Friend>? PingRequested;

    public static readonly double[] AvailableRanges = { 1.0, 3.0, 5.0, 10.0 };

    private double _currentMaxRange = 3.0;
    private double _targetMaxRange = 3.0;
    public double MaxRange => _currentMaxRange;

    private DispatcherTimer? _sweepTimer;
    private double _currentAngle;
    private int _activePingRings;
    private int _tooltipThrottle;

    private readonly List<FriendBlip> _blips = new();
    private static readonly Random _random = new();

    public RadarDisplay()
    {
        this.InitializeComponent();
        this.Loaded += RadarDisplay_Loaded;
        this.Unloaded += RadarDisplay_Unloaded;
    }

    public int CurrentSweepAngle => (int)_currentAngle;

    private void RadarDisplay_Loaded(object sender, RoutedEventArgs e)
    {
        StartSweepAnimation();
    }

    private void RadarDisplay_Unloaded(object sender, RoutedEventArgs e)
    {
        StopSweepAnimation();
    }

    private void StartSweepAnimation()
    {
        StopSweepAnimation();

        _sweepTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _sweepTimer.Tick += SweepTimer_Tick;
        _sweepTimer.Start();
    }

    private void StopSweepAnimation()
    {
        if (_sweepTimer != null)
        {
            _sweepTimer.Stop();
            _sweepTimer.Tick -= SweepTimer_Tick;
            _sweepTimer = null;
        }
    }

    private void SweepTimer_Tick(object? sender, object e)
    {
        _currentAngle += 1.1;
        if (_currentAngle >= 360)
        {
            _currentAngle -= 360;
        }

        SweepRotation.Angle = _currentAngle;
        SweepTrailRotation.Angle = _currentAngle;

        // Echo lines trail the main sweep at fixed offsets
        EchoRotation1.Angle = _currentAngle - 10;
        EchoRotation2.Angle = _currentAngle - 20;
        EchoRotation3.Angle = _currentAngle - 30;

        if (Math.Abs(_currentMaxRange - _targetMaxRange) > 0.01)
        {
            _currentMaxRange += (_targetMaxRange - _currentMaxRange) * 0.1;
            UpdateDistanceLabels();
        }

        CheckPingDetection();
        UpdateBlipPositions();
    }

    public void SetRange(double rangeMiles)
    {
        if (Array.IndexOf(AvailableRanges, rangeMiles) < 0) return;

        _targetMaxRange = rangeMiles;
        _currentMaxRange = rangeMiles;

        RefreshBlips();
    }

    private void RefreshBlips()
    {
        BlipsCanvas.Children.Clear();

        foreach (var blip in _blips)
        {
            blip.Container = null;
            AddBlipToCanvas(blip);
        }
    }

    public async System.Threading.Tasks.Task TriggerFullScanAsync()
    {
        var originalInterval = _sweepTimer?.Interval ?? TimeSpan.FromMilliseconds(30);
        var fastInterval = TimeSpan.FromMilliseconds(8);

        if (_sweepTimer != null)
        {
            _sweepTimer.Interval = fastInterval;
        }

        await System.Threading.Tasks.Task.Delay(1500);

        foreach (var blip in _blips)
        {
            TriggerPingAnimation(blip);
            await System.Threading.Tasks.Task.Delay(100);
        }

        if (_sweepTimer != null)
        {
            _sweepTimer.Interval = originalInterval;
        }
    }

    private void UpdateDistanceLabels()
    {
        var range = _currentMaxRange;
        var step = range / 3.0;

        DistanceLabel1.Text = $"{step:F0}mi";
        DistanceLabel2.Text = $"{step * 2:F0}mi";
        DistanceLabel3.Text = $"{range:F0}mi";
    }

    private void CheckPingDetection()
    {
        var now = DateTime.UtcNow;
        foreach (var blip in _blips)
        {
            if (blip.DistanceMiles > _currentMaxRange) continue;
            if (blip.Container == null) continue;

            // Cooldown: only ping once per sweep pass
            if ((now - blip.LastPingTime).TotalMilliseconds < 500) continue;

            var angleDiff = Math.Abs(blip.Angle - _currentAngle);
            if (angleDiff > 180) angleDiff = 360 - angleDiff;
            if (angleDiff < 2)
            {
                blip.LastPingTime = now;
                TriggerPingAnimation(blip);
            }
        }
    }

    private void TriggerPingAnimation(FriendBlip blip)
    {
        if (blip.Container == null) return;

        // Cap concurrent ping rings to avoid UI element buildup
        if (_activePingRings > 8) return;

        var ring = new Ellipse
        {
            Width = 16,
            Height = 16,
            Stroke = GetStatusBrush(blip.Status),
            StrokeThickness = 2,
            Opacity = 0.8
        };

        var normalizedDistance = blip.DistanceMiles / _currentMaxRange;
        var (x, y) = PolarToCartesian(normalizedDistance, blip.Angle);
        Canvas.SetLeft(ring, x - 8);
        Canvas.SetTop(ring, y - 8);

        _activePingRings++;
        BlipsCanvas.Children.Add(ring);

        var storyboard = new Storyboard();

        var scaleX = new DoubleAnimation
        {
            From = 1.0,
            To = 3.0,
            Duration = TimeSpan.FromMilliseconds(600)
        };

        var scaleY = new DoubleAnimation
        {
            From = 1.0,
            To = 3.0,
            Duration = TimeSpan.FromMilliseconds(600)
        };

        var fade = new DoubleAnimation
        {
            From = 0.8,
            To = 0.0,
            Duration = TimeSpan.FromMilliseconds(600)
        };

        ring.RenderTransform = new ScaleTransform { CenterX = 8, CenterY = 8 };

        Storyboard.SetTarget(scaleX, ring);
        Storyboard.SetTargetProperty(scaleX, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");

        Storyboard.SetTarget(scaleY, ring);
        Storyboard.SetTargetProperty(scaleY, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");

        Storyboard.SetTarget(fade, ring);
        Storyboard.SetTargetProperty(fade, "Opacity");

        storyboard.Children.Add(scaleX);
        storyboard.Children.Add(scaleY);
        storyboard.Children.Add(fade);

        storyboard.Completed += (s, e) =>
        {
            BlipsCanvas.Children.Remove(ring);
            _activePingRings--;
        };
        storyboard.Begin();

        FlashBlip(blip.Container);
    }

    private void FlashBlip(Grid container)
    {
        var storyboard = new Storyboard();

        var brightenX = new DoubleAnimation
        {
            From = 1.0,
            To = 1.5,
            Duration = TimeSpan.FromMilliseconds(100)
        };

        var brightenY = new DoubleAnimation
        {
            From = 1.0,
            To = 1.5,
            Duration = TimeSpan.FromMilliseconds(100)
        };

        var dimX = new DoubleAnimation
        {
            From = 1.5,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(200),
            BeginTime = TimeSpan.FromMilliseconds(100)
        };

        var dimY = new DoubleAnimation
        {
            From = 1.5,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(200),
            BeginTime = TimeSpan.FromMilliseconds(100)
        };

        Storyboard.SetTarget(brightenX, container);
        Storyboard.SetTargetProperty(brightenX, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");

        Storyboard.SetTarget(brightenY, container);
        Storyboard.SetTargetProperty(brightenY, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");

        Storyboard.SetTarget(dimX, container);
        Storyboard.SetTargetProperty(dimX, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");

        Storyboard.SetTarget(dimY, container);
        Storyboard.SetTargetProperty(dimY, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");

        storyboard.Children.Add(brightenX);
        storyboard.Children.Add(brightenY);
        storyboard.Children.Add(dimX);
        storyboard.Children.Add(dimY);
        storyboard.Begin();
    }

    private void UpdateBlipPositions()
    {
        _tooltipThrottle++;
        var updateTooltips = _tooltipThrottle % 30 == 0;

        foreach (var blip in _blips)
        {
            if (blip.Container == null) continue;

            if (!blip.TargetAngle.HasValue || !blip.TargetDistanceMiles.HasValue)
            {
                blip.TargetAngle = blip.Angle + _random.Next(-15, 15);
                blip.TargetDistanceMiles = Math.Max(0.1, blip.DistanceMiles + (_random.NextDouble() - 0.5) * 0.2);
            }

            var angleStep = (blip.TargetAngle.Value - blip.Angle) * 0.003;
            var distanceStep = (blip.TargetDistanceMiles.Value - blip.DistanceMiles) * 0.003;

            blip.Angle += angleStep;
            if (blip.Angle < 0) blip.Angle += 360;
            if (blip.Angle >= 360) blip.Angle -= 360;

            blip.DistanceMiles += distanceStep;
            blip.DistanceMiles = Math.Max(0.1, blip.DistanceMiles);

            var normalizedDistance = blip.DistanceMiles / _currentMaxRange;

            if (normalizedDistance > 1.0)
            {
                blip.Container.Visibility = Visibility.Collapsed;
                continue;
            }

            blip.Container.Visibility = Visibility.Visible;

            var (x, y) = PolarToCartesian(normalizedDistance, blip.Angle);
            Canvas.SetLeft(blip.Container, x - 8);
            Canvas.SetTop(blip.Container, y - 8);

            if (updateTooltips)
            {
                ToolTipService.SetToolTip(blip.Container, blip.GetTooltipText());
            }

            if (Math.Abs(angleStep) < 0.05 && Math.Abs(distanceStep) < 0.001)
            {
                blip.TargetAngle = blip.Angle + _random.Next(-15, 15);
                blip.TargetDistanceMiles = Math.Max(0.1, blip.DistanceMiles + (_random.NextDouble() - 0.5) * 0.2);
            }
        }
    }

    public void AddFriend(int id, string name, double distanceMiles, int angle, FriendStatus status)
    {
        var blip = new FriendBlip
        {
            Id = id,
            Name = name,
            DistanceMiles = distanceMiles,
            Angle = angle,
            Status = status
        };

        _blips.Add(blip);
        AddBlipToCanvas(blip);
    }

    public void ClearFriends()
    {
        _blips.Clear();
        BlipsCanvas.Children.Clear();
    }

    private void AddBlipToCanvas(FriendBlip blip)
    {
        var normalizedDistance = blip.DistanceMiles / _currentMaxRange;
        if (normalizedDistance > 1.0) return;

        var (x, y) = PolarToCartesian(normalizedDistance, blip.Angle);
        var statusBrush = GetStatusBrush(blip.Status);

        var container = new Grid
        {
            Width = 16,
            Height = 16,
            Tag = blip
        };

        var outerRing = new Ellipse
        {
            Width = 16,
            Height = 16,
            Stroke = statusBrush,
            StrokeThickness = 2,
            Fill = null,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var innerDot = new Ellipse
        {
            Width = 4,
            Height = 4,
            Fill = statusBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        container.Children.Add(outerRing);
        container.Children.Add(innerDot);

        ToolTipService.SetToolTip(container, blip.GetTooltipText());

        Canvas.SetLeft(container, x - 8);
        Canvas.SetTop(container, y - 8);

        container.RenderTransform = new ScaleTransform { CenterX = 8, CenterY = 8 };

        blip.Container = container;

        container.Tapped += (s, e) =>
        {
            e.Handled = true;
            BlipTapped?.Invoke(this, CreateFriendFromBlip(blip));
        };

        container.ContextFlyout = CreateBlipContextMenu(blip);

        BlipsCanvas.Children.Add(container);

        StartBlipPulseAnimation(container, blip.Status);
    }

    private (double x, double y) PolarToCartesian(double distance, double angleDegrees)
    {
        const double radarRadius = 150.0;
        const double centerX = 150.0;
        const double centerY = 150.0;

        // 0° at top: subtract 90° before converting to radians
        var angleRadians = (angleDegrees - 90) * Math.PI / 180.0;

        var x = centerX + (distance * radarRadius * Math.Cos(angleRadians));
        var y = centerY + (distance * radarRadius * Math.Sin(angleRadians));

        return (x, y);
    }

    private Brush GetStatusBrush(FriendStatus status)
    {
        return status switch
        {
            FriendStatus.Active => (Brush)Application.Current.Resources["StatusActiveBrush"],
            FriendStatus.Idle => (Brush)Application.Current.Resources["StatusIdleBrush"],
            FriendStatus.Away => (Brush)Application.Current.Resources["StatusAwayBrush"],
            _ => (Brush)Application.Current.Resources["PhosphorGreen100Brush"]
        };
    }

    private void StartBlipPulseAnimation(Grid container, FriendStatus status)
    {
        switch (status)
        {
            case FriendStatus.Active:
                StartActivePulseAnimation(container);
                break;
            case FriendStatus.Idle:
                StartIdleBreathingAnimation(container);
                break;
            case FriendStatus.Away:
                StartAwayFlickerAnimation(container);
                break;
        }
    }

    private void StartActivePulseAnimation(Grid container)
    {
        var storyboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

        var scaleXUp = new DoubleAnimation
        {
            From = 1.0,
            To = 1.15,
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        var scaleYUp = new DoubleAnimation
        {
            From = 1.0,
            To = 1.15,
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        var scaleXDown = new DoubleAnimation
        {
            From = 1.15,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(400),
            BeginTime = TimeSpan.FromMilliseconds(400),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        var scaleYDown = new DoubleAnimation
        {
            From = 1.15,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(400),
            BeginTime = TimeSpan.FromMilliseconds(400),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        Storyboard.SetTarget(scaleXUp, container);
        Storyboard.SetTargetProperty(scaleXUp, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");
        Storyboard.SetTarget(scaleYUp, container);
        Storyboard.SetTargetProperty(scaleYUp, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");
        Storyboard.SetTarget(scaleXDown, container);
        Storyboard.SetTargetProperty(scaleXDown, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");
        Storyboard.SetTarget(scaleYDown, container);
        Storyboard.SetTargetProperty(scaleYDown, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");

        storyboard.Children.Add(scaleXUp);
        storyboard.Children.Add(scaleYUp);
        storyboard.Children.Add(scaleXDown);
        storyboard.Children.Add(scaleYDown);

        storyboard.Begin();
    }

    private void StartIdleBreathingAnimation(Grid container)
    {
        var storyboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

        var scaleXUp = new DoubleAnimation
        {
            From = 1.0,
            To = 1.08,
            Duration = TimeSpan.FromMilliseconds(1500),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var scaleYUp = new DoubleAnimation
        {
            From = 1.0,
            To = 1.08,
            Duration = TimeSpan.FromMilliseconds(1500),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var opacityDown = new DoubleAnimation
        {
            From = 1.0,
            To = 0.7,
            Duration = TimeSpan.FromMilliseconds(1500),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var scaleXDown = new DoubleAnimation
        {
            From = 1.08,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(1500),
            BeginTime = TimeSpan.FromMilliseconds(1500),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var scaleYDown = new DoubleAnimation
        {
            From = 1.08,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(1500),
            BeginTime = TimeSpan.FromMilliseconds(1500),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var opacityUp = new DoubleAnimation
        {
            From = 0.7,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(1500),
            BeginTime = TimeSpan.FromMilliseconds(1500),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        Storyboard.SetTarget(scaleXUp, container);
        Storyboard.SetTargetProperty(scaleXUp, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");
        Storyboard.SetTarget(scaleYUp, container);
        Storyboard.SetTargetProperty(scaleYUp, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");
        Storyboard.SetTarget(opacityDown, container);
        Storyboard.SetTargetProperty(opacityDown, "Opacity");
        Storyboard.SetTarget(scaleXDown, container);
        Storyboard.SetTargetProperty(scaleXDown, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");
        Storyboard.SetTarget(scaleYDown, container);
        Storyboard.SetTargetProperty(scaleYDown, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");
        Storyboard.SetTarget(opacityUp, container);
        Storyboard.SetTargetProperty(opacityUp, "Opacity");

        storyboard.Children.Add(scaleXUp);
        storyboard.Children.Add(scaleYUp);
        storyboard.Children.Add(opacityDown);
        storyboard.Children.Add(scaleXDown);
        storyboard.Children.Add(scaleYDown);
        storyboard.Children.Add(opacityUp);

        storyboard.Begin();
    }

    private void StartAwayFlickerAnimation(Grid container)
    {
        var storyboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

        var holdLow = new DoubleAnimation
        {
            From = 0.5,
            To = 0.5,
            Duration = TimeSpan.FromMilliseconds(2000)
        };

        var flickerOn = new DoubleAnimation
        {
            From = 0.5,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(50),
            BeginTime = TimeSpan.FromMilliseconds(2000)
        };

        var flickerOff = new DoubleAnimation
        {
            From = 1.0,
            To = 0.3,
            Duration = TimeSpan.FromMilliseconds(80),
            BeginTime = TimeSpan.FromMilliseconds(2050)
        };

        var flickerOn2 = new DoubleAnimation
        {
            From = 0.3,
            To = 0.9,
            Duration = TimeSpan.FromMilliseconds(40),
            BeginTime = TimeSpan.FromMilliseconds(2130)
        };

        var settleDown = new DoubleAnimation
        {
            From = 0.9,
            To = 0.5,
            Duration = TimeSpan.FromMilliseconds(300),
            BeginTime = TimeSpan.FromMilliseconds(2170)
        };

        Storyboard.SetTarget(holdLow, container);
        Storyboard.SetTargetProperty(holdLow, "Opacity");
        Storyboard.SetTarget(flickerOn, container);
        Storyboard.SetTargetProperty(flickerOn, "Opacity");
        Storyboard.SetTarget(flickerOff, container);
        Storyboard.SetTargetProperty(flickerOff, "Opacity");
        Storyboard.SetTarget(flickerOn2, container);
        Storyboard.SetTargetProperty(flickerOn2, "Opacity");
        Storyboard.SetTarget(settleDown, container);
        Storyboard.SetTargetProperty(settleDown, "Opacity");

        storyboard.Children.Add(holdLow);
        storyboard.Children.Add(flickerOn);
        storyboard.Children.Add(flickerOff);
        storyboard.Children.Add(flickerOn2);
        storyboard.Children.Add(settleDown);

        storyboard.Begin();
    }

    private Friend CreateFriendFromBlip(FriendBlip blip)
    {
        return new Friend
        {
            Id = Guid.Empty,
            Name = blip.Name,
            DistanceMilesValue = blip.DistanceMiles,
            Angle = (int)blip.Angle,
            LastUpdated = blip.Status == FriendStatus.Active ? DateTime.UtcNow :
                          blip.Status == FriendStatus.Idle ? DateTime.UtcNow.AddMinutes(-3) :
                          DateTime.UtcNow.AddMinutes(-6)
        };
    }

    private MenuFlyout CreateBlipContextMenu(FriendBlip blip)
    {
        var menuFlyout = new MenuFlyout();

        var navigateItem = new MenuFlyoutItem
        {
            Text = "Navigate",
            Icon = new FontIcon { Glyph = "" }
        };
        navigateItem.Click += (s, e) => NavigateRequested?.Invoke(this, CreateFriendFromBlip(blip));

        var messageItem = new MenuFlyoutItem
        {
            Text = "Message",
            Icon = new FontIcon { Glyph = "" }
        };
        messageItem.Click += (s, e) => MessageRequested?.Invoke(this, CreateFriendFromBlip(blip));

        var pingItem = new MenuFlyoutItem
        {
            Text = "Ping",
            Icon = new FontIcon { Glyph = "" }
        };
        pingItem.Click += (s, e) =>
        {
            PingRequested?.Invoke(this, CreateFriendFromBlip(blip));
            if (blip.Container != null) TriggerPingAnimation(blip);
        };

        var viewDetailsItem = new MenuFlyoutItem
        {
            Text = "View Details",
            Icon = new FontIcon { Glyph = "" }
        };
        viewDetailsItem.Click += (s, e) => BlipTapped?.Invoke(this, CreateFriendFromBlip(blip));

        menuFlyout.Items.Add(navigateItem);
        menuFlyout.Items.Add(messageItem);
        menuFlyout.Items.Add(new MenuFlyoutSeparator());
        menuFlyout.Items.Add(pingItem);
        menuFlyout.Items.Add(viewDetailsItem);

        return menuFlyout;
    }

    private class FriendBlip
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double DistanceMiles { get; set; }
        public double Angle { get; set; }
        public FriendStatus Status { get; set; }
        public Grid? Container { get; set; }

        public double? TargetAngle { get; set; }
        public double? TargetDistanceMiles { get; set; }
        public DateTime LastPingTime { get; set; } = DateTime.MinValue;

        public string GetTooltipText()
        {
            var bearing = ((int)Angle).ToString("000");
            return $"{Name} - {DistanceMiles:F1} MI · {bearing}°";
        }
    }
}
