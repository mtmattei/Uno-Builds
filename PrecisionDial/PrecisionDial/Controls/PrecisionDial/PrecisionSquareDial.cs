using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;

namespace PrecisionDial.Controls;

public sealed partial class PrecisionSquareDial : Panel
{
    private readonly PrecisionSquareDialCanvas _canvas;
    private readonly HapticService _hapticService;
    private readonly InertiaEngine _inertiaEngine = new();
    private readonly SpringSolver _springSolver = new();
    private readonly VelocityTracker _velocityTracker = new();
    private DispatcherTimer? _animTimer;

    private bool _isDragging;
    private double _dragStartX;
    private double _dragStartValue;
    private int _lastDetentIndex;
    private double _lastValueForVelocity;
    private long _lastMoveTick;

    public PrecisionSquareDial()
    {
        _hapticService = new HapticService();
        _canvas = new PrecisionSquareDialCanvas(this);
        Children.Add(_canvas);
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerCanceled += OnPointerCanceled;
        ManipulationMode = ManipulationModes.TranslateX;
    }

    internal double NormalizedValue =>
        (Maximum - Minimum) > 0 ? (Value - Minimum) / (Maximum - Minimum) : 0.0;
    internal int CurrentDetentIndex =>
        (int)Math.Round(NormalizedValue * DetentCount);

    internal double DisplayNormalizedValue =>
        (_isDragging || _inertiaEngine.IsActive) ? NormalizedValue : _springSolver.Current;

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _inertiaEngine.Cancel();
        _velocityTracker.Reset();

        var pos = e.GetCurrentPoint(this).Position;
        _isDragging = true;
        _dragStartX = pos.X;
        _dragStartValue = Value;
        _lastValueForVelocity = Value;
        _lastMoveTick = Stopwatch.GetTimestamp();
        _lastDetentIndex = CurrentDetentIndex;
        _springSolver.SnapTo(NormalizedValue);

        CapturePointer(e.Pointer);
        _hapticService.Prepare();
        if (IsHapticEnabled) _hapticService.FireSelectionTick();
        DragStarted?.Invoke(this, new RoutedEventArgs());
        _canvas.Invalidate();
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDragging) return;

        var pos = e.GetCurrentPoint(this).Position;
        var now = Stopwatch.GetTimestamp();
        var dt = (double)(now - _lastMoveTick) / Stopwatch.Frequency;
        _lastMoveTick = now;

        var deltaX = pos.X - _dragStartX;
        var newValue = Math.Clamp(_dragStartValue + deltaX * Sensitivity, Minimum, Maximum);

        if (dt > 0)
            _velocityTracker.AddSample((newValue - _lastValueForVelocity) / dt);
        _lastValueForVelocity = newValue;

        var oldValue = Value;
        Value = newValue;
        var newDetentIndex = CurrentDetentIndex;
        if (newDetentIndex != _lastDetentIndex)
        {
            OnDetentCrossed(newDetentIndex);
            _lastDetentIndex = newDetentIndex;
        }
        if ((oldValue > Minimum && newValue <= Minimum) ||
            (oldValue < Maximum && newValue >= Maximum))
        {
            if (IsHapticEnabled) _hapticService.FireBoundaryStop();
        }
        _canvas.Invalidate();
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e) => EndDrag(e.Pointer);
    private void OnPointerCanceled(object sender, PointerRoutedEventArgs e) => EndDrag(e.Pointer);

    private void EndDrag(Pointer pointer)
    {
        if (!_isDragging) return;
        _isDragging = false;
        ReleasePointerCapture(pointer);
        _hapticService.Release();
        DragCompleted?.Invoke(this, new RoutedEventArgs());

        if (IsInertiaEnabled)
        {
            var avgVelocity = _velocityTracker.GetAverageVelocity();
            _inertiaEngine.Start(avgVelocity);
            if (_inertiaEngine.IsActive)
                EnsureTimerRunning();
        }
        _canvas.Invalidate();
    }

    private void EnsureTimerRunning()
    {
        if (_animTimer == null)
        {
            _animTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _animTimer.Tick += OnAnimTimerTick;
        }
        if (!_animTimer.IsEnabled)
            _animTimer.Start();
    }

    private void OnAnimTimerTick(object? sender, object e)
    {
        bool needsUpdate = false;

        if (_inertiaEngine.IsActive)
        {
            var delta = _inertiaEngine.Step(InertiaDecayRate, Value, Minimum, Maximum);
            if (delta.HasValue)
            {
                Value = Math.Clamp(Value + delta.Value, Minimum, Maximum);
                var newDetentIndex = CurrentDetentIndex;
                if (newDetentIndex != _lastDetentIndex)
                {
                    OnDetentCrossed(newDetentIndex);
                    _lastDetentIndex = newDetentIndex;
                }
                needsUpdate = true;
            }
            if (!_inertiaEngine.IsActive)
            {
                InertiaCompleted?.Invoke(this, new RoutedEventArgs());
                _springSolver.SnapTo(NormalizedValue);
            }
        }

        if (!_springSolver.IsSettled)
        {
            _springSolver.Step(0.016);
            needsUpdate = true;
        }

        if (needsUpdate)
            _canvas.Invalidate();
        else
            _animTimer?.Stop();
    }

    private void OnDetentCrossed(int newDetentIndex)
    {
        if (IsHapticEnabled)
        {
            if (newDetentIndex % 5 == 0) _hapticService.FireMajorDetentClick();
            else _hapticService.FireDetentClick();
        }
        _canvas.TriggerDetentPulse();

        var speed = _isDragging
            ? (float)(Math.Abs(_velocityTracker.GetAverageVelocity()) / 100.0)
            : (float)(Math.Abs(_inertiaEngine.CurrentVelocity) / 100.0);
        if (speed > 0.05f)
            _canvas.EmitParticles(Math.Clamp(speed, 0f, 1f));

        DetentCrossed?.Invoke(this, new DetentCrossedEventArgs(newDetentIndex));
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _canvas.Measure(availableSize);
        return _canvas.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _canvas.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
        return finalSize;
    }

    private void OnValueChanged(double oldValue, double newValue)
    {
        if (!_isDragging && !_inertiaEngine.IsActive)
        {
            _springSolver.SetTarget(NormalizedValue);
            EnsureTimerRunning();
        }
        ValueChanged?.Invoke(this, new DialValueChangedEventArgs(oldValue, newValue));
        _canvas.Invalidate();
    }
}
