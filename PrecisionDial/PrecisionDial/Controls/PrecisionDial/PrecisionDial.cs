using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using Windows.Foundation;

namespace PrecisionDial.Controls;

public sealed partial class PrecisionDial : Panel
{
    private readonly PrecisionDialCanvas _canvas;
    private readonly HapticService _hapticService;
    private readonly ClickAudioService _clickAudio = new();
    private readonly InertiaEngine _inertiaEngine = new();
    private readonly SpringSolver _springSolver = new();
    private readonly VelocityTracker _velocityTracker = new();
    private readonly AngularDragHandler _angularHandler = new();
    private DispatcherTimer? _animTimer;

    private bool _isDragging;
    private double _dragStartY;
    private double _dragStartValue;
    private int _lastDetentIndex;

    private InputMode _activeMode;
    private double _lastAngle;
    private double _lastPointerY;
    private double _lastValueForVelocity;
    private long _lastMoveTick;

    // ── Click-vs-drag dead zone (added to fix the volume-dial click bug) ─────
    // A click that doesn't move past this threshold (in pixels) does not change
    // the value at all and never triggers inertia. Once the threshold is crossed,
    // the drag baseline resets to the current pointer position so there is no
    // jump when real drag starts.
    private const double DragDeadZonePx = 4.0;
    // Inertia is only allowed if the user actually dragged (not just clicked).
    private const double InertiaMinDragPx = 8.0;
    private const double InertiaMinDurationMs = 80.0;
    // Velocity samples taken over very small dt are unreliable — drop them.
    // 8ms is half a 60Hz frame; anything shorter than that is jittery measurement
    // noise rather than real user motion.
    private const double MinVelocitySampleDtSec = 0.008;

    private double _pressX;
    private double _pressY;
    private long _pressTick;
    private bool _hasExceededDeadZone;
    private double _totalDragPx;

    private enum InputMode { Vertical, Angular }

    public PrecisionDial()
    {
        _hapticService = new HapticService();
        _canvas = new PrecisionDialCanvas(this);
        Children.Add(_canvas);
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerCanceled += OnPointerCanceled;
        ManipulationMode = ManipulationModes.TranslateY;

        // Lazy-prepare the audio service on first Loaded so we don't hit
        // MediaPlayer construction on every control instantiation before the
        // visual tree is ready.
        Loaded += (_, _) => _clickAudio.Prepare();
    }

    internal double NormalizedValue =>
        (Maximum - Minimum) > 0 ? (Value - Minimum) / (Maximum - Minimum) : 0.0;
    internal double RotationDegrees =>
        NormalizedValue * ArcSweepDegrees - (ArcSweepDegrees / 2.0);
    internal int CurrentDetentIndex =>
        (int)Math.Round(NormalizedValue * DetentCount);
    internal bool IsDragging => _isDragging;
    internal double CurrentVelocity => _inertiaEngine.CurrentVelocity;

    internal double DisplayNormalizedValue =>
        (_isDragging || _inertiaEngine.IsActive) ? NormalizedValue : _springSolver.Current;

    /// <summary>
    /// Knob rotation in degrees.
    /// In Value mode: linear from normalized value across the arc sweep.
    /// In Menu mode: snaps to the segment midpoint of the currently selected item so the
    /// knob points cleanly between gaps. The DashedArcRenderer caches segment midpoints,
    /// so we recompute the same formula here for the rotation source of truth.
    /// </summary>
    internal double DisplayRotationDegrees
    {
        get
        {
            if (DialMode == DialMode.Menu)
            {
                var count = MenuItems?.Count ?? 0;
                if (count <= 0) return 0;
                var idx = Math.Clamp(SelectedIndex, 0, count - 1);
                var arcSweep = ArcSweepDegrees;
                const float gapDeg = 2.4f;
                var totalGap = gapDeg * (count - 1);
                var segDeg = (arcSweep - totalGap) / count;
                var startAngle = 90.0 + (360.0 - arcSweep) / 2.0;
                var midAngle = startAngle + idx * (segDeg + gapDeg) + segDeg / 2.0;
                return midAngle + 90.0; // screen-east-zero → knob-up-zero
            }

            return DisplayNormalizedValue * ArcSweepDegrees - (ArcSweepDegrees / 2.0);
        }
    }

    internal void InvalidateCanvas() => _canvas?.Invalidate();

    /// <summary>
    /// 0 = fully collapsed (bottom-sheet hidden), 1 = fully lifted. When &lt; 1
    /// in Menu mode, the arc/segments/icons/knob group rotate so the selected
    /// item ends up at the top center of the dial — matching the visible
    /// portion of a partially-hidden bottom sheet.
    /// </summary>
    private double _menuLiftProgress = 1.0;
    internal double MenuLiftProgress
    {
        get => _menuLiftProgress;
        set
        {
            if (Math.Abs(_menuLiftProgress - value) < 0.001) return;
            _menuLiftProgress = value;
            _canvas?.Invalidate();
        }
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _inertiaEngine.Cancel();
        _velocityTracker.Reset();

        var pos = e.GetCurrentPoint(this).Position;
        _activeMode = DetectInputMode(pos);

        if (_activeMode == InputMode.Angular)
        {
            var cx = ActualWidth / 2;
            var cy = ActualHeight / 2;
            _lastAngle = Math.Atan2(pos.Y - cy, pos.X - cx) * (180.0 / Math.PI);
        }

        _isDragging = true;
        _dragStartY = pos.Y;
        _dragStartValue = Value;
        _lastValueForVelocity = Value;
        _lastMoveTick = Stopwatch.GetTimestamp();
        _lastPointerY = pos.Y;
        _lastDetentIndex = CurrentDetentIndex;
        _springSolver.SnapTo(NormalizedValue);

        // Click-vs-drag tracking — value won't change until pointer moves past
        // DragDeadZonePx, and inertia won't fire unless the drag exceeds the
        // InertiaMinDragPx + InertiaMinDurationMs thresholds.
        _pressX = pos.X;
        _pressY = pos.Y;
        _pressTick = _lastMoveTick;
        _hasExceededDeadZone = false;
        _totalDragPx = 0;

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

        // Update total displacement from press start (used for dead zone + inertia gate).
        // Use squared distance for the dead-zone early-exit so we avoid Math.Sqrt on
        // every pointer event that's still inside the click threshold — pointer
        // events can fire at 120+ Hz. Once we're past the dead zone we still need
        // the true distance for the inertia gate in EndDrag.
        var totalDx = pos.X - _pressX;
        var totalDy = pos.Y - _pressY;
        var totalDragSquared = totalDx * totalDx + totalDy * totalDy;

        if (!_hasExceededDeadZone && totalDragSquared < DragDeadZonePx * DragDeadZonePx)
        {
            // Still inside the dead zone — treat the press as a click so far.
            // No value change, no velocity sampling.
            return;
        }

        _totalDragPx = Math.Sqrt(totalDragSquared);

        if (!_hasExceededDeadZone)
        {

            // Just crossed the dead zone. Re-baseline so the value tracks smoothly
            // from the current pointer position with no jump.
            _hasExceededDeadZone = true;
            _dragStartY = pos.Y;
            _dragStartValue = Value;
            _lastPointerY = pos.Y;
            _lastValueForVelocity = Value;
            _lastMoveTick = Stopwatch.GetTimestamp();
            _velocityTracker.Reset();

            if (_activeMode == InputMode.Angular)
            {
                var cx = ActualWidth / 2;
                var cy = ActualHeight / 2;
                _lastAngle = Math.Atan2(pos.Y - cy, pos.X - cx) * (180.0 / Math.PI);
            }
            return;
        }

        var now = Stopwatch.GetTimestamp();
        var dt = (double)(now - _lastMoveTick) / Stopwatch.Frequency;
        _lastMoveTick = now;

        double newValue;
        if (_activeMode == InputMode.Angular)
        {
            var center = new Point(ActualWidth / 2, ActualHeight / 2);
            var valueDelta = _angularHandler.ComputeValueDelta(
                pos, center, ref _lastAngle, ArcSweepDegrees, Maximum - Minimum);
            newValue = Math.Clamp(Value + valueDelta, Minimum, Maximum);
        }
        else
        {
            var deltaY = _dragStartY - pos.Y;
            newValue = Math.Clamp(_dragStartValue + deltaY * Sensitivity, Minimum, Maximum);
        }

        // Only feed the velocity tracker when dt is large enough to give a stable estimate.
        // Sub-millisecond pointer events otherwise produce ridiculous velocities that the
        // inertia engine then flings the dial across.
        if (dt >= MinVelocitySampleDtSec)
            _velocityTracker.AddSample((newValue - _lastValueForVelocity) / dt);
        _lastValueForVelocity = newValue;
        _lastPointerY = pos.Y;

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

        // Inertia is only allowed if the user actually dragged — not if they just clicked.
        // A click that never crossed the dead zone, or a "drag" that was too short or too
        // brief, should not fling the dial.
        var dragDurationMs =
            (Stopwatch.GetTimestamp() - _pressTick) * 1000.0 / Stopwatch.Frequency;
        var wasRealDrag =
            _hasExceededDeadZone &&
            _totalDragPx >= InertiaMinDragPx &&
            dragDurationMs >= InertiaMinDurationMs;

        if (DialMode == DialMode.Menu)
        {
            // Menu mode: snap to nearest integer item, no inertia, fire SelectionConfirmed.
            var count = MenuItems?.Count ?? 0;
            if (count > 0)
            {
                var snapped = Math.Clamp(Math.Round(Value), 0, count - 1);
                if (snapped != Value)
                    Value = snapped;
            }
            RaiseSelectionConfirmedIfMenuMode();
        }
        else if (IsInertiaEnabled && wasRealDrag)
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

    private InputMode DetectInputMode(Point pos)
    {
        if (InteractionMode == DialInteractionMode.VerticalOnly) return InputMode.Vertical;
        if (InteractionMode == DialInteractionMode.AngularOnly) return InputMode.Angular;

        var cx = ActualWidth / 2;
        var cy = ActualHeight / 2;
        var dx = pos.X - cx;
        var dy = pos.Y - cy;
        var padding = Math.Min(ActualWidth, ActualHeight) * 0.05;
        var radius = Math.Min(cx, cy) - padding;
        var threshold = radius * 0.71 * 0.5;
        return (dx * dx + dy * dy) > threshold * threshold ? InputMode.Angular : InputMode.Vertical;
    }

    private void OnDetentCrossed(int newDetentIndex)
    {
        if (IsHapticEnabled)
        {
            if (newDetentIndex % 5 == 0) _hapticService.FireMajorDetentClick();
            else _hapticService.FireDetentClick();
        }
        if (IsAudioEnabled)
        {
            _clickAudio.PlayClick();
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

        // In menu mode, mirror Value into SelectedIndex (which renders the active segment).
        if (DialMode == DialMode.Menu)
        {
            var count = MenuItems?.Count ?? 0;
            if (count > 0)
            {
                var newIdx = Math.Clamp((int)Math.Round(newValue), 0, count - 1);
                if (newIdx != SelectedIndex)
                {
                    _suppressIndexFeedback = true;
                    SelectedIndex = newIdx;
                    _suppressIndexFeedback = false;
                }
            }
        }

        ValueChanged?.Invoke(this, new DialValueChangedEventArgs(oldValue, newValue));
        _canvas.Invalidate();
    }

    // ── v3: menu-mode plumbing ───────────────────────────────────────────────

    private bool _suppressIndexFeedback;

    private void OnDialModeChanged()
    {
        ApplyMenuModeRanges();
        _canvas?.Invalidate();
    }

    private void OnMenuItemsChanged()
    {
        ApplyMenuModeRanges();
        if (DialMode == DialMode.Menu)
        {
            UpdateSelectedItem();
        }
        _canvas?.Invalidate();
    }

    /// <summary>
    /// When in menu mode, force Min/Max/DetentCount to match MenuItems.Count so detent crossings,
    /// haptics and snapping align with item boundaries.
    /// </summary>
    private void ApplyMenuModeRanges()
    {
        if (DialMode != DialMode.Menu) return;

        var count = MenuItems?.Count ?? 0;
        if (count <= 0) return;

        Minimum = 0;
        Maximum = count - 1;
        DetentCount = count - 1;

        // Snap value to nearest int and sync the selection index.
        var snapped = Math.Clamp(Math.Round(Value), 0, count - 1);
        if (snapped != Value)
        {
            Value = snapped;
        }
        else
        {
            // Value didn't change but the index might still need updating.
            var newIdx = (int)snapped;
            if (newIdx != SelectedIndex)
            {
                _suppressIndexFeedback = true;
                SelectedIndex = newIdx;
                _suppressIndexFeedback = false;
            }
        }

        UpdateSelectedItem();
    }

    private void OnSelectedIndexChanged(int oldIndex, int newIndex)
    {
        UpdateSelectedItem();

        if (_suppressIndexFeedback) return;
        if (DialMode != DialMode.Menu) return;

        var count = MenuItems?.Count ?? 0;
        if (count <= 0) return;

        var clamped = Math.Clamp(newIndex, 0, count - 1);
        if ((double)clamped != Value)
        {
            Value = clamped;
        }
    }

    private void UpdateSelectedItem()
    {
        var items = MenuItems;
        if (items is null || items.Count == 0)
        {
            SelectedItem = null;
            return;
        }

        var idx = Math.Clamp(SelectedIndex, 0, items.Count - 1);
        SelectedItem = items[idx];
    }

    private void RaiseSelectionConfirmedIfMenuMode()
    {
        if (DialMode != DialMode.Menu) return;
        var items = MenuItems;
        if (items is null || items.Count == 0) return;

        var idx = Math.Clamp(SelectedIndex, 0, items.Count - 1);
        SelectionConfirmed?.Invoke(this, new MenuSelectionEventArgs(idx, items[idx]));
    }
}

public sealed class DialValueChangedEventArgs : EventArgs
{
    public double OldValue { get; }
    public double NewValue { get; }
    public DialValueChangedEventArgs(double oldValue, double newValue)
    { OldValue = oldValue; NewValue = newValue; }
}

public sealed class DetentCrossedEventArgs : EventArgs
{
    public int DetentIndex { get; }
    public DetentCrossedEventArgs(int detentIndex) { DetentIndex = detentIndex; }
}
