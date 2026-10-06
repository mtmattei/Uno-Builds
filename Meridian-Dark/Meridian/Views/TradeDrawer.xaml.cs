using System.Diagnostics;
using Meridian.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace Meridian.Views;

public sealed partial class TradeDrawer : UserControl
{
    public event EventHandler? CloseRequested;

    private Stock? _stock;
    private bool _isBuy = true;
    private string _orderType = "market";
    private DispatcherTimer? _autoCloseTimer;

    private static readonly SolidColorBrush GainBrush = new(Windows.UI.Color.FromArgb(0xFF, 0x5C, 0xA8, 0x7E));
    private static readonly SolidColorBrush LossBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xDA, 0x61, 0x54));
    private static readonly SolidColorBrush AccentBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xD2, 0xB2, 0x78));
    private static readonly SolidColorBrush AccentTintBrush = new(Windows.UI.Color.FromArgb(0x24, 0xD2, 0xB2, 0x78));
    private static readonly SolidColorBrush TransparentBrush = new(Microsoft.UI.Colors.Transparent);
    // Selected segment lifts off the drawer ground instead of dropping to white.
    private static readonly SolidColorBrush SelectedSegmentBrush = new(Windows.UI.Color.FromArgb(0xFF, 0x2A, 0x25, 0x1D));
    private static readonly SolidColorBrush GrayBrush = new(Windows.UI.Color.FromArgb(0xFF, 0x9C, 0x91, 0x7C));
    private static readonly SolidColorBrush InkBrush = new(Windows.UI.Color.FromArgb(0xFF, 0x15, 0x13, 0x0F));
    private static readonly SolidColorBrush SubmitDisabledBrush = new(Windows.UI.Color.FromArgb(0xFF, 0x38, 0x32, 0x28));
    private static readonly SolidColorBrush BorderBrush_ = new(Windows.UI.Color.FromArgb(0xFF, 0x38, 0x32, 0x28));

    public TradeDrawer()
    {
        this.InitializeComponent();

        CloseButton.Click += (_, _) =>
        {
            StopAndDisposeTimer();
            CloseRequested?.Invoke(this, EventArgs.Empty);
        };
        BuyButton.Click += (_, _) => SetSide(true);
        SellButton.Click += (_, _) => SetSide(false);
        MarketButton.Click += (_, _) => SetOrderType("market");
        LimitButton.Click += (_, _) => SetOrderType("limit");
        StopButton.Click += (_, _) => SetOrderType("stop");
        QuantityInput.TextChanged += OnQuantityTextChanged;
        QuantityInput.BeforeTextChanging += OnQuantityBeforeTextChanging;
        SubmitButton.Click += (_, _) => Submit();
        Unloaded += OnUnloaded;

        // Quick select buttons
        foreach (var child in QuickSelectPanel.Children)
        {
            if (child is Button b && b.Tag is string tag && int.TryParse(tag, out _))
            {
                b.Click += (s, _) =>
                {
                    if (s is Button qb && qb.Tag is string t)
                        QuantityInput.Text = t;
                };
            }
        }

        SetSide(true);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopAndDisposeTimer();
    }

    private void StopAndDisposeTimer()
    {
        if (_autoCloseTimer is not null)
        {
            _autoCloseTimer.Stop();
            _autoCloseTimer = null;
        }
    }

    private void OnQuantityBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        // Reject any text that is not a valid non-negative integer within range
        if (string.IsNullOrEmpty(args.NewText))
            return;

        if (!int.TryParse(args.NewText, out var value) || value < 0 || value > 99999)
        {
            args.Cancel = true;
        }
    }

    private void OnQuantityTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdatePreview();
    }

    public void FocusQuantityInput()
    {
        QuantityInput.Focus(FocusState.Programmatic);
    }

    public void SetStock(Stock stock, bool isSell = false, int prefillQty = 0)
    {
        _stock = stock;
        StockTicker.Text = stock.Ticker;
        StockName.Text = stock.Name;
        StockPrice.Text = $"${stock.Price:N2}";

        var isUp = stock.Pct >= 0;
        StockDelta.Text = $"{(isUp ? "+" : "")}{stock.Pct:N2}%";
        StockDelta.Foreground = isUp ? GainBrush : LossBrush;
        DeltaBadge.Background = isUp
            ? new SolidColorBrush(Windows.UI.Color.FromArgb(0x24, 0x5C, 0xA8, 0x7E))
            : new SolidColorBrush(Windows.UI.Color.FromArgb(0x24, 0xDA, 0x61, 0x54));

        // Reset form state
        _isBuy = !isSell;
        _orderType = "market";
        QuantityInput.Text = prefillQty > 0 ? prefillQty.ToString() : "";
        SetSide(!isSell);
        SetOrderType("market");
        FormPanel.Visibility = Visibility.Visible;
        ConfirmationPanel.Visibility = Visibility.Collapsed;
        StopAndDisposeTimer();
        UpdatePreview();
    }

    private void SetSide(bool isBuy)
    {
        _isBuy = isBuy;
        BuyButton.Background = isBuy ? SelectedSegmentBrush : TransparentBrush;
        BuyButton.Foreground = isBuy ? GainBrush : GrayBrush;
        SellButton.Background = !isBuy ? SelectedSegmentBrush : TransparentBrush;
        SellButton.Foreground = !isBuy ? LossBrush : GrayBrush;
        UpdatePreview();
    }

    private void SetOrderType(string type)
    {
        _orderType = type;

        var inactiveForeground = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x9C, 0x91, 0x7C));

        MarketButton.BorderBrush = type == "market" ? AccentBrush : BorderBrush_;
        MarketButton.Background = type == "market" ? AccentTintBrush : TransparentBrush;
        MarketButton.Foreground = type == "market" ? AccentBrush : inactiveForeground;

        LimitButton.BorderBrush = type == "limit" ? AccentBrush : BorderBrush_;
        LimitButton.Background = type == "limit" ? AccentTintBrush : TransparentBrush;
        LimitButton.Foreground = type == "limit" ? AccentBrush : inactiveForeground;

        StopButton.BorderBrush = type == "stop" ? AccentBrush : BorderBrush_;
        StopButton.Background = type == "stop" ? AccentTintBrush : TransparentBrush;
        StopButton.Foreground = type == "stop" ? AccentBrush : inactiveForeground;

        // Limit price panel with fadeUp animation
        if (type != "market" && LimitPricePanel.Visibility != Visibility.Visible)
        {
            LimitPricePanel.Visibility = Visibility.Visible;
            AnimateLimitPriceFadeUp();
        }
        else if (type == "market")
        {
            LimitPricePanel.Visibility = Visibility.Collapsed;
        }

        // Reset limit price when switching to Market (per interaction spec edge case)
        if (type == "market")
            LimitPriceInput.Text = "";

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (_stock == null) return;

        var qty = int.TryParse(QuantityInput.Text, out var q) ? q : 0;
        var side = _isBuy ? "Buy" : "Sell";
        var total = qty * _stock.Price;

        PreviewAction.Text = $"{side} {_stock.Ticker}";
        PreviewQuantity.Text = $"{qty} shares";
        PreviewPrice.Text = $"@ {(_orderType == "market" ? "Market" : $"${_stock.Price:N2}")}";
        PreviewTotal.Text = $"${total:N2}";

        if (qty > 0)
        {
            SubmitButton.IsEnabled = true;
            SubmitButton.Content = $"{side} {qty} {_stock.Ticker} · ${total:N2}";
            SubmitButton.Background = _isBuy ? GainBrush : LossBrush;
            // The dark palette lifts gain/loss to light tones, so the label has
            // to be the ink to stay legible (6.5:1 / 5.1:1 — white would be 2.9:1).
            SubmitButton.Foreground = InkBrush;
        }
        else
        {
            SubmitButton.IsEnabled = false;
            SubmitButton.Content = "Enter quantity";
            SubmitButton.Background = SubmitDisabledBrush;
            SubmitButton.Foreground = GrayBrush;
        }
    }

    private void Submit()
    {
        try
        {
            if (_stock == null) return;

            var qty = int.TryParse(QuantityInput.Text, out var q) ? q : 0;
            if (qty <= 0) return;

            var side = _isBuy ? "Buy" : "Sell";
            ConfirmationSummary.Text = $"{side} {qty} {_stock.Ticker} @ {(_orderType == "market" ? "Market" : "Limit")}";

            FormPanel.Visibility = Visibility.Collapsed;
            ConfirmationPanel.Visibility = Visibility.Visible;

            // Dispose any existing timer before creating a new one
            StopAndDisposeTimer();

            // Auto-close after 1.8s
            _autoCloseTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.8) };
            _autoCloseTimer.Tick += (_, _) =>
            {
                StopAndDisposeTimer();
                CloseRequested?.Invoke(this, EventArgs.Empty);
            };
            _autoCloseTimer.Start();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"TradeDrawer.Submit failed: {ex}");
        }
    }

    // ── Micro-interactions ───────────────────────────────────────────

    private void OnSubmitPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button btn || btn.RenderTransform is not CompositeTransform) return;
        var anim = new DoubleAnimation { To = 1.02, Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        var animY = new DoubleAnimation { To = 1.02, Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        Storyboard.SetTarget(anim, btn);
        Storyboard.SetTargetProperty(anim, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
        Storyboard.SetTarget(animY, btn);
        Storyboard.SetTargetProperty(animY, "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");
        var sb = new Storyboard(); sb.Children.Add(anim); sb.Children.Add(animY); sb.Begin();
    }

    private void OnSubmitPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button btn || btn.RenderTransform is not CompositeTransform) return;
        var anim = new DoubleAnimation { To = 1.0, Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        var animY = new DoubleAnimation { To = 1.0, Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        Storyboard.SetTarget(anim, btn);
        Storyboard.SetTargetProperty(anim, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
        Storyboard.SetTarget(animY, btn);
        Storyboard.SetTargetProperty(animY, "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");
        var sb = new Storyboard(); sb.Children.Add(anim); sb.Children.Add(animY); sb.Begin();
    }

    private void OnClosePointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button btn || btn.RenderTransform is not RotateTransform) return;
        var anim = new DoubleAnimation { To = 90, Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        Storyboard.SetTarget(anim, btn);
        Storyboard.SetTargetProperty(anim, "(UIElement.RenderTransform).(RotateTransform.Angle)");
        var sb = new Storyboard(); sb.Children.Add(anim); sb.Begin();
    }

    private void OnClosePointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button btn || btn.RenderTransform is not RotateTransform) return;
        var anim = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        Storyboard.SetTarget(anim, btn);
        Storyboard.SetTargetProperty(anim, "(UIElement.RenderTransform).(RotateTransform.Angle)");
        var sb = new Storyboard(); sb.Children.Add(anim); sb.Begin();
    }

    private void AnimateLimitPriceFadeUp()
    {
        var fadeIn = new DoubleAnimation
        {
            From = 0, To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(fadeIn, LimitPricePanel);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");

        var slideUp = new DoubleAnimation
        {
            From = 12, To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(slideUp, LimitPricePanel);
        Storyboard.SetTargetProperty(slideUp, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");

        var sb = new Storyboard();
        sb.Children.Add(fadeIn);
        sb.Children.Add(slideUp);
        sb.Begin();
    }
}
