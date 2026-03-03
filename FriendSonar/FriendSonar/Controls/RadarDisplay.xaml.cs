using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.Storage;
using FriendSonar.Models;

namespace FriendSonar.Controls;

public sealed partial class RadarDisplay : UserControl
{
    // Event for when a blip is tapped
    public event EventHandler<Friend>? BlipTapped;
    // Events for long-press actions
    public event EventHandler<Friend>? NavigateRequested;
    public event EventHandler<Friend>? MessageRequested;
    public event EventHandler<Friend>? PingRequested;

    // Range zoom
    private double _currentMaxRange = 3.0; // Default 3 miles
    private double _targetMaxRange = 3.0;
    public static readonly double[] AvailableRanges = { 1.0, 3.0, 5.0, 10.0 };

    public double MaxRange => _currentMaxRange;

    private DispatcherTimer? _sweepTimer;
    private double _currentAngle = 0;
    private List<FriendBlip> _blips = new();
    private RotateTransform? _sweepRotation;
    private RotateTransform? _sweepTrailRotation;
    private RotateTransform? _echoRotation1;
    private RotateTransform? _echoRotation2;
    private RotateTransform? _echoRotation3;
    private Canvas? _blipsCanvas;
    private bool _bootAnimationComplete = false;
    private List<FriendBlip> _pendingBlips = new();
    private static readonly Random _random = new();

    // Named elements for boot animation
    private Ellipse? _ring1;
    private Ellipse? _ring2;
    private Ellipse? _ring3;
    private Ellipse? _ring4;
    private Line? _crosshairH;
    private Line? _crosshairV;
    private Line? _sweepLine;
    private Line? _echoLine1;
    private Line? _echoLine2;
    private Line? _echoLine3;
    private Canvas? _sweepTrailCanvas;

    // Distance labels
    private TextBlock? _distanceLabel1;
    private TextBlock? _distanceLabel2;
    private TextBlock? _distanceLabel3;

    public RadarDisplay()
    {
        this.InitializeComponent();
        this.Loaded += RadarDisplay_Loaded;
        this.Unloaded += RadarDisplay_Unloaded;
    }

    private void RadarDisplay_Loaded(object sender, RoutedEventArgs e)
    {
        // Get references for boot animation elements first
        _ring1 = this.FindName("Ring1") as Ellipse;
        _ring2 = this.FindName("Ring2") as Ellipse;
        _ring3 = this.FindName("Ring3") as Ellipse;
        _ring4 = this.FindName("Ring4") as Ellipse;
        _crosshairH = this.FindName("CrosshairH") as Line;
        _crosshairV = this.FindName("CrosshairV") as Line;
        _sweepLine = this.FindName("SweepLine") as Line;
        _echoLine1 = this.FindName("EchoLine1") as Line;
        _echoLine2 = this.FindName("EchoLine2") as Line;
        _echoLine3 = this.FindName("EchoLine3") as Line;
        _sweepTrailCanvas = this.FindName("SweepTrailCanvas") as Canvas;
        _blipsCanvas = this.FindName("BlipsCanvas") as Canvas;

        // Distance labels
        _distanceLabel1 = this.FindName("DistanceLabel1") as TextBlock;
        _distanceLabel2 = this.FindName("DistanceLabel2") as TextBlock;
        _distanceLabel3 = this.FindName("DistanceLabel3") as TextBlock;

        // Get RotateTransforms directly from elements (more reliable than FindName for nested transforms)
        _sweepRotation = _sweepLine?.RenderTransform as RotateTransform;
        _sweepTrailRotation = _sweepTrailCanvas?.RenderTransform as RotateTransform;
        _echoRotation1 = _echoLine1?.RenderTransform as RotateTransform;
        _echoRotation2 = _echoLine2?.RenderTransform as RotateTransform;
        _echoRotation3 = _echoLine3?.RenderTransform as RotateTransform;

        // Always skip boot animation and start sweep immediately
        _bootAnimationComplete = true;
        StartSweepAnimation();
    }

    private bool IsFirstLaunch()
    {
        try
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            var hasBooted = localSettings.Values["HasBootedBefore"] as bool?;

            if (hasBooted != true)
            {
                localSettings.Values["HasBootedBefore"] = true;
                return true;
            }
            return false;
        }
        catch
        {
            // LocalSettings may not be available on all platforms
            return false;
        }
    }

    private void StartBootAnimation()
    {
        // Initially hide elements
        if (_ring1 != null) _ring1.Opacity = 0;
        if (_ring2 != null) _ring2.Opacity = 0;
        if (_ring3 != null) _ring3.Opacity = 0;
        if (_ring4 != null) _ring4.Opacity = 0;
        if (_crosshairH != null) _crosshairH.Opacity = 0;
        if (_crosshairV != null) _crosshairV.Opacity = 0;
        if (_sweepLine != null) _sweepLine.Opacity = 0;
        if (_echoLine1 != null) _echoLine1.Opacity = 0;
        if (_echoLine2 != null) _echoLine2.Opacity = 0;
        if (_echoLine3 != null) _echoLine3.Opacity = 0;
        if (_sweepTrailCanvas != null) _sweepTrailCanvas.Opacity = 0;

        var storyboard = new Storyboard();

        // Ring animations - scale from 0 to 1 with stagger
        AddRingBootAnimation(storyboard, _ring1, TimeSpan.Zero);
        AddRingBootAnimation(storyboard, _ring2, TimeSpan.FromMilliseconds(100));
        AddRingBootAnimation(storyboard, _ring3, TimeSpan.FromMilliseconds(200));
        AddRingBootAnimation(storyboard, _ring4, TimeSpan.FromMilliseconds(300));

        // Crosshairs fade in
        AddFadeInAnimation(storyboard, _crosshairH, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(300));
        AddFadeInAnimation(storyboard, _crosshairV, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(300));

        // Sweep line and trail fade in
        AddFadeInAnimation(storyboard, _sweepLine, TimeSpan.FromMilliseconds(800), TimeSpan.FromMilliseconds(200));
        AddFadeInAnimation(storyboard, _sweepTrailCanvas, TimeSpan.FromMilliseconds(800), TimeSpan.FromMilliseconds(200));
        AddFadeInAnimation(storyboard, _echoLine1, TimeSpan.FromMilliseconds(850), TimeSpan.FromMilliseconds(150));
        AddFadeInAnimation(storyboard, _echoLine2, TimeSpan.FromMilliseconds(900), TimeSpan.FromMilliseconds(150));
        AddFadeInAnimation(storyboard, _echoLine3, TimeSpan.FromMilliseconds(950), TimeSpan.FromMilliseconds(150));

        storyboard.Completed += (s, e) =>
        {
            _bootAnimationComplete = true;
            StartSweepAnimation();

            // Add any pending blips with fade-in
            foreach (var blip in _pendingBlips)
            {
                AddBlipToCanvas(blip);
            }
            _pendingBlips.Clear();
        };

        storyboard.Begin();
    }

    private void AddRingBootAnimation(Storyboard storyboard, Ellipse? ring, TimeSpan beginTime)
    {
        if (ring == null) return;

        var scaleTransform = ring.RenderTransform as ScaleTransform;
        if (scaleTransform == null) return;

        // Set initial scale to 0
        scaleTransform.ScaleX = 0;
        scaleTransform.ScaleY = 0;

        var scaleX = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(400),
            BeginTime = beginTime,
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
        };

        var scaleY = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(400),
            BeginTime = beginTime,
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
        };

        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = ring == _ring2 ? 0.8 : ring == _ring3 ? 0.6 : ring == _ring4 ? 0.4 : 1.0,
            Duration = TimeSpan.FromMilliseconds(300),
            BeginTime = beginTime
        };

        Storyboard.SetTarget(scaleX, ring);
        Storyboard.SetTargetProperty(scaleX, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");
        Storyboard.SetTarget(scaleY, ring);
        Storyboard.SetTargetProperty(scaleY, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");
        Storyboard.SetTarget(fadeIn, ring);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");

        storyboard.Children.Add(scaleX);
        storyboard.Children.Add(scaleY);
        storyboard.Children.Add(fadeIn);
    }

    private void AddFadeInAnimation(Storyboard storyboard, UIElement? element, TimeSpan beginTime, TimeSpan duration)
    {
        if (element == null) return;

        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = duration,
            BeginTime = beginTime,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        Storyboard.SetTarget(fadeIn, element);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");

        storyboard.Children.Add(fadeIn);
    }

    private void RadarDisplay_Unloaded(object sender, RoutedEventArgs e)
    {
        StopSweepAnimation();
    }

    private void StartSweepAnimation()
    {
        StopSweepAnimation();

        if (_sweepRotation == null) return;

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

        if (_sweepRotation != null) _sweepRotation.Angle = _currentAngle;
        if (_sweepTrailRotation != null) _sweepTrailRotation.Angle = _currentAngle;

        // Echo lines follow behind the main sweep at fixed offsets
        if (_echoRotation1 != null) _echoRotation1.Angle = _currentAngle - 10;
        if (_echoRotation2 != null) _echoRotation2.Angle = _currentAngle - 20;
        if (_echoRotation3 != null) _echoRotation3.Angle = _currentAngle - 30;

        // Smooth range interpolation
        if (Math.Abs(_currentMaxRange - _targetMaxRange) > 0.01)
        {
            _currentMaxRange += (_targetMaxRange - _currentMaxRange) * 0.1; // Smooth easing
            UpdateDistanceLabels();
        }

        // Check for sweep passing over blips and trigger ping animation
        CheckPingDetection();

        // Update blip positions for live simulation
        UpdateBlipPositions();
    }

    public int CurrentSweepAngle => (int)_currentAngle;

    public void SetRange(double rangeMiles)
    {
        if (Array.IndexOf(AvailableRanges, rangeMiles) < 0) return;

        _targetMaxRange = rangeMiles;
        _currentMaxRange = rangeMiles; // Update immediately for filtering

        // Refresh all blips to show/hide based on new range
        RefreshBlips();
    }

    private void RefreshBlips()
    {
        if (_blipsCanvas == null) return;

        // Clear canvas and re-add all blips
        _blipsCanvas.Children.Clear();

        foreach (var blip in _blips)
        {
            blip.Container = null; // Reset container reference
            AddBlipToCanvas(blip);
        }
    }

    public async System.Threading.Tasks.Task TriggerFullScanAsync()
    {
        // Speed up the sweep for a "full scan" effect
        var originalInterval = _sweepTimer?.Interval ?? TimeSpan.FromMilliseconds(30);
        var fastInterval = TimeSpan.FromMilliseconds(8); // Much faster rotation

        if (_sweepTimer != null)
        {
            _sweepTimer.Interval = fastInterval;
        }

        // Wait for approximately one full revolution at the fast speed
        await System.Threading.Tasks.Task.Delay(1500);

        // Ping all blips
        foreach (var blip in _blips)
        {
            TriggerPingAnimation(blip);
            await System.Threading.Tasks.Task.Delay(100); // Stagger the pings
        }

        // Return to normal speed
        if (_sweepTimer != null)
        {
            _sweepTimer.Interval = originalInterval;
        }
    }

    private void UpdateDistanceLabels()
    {
        // Update the distance labels based on current range
        var range = _currentMaxRange;
        var step = range / 3.0;

        if (_distanceLabel1 != null)
            _distanceLabel1.Text = $"{step:F0}mi";
        if (_distanceLabel2 != null)
            _distanceLabel2.Text = $"{step * 2:F0}mi";
        if (_distanceLabel3 != null)
            _distanceLabel3.Text = $"{range:F0}mi";
    }

    private void CheckPingDetection()
    {
        var now = DateTime.UtcNow;
        foreach (var blip in _blips)
        {
            if (blip.DistanceMiles > _currentMaxRange) continue;
            if (blip.Container == null) continue;

            // Cooldown: only ping once per sweep pass (at least 500ms between pings)
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

    private int _activePingRings;

    private void TriggerPingAnimation(FriendBlip blip)
    {
        if (_blipsCanvas == null || blip.Container == null) return;

        // Limit concurrent ping rings to prevent UI element buildup
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
        _blipsCanvas.Children.Add(ring);

        // Animate ring expansion and fade
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
            _blipsCanvas.Children.Remove(ring);
            _activePingRings--;
        };
        storyboard.Begin();

        // Flash the blip container
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

    private int _tooltipThrottle;

    private void UpdateBlipPositions()
    {
        _tooltipThrottle++;
        var updateTooltips = _tooltipThrottle % 30 == 0; // Update tooltips ~twice per second

        foreach (var blip in _blips)
        {
            if (blip.Container == null) continue;

            // Update target position (very slow drift)
            if (!blip.TargetAngle.HasValue || !blip.TargetDistanceMiles.HasValue)
            {
                blip.TargetAngle = blip.Angle + _random.Next(-15, 15);
                blip.TargetDistanceMiles = Math.Max(0.1, blip.DistanceMiles + (_random.NextDouble() - 0.5) * 0.2);
            }

            // Interpolate towards target using doubles for smooth sub-pixel movement
            var angleStep = (blip.TargetAngle.Value - blip.Angle) * 0.003;
            var distanceStep = (blip.TargetDistanceMiles.Value - blip.DistanceMiles) * 0.003;

            blip.Angle += angleStep;
            if (blip.Angle < 0) blip.Angle += 360;
            if (blip.Angle >= 360) blip.Angle -= 360;

            blip.DistanceMiles += distanceStep;
            blip.DistanceMiles = Math.Max(0.1, blip.DistanceMiles);

            // Convert miles to normalized distance for display
            var normalizedDistance = blip.DistanceMiles / _currentMaxRange;

            // Hide if outside range
            if (normalizedDistance > 1.0)
            {
                blip.Container.Visibility = Visibility.Collapsed;
                continue;
            }
            else
            {
                blip.Container.Visibility = Visibility.Visible;
            }

            // Update visual position
            var (x, y) = PolarToCartesian(normalizedDistance, blip.Angle);
            Canvas.SetLeft(blip.Container, x - 8);
            Canvas.SetTop(blip.Container, y - 8);

            // Throttle tooltip updates to reduce allocations
            if (updateTooltips)
            {
                ToolTipService.SetToolTip(blip.Container, blip.GetTooltipText());
            }

            // Check if reached target
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

        // If boot animation is still running, queue the blip for later
        if (!_bootAnimationComplete)
        {
            _pendingBlips.Add(blip);
        }
        else
        {
            AddBlipToCanvas(blip);
        }
    }

    public void ClearFriends()
    {
        _blips.Clear();
        _blipsCanvas?.Children.Clear();
    }

    private void AddBlipToCanvas(FriendBlip blip)
    {
        if (_blipsCanvas == null) return;

        // Convert miles to normalized distance (0-1) based on current range
        var normalizedDistance = blip.DistanceMiles / _currentMaxRange;

        // Hide blips outside the current range
        if (normalizedDistance > 1.0)
        {
            return; // Don't add blips outside current range
        }

        // Convert polar to cartesian coordinates
        var (x, y) = PolarToCartesian(normalizedDistance, blip.Angle);

        var statusBrush = GetStatusBrush(blip.Status);

        // Create container grid for the blip components
        var container = new Grid
        {
            Width = 16,
            Height = 16,
            Tag = blip
        };

        // Outer hollow ring (stroke only, no fill)
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

        // Inner dot (smaller filled ellipse)
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

        // Add tooltip
        ToolTipService.SetToolTip(container, blip.GetTooltipText());

        // Position the blip (center the 16px container)
        Canvas.SetLeft(container, x - 8);
        Canvas.SetTop(container, y - 8);

        // Add scale transform for pulse animation
        var scaleTransform = new ScaleTransform
        {
            CenterX = 8,
            CenterY = 8
        };
        container.RenderTransform = scaleTransform;

        // Store references
        blip.Container = container;
        blip.OuterRing = outerRing;
        blip.InnerDot = innerDot;

        // Add tap handler for blip selection
        container.Tapped += (s, e) =>
        {
            e.Handled = true;
            var friend = CreateFriendFromBlip(blip);
            BlipTapped?.Invoke(this, friend);
        };

        // Add context menu for long-press actions
        var menuFlyout = CreateBlipContextMenu(blip);
        container.ContextFlyout = menuFlyout;

        _blipsCanvas.Children.Add(container);

        // Start pulse animation
        StartBlipPulseAnimation(container, blip.Status);
    }

    private (double x, double y) PolarToCartesian(double distance, double angleDegrees)
    {
        const double radarRadius = 150.0; // Half of 300px
        const double centerX = 150.0;
        const double centerY = 150.0;

        // Convert to radians and adjust for 0° at top (subtract 90°)
        double angleRadians = (angleDegrees - 90) * Math.PI / 180.0;

        double x = centerX + (distance * radarRadius * Math.Cos(angleRadians));
        double y = centerY + (distance * radarRadius * Math.Sin(angleRadians));

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
        // Active: Gentle, quick pulse animation
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
        // Idle: Slow, subtle breathing animation
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
        // Away: Occasional flicker like weak signal
        var storyboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

        // Hold at low opacity
        var holdLow = new DoubleAnimation
        {
            From = 0.5,
            To = 0.5,
            Duration = TimeSpan.FromMilliseconds(2000)
        };

        // Quick flicker on
        var flickerOn = new DoubleAnimation
        {
            From = 0.5,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(50),
            BeginTime = TimeSpan.FromMilliseconds(2000)
        };

        // Quick flicker off
        var flickerOff = new DoubleAnimation
        {
            From = 1.0,
            To = 0.3,
            Duration = TimeSpan.FromMilliseconds(80),
            BeginTime = TimeSpan.FromMilliseconds(2050)
        };

        // Second flicker on
        var flickerOn2 = new DoubleAnimation
        {
            From = 0.3,
            To = 0.9,
            Duration = TimeSpan.FromMilliseconds(40),
            BeginTime = TimeSpan.FromMilliseconds(2130)
        };

        // Settle back to dim
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
            Id = Guid.Empty, // Blip uses internal int Id
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
            Icon = new FontIcon { Glyph = "\uE707" }
        };
        navigateItem.Click += (s, e) =>
        {
            NavigateRequested?.Invoke(this, CreateFriendFromBlip(blip));
        };

        var messageItem = new MenuFlyoutItem
        {
            Text = "Message",
            Icon = new FontIcon { Glyph = "\uE715" }
        };
        messageItem.Click += (s, e) =>
        {
            MessageRequested?.Invoke(this, CreateFriendFromBlip(blip));
        };

        var pingItem = new MenuFlyoutItem
        {
            Text = "Ping",
            Icon = new FontIcon { Glyph = "\uE71C" }
        };
        pingItem.Click += (s, e) =>
        {
            PingRequested?.Invoke(this, CreateFriendFromBlip(blip));
            TriggerManualPing(blip);
        };

        var viewDetailsItem = new MenuFlyoutItem
        {
            Text = "View Details",
            Icon = new FontIcon { Glyph = "\uE946" }
        };
        viewDetailsItem.Click += (s, e) =>
        {
            BlipTapped?.Invoke(this, CreateFriendFromBlip(blip));
        };

        menuFlyout.Items.Add(navigateItem);
        menuFlyout.Items.Add(messageItem);
        menuFlyout.Items.Add(new MenuFlyoutSeparator());
        menuFlyout.Items.Add(pingItem);
        menuFlyout.Items.Add(viewDetailsItem);

        return menuFlyout;
    }

    private void TriggerManualPing(FriendBlip blip)
    {
        // Trigger a ping animation on the blip
        if (blip.Container != null)
        {
            TriggerPingAnimation(blip);
        }
    }

    private class FriendBlip
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double DistanceMiles { get; set; }
        public double Angle { get; set; }
        public FriendStatus Status { get; set; }
        public Grid? Container { get; set; }
        public Ellipse? OuterRing { get; set; }
        public Ellipse? InnerDot { get; set; }

        // Movement simulation
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
