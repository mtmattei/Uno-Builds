using Composer.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Composer.Views.Controls;

/// <summary>
/// Eyebrow + italic-Fraunces title + serif subtitle + per-state badge, with
/// an optional ↳ recap row above. Six discrete DPs per
/// <c>docs/ARCHITECTURE-BRIEF-from-scratch.md</c> §12.1.
///
/// LayerIndex / LayerLabel / LayerState / Recap / Title / Subtitle replace
/// the previous single-Header DP that consumed a LayerHeaderModel record.
/// Each is typed object so MVUX bindable wrappers can be unwrapped via
/// MvuxValueReader the same way ActiveCanvas handles its header value.
/// </summary>
public sealed partial class ActiveLayerHeader : UserControl
{
    public ActiveLayerHeader() => this.InitializeComponent();

    // ─── LayerIndex ─────────────────────────────────────────────────────
    public static readonly DependencyProperty LayerIndexProperty =
        DependencyProperty.Register(nameof(LayerIndex), typeof(object), typeof(ActiveLayerHeader),
            new PropertyMetadata(null, OnAnyChanged));
    public object? LayerIndex
    {
        get => GetValue(LayerIndexProperty);
        set => SetValue(LayerIndexProperty, value);
    }

    // ─── LayerLabel ─────────────────────────────────────────────────────
    public static readonly DependencyProperty LayerLabelProperty =
        DependencyProperty.Register(nameof(LayerLabel), typeof(object), typeof(ActiveLayerHeader),
            new PropertyMetadata(null, OnAnyChanged));
    public object? LayerLabel
    {
        get => GetValue(LayerLabelProperty);
        set => SetValue(LayerLabelProperty, value);
    }

    // ─── LayerState ─────────────────────────────────────────────────────
    public static readonly DependencyProperty LayerStateProperty =
        DependencyProperty.Register(nameof(LayerState), typeof(object), typeof(ActiveLayerHeader),
            new PropertyMetadata(null, OnAnyChanged));
    public object? LayerState
    {
        get => GetValue(LayerStateProperty);
        set => SetValue(LayerStateProperty, value);
    }

    // ─── Recap ──────────────────────────────────────────────────────────
    public static readonly DependencyProperty RecapProperty =
        DependencyProperty.Register(nameof(Recap), typeof(object), typeof(ActiveLayerHeader),
            new PropertyMetadata(null, OnAnyChanged));
    public object? Recap
    {
        get => GetValue(RecapProperty);
        set => SetValue(RecapProperty, value);
    }

    // ─── Title ──────────────────────────────────────────────────────────
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(object), typeof(ActiveLayerHeader),
            new PropertyMetadata(null, OnAnyChanged));
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // ─── Subtitle ───────────────────────────────────────────────────────
    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(object), typeof(ActiveLayerHeader),
            new PropertyMetadata(null, OnAnyChanged));
    public object? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    private static void OnAnyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ActiveLayerHeader h) h.Apply();
    }

    private void Apply()
    {
        // LayerLabel — string (already projected from "01 · STACK" format).
        LayerLabelText.Text = AsString(LayerLabel) ?? string.Empty;

        // Title + Subtitle — strings.
        TitleText.Text    = AsString(Title)    ?? string.Empty;
        SubtitleText.Text = AsString(Subtitle) ?? string.Empty;

        // Recap — nullable string. Row collapses when null/empty (Stack layer).
        var recap = AsString(Recap);
        if (string.IsNullOrWhiteSpace(recap))
        {
            RecapRow.Visibility = Visibility.Collapsed;
        }
        else
        {
            RecapText.Text       = recap;
            RecapRow.Visibility  = Visibility.Visible;
        }

        // LayerState — enum value; map to the per-state badge text. Badge
        // visibility flips on/off so the eyebrow row collapses when the
        // layer is Clean / Locked.
        var state = AsLayerState(LayerState);
        var badge = state switch
        {
            Composer.Models.LayerState.Dirty       => "EDITED — PREVIEW PENDING",
            Composer.Models.LayerState.Previewing  => "PREVIEW — REVIEW AND ACCEPT",
            _                                       => string.Empty,
        };
        StateBadgeText.Text = badge;
        StateBadgeText.Visibility = string.IsNullOrEmpty(badge)
            ? Microsoft.UI.Xaml.Visibility.Collapsed
            : Microsoft.UI.Xaml.Visibility.Visible;
    }

    private static string? AsString(object? value)
    {
        if (value is null) return null;
        if (value is string s) return s;
        return Composer.Views.MvuxValueReader.Unwrap<string>(value);
    }

    private static Composer.Models.LayerState AsLayerState(object? value)
    {
        if (value is Composer.Models.LayerState direct) return direct;
        if (value is null) return Composer.Models.LayerState.Clean;
        // MVUX wrapper may box the enum; reflect on it.
        var t = value.GetType();
        foreach (var name in new[] { "Value", "Current", "Model", "_value" })
        {
            var prop = t.GetProperty(name);
            if (prop?.GetValue(value) is Composer.Models.LayerState s) return s;
            var field = t.GetField(name, System.Reflection.BindingFlags.Instance
                                       | System.Reflection.BindingFlags.Public
                                       | System.Reflection.BindingFlags.NonPublic);
            if (field?.GetValue(value) is Composer.Models.LayerState f) return f;
        }
        return Composer.Models.LayerState.Clean;
    }
}
