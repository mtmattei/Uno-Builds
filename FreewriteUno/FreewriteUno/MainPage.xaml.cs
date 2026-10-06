using System.Text;
using FreewriteUno.InlineAi.Models;
using FreewriteUno.InlineAi.Presentation;
using FreewriteUno.InlineAi.Presentation.Controls;
using FreewriteUno.InlineAi.Services;
using FreewriteUno.Services;
using FreewriteUno.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.ViewManagement;

namespace FreewriteUno;

public sealed partial class MainPage : Page
{
    private const int ChromeFadeOutDelayMs = 2500;
    private const int ChromeFadeOutDurationMs = 240;
    private const int ChromeFadeInDurationMs = 160;
    private const int ChromeHoverLeaveDelayMs = 1500;
    // Toolbar never fully disappears; a ghost at ~18% opacity hints at its location
    // so writers can find it without remembering "tap the bottom of the screen".
    private const double ChromeRestOpacity = 0.18;
    private const int ThemeCrossFadeMs = 200;
    private const int ToastVisibleMs = 1800;
    private const int BackspaceFlashMs = 80;
    private const int ChatPromptUrlMaxLength = 6000;

    // Inline AI overlay anchoring (review mode).
    private const double InlineCardWidth = 390;
    private const double InlineEstimatedPillWidth = 150;
    private const double InlineEstimatedCardHeight = 440;
    private const double InlineGap = 8;

    private const string ChatPromptPrefix =
        "Below is my freewrite entry. Don't analyze it as therapy or coaching. Reply as a thoughtful reader.";

    public MainViewModel ViewModel { get; }

    private DispatcherTimer? _countdownTimer;
    private DispatcherTimer? _chromeFadeOutTimer;
    private DispatcherTimer? _chromeHoverLeaveTimer;
    private DispatcherTimer? _toastTimer;
    private DispatcherTimer? _backspaceFlashTimer;
    private Storyboard? _placeholderShimmerStoryboard;
    private bool _animationsEnabled = true;

    private BindableInlineAiModel? _inlineVm;
    private IEditorBridge? _inlineBridge;
    private Rect _selectionPageRect;

    public MainPage()
    {
        ViewModel = App.Services.GetRequiredService<MainViewModel>();
        ViewModel.OnCopyChatPrompt = CopyChatPromptAsync;
        ViewModel.OnExportPdf = ExportPdfAsync;
        ViewModel.OnPersistentToast = ShowPersistentToast;
        ViewModel.OnClearPersistentToast = ClearPersistentToast;
        ViewModel.OnRevealFolder = RevealFolderAsync;
        ViewModel.OnThemeAboutToChange = AnimateThemeCrossFade;
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        this.InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public Microsoft.UI.Xaml.Media.FontFamily FontFamilyForName(string name)
    {
        var key = name switch
        {
            "Newsreader" => "NewsreaderFontFamily",
            "JetBrains Mono" => "JetBrainsMonoFontFamily",
            _ => "LatoFontFamily",
        };
        return (Microsoft.UI.Xaml.Media.FontFamily)Application.Current.Resources[key];
    }

    public Microsoft.UI.Xaml.Media.Brush ActiveOrInactiveBrush(bool isActive)
    {
        var key = isActive ? "ChromeIconBrushActive" : "ChromeIconBrushInactive";
        return (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources[key];
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        DetectReducedMotion();
        ConfigureBackspaceToggleForPlatform();
        StripTooltipsOnTouchPlatforms();
        ApplyThemeToRoot();
        StartCountdownTimer();
        StartPlaceholderShimmer();
        SetupInlineAi();
        await ViewModel.InitializeAsync();
        EditorTextBox.Focus(FocusState.Programmatic);
        EditorTextBox.SelectionStart = ViewModel.Text.Length;
    }

    private void StartPlaceholderShimmer()
    {
        if (!_animationsEnabled) return;

        // Brush-opacity pulse: the placeholder text's color brush fades 0.35 -> 0.75
        // -> 0.35 over 2.6s, looping forever. The text reads as a soft glow that
        // sweeps in intensity. Animating Brush.Opacity (not TextBlock.Opacity)
        // avoids conflict with the binding that toggles the placeholder on/off.
        // LinearGradientBrush sweep is the cleaner spatial effect but doesn't
        // render reliably on TextBlock.Foreground in Uno Skia today.
        var up = new DoubleAnimation
        {
            From = 0.35, To = 0.75,
            Duration = TimeSpan.FromSeconds(1.3),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
        };
        Storyboard.SetTarget(up, PlaceholderShimmerBrush);
        Storyboard.SetTargetProperty(up, "Opacity");

        var down = new DoubleAnimation
        {
            From = 0.75, To = 0.35,
            BeginTime = TimeSpan.FromSeconds(1.3),
            Duration = TimeSpan.FromSeconds(1.3),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
        };
        Storyboard.SetTarget(down, PlaceholderShimmerBrush);
        Storyboard.SetTargetProperty(down, "Opacity");

        _placeholderShimmerStoryboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };
        _placeholderShimmerStoryboard.Children.Add(up);
        _placeholderShimmerStoryboard.Children.Add(down);
        _placeholderShimmerStoryboard.Begin();
    }

    private void StripTooltipsOnTouchPlatforms()
    {
#if __ANDROID__
        // Tooltips on Android trigger via long-press and float to mid-screen with
        // desktop-flavored copy. Clear them across the page; touch UX uses
        // AutomationProperties.Name (TalkBack) for discoverability instead.
        foreach (var btn in EnumerateDescendants<Button>(this))
        {
            ToolTipService.SetToolTip(btn, null);
        }
#endif
    }

    private static IEnumerable<T> EnumerateDescendants<T>(Microsoft.UI.Xaml.DependencyObject root)
        where T : Microsoft.UI.Xaml.DependencyObject
    {
        var count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(root, i);
            if (child is T match) yield return match;
            foreach (var nested in EnumerateDescendants<T>(child)) yield return nested;
        }
    }

    private void ApplyThemeToRoot()
    {
        // Page-level RequestedTheme alone doesn't trigger re-resolution for app-level
        // MaterialToolkitTheme resources. Set it on the Window's root content so the
        // theme dictionaries actually swap.
        if (App.MainAppWindow?.Content is FrameworkElement root)
        {
            root.RequestedTheme = ViewModel.Theme;
        }
    }

    private void ConfigureBackspaceToggleForPlatform()
    {
#if __ANDROID__
        // v1 ships Android with the backspace lock disabled per Architecture Brief §6.5
        // fallback. The InputConnection shim lands in v1.1.
        BackspaceButton.IsEnabled = false;
        BackspaceButton.Opacity = 0.55;
        ToolTipService.SetToolTip(BackspaceButton, "Backspace lock — available in v1.1");
#endif
    }

    private void DetectReducedMotion()
    {
        try
        {
            var settings = new UISettings();
            _animationsEnabled = settings.AnimationsEnabled;
        }
        catch
        {
            _animationsEnabled = true;
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        // Break VM->Page delegate references now that the VM is a singleton.
        // Without this, a recreated Page would leave the VM pointing at the stale instance.
        ViewModel.OnCopyChatPrompt = null;
        ViewModel.OnExportPdf = null;
        ViewModel.OnPersistentToast = null;
        ViewModel.OnClearPersistentToast = null;
        ViewModel.OnRevealFolder = null;
        ViewModel.OnThemeAboutToChange = null;
        _countdownTimer?.Stop();
        _chromeFadeOutTimer?.Stop();
        _chromeHoverLeaveTimer?.Stop();
        _toastTimer?.Stop();
        _backspaceFlashTimer?.Stop();
        _placeholderShimmerStoryboard?.Stop();

        // The editor bridge is a DI singleton; clear its delegates so a recreated Page
        // doesn't leave it pointing at this stale instance (mirrors the VM-delegate cleanup above).
        if (_inlineBridge is not null)
        {
            _inlineBridge.Applier = null;
            _inlineBridge.SelectionReleaser = null;
        }
        ActionPill.Activated -= OnPillActivated;
        ChatPopup.Opened -= OnChatOpened;
    }

    private void StartCountdownTimer()
    {
        _countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _countdownTimer.Tick += (_, _) =>
        {
            if (!ViewModel.TimerIsRunning) return;
            if (ViewModel.TimeRemaining <= 0)
            {
                ViewModel.TimerIsRunning = false;
                // Countdown complete → enter review mode so the inline AI pill becomes available.
                ViewModel.IsReviewMode = true;
                return;
            }
            ViewModel.TimeRemaining -= 1;
        };
        _countdownTimer.Start();
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainViewModel.Theme):
                ApplyThemeToRoot();
                break;
            case nameof(MainViewModel.TimerIsRunning):
                HandleTimerRunningChanged();
                break;
            case nameof(MainViewModel.TimeRemaining):
                if (ViewModel.TimeRemaining == 0 && !ViewModel.TimerIsRunning)
                {
                    SnapChromeVisible();
                }
                break;
            case nameof(MainViewModel.IsReviewMode):
                if (ViewModel.IsReviewMode)
                {
                    // Entering review mode (e.g. tapping the sparkle): if text is already selected,
                    // surface the pill now instead of waiting for the next selection gesture.
                    EvaluateSelection();
                }
                else
                {
                    // Leaving review mode dismisses any open chat and clears the held selection.
                    ChatPopup.IsOpen = false;
                    ReleaseInlineSelection();
                }
                break;
        }
    }

    // ─── Chrome fade ───────────────────────────────────────────────────────

    private void HandleTimerRunningChanged()
    {
        _chromeFadeOutTimer?.Stop();
        _chromeHoverLeaveTimer?.Stop();
        if (ViewModel.TimerIsRunning)
        {
            if (_chromeFadeOutTimer is null)
            {
                _chromeFadeOutTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ChromeFadeOutDelayMs) };
                _chromeFadeOutTimer.Tick += OnChromeFadeOutTick;
            }
            else
            {
                _chromeFadeOutTimer.Interval = TimeSpan.FromMilliseconds(ChromeFadeOutDelayMs);
            }
            _chromeFadeOutTimer.Start();
        }
        else
        {
            SnapChromeVisible();
        }
    }

    private void OnChromeFadeOutTick(object? sender, object e)
    {
        _chromeFadeOutTimer?.Stop();
        FadeChrome(ChromeRestOpacity, ChromeFadeOutDurationMs);
    }

    private void SnapChromeVisible()
    {
        _chromeFadeOutTimer?.Stop();
        _chromeHoverLeaveTimer?.Stop();
        ViewModel.ChromeOpacity = 1.0;
    }

    private void FadeChrome(double target, int durationMs)
    {
        if (!_animationsEnabled)
        {
            ViewModel.ChromeOpacity = target;
            return;
        }
        var start = ViewModel.ChromeOpacity;
        var animation = new DoubleAnimation
        {
            From = start,
            To = target,
            Duration = TimeSpan.FromMilliseconds(durationMs),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
        };
        Storyboard.SetTarget(animation, ChromeContainer);
        Storyboard.SetTargetProperty(animation, "Opacity");
        var sb = new Storyboard();
        sb.Children.Add(animation);
        sb.Completed += (_, _) => ViewModel.ChromeOpacity = target;
        sb.Begin();
    }

    private void BottomHoverSensor_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (!ViewModel.TimerIsRunning) return;
        KeepChromeAlive();
    }

    private void BottomHoverSensor_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!ViewModel.TimerIsRunning) return;
        ScheduleChromeFadeAfterHoverLeave();
    }

    private void ChromeContainer_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        // Cancel any scheduled fade-out — the writer is interacting with the chrome.
        // Applies whether or not the timer is running; the chrome should never disappear
        // while the pointer is on it.
        _chromeFadeOutTimer?.Stop();
        _chromeHoverLeaveTimer?.Stop();
        FadeChrome(1.0, ChromeFadeInDurationMs);
    }

    private void ChromeContainer_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!ViewModel.TimerIsRunning) return;
        ScheduleChromeFadeAfterHoverLeave();
    }

    private void ChromeContainer_GotFocus(object sender, RoutedEventArgs e)
    {
        // Keyboard focus into the chrome must reveal it — otherwise a Tab user lands on an
        // invisible button when the timer has faded the chrome out. WCAG 2.4.7.
        _chromeFadeOutTimer?.Stop();
        _chromeHoverLeaveTimer?.Stop();
        FadeChrome(1.0, ChromeFadeInDurationMs);
    }

    private void KeepChromeAlive()
    {
        _chromeHoverLeaveTimer?.Stop();
        FadeChrome(1.0, ChromeFadeInDurationMs);
    }

    private void ScheduleChromeFadeAfterHoverLeave()
    {
        _chromeHoverLeaveTimer?.Stop();
        if (_chromeHoverLeaveTimer is null)
        {
            _chromeHoverLeaveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ChromeHoverLeaveDelayMs) };
            _chromeHoverLeaveTimer.Tick += OnChromeHoverLeaveTick;
        }
        _chromeHoverLeaveTimer.Start();
    }

    private void OnChromeHoverLeaveTick(object? sender, object e)
    {
        _chromeHoverLeaveTimer?.Stop();
        FadeChrome(ChromeRestOpacity, ChromeFadeOutDurationMs);
    }

    // ─── Theme cross-fade ──────────────────────────────────────────────────

    private void AnimateThemeCrossFade(Microsoft.UI.Xaml.ElementTheme outgoingTheme)
    {
        if (!_animationsEnabled)
        {
            ThemeFadeOverlay.Opacity = 0;
            return;
        }
        // OnThemeChanging fires BEFORE the theme is swapped, so resources still resolve
        // to the outgoing theme here. Pull the surface brush from the active theme dictionary
        // instead of hardcoding the palette in code-behind.
        var outgoingBrush = (Brush)Application.Current.Resources["EditorBackgroundBrush"];
        ThemeFadeOverlay.Background = outgoingBrush;
        ThemeFadeOverlay.Opacity = 1.0;
        _ = outgoingTheme; // signature kept for the OnThemeAboutToChange contract

        var anim = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = TimeSpan.FromMilliseconds(ThemeCrossFadeMs),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
        };
        Storyboard.SetTarget(anim, ThemeFadeOverlay);
        Storyboard.SetTargetProperty(anim, "Opacity");
        var sb = new Storyboard();
        sb.Children.Add(anim);
        sb.Begin();
    }

    // ─── Timer scroll-wheel and double-click ───────────────────────────────

    private void TimerButton_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var props = e.GetCurrentPoint(TimerButton).Properties;
        var delta = props.MouseWheelDelta;
        if (delta == 0) return;
        ViewModel.AdjustTimer(delta > 0 ? 5 : -5);
        e.Handled = true;
    }

    private void TimerButton_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        ViewModel.ResetTimerCommand.Execute(null);
        e.Handled = true;
    }

    // ─── Backspace lock ────────────────────────────────────────────────────

    private void EditorTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (!ViewModel.BackspaceDisabled) return;
        if (e.Key == VirtualKey.Back || e.Key == VirtualKey.Delete)
        {
            e.Handled = true;
            FlashBackspace();
        }
    }

    private void FlashBackspace()
    {
        BackspaceFlash.Opacity = 0.6;
        _backspaceFlashTimer?.Stop();
        if (_backspaceFlashTimer is null)
        {
            _backspaceFlashTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(BackspaceFlashMs) };
            _backspaceFlashTimer.Tick += OnBackspaceFlashTick;
        }
        _backspaceFlashTimer.Start();
    }

    private void OnBackspaceFlashTick(object? sender, object e)
    {
        _backspaceFlashTimer?.Stop();
        if (!_animationsEnabled)
        {
            BackspaceFlash.Opacity = 0;
            return;
        }
        var anim = new DoubleAnimation
        {
            From = 0.6,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(120),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut },
        };
        Storyboard.SetTarget(anim, BackspaceFlash);
        Storyboard.SetTargetProperty(anim, "Opacity");
        var sb = new Storyboard();
        sb.Children.Add(anim);
        sb.Begin();
    }

    // ─── Inline AI (review mode) ───────────────────────────────────────────

    private void SetupInlineAi()
    {
        var services = App.Services;
        var ai = services.GetRequiredService<IAiService>();
        var clipboard = services.GetRequiredService<IClipboardService>();
        _inlineBridge = services.GetRequiredService<IEditorBridge>();
        _inlineBridge.Applier = ApplyRewriteAsync;
        _inlineBridge.SelectionReleaser = ReleaseInlineSelection;

        // The generated MVUX ViewModel is the DataContext for both popups; the chat view binds
        // its feeds/commands via {Binding}, and ChatPopup.IsOpen two-ways against IsChatOpen.
        _inlineVm = new BindableInlineAiModel(ai, clipboard, _inlineBridge);
        PillPopup.DataContext = _inlineVm;
        ChatPopup.DataContext = _inlineVm;

        // Selection-end fires on pointer-up (mouse) and key-up (keyboard); handledEventsToo so the
        // TextBox's own handling doesn't swallow them.
        EditorTextBox.AddHandler(PointerReleasedEvent, new PointerEventHandler(OnEditorSelectionGesture), handledEventsToo: true);
        EditorTextBox.AddHandler(KeyUpEvent, new KeyEventHandler(OnEditorKeyUp), handledEventsToo: true);
        ActionPill.Activated += OnPillActivated;
        ChatPopup.Opened += OnChatOpened;
    }

    private void OnEditorSelectionGesture(object sender, PointerRoutedEventArgs e) => EvaluateSelection();

    private void OnEditorKeyUp(object sender, KeyRoutedEventArgs e) => EvaluateSelection();

    private void EvaluateSelection()
    {
        // Gated to review mode — the writing phase stays distraction-free.
        if (_inlineBridge is null || !ViewModel.IsReviewMode || ChatPopup.IsOpen)
        {
            return;
        }

        if (!SelectionAnchor.TryRead(EditorTextBox, out var context))
        {
            PillPopup.IsOpen = false;
            return;
        }

        _inlineBridge.CurrentSelection = context;
        var rectInEditor = TryMeasureSelectionRect(context.Start, context.Length, out var measured)
            ? measured
            : default;
        _selectionPageRect = ToEditorRootRect(rectInEditor);
        PositionPill(_selectionPageRect);
        PillPopup.IsOpen = true;
    }

    /// <summary>
    /// Approximates the selection's bounding rect in the editor's content coordinates by measuring a
    /// hidden mirror TextBlock — TextBox exposes no selection geometry on Skia. Locates the vertical
    /// band of the selected line(s); horizontal extent spans the text column (per-glyph X isn't
    /// recoverable). Returns false when the editor isn't laid out yet.
    /// </summary>
    private bool TryMeasureSelectionRect(int start, int length, out Rect rect)
    {
        rect = default;
        var text = EditorTextBox.Text ?? string.Empty;
        var width = EditorTextBox.ActualWidth;
        if (width <= 0 || start < 0 || start > text.Length)
        {
            return false;
        }

        var padding = EditorTextBox.Padding;
        var contentWidth = Math.Max(1, width - padding.Left - padding.Right);
        MeasureBlock.FontFamily = EditorTextBox.FontFamily;
        MeasureBlock.FontSize = EditorTextBox.FontSize;
        MeasureBlock.Width = contentWidth;
        var avail = new Size(contentWidth, double.PositiveInfinity);

        MeasureBlock.Text = "Ag";
        MeasureBlock.Measure(avail);
        var lineH = MeasureBlock.DesiredSize.Height;
        if (lineH <= 0)
        {
            return false;
        }

        MeasureBlock.Text = start == 0 ? string.Empty : text[..start];
        MeasureBlock.Measure(avail);
        var beforeH = MeasureBlock.DesiredSize.Height;

        var end = Math.Min(text.Length, start + length);
        MeasureBlock.Text = text[..end];
        MeasureBlock.Measure(avail);
        var throughH = MeasureBlock.DesiredSize.Height;

        var lineTop = Math.Max(0, beforeH - lineH);
        var height = Math.Max(lineH, throughH - lineTop);
        rect = new Rect(padding.Left, lineTop + padding.Top, contentWidth, height);
        return true;
    }

    private Rect ToEditorRootRect(Rect rectInEditor)
    {
        // Degenerate rect → platform didn't supply bounds; fall back to a band near the editor top.
        if (rectInEditor.Width <= 0 && rectInEditor.Height <= 0)
        {
            var fallback = EditorTextBox.TransformToVisual(EditorRoot).TransformPoint(new Point(0, 0));
            return new Rect(fallback.X + 24, fallback.Y + 24, Math.Min(240, EditorTextBox.ActualWidth), 24);
        }

        var topLeft = EditorTextBox.TransformToVisual(EditorRoot).TransformPoint(new Point(rectInEditor.X, rectInEditor.Y));
        return new Rect(topLeft.X, topLeft.Y, rectInEditor.Width, rectInEditor.Height);
    }

    private void PositionPill(Rect anchor)
    {
        var x = anchor.X + (anchor.Width / 2) - (InlineEstimatedPillWidth / 2);
        var y = anchor.Y - 44 - InlineGap;
        PillPopup.HorizontalOffset = ClampOffset(x, InlineGap, EditorRoot.ActualWidth - InlineEstimatedPillWidth - InlineGap);
        PillPopup.VerticalOffset = Math.Max(InlineGap, y);
    }

    private void OnPillActivated(object sender, RoutedEventArgs e)
    {
        // The pill buttons' OpenChat command sets IsChatOpen → ChatPopup opens via the two-way binding.
        PillPopup.IsOpen = false;
        PositionCard(_selectionPageRect);
        PaintHighlight(_selectionPageRect);
    }

    private void PositionCard(Rect anchor)
    {
        var belowY = anchor.Y + anchor.Height + InlineGap;
        var fitsBelow = belowY + InlineEstimatedCardHeight <= EditorRoot.ActualHeight - InlineGap;
        var y = fitsBelow ? belowY : anchor.Y - InlineEstimatedCardHeight - InlineGap;

        ChatPopup.HorizontalOffset = ClampOffset(anchor.X, InlineGap, Math.Max(InlineGap, EditorRoot.ActualWidth - InlineCardWidth - InlineGap));
        ChatPopup.VerticalOffset = Math.Max(InlineGap, y);
    }

    private void OnChatOpened(object? sender, object e) => ChatView.FocusComposer();

    private void PaintHighlight(Rect anchor)
    {
        Canvas.SetLeft(SelectionHighlight, anchor.X);
        Canvas.SetTop(SelectionHighlight, anchor.Y);
        SelectionHighlight.Width = anchor.Width;
        SelectionHighlight.Height = anchor.Height;
        SelectionHighlight.Visibility = Visibility.Visible;
    }

    private void ReleaseInlineSelection()
    {
        SelectionHighlight.Visibility = Visibility.Collapsed;
        PillPopup.IsOpen = false;
    }

    /// <summary>
    /// Writes a rewrite back over the original range. Offsets were captured against the live TextBox
    /// text (leading "\n\n" prefix included), so Select targets the right characters directly; the
    /// resulting TextChanged flows through the VM's prefix-guard + debounced save like any edit.
    /// </summary>
    private Task<bool> ApplyRewriteAsync(RewriteResult result)
    {
        try
        {
            var text = EditorTextBox.Text ?? string.Empty;
            var start = Math.Clamp(result.TargetStart, 0, text.Length);
            var length = Math.Clamp(result.TargetLength, 0, text.Length - start);
            EditorTextBox.Select(start, length);
            EditorTextBox.SelectedText = result.Text;
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private static double ClampOffset(double value, double min, double max) =>
        max < min ? min : Math.Clamp(value, min, max);

    // ─── Chat handoff ──────────────────────────────────────────────────────

    private void ChatClaude_Click(object sender, RoutedEventArgs e)
        => ViewModel.CopyChatPromptCommand.Execute("claude");

    private void ChatChatGpt_Click(object sender, RoutedEventArgs e)
        => ViewModel.CopyChatPromptCommand.Execute("chatgpt");

    private async Task CopyChatPromptAsync(string target)
    {
        var content = ViewModel.Text.TrimStart('\n', '\r').Trim();
        var prompt = ChatPromptPrefix + "\n\n" + content;
        var encoded = Uri.EscapeDataString(prompt);

        var serviceLabel = target == "claude" ? "Claude" : "ChatGPT";
        var baseUrl = target == "claude"
            ? "https://claude.ai/new?q="
            : "https://chat.openai.com/?q=";

        if (encoded.Length < ChatPromptUrlMaxLength)
        {
            try
            {
                await Launcher.LaunchUriAsync(new Uri(baseUrl + encoded));
                return;
            }
            catch
            {
                // fall through to clipboard
            }
        }

        try
        {
            var pkg = new DataPackage();
            pkg.SetText(prompt);
            Clipboard.SetContent(pkg);
            ShowToast($"Prompt copied for {serviceLabel}");
        }
        catch
        {
            ShowToast("Couldn't copy to clipboard.");
        }
    }

    // ─── PDF export (M3 stub — wires picker, exporter implementation lands later) ──

    private async Task ExportPdfAsync()
    {
        try
        {
            var exporter = App.Services.GetRequiredService<IPdfExporter>();
            var suggestedName = BuildPdfFileName(ViewModel.Text);
            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("PDF document", new List<string> { ".pdf" });
            picker.SuggestedFileName = suggestedName;

            var window = App.MainAppWindow;
            if (window is not null)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            }

            var file = await picker.PickSaveFileAsync();
            if (file is null) return;

            var bytes = await exporter.RenderAsync(ViewModel.Text, suggestedName);
            await File.WriteAllBytesAsync(file.Path, bytes);
            ShowToast($"Saved to {file.Name}");
        }
        catch
        {
            ShowToast("Couldn't export PDF.");
        }
    }

    private static string BuildPdfFileName(string text)
    {
        var trimmed = text.TrimStart('\n', '\r').Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return $"Entry {DateTimeOffset.Now:yyyy-MM-dd}";
        }
        var words = trimmed.Split(new[] { ' ', '\n', '\r', '\t' }, 5, StringSplitOptions.RemoveEmptyEntries);
        var first4 = string.Join(' ', words.Take(4));
        var sb = new StringBuilder();
        foreach (var ch in first4)
        {
            if (char.IsLetterOrDigit(ch) || ch == ' ') sb.Append(ch);
        }
        var cleaned = sb.ToString().Trim();
        return string.IsNullOrEmpty(cleaned) ? $"Entry {DateTimeOffset.Now:yyyy-MM-dd}" : cleaned;
    }

    // ─── Show entries folder in OS file manager ────────────────────────────

    private Task RevealFolderAsync(string path)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{path}\"",
                    UseShellExecute = true,
                });
            }
            else if (OperatingSystem.IsMacOS())
            {
                System.Diagnostics.Process.Start("open", path);
            }
            else if (OperatingSystem.IsLinux())
            {
                System.Diagnostics.Process.Start("xdg-open", path);
            }
        }
        catch
        {
            ShowToast("Couldn't open entries folder.");
        }
        return Task.CompletedTask;
    }

    // ─── Toast ─────────────────────────────────────────────────────────────

    // ─── Persistent toast ──────────────────────────────────────────────────

    private void ShowPersistentToast(string text)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            PersistentToastText.Text = text;
            PersistentToastBorder.Visibility = Visibility.Visible;
        });
    }

    private void ClearPersistentToast()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            PersistentToastBorder.Visibility = Visibility.Collapsed;
        });
    }

    // ─── Keyboard accelerators ─────────────────────────────────────────────

    private void OnNewEntryAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.NewEntryCommand.Execute(null);
        args.Handled = true;
    }

    private void OnToggleSidebarAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.ToggleSidebarCommand.Execute(null);
        args.Handled = true;
    }

    private void OnToggleTimerAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.ToggleTimerCommand.Execute(null);
        args.Handled = true;
    }

    private void OnEscapeAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (ViewModel.IsSidebarOpen)
        {
            ViewModel.IsSidebarOpen = false;
            args.Handled = true;
        }
    }

    private void ShowToast(string text)
    {
        ToastText.Text = text;
        if (_animationsEnabled)
        {
            var inAnim = new DoubleAnimation
            {
                From = 0,
                To = 0.92,
                Duration = TimeSpan.FromMilliseconds(160),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut },
            };
            Storyboard.SetTarget(inAnim, ToastBorder);
            Storyboard.SetTargetProperty(inAnim, "Opacity");
            var sb = new Storyboard();
            sb.Children.Add(inAnim);
            sb.Begin();
        }
        else
        {
            ToastBorder.Opacity = 0.92;
        }

        _toastTimer?.Stop();
        if (_toastTimer is null)
        {
            _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ToastVisibleMs) };
            _toastTimer.Tick += OnToastDismissTick;
        }
        _toastTimer.Start();
    }

    private void OnToastDismissTick(object? sender, object e)
    {
        _toastTimer?.Stop();
        if (!_animationsEnabled)
        {
            ToastBorder.Opacity = 0;
            return;
        }
        var outAnim = new DoubleAnimation
        {
            From = 0.92,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(240),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
        };
        Storyboard.SetTarget(outAnim, ToastBorder);
        Storyboard.SetTargetProperty(outAnim, "Opacity");
        var sb = new Storyboard();
        sb.Children.Add(outAnim);
        sb.Begin();
    }
}
