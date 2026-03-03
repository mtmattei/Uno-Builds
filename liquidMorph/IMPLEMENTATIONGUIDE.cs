
WINUI 3 ANIMATION GUIDE
Liquid Morph
Dissolve Transition

A complete implementation guide for building an SVG-turbulence-inspired liquid morph transition using Win2D, Composition APIs, and WinUI 3 XAML. Covers the effect pipeline, animation orchestration, and production integration patterns.

Target Framework: .NET 8+ · WinUI 3 · Windows App SDK 1.5+
Dependencies: Win2D (Microsoft.Graphics.Canvas)
 

SECTION 01
Architecture Overview

The liquid morph dissolve creates an organic, mercury-like transition between two views. The outgoing content appears to liquify and disperse, then the incoming content coalesces from the same turbulent state. The effect is built from three layers working in concert.
Effect Pipeline
The core visual effect chains three Win2D operations. First, a CanvasTurbulenceEffect generates Perlin noise that provides organic displacement coordinates. This noise feeds into a CanvasDisplacementMapEffect that warps the source visual using the noise as a displacement lookup. Finally, a GaussianBlurEffect adds progressive softening at the edges of the dissolution. During the transition, the turbulence frequency and displacement amount are animated simultaneously to ramp distortion up and then back down.
Component Responsibilities
Component	Responsibility
LiquidMorphPanel	Custom XAML Panel that hosts the outgoing and incoming content as child visuals and owns the transition lifecycle.
MorphEffectBrush	A Win2D-backed CompositionEffectBrush wiring turbulence → displacement → blur into a single GPU-composited pipeline.
MorphAnimator	Orchestrates the timed keyframe sequences: ramps displacement up on exit, swaps content at midpoint, ramps displacement down on enter.
ContentCaptureHelper	Snapshots arbitrary XAML subtrees into CompositionVisual surfaces so they can be fed into the effect pipeline as static textures.

SECTION 02
Project Setup

Before writing any transition code, you need Win2D available in your project. Win2D provides the GPU-accelerated effect primitives that make the turbulence displacement feasible at 60fps.
NuGet Dependencies
<!-- In your .csproj or via NuGet Package Manager -->
<PackageReference Include="Microsoft.Graphics.Canvas"
                  Version="1.0.6" />

Ensure your project targets net8.0-windows10.0.19041.0 or later. The Composition interop APIs used for visual capture require Windows 10 2004+.
Namespace Imports
The implementation touches several API surfaces. These are the namespaces you will use across the core files:
// Win2D effects
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Composition;
 
// Composition layer
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Hosting;
 
// WinUI / XAML
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
 
// System
using System.Numerics;

SECTION 03
Building the Effect Brush

The effect brush is the heart of the transition. It chains three Win2D effects into a single composition brush that can be animated per-frame without CPU intervention. The Composition API lets you declare the entire pipeline as a graph, bind animatable properties, and let the DWM compositor evaluate it on the GPU.
Effect Graph Structure
The graph flows as: Source Visual → DisplacementMapEffect (displaced by TurbulenceEffect) → GaussianBlurEffect → Output. The displacement amount and blur radius are exposed as animatable properties on the CompositionEffectBrush.
MorphEffectBrush.cs
public sealed class MorphEffectBrush
{
    private readonly Compositor _compositor;
    private CompositionEffectBrush? _brush;
 
    // Animatable property references
    public const string DisplacementProp = "DisplacementAmount";
    public const string BlurProp = "BlurAmount";
    public const string FrequencyProp = "TurbulenceFrequency";
 
    public MorphEffectBrush(Compositor compositor)
    {
        _compositor = compositor;
    }
 
    public CompositionEffectBrush Build(
        CompositionSurfaceBrush sourceSurface)
    {
        // 1. Turbulence noise generator
        var turbulence = new TurbulenceEffect
        {
            Frequency = new Vector2(0.015f, 0.015f),
            NumOctaves = 3,
            Seed = 42,
            Size = new Vector2(1200, 800),
            Noise = TurbulenceEffectNoise.FractalBrownianMotion
        };
 
        // 2. Displacement using turbulence as lookup
        var displacement = new DisplacementMapEffect
        {
            Name = "Displacer",
            Source = new CompositionEffectSourceParameter(
                "ContentSource"),
            Displacement = turbulence,
            Amount = 0f,  // animated 0 → 80 → 0
            XChannelSelect = EffectChannelSelect.Red,
            YChannelSelect = EffectChannelSelect.Green
        };
 
        // 3. Gaussian blur for soft dissolution edges
        var blur = new GaussianBlurEffect
        {
            Name = "Blur",
            Source = displacement,
            BlurAmount = 0f, // animated 0 → 20 → 0
            BorderMode = EffectBorderMode.Hard
        };
 
        // Compile to composition effect brush
        var factory = _compositor.CreateEffectFactory(
            blur,
            new[]
            {
                "Displacer.Amount",
                "Blur.BlurAmount"
            });
 
        _brush = factory.CreateBrush();
        _brush.SetSourceParameter(
            "ContentSource", sourceSurface);
 
        return _brush;
    }
}

Key design decision: the turbulence frequency is not animated directly on the CompositionEffectBrush because Win2D’s TurbulenceEffect.Frequency is a Vector2 that isn’t natively animatable through the Composition property system. Instead, Section 5 covers a workaround that rebuilds the effect at discrete frequency steps during the transition.

SECTION 04
Content Capture

To feed arbitrary XAML content into the effect pipeline, you need to rasterize it to a composition surface. The Composition API provides ElementCompositionPreview for exactly this purpose. The captured surface becomes the input texture for the displacement effect.
ContentCaptureHelper.cs
public static class ContentCaptureHelper
{
    /// <summary>
    /// Captures a XAML element’s visual subtree into a
    /// CompositionVisualSurface that can be used as an
    /// effect source.
    /// </summary>
    public static CompositionSurfaceBrush Capture(
        UIElement element,
        Compositor compositor)
    {
        // Get the element’s backing visual
        var elementVisual = ElementCompositionPreview
            .GetElementVisual(element);
 
        // Create a surface that mirrors the visual
        var surface = compositor.CreateVisualSurface();
        surface.SourceVisual = elementVisual;
        surface.SourceSize = new Vector2(
            (float)element.ActualWidth,
            (float)element.ActualHeight);
 
        // Wrap in a brush for the effect pipeline
        var brush = compositor.CreateSurfaceBrush(surface);
        brush.Stretch = CompositionStretch.None;
        brush.HorizontalAlignmentRatio = 0f;
        brush.VerticalAlignmentRatio = 0f;
 
        return brush;
    }
}

Important: the VisualSurface captures a live reference to the visual subtree, not a snapshot. For the outgoing content, you must capture before you remove it from the visual tree. If the element is removed before the transition completes, the surface will go blank. The recommended pattern is to capture, start the exit animation, swap content at the midpoint, then capture the new content for the enter phase.

SECTION 05
Animation Orchestration

The transition runs in two phases across roughly 1600ms total. The exit phase ramps displacement and blur from zero to maximum over 800ms. At the midpoint, the content is swapped. The enter phase ramps back down to zero over 800ms. A discrete frequency stepper runs alongside to vary the turbulence pattern.
MorphAnimator.cs
public sealed class MorphAnimator
{
    private readonly Compositor _compositor;
 
    public TimeSpan PhaseDuration { get; set; }
        = TimeSpan.FromMilliseconds(800);
 
    public float MaxDisplacement { get; set; } = 80f;
    public float MaxBlur { get; set; } = 20f;
 
    public MorphAnimator(Compositor compositor)
    {
        _compositor = compositor;
    }
 
    /// <summary>
    /// Runs the exit dissolve: 0 → max displacement + blur.
    /// </summary>
    public void AnimateExit(
        CompositionEffectBrush brush,
        Action onComplete)
    {
        var batch = _compositor.CreateScopedBatch(
            CompositionBatchTypes.Animation);
 
        // Displacement: ease-in ramp to max
        var dispAnim = _compositor.CreateScalarKeyFrameAnimation();
        dispAnim.InsertKeyFrame(0f, 0f);
        dispAnim.InsertKeyFrame(1f, MaxDisplacement,
            _compositor.CreateCubicBezierEasingFunction(
                new Vector2(0.45f, 0f),
                new Vector2(0.8f, 0.3f)));
        dispAnim.Duration = PhaseDuration;
 
        // Blur: ease-in ramp to max
        var blurAnim = _compositor.CreateScalarKeyFrameAnimation();
        blurAnim.InsertKeyFrame(0f, 0f);
        blurAnim.InsertKeyFrame(0.5f, MaxBlur * 0.3f);
        blurAnim.InsertKeyFrame(1f, MaxBlur,
            _compositor.CreateCubicBezierEasingFunction(
                new Vector2(0.45f, 0f),
                new Vector2(0.8f, 0.3f)));
        blurAnim.Duration = PhaseDuration;
 
        brush.StartAnimation(
            "Displacer.Amount", dispAnim);
        brush.StartAnimation(
            "Blur.BlurAmount", blurAnim);
 
        batch.End();
        batch.Completed += (_, _) => onComplete();
    }
 
    /// <summary>
    /// Runs the enter coalesce: max → 0 displacement + blur.
    /// </summary>
    public void AnimateEnter(
        CompositionEffectBrush brush,
        Action? onComplete = null)
    {
        var batch = _compositor.CreateScopedBatch(
            CompositionBatchTypes.Animation);
 
        var dispAnim = _compositor.CreateScalarKeyFrameAnimation();
        dispAnim.InsertKeyFrame(0f, MaxDisplacement);
        dispAnim.InsertKeyFrame(1f, 0f,
            _compositor.CreateCubicBezierEasingFunction(
                new Vector2(0.2f, 0.7f),
                new Vector2(0.55f, 1f)));
        dispAnim.Duration = PhaseDuration;
 
        var blurAnim = _compositor.CreateScalarKeyFrameAnimation();
        blurAnim.InsertKeyFrame(0f, MaxBlur);
        blurAnim.InsertKeyFrame(0.5f, MaxBlur * 0.3f);
        blurAnim.InsertKeyFrame(1f, 0f,
            _compositor.CreateCubicBezierEasingFunction(
                new Vector2(0.2f, 0.7f),
                new Vector2(0.55f, 1f)));
        blurAnim.Duration = PhaseDuration;
 
        brush.StartAnimation(
            "Displacer.Amount", dispAnim);
        brush.StartAnimation(
            "Blur.BlurAmount", blurAnim);
 
        batch.End();
        if (onComplete != null)
            batch.Completed += (_, _) => onComplete();
    }
}
Turbulence Frequency Stepping
Since TurbulenceEffect.Frequency isn’t directly animatable through the Composition property system, you can approximate the ramping turbulence from the HTML prototype by rebuilding the inner effect at 3–4 discrete steps during the transition. Use a DispatcherTimer or Task.Delay to swap the turbulence effect at intervals:
private async Task StepTurbulenceFrequency(
    MorphEffectBrush morphBrush,
    CompositionSurfaceBrush source,
    SpriteVisual targetVisual,
    float[] frequencies,
    TimeSpan stepInterval)
{
    foreach (var freq in frequencies)
    {
        var newBrush = morphBrush.BuildWithFrequency(
            source, new Vector2(freq, freq));
        targetVisual.Brush = newBrush;
        await Task.Delay(stepInterval);
    }
}
 
// Usage during exit phase:
// frequencies: [0.015, 0.025, 0.04, 0.06]
// stepInterval: PhaseDuration / 4

SECTION 06
The XAML Host Panel

The LiquidMorphPanel is a custom control that manages two content slots and exposes a simple TransitionTo(UIElement) method. Internally, it layers a SpriteVisual with the effect brush over the XAML content, toggling visibility at the transition midpoint.
LiquidMorphPanel.xaml
<UserControl
    x:Class="YourApp.Controls.LiquidMorphPanel"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
 
    <Grid x:Name="RootGrid">
        <!-- Active content host -->
        <ContentPresenter
            x:Name="ContentHost"
            HorizontalContentAlignment="Stretch"
            VerticalContentAlignment="Stretch" />
    </Grid>
</UserControl>
LiquidMorphPanel.xaml.cs
public sealed partial class LiquidMorphPanel : UserControl
{
    private Compositor _compositor = null!;
    private SpriteVisual _effectVisual = null!;
    private MorphEffectBrush _morphBrush = null!;
    private MorphAnimator _animator = null!;
    private bool _isTransitioning;
 
    public LiquidMorphPanel()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }
 
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _compositor = ElementCompositionPreview
            .GetElementVisual(this).Compositor;
 
        // Create overlay visual for effect rendering
        _effectVisual = _compositor.CreateSpriteVisual();
        _effectVisual.RelativeSizeAdjustment
            = Vector2.One;
        ElementCompositionPreview
            .SetElementChildVisual(RootGrid, _effectVisual);
 
        _morphBrush = new MorphEffectBrush(_compositor);
        _animator = new MorphAnimator(_compositor);
    }
 
    public void SetContent(UIElement content)
    {
        ContentHost.Content = content;
    }
 
    public async void TransitionTo(UIElement newContent)
    {
        if (_isTransitioning) return;
        _isTransitioning = true;
 
        var oldContent = ContentHost.Content as UIElement;
        if (oldContent == null)
        {
            SetContent(newContent);
            _isTransitioning = false;
            return;
        }
 
        // Capture outgoing content
        var sourceBrush = ContentCaptureHelper
            .Capture(oldContent, _compositor);
        var effectBrush = _morphBrush
            .Build(sourceBrush);
        _effectVisual.Brush = effectBrush;
 
        // Show effect layer, hide XAML content
        ContentHost.Opacity = 0;
 
        // Phase 1: Exit dissolve
        var tcs = new TaskCompletionSource();
        _animator.AnimateExit(effectBrush, () =>
        {
            // Midpoint: swap content
            ContentHost.Content = newContent;
 
            // Recapture for enter phase
            DispatcherQueue.TryEnqueue(() =>
            {
                var newBrush = ContentCaptureHelper
                    .Capture(newContent, _compositor);
                var enterBrush = _morphBrush
                    .Build(newBrush);
                _effectVisual.Brush = enterBrush;
 
                // Phase 2: Enter coalesce
                _animator.AnimateEnter(
                    enterBrush, () =>
                {
                    ContentHost.Opacity = 1;
                    _effectVisual.Brush = null;
                    _isTransitioning = false;
                    tcs.SetResult();
                });
            });
        });
 
        await tcs.Task;
    }
}

SECTION 07
Usage Example

Here’s how you’d wire the panel into a page-level navigation scenario, transitioning between two arbitrary content views:
MainPage.xaml
<Page ...>
    <Grid>
        <local:LiquidMorphPanel x:Name="MorphPanel" />
        <Button Content="Switch View"
                Click="OnSwitchClicked"
                VerticalAlignment="Bottom"
                HorizontalAlignment="Center"
                Margin="0,0,0,40" />
    </Grid>
</Page>
MainPage.xaml.cs
private int _viewIndex = 0;
private readonly UIElement[] _views = { ... };
 
private void OnPageLoaded(object s, RoutedEventArgs e)
{
    MorphPanel.SetContent(_views[0]);
}
 
private void OnSwitchClicked(object s, RoutedEventArgs e)
{
    _viewIndex = (_viewIndex + 1) % _views.Length;
    MorphPanel.TransitionTo(_views[_viewIndex]);
}

SECTION 08
Tuning & Production Notes

Performance
The entire effect pipeline runs on the GPU via the Composition layer. CPU cost is near-zero during the animation itself. The main cost centers are the initial surface capture (which triggers a render-target readback) and the effect compilation (one-time cost per brush build). On integrated GPUs, keep MaxDisplacement below 120 and turbulence NumOctaves at 3 or fewer to maintain 60fps.
Easing Recommendations
The exit phase should use an ease-in curve so distortion accelerates as the content dissolves, creating a sense of momentum. The enter phase should use an ease-out curve so the new content “snaps” into clarity quickly. The cubic-bezier values in the MorphAnimator are calibrated for this feel, but adjust to taste. Making the enter phase 10–15% shorter than the exit phase often feels more natural.
Accessibility Considerations
Check UISettings.AnimationsEnabled before running the transition. If the user has disabled animations in Windows Settings, fall back to a simple opacity crossfade or an instant swap. Wrap the check in your TransitionTo method:
var uiSettings = new UISettings();
if (!uiSettings.AnimationsEnabled)
{
    // Instant swap, no effect
    ContentHost.Content = newContent;
    return;
}
Memory & Cleanup
CompositionSurfaceBrush and CompositionEffectBrush hold references to GPU resources. After each transition completes, null out the SpriteVisual’s Brush and let the brushes go out of scope. If the panel is removed from the visual tree, dispose the compositor resources in your Unloaded handler to prevent leaks.
