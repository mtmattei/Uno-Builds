using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace KineticSculptor.Sculptor;

public sealed class SculptorSurface : SKCanvasElement
{
    private readonly Simulation _simulation = new();
    private readonly PointerInput _input = new();
    private readonly Renderer _renderer = new();
    private readonly Stopwatch _sw = Stopwatch.StartNew();
    private long _lastTickMs;
    private float _lastW, _lastH;
    private bool _renderingHooked;

    public event EventHandler? FirstInteraction;
    private bool _firstInteractionFired;

    public SculptorSurface()
    {
        Loaded += OnSurfaceLoaded;
        Unloaded += OnSurfaceUnloaded;

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerCanceled += OnPointerCanceled;
        PointerCaptureLost += OnPointerCanceled;
        PointerExited += OnPointerCanceled;
    }

    private void OnSurfaceLoaded(object sender, RoutedEventArgs e)
    {
        if (!IsSupportedOnCurrentPlatform()) return;
        if (!_renderingHooked)
        {
            CompositionTarget.Rendering += OnRendering;
            _renderingHooked = true;
            _lastTickMs = _sw.ElapsedMilliseconds;
        }
    }

    private void OnSurfaceUnloaded(object sender, RoutedEventArgs e)
    {
        if (_renderingHooked)
        {
            CompositionTarget.Rendering -= OnRendering;
            _renderingHooked = false;
        }
    }

    private void OnRendering(object? sender, object e)
    {
        var size = ActualSize;
        float w = size.X;
        float h = size.Y;
        if (w <= 0f || h <= 0f) return;

        if (w != _lastW || h != _lastH)
        {
            _simulation.Resize(w, h);
            _lastW = w;
            _lastH = h;
        }

        long now = _sw.ElapsedMilliseconds;
        float dt = MathF.Min((now - _lastTickMs) / 1000f, 0.05f);
        _lastTickMs = now;
        float t = now / 1000f;

        _simulation.Step(dt, t, _input);
        Invalidate();
    }

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        _renderer.Draw(canvas, area, _simulation, _input);
    }

    private void NotifyFirstInteraction()
    {
        if (!_firstInteractionFired)
        {
            _firstInteractionFired = true;
            FirstInteraction?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var pt = e.GetCurrentPoint(this).Position;
        CapturePointer(e.Pointer);
        _input.OnDown((float)pt.X, (float)pt.Y);
        NotifyFirstInteraction();
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var pt = e.GetCurrentPoint(this).Position;
        var impulse = _input.OnMove((float)pt.X, (float)pt.Y);
        if (impulse is { } imp)
        {
            _simulation.AddPush(imp.X, imp.Y, imp.Dx, imp.Dy, imp.Life, imp.Speed);
        }
        NotifyFirstInteraction();
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        var pt = e.GetCurrentPoint(this).Position;
        bool wasTap = _input.OnUp();
        if (wasTap)
        {
            _simulation.AddShock((float)pt.X, (float)pt.Y);
        }
        ReleasePointerCapture(e.Pointer);
    }

    private void OnPointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        _input.Cancel();
    }
}
