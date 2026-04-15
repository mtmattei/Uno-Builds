using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PrecisionDial.Controls;

namespace PrecisionDial.Samples;

/// <summary>
/// Android-style shell: a single screen with a menu dial that lives at the
/// bottom-center, mostly hidden. Tap the visible portion → it lifts on a spring,
/// the user rotates to a menu item, and on selection it auto-collapses.
///
/// The selection text in the upper-middle animates on change (slide + fade)
/// and an ambient amber comet travels around the screen perimeter.
///
/// All animations reuse the existing <see cref="SpringSolver"/> physics so the
/// motion language matches the rest of the app.
/// </summary>
public sealed partial class AndroidShellPage : Page
{
    // ── Bottom-sheet positions ───────────────────────────────────────────────
    private const double HiddenY = 170.0;
    private const double LiftedY = -20.0;

    // ── Selection text transition tunables ───────────────────────────────────
    private const double TextSlideDistance = 16.0; // pixels — shorter travel = snappier
    private const double TextFadeOutDurationMs = 75.0;
    private const double TextSwapDelayMs = 10.0;

    // ── Lift spring ──────────────────────────────────────────────────────────
    private readonly SpringSolver _liftSolver = new()
    {
        Stiffness = 220,
        Damping = 22,
        Mass = 1,
    };

    // ── Selection-label spring (drives Y + opacity together) ─────────────────
    // Stiffer than the lift spring so the text snaps into place quickly.
    private readonly SpringSolver _labelYSolver = new()
    {
        Stiffness = 520,
        Damping = 28,
        Mass = 1,
    };

    private readonly SpringSolver _labelOpacitySolver = new()
    {
        Stiffness = 520,
        Damping = 32,
        Mass = 1,
    };

    private DispatcherTimer? _animTimer;
    private bool _isLifted;

    // Selection text transition state
    private string _pendingText = "MUSIC";
    private bool _textIsFadingOut;
    private long _fadeOutStartTick;

    // Long-press → volume mode state. When the user holds the shield, the dial
    // lifts in DialMode.Value so they can dial a continuous 0-100 value.
    // _isHolding suppresses the Tapped event that also fires on release.
    private bool _isHolding;
    private bool _inVolumeMode;
    private int _savedMenuIndex;
    private string _savedMenuLabel = "MUSIC";
    private const double VolumeDefaultValue = 65;
    private const double VolumeMinimum = 0;
    private const double VolumeMaximum = 100;
    private const int VolumeDetentCount = 20;

    public AndroidShellPage()
    {
        InitializeComponent();

        // Icons come from the bundled Phosphor Icons TTF — identical rendering across
        // Windows, Android, iOS and WASM.
        ModeDial.MenuItems = new List<DialMenuItem>
        {
            new() { Label = "MUSIC",    Icon = "\uE33C", Tag = "music"    }, // ph-music-note
            new() { Label = "RADIO",    Icon = "\uE77E", Tag = "radio"    }, // ph-radio
            new() { Label = "PODCAST",  Icon = "\uE326", Tag = "podcast"  }, // ph-microphone
            new() { Label = "VIDEO",    Icon = "\uE4DA", Tag = "video"    }, // ph-video-camera
            new() { Label = "SETTINGS", Icon = "\uE270", Tag = "settings" }, // ph-gear
        };

        ModeDial.SelectionConfirmed += OnSelectionConfirmed;
        ModeDial.ValueChanged += OnDialValueChanged;
        ModeDial.DragCompleted += OnDialDragCompleted;

        // Snap the lift spring to the hidden position before the page is shown.
        _liftSolver.SnapTo(HiddenY);
        DialTranslate.Y = HiddenY;
        TapShield.IsHitTestVisible = true;

        // Snap the label springs to "settled at zero".
        _labelYSolver.SnapTo(0);
        _labelOpacitySolver.SnapTo(1.0);

        // Hook Android / WinUI system back so the user can dismiss the lifted
        // sheet without committing a selection. We subscribe on Loaded and
        // unsubscribe on Unloaded so the handler doesn't leak across pages.
        Loaded += OnLoadedHookBack;
        Unloaded += OnUnloadedUnhookBack;
    }

    private void OnLoadedHookBack(object sender, RoutedEventArgs e)
    {
        try
        {
            var nav = Windows.UI.Core.SystemNavigationManager.GetForCurrentView();
            nav.BackRequested += OnBackRequested;
            // Make sure the system actually forwards back presses to us.
            nav.AppViewBackButtonVisibility =
                Windows.UI.Core.AppViewBackButtonVisibility.Visible;
        }
        catch { /* unsupported on this target — ignore */ }
    }

    private void OnUnloadedUnhookBack(object sender, RoutedEventArgs e)
    {
        try
        {
            var nav = Windows.UI.Core.SystemNavigationManager.GetForCurrentView();
            nav.BackRequested -= OnBackRequested;
        }
        catch { /* ignore */ }
    }

    private void OnBackRequested(object? sender, Windows.UI.Core.BackRequestedEventArgs e)
    {
        if (_isLifted)
        {
            Collapse();
            e.Handled = true;
        }
    }

    private void OnBackdropTapped(object sender, TappedRoutedEventArgs e)
    {
        if (_isLifted)
        {
            Collapse();
        }
    }

    // ── Dial → text plumbing ─────────────────────────────────────────────────

    private void OnDialValueChanged(object? sender, DialValueChangedEventArgs e)
    {
        if (_inVolumeMode)
        {
            // Volume mode: show the raw integer value directly, no transition
            // (continuous dragging would hammer the slide animation).
            var valueText = ((int)Math.Round(e.NewValue)).ToString();
            _pendingText = valueText;
            SelectionLabel.Text = valueText;
            return;
        }

        var item = ModeDial.SelectedItem;
        if (item?.Label is null) return;
        if (item.Label == _pendingText && !_textIsFadingOut) return;

        BeginTextTransition(item.Label);
    }

    private void OnDialDragCompleted(object sender, RoutedEventArgs e)
    {
        // Volume mode auto-collapses ~500ms after the user releases so they
        // see the final value settle before the sheet drops.
        if (!_inVolumeMode || !_isLifted) return;

        var collapseTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        collapseTimer.Tick += (_, _) =>
        {
            collapseTimer.Stop();
            Collapse();
        };
        collapseTimer.Start();
    }

    private void OnSelectionConfirmed(object? sender, MenuSelectionEventArgs args)
    {
        if (args.SelectedItem?.Label is { } label && label != _pendingText)
        {
            BeginTextTransition(label);
        }

        // Brief pause so the user sees the confirmed segment before it slides away.
        var collapseTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(220) };
        collapseTimer.Tick += (_, _) =>
        {
            collapseTimer.Stop();
            Collapse();
        };
        collapseTimer.Start();
    }

    // ── Text transition state machine ────────────────────────────────────────

    private void BeginTextTransition(string newText)
    {
        _pendingText = newText;
        _textIsFadingOut = true;
        _fadeOutStartTick = System.Diagnostics.Stopwatch.GetTimestamp();

        // Slide current label up and fade out simultaneously.
        _labelYSolver.SetTarget(-TextSlideDistance);
        _labelOpacitySolver.SetTarget(0.0);
        EnsureAnimTimer();
    }

    private void CompleteTextSwapIn()
    {
        // Replace the text content, jump to "below + invisible", then animate up to "rest".
        SelectionLabel.Text = _pendingText;
        _labelYSolver.SnapTo(TextSlideDistance);
        _labelOpacitySolver.SnapTo(0.0);
        SelectionLabelTranslate.Y = TextSlideDistance;
        SelectionLabel.Opacity = 0;

        _labelYSolver.SetTarget(0);
        _labelOpacitySolver.SetTarget(1.0);
        _textIsFadingOut = false;
        EnsureAnimTimer();
    }

    // ── Tap to lift / long-press for volume ──────────────────────────────────

    private void OnShieldTapped(object sender, TappedRoutedEventArgs e)
    {
        // Suppress the Tapped that fires on release after a Holding gesture.
        if (_isHolding) { _isHolding = false; return; }
        if (_isLifted) return;

        Lift(asVolumeMode: false);
    }

    private void OnShieldHolding(object sender, HoldingRoutedEventArgs e)
    {
        if (e.HoldingState != Microsoft.UI.Input.HoldingState.Started) return;
        if (_isLifted) return;

        _isHolding = true;
        Lift(asVolumeMode: true);
    }

    private void Lift(bool asVolumeMode)
    {
        if (asVolumeMode)
        {
            EnterVolumeMode();
        }

        _isLifted = true;
        // Shield stays hit-test-visible during the lift animation so a stray
        // touch mid-flight can't grab the dial before it finishes rising.
        // It's flipped off in OnAnimTick once the lift spring settles.
        HintText.Opacity = 0;
        _liftSolver.SetTarget(LiftedY);
        EnsureAnimTimer();
    }

    private void Collapse()
    {
        _isLifted = false;
        // Re-enable shield + hide backdrop immediately on collapse start so a
        // mid-collapse tap re-lifts the sheet instead of trying to dismiss.
        TapShield.IsHitTestVisible = true;
        Backdrop.IsHitTestVisible = false;
        HintText.Opacity = 1;
        _liftSolver.SetTarget(HiddenY);
        EnsureAnimTimer();

        // If we were in volume mode, restore the menu so the resting state is
        // always the menu with the last selected item at the top.
        if (_inVolumeMode)
        {
            ExitVolumeMode();
        }
    }

    // ── Volume-mode transition ───────────────────────────────────────────────

    private void EnterVolumeMode()
    {
        // Save the current menu selection so we can restore it on collapse.
        _savedMenuIndex = ModeDial.SelectedIndex;
        _savedMenuLabel = ModeDial.SelectedItem?.Label ?? _pendingText;

        ModeDial.DialMode = DialMode.Value;
        ModeDial.Minimum = VolumeMinimum;
        ModeDial.Maximum = VolumeMaximum;
        ModeDial.DetentCount = VolumeDetentCount;
        ModeDial.Value = VolumeDefaultValue;

        _inVolumeMode = true;
        _pendingText = ((int)VolumeDefaultValue).ToString();
        SelectionLabel.Text = _pendingText;
    }

    private void ExitVolumeMode()
    {
        _inVolumeMode = false;
        ModeDial.DialMode = DialMode.Menu;
        // ApplyMenuModeRanges resets Min/Max/DetentCount internally when
        // DialMode changes. Restoring the selection after that brings the
        // previous menu item back.
        ModeDial.SelectedIndex = _savedMenuIndex;
        SelectionLabel.Text = _savedMenuLabel;
        _pendingText = _savedMenuLabel;
    }

    // ── Animation loop ───────────────────────────────────────────────────────

    private void EnsureAnimTimer()
    {
        if (_animTimer == null)
        {
            _animTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _animTimer.Tick += OnAnimTick;
        }
        if (!_animTimer.IsEnabled) _animTimer.Start();
    }

    private void OnAnimTick(object? sender, object e)
    {
        const double dt = 0.016;

        // Lift / collapse spring
        var liftCurrent = _liftSolver.Step(dt);
        DialTranslate.Y = liftCurrent;

        // ── Tie the perimeter light to the lift position ─────────────────────
        // Map dial Y in [HiddenY, LiftedY] → light opacity in [0, 1]. The light
        // grows as the menu rises and dissipates as it drops, then is hidden
        // entirely so the perimeter canvas stops invalidating itself.
        var liftProgress = Math.Clamp((HiddenY - liftCurrent) / (HiddenY - LiftedY), 0.0, 1.0);
        LightBorder.Opacity = liftProgress;
        LightBorder.Visibility = liftProgress > 0.001
            ? Microsoft.UI.Xaml.Visibility.Visible
            : Microsoft.UI.Xaml.Visibility.Collapsed;

        // Feed lift progress into the dial itself so the menu arc rotates to
        // bring the selected segment to the top center as it collapses.
        ModeDial.MenuLiftProgress = liftProgress;

        // If we are mid fade-out and enough time has passed, swap in the new text.
        if (_textIsFadingOut)
        {
            var elapsedMs =
                System.Diagnostics.Stopwatch.GetElapsedTime(_fadeOutStartTick).TotalMilliseconds;
            if (elapsedMs >= TextFadeOutDurationMs + TextSwapDelayMs)
            {
                CompleteTextSwapIn();
            }
        }

        // Selection text spring (Y + opacity in lockstep)
        var labelY = _labelYSolver.Step(dt);
        var labelOpacity = _labelOpacitySolver.Step(dt);
        SelectionLabelTranslate.Y = labelY;
        SelectionLabel.Opacity = Math.Clamp(labelOpacity, 0, 1);

        // Lift-spring settled handoff: once the dial has fully arrived at its
        // target, flip the hit-test owners. While lifted, the shield stops
        // absorbing taps so the dial gets direct drag events, and the backdrop
        // starts catching tap-outside-to-dismiss. While collapsed, the shield
        // catches lift taps and the backdrop is off.
        if (_liftSolver.IsSettled)
        {
            var wantShieldOn = !_isLifted;
            var wantBackdropOn = _isLifted;
            if (TapShield.IsHitTestVisible != wantShieldOn)
                TapShield.IsHitTestVisible = wantShieldOn;
            if (Backdrop.IsHitTestVisible != wantBackdropOn)
                Backdrop.IsHitTestVisible = wantBackdropOn;
        }

        // Stop the timer only when nothing is animating.
        if (_liftSolver.IsSettled
            && _labelYSolver.IsSettled
            && _labelOpacitySolver.IsSettled
            && !_textIsFadingOut)
        {
            _animTimer?.Stop();
        }
    }
}
