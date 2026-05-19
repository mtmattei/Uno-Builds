using System;
using System.Collections.Generic;
using System.ComponentModel;
using Composer.Models;
using Composer.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Composer.Views.Layers;

/// <summary>
/// Layer 1 — Intent. Tabular grid of four named fields (App type / Primary
/// user / Workflow / Platforms) plus an optional Notes textarea, per
/// <c>docs/DESIGN-BRIEF.md</c> §6.
///
/// Each field is an inline TextBox; any edit calls <c>SetIntent</c> through
/// the MVUX command surface, which marks the layer dirty so the composer
/// footer flips from "Lock and continue" to "Generate preview". In
/// previewing state the inputs become read-only and changed rows get the
/// amber-tint backdrop + "Proposed" annotation.
/// </summary>
public sealed partial class IntentCard : UserControl
{
    private INotifyPropertyChanged? _vm;
    private bool _suppress;

    private TextBox? _appTypeInput;
    private TextBox? _primaryUserInput;
    private TextBox? _workflowInput;
    private TextBox? _platformsInput;
    private readonly Dictionary<string, Border> _rowBackdrops = new();
    private readonly Dictionary<string, TextBlock> _rowProposedBadges = new();

    public IntentCard()
    {
        this.InitializeComponent();
        this.DataContextChanged += (_, args) => Attach(args.NewValue);
        this.Unloaded += (_, _) => Detach();
        BuildFactRows();
    }

    private void Attach(object? dc)
    {
        Detach();
        if (dc is INotifyPropertyChanged inpc)
        {
            _vm = inpc;
            _vm.PropertyChanged += OnVmPropertyChanged;
        }
        Render();
    }

    private void Detach()
    {
        if (_vm is null) return;
        _vm.PropertyChanged -= OnVmPropertyChanged;
        _vm = null;
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is "Intent" or "LayerStates")
            DispatcherQueue?.TryEnqueue(Render);
    }

    private IntentValues? ReadIntent() =>
        Composer.Views.MvuxValueReader.Read<IntentValues>(DataContext, "Intent");

    private bool IsPreviewing()
    {
        var dc = DataContext;
        if (dc is null) return false;
        var statesProp = Composer.Views.MvuxValueReader.ReadRaw(dc, "LayerStates");
        if (statesProp is null) return false;
        var dict = Composer.Views.MvuxValueReader.Unwrap<System.Collections.Immutable.IImmutableDictionary<LayerKind, LayerState>>(statesProp);
        if (dict is null && statesProp is System.Collections.Immutable.IImmutableDictionary<LayerKind, LayerState> direct)
            dict = direct;
        return dict is not null
            && dict.TryGetValue(LayerKind.Intent, out var s)
            && s == LayerState.Previewing;
    }

    private IntentValues? ReadProposedIntent()
    {
        var raw = Composer.Views.MvuxValueReader.InvokeMethod(DataContext, "GetPreviewValues", LayerKind.Intent);
        return raw as IntentValues
            ?? Composer.Views.MvuxValueReader.Unwrap<IntentValues>(raw);
    }

    /// <summary>Lay out four (label / value / proposed-badge) rows once;
    /// <see cref="Render"/> only updates contents.</summary>
    private void BuildFactRows()
    {
        var rows = new[]
        {
            ("APP TYPE",     "AppType"),
            ("PRIMARY USER", "PrimaryUser"),
            ("WORKFLOW",     "Workflow"),
            ("PLATFORMS",    "Platforms"),
        };

        for (var i = 0; i < rows.Length; i++)
        {
            var (caption, field) = rows[i];

            // Row backdrop spans all 3 columns; toggled to amberSoft when
            // previewing + value differs. Hairline divider sits at the bottom.
            var backdrop = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                Margin = new Thickness(0),
            };
            Grid.SetRow(backdrop, i);
            Grid.SetColumn(backdrop, 0);
            Grid.SetColumnSpan(backdrop, 3);
            FactsGrid.Children.Add(backdrop);
            _rowBackdrops[field] = backdrop;

            // Eyebrow label (col 0)
            var label = new TextBlock
            {
                Text = caption,
                FontFamily = (FontFamily)Application.Current.Resources["MonoFontFamily"],
                FontSize = 11,
                FontWeight = Microsoft.UI.Text.FontWeights.Medium,
                CharacterSpacing = 160,
                Foreground = AppBrushes.Ink3,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 14, 0, 14),
            };
            Grid.SetRow(label, i);
            Grid.SetColumn(label, 0);
            FactsGrid.Children.Add(label);

            // Editable value (col 1) — borderless TextBox, full-width.
            var input = new TextBox
            {
                Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0, 12, 0, 12),
                FontFamily = (FontFamily)Application.Current.Resources["SansFontFamily"],
                FontSize = 14,
                Foreground = AppBrushes.Ink,
                Margin = new Thickness(0),
            };
            input.Tag = field;
            input.TextChanged += OnFieldChanged;
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(input, $"Intent field {caption}");
            Grid.SetRow(input, i);
            Grid.SetColumn(input, 1);
            FactsGrid.Children.Add(input);

            switch (field)
            {
                case "AppType":     _appTypeInput     = input; break;
                case "PrimaryUser": _primaryUserInput = input; break;
                case "Workflow":    _workflowInput    = input; break;
                case "Platforms":   _platformsInput   = input; break;
            }

            // Proposed annotation column (col 2) — only filled in previewing.
            var proposedBadge = new TextBlock
            {
                Text = string.Empty,
                FontFamily = (FontFamily)Application.Current.Resources["MonoFontFamily"],
                FontSize = 9,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                CharacterSpacing = 180,
                Foreground = AppBrushes.Amber,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 14, 8, 14),
            };
            Grid.SetRow(proposedBadge, i);
            Grid.SetColumn(proposedBadge, 2);
            FactsGrid.Children.Add(proposedBadge);
            _rowProposedBadges[field] = proposedBadge;

            // Hairline divider at the bottom of each row except the last.
            if (i < rows.Length - 1)
            {
                var divider = new Border
                {
                    Height = 1,
                    Background = AppBrushes.Hairline,
                    VerticalAlignment = VerticalAlignment.Bottom,
                };
                Grid.SetRow(divider, i);
                Grid.SetColumn(divider, 0);
                Grid.SetColumnSpan(divider, 3);
                FactsGrid.Children.Add(divider);
            }
        }
    }

    private void Render()
    {
        var current = ReadIntent() ?? IntentValues.Default;
        var previewing = IsPreviewing();
        var proposed = previewing ? ReadProposedIntent() : null;
        var displayed = proposed ?? current;

        _suppress = true;
        if (_appTypeInput     is { } a) { a.Text = displayed.AppType;     a.IsReadOnly = previewing; }
        if (_primaryUserInput is { } p) { p.Text = displayed.PrimaryUser; p.IsReadOnly = previewing; }
        if (_workflowInput    is { } w) { w.Text = displayed.Workflow;    w.IsReadOnly = previewing; }
        if (_platformsInput   is { } pl){ pl.Text = displayed.Platforms;  pl.IsReadOnly = previewing; }
        NotesInput.Text       = displayed.Notes;
        NotesInput.IsReadOnly = previewing;
        _suppress = false;

        // Diff visualization — amber-tint backdrop + "PROPOSED" badge on
        // any row whose proposed value differs from the current value.
        // Resolves AmberSoftBrush from the active theme dictionary so it
        // adapts to dark theme automatically.
        var amberSoft   = (Brush)Application.Current.Resources["AmberSoftBrush"];
        var transparent = new SolidColorBrush(Color.FromArgb(0x00, 0, 0, 0));

        UpdateRowDiff("AppType",     previewing, current.AppType,     displayed.AppType,     amberSoft, transparent);
        UpdateRowDiff("PrimaryUser", previewing, current.PrimaryUser, displayed.PrimaryUser, amberSoft, transparent);
        UpdateRowDiff("Workflow",    previewing, current.Workflow,    displayed.Workflow,    amberSoft, transparent);
        UpdateRowDiff("Platforms",   previewing, current.Platforms,   displayed.Platforms,   amberSoft, transparent);

        // Example-values banner — shown when any field still matches the
        // field-service archetype defaults. Suppressed in previewing state
        // since the AI may have moved values away from the defaults already.
        var anyExample = !previewing && (
            current.AppType     == IntentValues.Default.AppType
            || current.PrimaryUser == IntentValues.Default.PrimaryUser
            || current.Workflow    == IntentValues.Default.Workflow
            || current.Platforms   == IntentValues.Default.Platforms);
        ExampleBanner.Visibility = anyExample ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnClearAllClick(object sender, RoutedEventArgs e)
    {
        // Wipe every field. SetIntent marks dirty, which the composer footer
        // picks up so Generate-preview becomes available immediately.
        var cleared = new IntentValues(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        InvokeSetIntent(cleared);
    }

    private void UpdateRowDiff(string field, bool previewing, string oldValue, string newValue, Brush tint, Brush clear)
    {
        var changed = previewing && !string.Equals(oldValue, newValue, StringComparison.Ordinal);
        if (_rowBackdrops.TryGetValue(field, out var bg))
            bg.Background = changed ? tint : clear;
        if (_rowProposedBadges.TryGetValue(field, out var badge))
            badge.Text = changed ? "PROPOSED" : string.Empty;
    }

    private void OnFieldChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppress) return;
        if (sender is not TextBox tb || tb.Tag is not string field) return;

        var current = ReadIntent() ?? IntentValues.Default;
        var updated = field switch
        {
            "AppType"     => current with { AppType = tb.Text },
            "PrimaryUser" => current with { PrimaryUser = tb.Text },
            "Workflow"    => current with { Workflow = tb.Text },
            "Platforms"   => current with { Platforms = tb.Text },
            _             => current,
        };
        // Equality guard — TextChanged is dispatcher-queued; the suppress
        // flag may have cleared between setting Text in Render() and the
        // handler firing. Avoid spurious SetIntent → MarkDirty calls on
        // cold-launch by short-circuiting when nothing changed.
        if (updated == current) return;
        InvokeSetIntent(updated);
    }

    private void OnNotesChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppress) return;
        var current = ReadIntent() ?? IntentValues.Default;
        var updated = current with { Notes = NotesInput.Text };
        if (updated == current) return;
        InvokeSetIntent(updated);
    }

    private void InvokeSetIntent(IntentValues updated)
    {
        var page = FindParent<Page>(this);
        Composer.Views.MvuxCommandInvoker.Invoke(page?.DataContext, "SetIntent", updated);
    }

    private static T? FindParent<T>(DependencyObject child) where T : class
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent is not null && parent is not T)
            parent = VisualTreeHelper.GetParent(parent);
        return parent as T;
    }
}
