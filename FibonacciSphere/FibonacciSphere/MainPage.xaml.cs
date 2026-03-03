using System;
using System.Diagnostics;
using System.Numerics;
using FibonacciSphere.ViewModels;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace FibonacciSphere;

public sealed partial class MainPage : Page
{
    public SphereViewModel ViewModel { get; } = new();

    private readonly DispatcherTimer _renderTimer;
    private readonly Stopwatch _stopwatch;
    private long _lastFrameTime;
    private int _frameCount;
    private float _fps;

    // Interaction state
    private bool _isDragging;
    private Vector2 _lastPointerPosition;
    private bool _isShiftPressed;

    public MainPage()
    {
        this.InitializeComponent();

        _stopwatch = Stopwatch.StartNew();
        _lastFrameTime = _stopwatch.ElapsedMilliseconds;

        // Set up render timer for animation loop (targeting 60 FPS)
        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16.67) // ~60 FPS
        };
        _renderTimer.Tick += OnRenderTick;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        // Track keyboard state for multi-select
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Canvas.ViewModel = ViewModel;
        ViewModel.ApplySettings();
        _renderTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _renderTimer.Stop();
    }

    private void OnRenderTick(object? sender, object e)
    {
        // Calculate delta time
        long currentTime = _stopwatch.ElapsedMilliseconds;
        float deltaTime = (currentTime - _lastFrameTime) / 1000f;
        _lastFrameTime = currentTime;

        // Update FPS counter
        _frameCount++;
        if (_frameCount >= 30)
        {
            _fps = _frameCount / (deltaTime * 30);
            _frameCount = 0;
            FpsCounter.Text = $"{_fps:F0} FPS";
        }

        // Update animation state
        ViewModel.Renderer.Update(deltaTime);

        // Request redraw
        Canvas.RequestInvalidate();
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(Canvas);
        var position = new Vector2((float)point.Position.X, (float)point.Position.Y);

        _lastPointerPosition = position;

        // Check if clicking on a point
        var hitPoint = ViewModel.Renderer.HitTest(position);

        if (hitPoint != null)
        {
            // Handle point selection
            if (!_isShiftPressed)
            {
                ViewModel.Renderer.ClearSelection();
            }

            hitPoint.IsSelected = !hitPoint.IsSelected;
            ViewModel.UpdateSelectedPointInfo(hitPoint.IsSelected ? hitPoint : null);
            ViewModel.UpdateSelectedCount();
        }
        else
        {
            // Start dragging for rotation
            _isDragging = true;
            Canvas.CapturePointer(e.Pointer);
        }

        e.Handled = true;
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(Canvas);
        var position = new Vector2((float)point.Position.X, (float)point.Position.Y);

        if (_isDragging)
        {
            // Calculate rotation delta
            var delta = position - _lastPointerPosition;
            float sensitivity = 0.005f;

            ViewModel.Renderer.ManualRotate(delta.X * sensitivity, delta.Y * sensitivity);
            _lastPointerPosition = position;
        }
        else
        {
            // Update hover state
            ViewModel.Renderer.ClearHover();
            var hitPoint = ViewModel.Renderer.HitTest(position);
            if (hitPoint != null)
            {
                hitPoint.IsHovered = true;
            }
        }

        e.Handled = true;
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            Canvas.ReleasePointerCapture(e.Pointer);
        }

        e.Handled = true;
    }

    private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(Canvas);
        var delta = point.Properties.MouseWheelDelta;

        // Adjust camera distance (zoom)
        float zoomSensitivity = 0.001f;
        ViewModel.CameraDistance = System.Math.Clamp(
            ViewModel.CameraDistance - delta * zoomSensitivity,
            1.5,
            10.0);

        e.Handled = true;
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Shift)
        {
            _isShiftPressed = true;
        }
    }

    private void OnKeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Shift)
        {
            _isShiftPressed = false;
        }
    }
}
