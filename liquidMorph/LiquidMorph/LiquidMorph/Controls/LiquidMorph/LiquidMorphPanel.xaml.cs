using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace LiquidMorph.Controls.LiquidMorph;

/// <summary>
/// Custom panel providing liquid morph dissolve transitions.
/// Uses two Border hosts (A/B) and cross-fades opacity at the midpoint.
/// All ChildrenTransitions / Transitions are explicitly emptied so
/// the framework never injects slide/entrance animations.
/// </summary>
public sealed partial class LiquidMorphPanel : UserControl
{
    private MorphCanvas? _morphCanvas;
    private MorphAnimator? _animator;
    private bool _isTransitioning;
    private bool _useHostA = true;

    private Border ActiveHost => _useHostA ? ContentHostA : ContentHostB;
    private Border InactiveHost => _useHostA ? ContentHostB : ContentHostA;

    public LiquidMorphPanel()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _animator = new MorphAnimator(DispatcherQueue);
        _animator.FrameUpdated += OnAnimationFrame;
    }

    public void SetContent(UIElement content)
    {
        SuppressTransitions(content);
        ActiveHost.Child = content;
        ActiveHost.Opacity = 1;
    }

    public async void TransitionTo(UIElement newContent)
    {
        if (_isTransitioning) return;
        _isTransitioning = true;

        // Suppress any framework entrance animations on the incoming content
        SuppressTransitions(newContent);

        var oldContent = ActiveHost.Child;
        if (oldContent is null)
        {
            SetContent(newContent);
            _isTransitioning = false;
            return;
        }

        if (IsReducedMotionEnabled())
        {
            ActiveHost.Opacity = 0;
            _useHostA = !_useHostA;
            ActiveHost.Child = newContent;
            ActiveHost.Opacity = 1;
            _isTransitioning = false;
            return;
        }

        // Capture outgoing content
        var sourceBitmap = await ContentCaptureHelper.CaptureAsync(oldContent);
        if (sourceBitmap is null)
        {
            ActiveHost.Opacity = 0;
            _useHostA = !_useHostA;
            ActiveHost.Child = newContent;
            ActiveHost.Opacity = 1;
            _isTransitioning = false;
            return;
        }

        // Place new content in inactive host, invisible for now.
        // Content is added BEFORE the morph canvas so it sits behind.
        InactiveHost.Child = newContent;
        InactiveHost.Opacity = 0;

        _morphCanvas = new MorphCanvas
        {
            IsActive = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            EdgePadding = 0,
            DisplacementAmount = 0,
            BlurAmount = 0,
        };
        _morphCanvas.Reseed();
        _morphCanvas.SetSourceBitmap(sourceBitmap);
        RootGrid.Children.Add(_morphCanvas);
        _morphCanvas.RequestRedraw();

        // Hide live content immediately - the morph canvas now shows
        // the captured bitmap, so the live element underneath is redundant.
        // This prevents it bleeding through the transparent canvas background.
        ActiveHost.Opacity = 0;

        var tcs = new TaskCompletionSource();
        var activeRef = ActiveHost;
        var inactiveRef = InactiveHost;

        _animator!.StartTransition(
            onMidpoint: () =>
            {
                // Cross-fade at peak distortion.
                // Content stays perfectly still - only opacity changes.
                // The morph canvas is fully opaque on top so this is invisible.
                activeRef.Opacity = 0;
                inactiveRef.Opacity = 1;

                // Capture new content for enter phase
                DispatcherQueue.TryEnqueue(async () =>
                {
                    await Task.Yield();
                    await Task.Yield();

                    var newBitmap = await ContentCaptureHelper.CaptureAsync(newContent);
                    if (newBitmap is not null && _morphCanvas is not null)
                    {
                        _morphCanvas.SetSourceBitmap(newBitmap);
                    }
                });
            },
            onComplete: () =>
            {
                // Clear old host
                activeRef.Child = null;

                // Swap active/inactive for next transition
                _useHostA = !_useHostA;

                // Remove morph canvas
                if (_morphCanvas is not null)
                {
                    RootGrid.Children.Remove(_morphCanvas);
                    _morphCanvas.Cleanup();
                    _morphCanvas = null;
                }

                _isTransitioning = false;
                tcs.TrySetResult();
            });

        await tcs.Task;
    }

    private void OnAnimationFrame()
    {
        if (_morphCanvas is null || _animator is null) return;

        _morphCanvas.DisplacementAmount = _animator.CurrentDisplacement;
        _morphCanvas.BlurAmount = _animator.CurrentBlur;
        _morphCanvas.AnimationProgress = _animator.CurrentProgress;
        _morphCanvas.ScaleAmount = _animator.CurrentScale;
        _morphCanvas.ContentOpacity = _animator.CurrentContentOpacity;
        _morphCanvas.RequestRedraw();
    }

    /// <summary>
    /// Recursively strip all implicit theme transitions from an element tree
    /// so the framework never injects slide/entrance animations.
    /// </summary>
    private static void SuppressTransitions(UIElement element)
    {
        if (element is FrameworkElement fe)
            fe.Transitions = new TransitionCollection();

        if (element is Border b && b.Child is Panel childPanel)
        {
            childPanel.ChildrenTransitions = new TransitionCollection();
            SuppressChildPanelTransitions(childPanel);
        }
        else if (element is Panel p)
        {
            p.ChildrenTransitions = new TransitionCollection();
            SuppressChildPanelTransitions(p);
        }
    }

    private static void SuppressChildPanelTransitions(Panel panel)
    {
        foreach (var child in panel.Children)
        {
            if (child is FrameworkElement cfe)
                cfe.Transitions = new TransitionCollection();

            if (child is Panel cp)
            {
                cp.ChildrenTransitions = new TransitionCollection();
                SuppressChildPanelTransitions(cp);
            }
            else if (child is Border cb && cb.Child is Panel innerPanel)
            {
                innerPanel.ChildrenTransitions = new TransitionCollection();
                SuppressChildPanelTransitions(innerPanel);
            }
        }
    }

    private static bool IsReducedMotionEnabled()
    {
        try
        {
            var uiSettings = new Windows.UI.ViewManagement.UISettings();
            return !uiSettings.AnimationsEnabled;
        }
        catch
        {
            return false;
        }
    }
}
