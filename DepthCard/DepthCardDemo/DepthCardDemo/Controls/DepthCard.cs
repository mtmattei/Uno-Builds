using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Foundation;
using DepthCardDemo.Helpers;

namespace DepthCardDemo.Controls;

/// <summary>
/// A 3D interactive card component that creates a diorama-like parallax effect.
/// The card tilts toward the cursor position while internal DepthLayer elements
/// separate along the Z-axis based on their assigned depth values.
/// </summary>
public partial class DepthCard : ContentControl
{
    private bool _isHovered;
    private double _rotateX;
    private double _rotateY;
    private CompositeTransform? _cardTransform;
    private Storyboard? _resetStoryboard;

    // Removed: Glare and Edge Lighting overlays (disabled - GlareOpacity=0, EnableEdgeLighting=False)

    // Living Presence (Breathing)
    private Storyboard? _breathingStoryboard;
    private readonly Random _random = new Random();

    // Removed: Layer caching system (unused - no DepthLayers or magnetic elements in use)

    // Performance: Pointer throttling
    private DateTime _lastPointerUpdate = DateTime.MinValue;

    #region Dependency Properties

    public static readonly DependencyProperty IntensityProperty =
        DependencyProperty.Register(
            nameof(Intensity),
            typeof(double),
            typeof(DepthCard),
            new PropertyMetadata(15.0));

    /// <summary>
    /// Maximum tilt angle in degrees. Default: 15
    /// </summary>
    public double Intensity
    {
        get => (double)GetValue(IntensityProperty);
        set => SetValue(IntensityProperty, value);
    }

    public static readonly DependencyProperty DepthScaleProperty =
        DependencyProperty.Register(
            nameof(DepthScale),
            typeof(double),
            typeof(DepthCard),
            new PropertyMetadata(50.0));

    /// <summary>
    /// Z-axis separation multiplier for parallax effect. Default: 50
    /// </summary>
    public double DepthScale
    {
        get => (double)GetValue(DepthScaleProperty);
        set => SetValue(DepthScaleProperty, value);
    }

    public static readonly DependencyProperty GlareOpacityProperty =
        DependencyProperty.Register(
            nameof(GlareOpacity),
            typeof(double),
            typeof(DepthCard),
            new PropertyMetadata(0.2));

    /// <summary>
    /// Maximum opacity of the glare effect (0-1). Default: 0.2
    /// </summary>
    public double GlareOpacity
    {
        get => (double)GetValue(GlareOpacityProperty);
        set => SetValue(GlareOpacityProperty, value);
    }

    public static readonly DependencyProperty GlareColorProperty =
        DependencyProperty.Register(
            nameof(GlareColor),
            typeof(Windows.UI.Color),
            typeof(DepthCard),
            new PropertyMetadata(Windows.UI.Color.FromArgb(255, 255, 255, 255)));

    /// <summary>
    /// Color of the glare gradient. Default: White
    /// </summary>
    public Windows.UI.Color GlareColor
    {
        get => (Windows.UI.Color)GetValue(GlareColorProperty);
        set => SetValue(GlareColorProperty, value);
    }

    public static readonly DependencyProperty IsInteractiveProperty =
        DependencyProperty.Register(
            nameof(IsInteractive),
            typeof(bool),
            typeof(DepthCard),
            new PropertyMetadata(true));

    /// <summary>
    /// Whether the card responds to pointer interactions. Default: true
    /// </summary>
    public bool IsInteractive
    {
        get => (bool)GetValue(IsInteractiveProperty);
        set => SetValue(IsInteractiveProperty, value);
    }

    public static readonly DependencyProperty EnableEdgeLightingProperty =
        DependencyProperty.Register(
            nameof(EnableEdgeLighting),
            typeof(bool),
            typeof(DepthCard),
            new PropertyMetadata(false));

    /// <summary>
    /// Whether the edge lighting effect is enabled. Default: false
    /// </summary>
    public bool EnableEdgeLighting
    {
        get => (bool)GetValue(EnableEdgeLightingProperty);
        set => SetValue(EnableEdgeLightingProperty, value);
    }

    public static readonly DependencyProperty EdgeLightIntensityProperty =
        DependencyProperty.Register(
            nameof(EdgeLightIntensity),
            typeof(double),
            typeof(DepthCard),
            new PropertyMetadata(DepthCardConstants.DEFAULT_EDGE_LIGHT_INTENSITY));

    /// <summary>
    /// Intensity of the edge light effect (0-1). Default: 0.3
    /// </summary>
    public double EdgeLightIntensity
    {
        get => (double)GetValue(EdgeLightIntensityProperty);
        set => SetValue(EdgeLightIntensityProperty, value);
    }

    public static readonly DependencyProperty EdgeLightColorProperty =
        DependencyProperty.Register(
            nameof(EdgeLightColor),
            typeof(Windows.UI.Color),
            typeof(DepthCard),
            new PropertyMetadata(Windows.UI.Color.FromArgb(255, 255, 255, 255)));

    /// <summary>
    /// Color of the edge light. Default: White
    /// </summary>
    public Windows.UI.Color EdgeLightColor
    {
        get => (Windows.UI.Color)GetValue(EdgeLightColorProperty);
        set => SetValue(EdgeLightColorProperty, value);
    }

    public static readonly DependencyProperty EnableBreathingEffectProperty =
        DependencyProperty.Register(
            nameof(EnableBreathingEffect),
            typeof(bool),
            typeof(DepthCard),
            new PropertyMetadata(true, OnBreathingEffectPropertyChanged));

    /// <summary>
    /// Whether the subtle breathing animation is enabled when idle. Default: true
    /// </summary>
    public bool EnableBreathingEffect
    {
        get => (bool)GetValue(EnableBreathingEffectProperty);
        set => SetValue(EnableBreathingEffectProperty, value);
    }

    private static void OnBreathingEffectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DepthCard card)
        {
            if ((bool)e.NewValue)
                card.StartBreathingAnimation();
            else
                card.StopBreathingAnimation();
        }
    }

    public static readonly DependencyProperty BreathingCycleProperty =
        DependencyProperty.Register(
            nameof(BreathingCycle),
            typeof(double),
            typeof(DepthCard),
            new PropertyMetadata(DepthCardConstants.DEFAULT_BREATHING_CYCLE_MS));

    /// <summary>
    /// Duration of one breathing cycle in milliseconds. Default: 4000ms
    /// </summary>
    public double BreathingCycle
    {
        get => (double)GetValue(BreathingCycleProperty);
        set => SetValue(BreathingCycleProperty, value);
    }

    public static readonly DependencyProperty BreathingIntensityProperty =
        DependencyProperty.Register(
            nameof(BreathingIntensity),
            typeof(double),
            typeof(DepthCard),
            new PropertyMetadata(DepthCardConstants.DEFAULT_BREATHING_INTENSITY));

    /// <summary>
    /// Intensity of the breathing effect (scale multiplier). Default: 0.008 (0.8%)
    /// </summary>
    public double BreathingIntensity
    {
        get => (double)GetValue(BreathingIntensityProperty);
        set => SetValue(BreathingIntensityProperty, value);
    }

    public static readonly DependencyProperty FloatingRangeProperty =
        DependencyProperty.Register(
            nameof(FloatingRange),
            typeof(double),
            typeof(DepthCard),
            new PropertyMetadata(8.0));

    /// <summary>
    /// Range of the floating effect in pixels. Default: 8.0
    /// </summary>
    public double FloatingRange
    {
        get => (double)GetValue(FloatingRangeProperty);
        set => SetValue(FloatingRangeProperty, value);
    }

    public static readonly DependencyProperty FloatingCycleProperty =
        DependencyProperty.Register(
            nameof(FloatingCycle),
            typeof(double),
            typeof(DepthCard),
            new PropertyMetadata(6000.0));

    /// <summary>
    /// Duration of one floating cycle in milliseconds. Default: 6000ms
    /// </summary>
    public double FloatingCycle
    {
        get => (double)GetValue(FloatingCycleProperty);
        set => SetValue(FloatingCycleProperty, value);
    }

    #endregion

    #region Attached Properties for Child Layers

    public static readonly DependencyProperty CurrentRotateXProperty =
        DependencyProperty.RegisterAttached(
            "CurrentRotateX",
            typeof(double),
            typeof(DepthCard),
            new PropertyMetadata(0.0));

    public static double GetCurrentRotateX(DependencyObject obj) =>
        (double)obj.GetValue(CurrentRotateXProperty);

    public static void SetCurrentRotateX(DependencyObject obj, double value) =>
        obj.SetValue(CurrentRotateXProperty, value);

    public static readonly DependencyProperty CurrentRotateYProperty =
        DependencyProperty.RegisterAttached(
            "CurrentRotateY",
            typeof(double),
            typeof(DepthCard),
            new PropertyMetadata(0.0));

    public static double GetCurrentRotateY(DependencyObject obj) =>
        (double)obj.GetValue(CurrentRotateYProperty);

    public static void SetCurrentRotateY(DependencyObject obj, double value) =>
        obj.SetValue(CurrentRotateYProperty, value);

    public static readonly DependencyProperty IsCardHoveredProperty =
        DependencyProperty.RegisterAttached(
            "IsCardHovered",
            typeof(bool),
            typeof(DepthCard),
            new PropertyMetadata(false));

    public static bool GetIsCardHovered(DependencyObject obj) =>
        (bool)obj.GetValue(IsCardHoveredProperty);

    public static void SetIsCardHovered(DependencyObject obj, bool value) =>
        obj.SetValue(IsCardHoveredProperty, value);

    public static readonly DependencyProperty CardDepthScaleProperty =
        DependencyProperty.RegisterAttached(
            "CardDepthScale",
            typeof(double),
            typeof(DepthCard),
            new PropertyMetadata(50.0));

    public static double GetCardDepthScale(DependencyObject obj) =>
        (double)obj.GetValue(CardDepthScaleProperty);

    public static void SetCardDepthScale(DependencyObject obj, double value) =>
        obj.SetValue(CardDepthScaleProperty, value);

    #endregion

    #region Attached Properties for Magnetic Elements

    public static readonly DependencyProperty IsMagneticProperty =
        DependencyProperty.RegisterAttached(
            "IsMagnetic",
            typeof(bool),
            typeof(DepthCard),
            new PropertyMetadata(false, OnMagneticPropertyChanged));

    public static bool GetIsMagnetic(DependencyObject obj) =>
        (bool)obj.GetValue(IsMagneticProperty);

    public static void SetIsMagnetic(DependencyObject obj, bool value) =>
        obj.SetValue(IsMagneticProperty, value);

    private static void OnMagneticPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Magnetic system removed - handler kept for compatibility
    }

    private static DepthCard? FindParentDepthCard(DependencyObject element)
    {
        var parent = VisualTreeHelper.GetParent(element);
        while (parent != null)
        {
            if (parent is DepthCard card)
                return card;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }

    public static readonly DependencyProperty MagneticStrengthProperty =
        DependencyProperty.RegisterAttached(
            "MagneticStrength",
            typeof(double),
            typeof(DepthCard),
            new PropertyMetadata(DepthCardConstants.DEFAULT_MAGNETIC_STRENGTH));

    public static double GetMagneticStrength(DependencyObject obj) =>
        (double)obj.GetValue(MagneticStrengthProperty);

    public static void SetMagneticStrength(DependencyObject obj, double value) =>
        obj.SetValue(MagneticStrengthProperty, value);

    public static readonly DependencyProperty MagneticRangeProperty =
        DependencyProperty.RegisterAttached(
            "MagneticRange",
            typeof(double),
            typeof(DepthCard),
            new PropertyMetadata(DepthCardConstants.DEFAULT_MAGNETIC_RANGE));

    public static double GetMagneticRange(DependencyObject obj) =>
        (double)obj.GetValue(MagneticRangeProperty);

    public static void SetMagneticRange(DependencyObject obj, double value) =>
        obj.SetValue(MagneticRangeProperty, value);

    #endregion

    #region Events

    public event EventHandler<DepthCardTiltChangedEventArgs>? TiltChanged;
    public event EventHandler? HoverStarted;
    public event EventHandler? HoverEnded;

    #endregion

    public DepthCard()
    {
        DefaultStyleKey = typeof(DepthCard);

        PointerEntered += OnPointerEntered;
        PointerExited += OnPointerExited;
        PointerMoved += OnPointerMoved;
        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Update transform center based on actual size
        UpdateTransformCenter();

        // Start breathing animation if enabled
        if (EnableBreathingEffect)
        {
            StartBreathingAnimation();
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Update transform center when size changes
        UpdateTransformCenter();
    }

    private void UpdateTransformCenter()
    {
        if (_cardTransform != null && ActualWidth > 0 && ActualHeight > 0)
        {
            // Set the center point for skew transformations to pivot from card center
            _cardTransform.CenterX = ActualWidth / 2;
            _cardTransform.CenterY = ActualHeight / 2;
        }
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _cardTransform = GetTemplateChild("CardTransform") as CompositeTransform;

        // Initialize transform center
        UpdateTransformCenter();
    }

    // Removed: Layer Caching region (unused - no DepthLayers or magnetic elements)

    #region Pointer Event Handlers

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (!IsInteractive) return;

        _isHovered = true;
        HoverStarted?.Invoke(this, EventArgs.Empty);

        // Stop any running reset animation
        _resetStoryboard?.Stop();

        // Pause breathing animation while hovering
        _breathingStoryboard?.Pause();

        // Initialize transform to current state to avoid jerk
        if (_cardTransform != null)
        {
            // Ensure we start from neutral tilt state when hovering
            _rotateX = 0;
            _rotateY = 0;
        }
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!IsInteractive) return;

        _isHovered = false;
        HoverEnded?.Invoke(this, EventArgs.Empty);

        // Animate back to neutral with spring effect
        AnimateToNeutral();

        // Restart breathing animation after hover (don't use Resume as it can throw ObjectDisposedException)
        if (EnableBreathingEffect)
        {
            StartBreathingAnimation();
        }
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!IsInteractive || !_isHovered) return;

        // Performance: Throttle pointer events to ~60fps
        var now = DateTime.UtcNow;
        if ((now - _lastPointerUpdate).TotalMilliseconds < DepthCardConstants.POINTER_THROTTLE_MS)
            return;

        _lastPointerUpdate = now;

        // Get intensity multiplier based on input device type
        var intensityMultiplier = InputHelper.GetIntensityMultiplier(e);

        var position = e.GetCurrentPoint(this).Position;
        UpdateTilt(position, intensityMultiplier);
    }

    #endregion

    #region Tilt and Transform

    private void UpdateTilt(Point position, double intensityMultiplier = 1.0)
    {
        if (ActualWidth <= 0 || ActualHeight <= 0) return;

        var centerX = ActualWidth / 2;
        var centerY = ActualHeight / 2;

        // Calculate percentage from center (-1 to +1)
        var percentX = Math.Clamp((position.X - centerX) / centerX, -1, 1);
        var percentY = Math.Clamp((position.Y - centerY) / centerY, -1, 1);

        // Calculate rotation angles with intensity multiplier for touch devices
        _rotateY = percentX * Intensity * intensityMultiplier;
        _rotateX = -percentY * Intensity * intensityMultiplier;

        // Apply tilt transforms
        ApplyTransform();

        // Fire event for child elements (like Simple3DObject)
        TiltChanged?.Invoke(this, new DepthCardTiltChangedEventArgs(_rotateX, _rotateY));
    }

    private void ApplyTransform()
    {
        if (_cardTransform == null) return;

        // Simulate 3D perspective using skew + scale transforms
        // SkewX/SkewY create the tilt effect
        _cardTransform.SkewX = _rotateY * DepthCardConstants.SKEW_X_MULTIPLIER;
        _cardTransform.SkewY = -_rotateX * DepthCardConstants.SKEW_Y_MULTIPLIER;

        // Add perspective scaling: edges rotating away shrink, edges toward grow
        // Calculate perspective scale factors (1.0 = neutral, <1.0 = shrink, >1.0 = grow)
        double perspectiveFactor = 0.002; // Reduced for smoother transition
        double scaleX = 1.0 - Math.Abs(_rotateY) * perspectiveFactor;
        double scaleY = 1.0 - Math.Abs(_rotateX) * perspectiveFactor;

        // Apply subtle scaling to enhance depth illusion
        _cardTransform.ScaleX = scaleX;
        _cardTransform.ScaleY = scaleY;

        // Card stays centered while rotating with simulated 3D perspective
        // Edges rotating away appear smaller (further), creating depth illusion
    }

    // Removed: UpdateGlare, UpdateEdgeLighting, UpdateMagneticElements, ApplyMagneticTranslation, ResetMagneticElements (~280 lines)

    #endregion

    #region Animation

    private void AnimateToNeutral()
    {
        if (_cardTransform == null) return;

        // Check if animations are enabled (accessibility)
        if (!AccessibilityHelper.AreAnimationsEnabled())
        {
            // Skip animation, apply final values immediately
            _cardTransform.SkewX = 0;
            _cardTransform.SkewY = 0;
            _cardTransform.ScaleX = 1.0;
            _cardTransform.ScaleY = 1.0;
            _rotateX = 0;
            _rotateY = 0;
            return;
        }

        _resetStoryboard = new Storyboard();

        // Spring-like easing with overshoot
        var easing = new ElasticEase
        {
            EasingMode = EasingMode.EaseOut,
            Oscillations = DepthCardConstants.ELASTIC_OSCILLATIONS,
            Springiness = DepthCardConstants.ELASTIC_SPRINGINESS
        };

        var duration = TimeSpan.FromMilliseconds(DepthCardConstants.RESET_ANIMATION_DURATION_MS);

        // Animate SkewX back to 0
        var skewXAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = duration,
            EasingFunction = easing
        };
        Storyboard.SetTarget(skewXAnimation, _cardTransform);
        Storyboard.SetTargetProperty(skewXAnimation, "SkewX");
        _resetStoryboard.Children.Add(skewXAnimation);

        // Animate SkewY back to 0
        var skewYAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = duration,
            EasingFunction = easing
        };
        Storyboard.SetTarget(skewYAnimation, _cardTransform);
        Storyboard.SetTargetProperty(skewYAnimation, "SkewY");
        _resetStoryboard.Children.Add(skewYAnimation);

        // Animate ScaleX back to 1.0
        var scaleXAnimation = new DoubleAnimation
        {
            To = 1.0,
            Duration = duration,
            EasingFunction = easing
        };
        Storyboard.SetTarget(scaleXAnimation, _cardTransform);
        Storyboard.SetTargetProperty(scaleXAnimation, "ScaleX");
        _resetStoryboard.Children.Add(scaleXAnimation);

        // Animate ScaleY back to 1.0
        var scaleYAnimation = new DoubleAnimation
        {
            To = 1.0,
            Duration = duration,
            EasingFunction = easing
        };
        Storyboard.SetTarget(scaleYAnimation, _cardTransform);
        Storyboard.SetTargetProperty(scaleYAnimation, "ScaleY");
        _resetStoryboard.Children.Add(scaleYAnimation);

        _resetStoryboard.Completed += (s, e) =>
        {
            _rotateX = 0;
            _rotateY = 0;
        };

        _resetStoryboard.Begin();
    }

    #endregion

    // Removed: Layer Notification region (unused - no DepthLayers)

    #region Breathing Animation

    private void StartBreathingAnimation()
    {
        if (_cardTransform == null) return;

        // Check if animations are enabled (accessibility)
        if (!AccessibilityHelper.AreAnimationsEnabled())
            return;

        // Stop existing animation
        StopBreathingAnimation();

        _breathingStoryboard = new Storyboard();
        _breathingStoryboard.RepeatBehavior = RepeatBehavior.Forever;

        // Use SineEase for smooth, natural breathing
        var easing = new SineEase
        {
            EasingMode = EasingMode.EaseInOut
        };

        var randomOffset = TimeSpan.FromMilliseconds(_random.Next(0, (int)BreathingCycle));

        // Note: Scale animations removed to avoid conflict with perspective scaling during tilt
        // Only floating X/Y translation for ambient movement
        var floatingDuration = TimeSpan.FromMilliseconds(FloatingCycle);
        var floatingHalfCycle = TimeSpan.FromMilliseconds(FloatingCycle / 2);

        // TranslateX - Horizontal floating (figure-8 pattern)
        var translateXAnimation1 = new DoubleAnimation
        {
            From = 0,
            To = FloatingRange,
            Duration = floatingHalfCycle,
            EasingFunction = easing,
            BeginTime = randomOffset
        };
        Storyboard.SetTarget(translateXAnimation1, _cardTransform);
        Storyboard.SetTargetProperty(translateXAnimation1, "TranslateX");
        _breathingStoryboard.Children.Add(translateXAnimation1);

        var translateXAnimation2 = new DoubleAnimation
        {
            From = FloatingRange,
            To = 0,
            Duration = floatingHalfCycle,
            EasingFunction = easing,
            BeginTime = randomOffset + floatingHalfCycle
        };
        Storyboard.SetTarget(translateXAnimation2, _cardTransform);
        Storyboard.SetTargetProperty(translateXAnimation2, "TranslateX");
        _breathingStoryboard.Children.Add(translateXAnimation2);

        // TranslateY - Vertical floating (offset from X for circular motion)
        var floatingQuarterCycle = TimeSpan.FromMilliseconds(FloatingCycle / 4);

        var translateYAnimation1 = new DoubleAnimation
        {
            From = 0,
            To = FloatingRange * 0.7,
            Duration = floatingHalfCycle,
            EasingFunction = easing,
            BeginTime = randomOffset + floatingQuarterCycle
        };
        Storyboard.SetTarget(translateYAnimation1, _cardTransform);
        Storyboard.SetTargetProperty(translateYAnimation1, "TranslateY");
        _breathingStoryboard.Children.Add(translateYAnimation1);

        var translateYAnimation2 = new DoubleAnimation
        {
            From = FloatingRange * 0.7,
            To = 0,
            Duration = floatingHalfCycle,
            EasingFunction = easing,
            BeginTime = randomOffset + floatingQuarterCycle + floatingHalfCycle
        };
        Storyboard.SetTarget(translateYAnimation2, _cardTransform);
        Storyboard.SetTargetProperty(translateYAnimation2, "TranslateY");
        _breathingStoryboard.Children.Add(translateYAnimation2);

        // When the storyboard completes one cycle, restart it
        _breathingStoryboard.Completed += (s, e) =>
        {
            if (EnableBreathingEffect && _breathingStoryboard != null)
            {
                _breathingStoryboard.Begin();
            }
        };

        _breathingStoryboard.Begin();
    }

    private void StopBreathingAnimation()
    {
        if (_breathingStoryboard != null)
        {
            _breathingStoryboard.Stop();
            _breathingStoryboard = null;
        }

        // Reset scale and translation to default
        if (_cardTransform != null)
        {
            _cardTransform.ScaleX = 1.0;
            _cardTransform.ScaleY = 1.0;
            _cardTransform.TranslateX = 0;
            _cardTransform.TranslateY = 0;
        }
    }

    #endregion
}

public class DepthCardTiltChangedEventArgs : EventArgs
{
    public double RotateX { get; }
    public double RotateY { get; }

    public DepthCardTiltChangedEventArgs(double rotateX, double rotateY)
    {
        RotateX = rotateX;
        RotateY = rotateY;
    }
}
