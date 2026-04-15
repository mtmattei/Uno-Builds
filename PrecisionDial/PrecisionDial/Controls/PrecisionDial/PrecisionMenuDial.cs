using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI;

namespace PrecisionDial.Controls;

/// <summary>
/// Rotary menu selector built on PrecisionDial. Each detent maps to a labelled
/// menu item with an optional icon. Selection changes animate with spring physics.
/// </summary>
public sealed partial class PrecisionMenuDial : Panel
{
    // ── Visual tree ───────────────────────────────────────────────────────────

    private readonly PrecisionDial _dial;
    private readonly StackPanel _itemsPanel;
    private readonly List<ItemView> _items = new();
    private const double LabelsGap = 28.0;

    // ── Animation ─────────────────────────────────────────────────────────────

    private DispatcherTimer? _animTimer;
    private const double OpacitySpeed = 11.0;   // ~250ms
    private const double TYSpeed = 12.0;
    private const double ExitTYSpeed = 14.0;
    private const double DotStiffness = 900.0;
    private const double DotDamping = 16.0;
    private const double IconStiffness = 700.0; // slightly softer than dot
    private const double IconDamping = 18.0;

    // ── Colours ───────────────────────────────────────────────────────────────

    private static readonly Color Amber = Color.FromArgb(255, 212, 169, 89);
    private static readonly Color Dim = Color.FromArgb(50, 255, 255, 255);

    // ── Per-item state ────────────────────────────────────────────────────────

    private sealed class ItemView
    {
        public StackPanel Row = null!;
        public TranslateTransform RowTranslate = null!;
        public Border Dot = null!;
        public ScaleTransform DotScaleTransform = null!;
        public TextBlock Icon = null!;
        public ScaleTransform IconScaleTransform = null!;
        public TextBlock Label = null!;

        // Animated values
        public double Opacity = 0.0;
        public double TranslateY = 12.0;

        // Dot spring
        public double DotScalePos = 0.0;
        public double DotScaleVel = 0.0;

        // Icon spring
        public double IconScalePos = 1.0;
        public double IconScaleVel = 0.0;

        // Targets
        public double TargetOpacity = 0.3;
        public double TargetTY = 0.0;
        public double TargetDotScale = 0.0;
        public double TargetIconScale = 1.0;

        // Staggered entrance
        public long DelayEndTick;
        public bool EntranceStarted = false;
    }

    // ── Public event ─────────────────────────────────────────────────────────

    public event EventHandler<int>? SelectionChanged;

    // ── Construction ─────────────────────────────────────────────────────────

    public PrecisionMenuDial()
    {
        _dial = new PrecisionDial
        {
            IsHapticEnabled = true,
            ArcSweepDegrees = 270,
            Minimum = 0, Maximum = 1, DetentCount = 1,
            AccentBrush = new SolidColorBrush(Amber),
        };
        _dial.ValueChanged += OnDialValueChanged;

        _itemsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 10,
        };

        Children.Add(_dial);
        Children.Add(_itemsPanel);
    }

    // ── Layout ────────────────────────────────────────────────────────────────

    protected override Size MeasureOverride(Size availableSize)
    {
        var s = DialSize;
        _dial.Width = s;
        _dial.Height = s;
        _dial.Measure(new Size(s, s));
        var labW = Math.Max(0, availableSize.Width - s - LabelsGap);
        _itemsPanel.Measure(new Size(labW, availableSize.Height));
        return new Size(
            s + LabelsGap + _itemsPanel.DesiredSize.Width,
            Math.Max(s, _itemsPanel.DesiredSize.Height));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var s = DialSize;
        _dial.Arrange(new Rect(0, Math.Max(0, (finalSize.Height - s) / 2), s, s));
        var labH = _itemsPanel.DesiredSize.Height;
        var labW = Math.Max(0, finalSize.Width - s - LabelsGap);
        _itemsPanel.Arrange(new Rect(s + LabelsGap, Math.Max(0, (finalSize.Height - labH) / 2), labW, labH));
        return finalSize;
    }

    // ── DP callbacks ─────────────────────────────────────────────────────────

    private void OnDialSizeChanged() { _dial.Width = DialSize; _dial.Height = DialSize; InvalidateMeasure(); }
    private void OnAccentBrushChanged() { if (AccentBrush is not null) _dial.AccentBrush = AccentBrush; }

    // ── Item construction ─────────────────────────────────────────────────────

    private void RebuildItems()
    {
        StopAnimTimer();
        _itemsPanel.Children.Clear();
        _items.Clear();

        var labels = MenuItems;
        if (labels is null || labels.Count == 0) { _dial.Maximum = 1; _dial.DetentCount = 1; return; }

        var icons = MenuIcons;
        int count = labels.Count;
        _dial.Minimum = 0;
        _dial.Maximum = count - 1;
        _dial.DetentCount = count - 1;
        _dial.Value = 0;

        long now = Stopwatch.GetTimestamp();
        double tpms = Stopwatch.Frequency / 1000.0;

        for (int i = 0; i < count; i++)
        {
            // Dot (selection indicator)
            var dotScaleTx = new ScaleTransform { ScaleX = 0, ScaleY = 0 };
            var dot = new Border
            {
                Width = 6, Height = 6, CornerRadius = new CornerRadius(3),
                Background = new SolidColorBrush(Amber),
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = 0,
                RenderTransform = dotScaleTx,
                RenderTransformOrigin = new Point(0.5, 0.5),
            };

            // Icon
            var iconScaleTx = new ScaleTransform { ScaleX = 1, ScaleY = 1 };
            string iconGlyph = (icons is not null && i < icons.Count) ? icons[i] : string.Empty;
            var icon = new TextBlock
            {
                Text = iconGlyph,
                FontSize = 14,
                Width = 20,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Dim),
                Opacity = 0,
                RenderTransform = iconScaleTx,
                RenderTransformOrigin = new Point(0.5, 0.5),
            };

            // Label
            var label = new TextBlock
            {
                Text = labels[i],
                FontSize = 13,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Dim),
                CharacterSpacing = 150,
                Opacity = 0,
            };

            // Row
            var rowTranslate = new TranslateTransform { Y = 12 };
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                VerticalAlignment = VerticalAlignment.Center,
                RenderTransform = rowTranslate,
            };
            row.Children.Add(dot);
            if (iconGlyph.Length > 0) row.Children.Add(icon);
            row.Children.Add(label);
            _itemsPanel.Children.Add(row);

            _items.Add(new ItemView
            {
                Row = row, RowTranslate = rowTranslate,
                Dot = dot, DotScaleTransform = dotScaleTx,
                Icon = icon, IconScaleTransform = iconScaleTx,
                Label = label,
                Opacity = 0, TranslateY = 12,
                DotScalePos = 0, DotScaleVel = 0,
                IconScalePos = 1, IconScaleVel = 0,
                // First item is active at load, rest are dim
                TargetOpacity = i == 0 ? 1.0 : 0.3,
                TargetTY = 0,
                TargetDotScale = i == 0 ? 1.0 : 0.0,
                TargetIconScale = i == 0 ? 1.15 : 1.0,
                DelayEndTick = now + (long)((40 + i * 60) * tpms),
                EntranceStarted = false,
            });
        }

        SelectedIndex = 0;
        EnsureAnimTimer();
    }

    // ── Dial interaction ──────────────────────────────────────────────────────

    private void OnDialValueChanged(object? sender, DialValueChangedEventArgs e)
    {
        int count = _items.Count;
        if (count == 0) return;
        int newIdx = (int)Math.Round(Math.Clamp(e.NewValue, 0, count - 1));
        int oldIdx = SelectedIndex;
        if (newIdx == oldIdx) return;

        SelectedIndex = newIdx;

        for (int i = 0; i < count; i++)
        {
            var iv = _items[i];
            if (i == newIdx)
            {
                // Enter from above
                iv.TranslateY = -8;
                iv.RowTranslate.Y = -8;
                iv.TargetTY = 0;
                iv.TargetOpacity = 1.0;
                iv.TargetDotScale = 1.0;
                iv.TargetIconScale = 1.15;
                iv.DotScaleVel = 2.5;   // spring kick
                iv.IconScaleVel = 1.5;
            }
            else if (i == oldIdx)
            {
                iv.TargetTY = 4;
                iv.TargetOpacity = 0.3;
                iv.TargetDotScale = 0.0;
                iv.TargetIconScale = 1.0;
            }
            else
            {
                iv.TargetOpacity = 0.3;
                iv.TargetTY = 0;
                iv.TargetDotScale = 0.0;
                iv.TargetIconScale = 1.0;
            }
        }

        SelectionChanged?.Invoke(this, newIdx);
        EnsureAnimTimer();
    }

    // ── Animation loop ────────────────────────────────────────────────────────

    private void EnsureAnimTimer()
    {
        if (_animTimer is null)
        {
            _animTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _animTimer.Tick += OnAnimTick;
        }
        if (!_animTimer.IsEnabled) _animTimer.Start();
    }

    private void StopAnimTimer() => _animTimer?.Stop();

    private void OnAnimTick(object? sender, object e)
    {
        const double dt = 0.016;
        bool anyActive = false;
        long now = Stopwatch.GetTimestamp();

        for (int i = 0; i < _items.Count; i++)
        {
            var iv = _items[i];

            if (!iv.EntranceStarted)
            {
                if (now < iv.DelayEndTick) { anyActive = true; continue; }
                iv.EntranceStarted = true;
            }

            bool settled = true;

            // Opacity (row)
            double opDelta = iv.TargetOpacity - iv.Opacity;
            if (Math.Abs(opDelta) > 0.002)
            { iv.Opacity += opDelta * (1 - Math.Exp(-OpacitySpeed * dt)); settled = false; }
            else iv.Opacity = iv.TargetOpacity;

            // TranslateY
            double tyDelta = iv.TargetTY - iv.TranslateY;
            double tySpeed = iv.TargetTY > 0 ? ExitTYSpeed : TYSpeed;
            if (Math.Abs(tyDelta) > 0.05)
            { iv.TranslateY += tyDelta * (1 - Math.Exp(-tySpeed * dt)); settled = false; }
            else iv.TranslateY = iv.TargetTY;

            // Dot spring
            double dotErr = iv.TargetDotScale - iv.DotScalePos;
            iv.DotScaleVel += (DotStiffness * dotErr - DotDamping * iv.DotScaleVel) * dt;
            iv.DotScalePos += iv.DotScaleVel * dt;
            if (Math.Abs(dotErr) > 0.003 || Math.Abs(iv.DotScaleVel) > 0.01) settled = false;
            else { iv.DotScalePos = iv.TargetDotScale; iv.DotScaleVel = 0; }

            // Icon spring
            double iconErr = iv.TargetIconScale - iv.IconScalePos;
            iv.IconScaleVel += (IconStiffness * iconErr - IconDamping * iv.IconScaleVel) * dt;
            iv.IconScalePos += iv.IconScaleVel * dt;
            if (Math.Abs(iconErr) > 0.003 || Math.Abs(iv.IconScaleVel) > 0.01) settled = false;
            else { iv.IconScalePos = iv.TargetIconScale; iv.IconScaleVel = 0; }

            // Apply to visual tree
            iv.Row.Opacity = iv.Opacity;
            iv.RowTranslate.Y = iv.TranslateY;

            double dotS = Math.Clamp(iv.DotScalePos, 0, 1.4);
            iv.DotScaleTransform.ScaleX = dotS;
            iv.DotScaleTransform.ScaleY = dotS;
            iv.Dot.Opacity = Math.Clamp(iv.DotScalePos, 0, 1);

            double iconS = Math.Clamp(iv.IconScalePos, 0.8, 1.3);
            iv.IconScaleTransform.ScaleX = iconS;
            iv.IconScaleTransform.ScaleY = iconS;

            // Colour — lerp from dim to amber based on active proximity
            double t = Math.Clamp(iv.DotScalePos, 0, 1);
            var colour = LerpBrush(Dim, Amber, t);
            iv.Label.Foreground = colour;
            iv.Icon.Foreground = colour;
            iv.Icon.Opacity = 0.45 + t * 0.55; // dim → fully opaque

            if (!settled) anyActive = true;
        }

        if (!anyActive) StopAnimTimer();
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    private static SolidColorBrush LerpBrush(Color from, Color to, double t)
    {
        t = Math.Clamp(t, 0, 1);
        return new SolidColorBrush(Color.FromArgb(
            (byte)(from.A + (to.A - from.A) * t),
            (byte)(from.R + (to.R - from.R) * t),
            (byte)(from.G + (to.G - from.G) * t),
            (byte)(from.B + (to.B - from.B) * t)));
    }
}
