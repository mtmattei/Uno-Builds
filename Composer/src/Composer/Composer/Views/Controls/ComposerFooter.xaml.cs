using System;
using Composer.Models;
using Composer.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;

namespace Composer.Views.Controls;

/// <summary>
/// Bottom-of-canvas composer per v11 interaction brief §5–§6.
/// Three states drive eyebrow word, primary action, and acknowledgment line:
///   Clean      → REFINING   · Lock and continue → (Continue → on Intent)
///   Dirty      → LISTENING  · Generate preview →
///   Previewing → PROPOSING  · Accept and lock →  (+ Discard preview)
///
/// Suggestion chips row is always visible. Click sets the prompt + marks
/// dirty + focuses the textarea. Cmd/Ctrl+Enter routes to the primary
/// action; Esc in Previewing discards the preview.
/// </summary>
public sealed partial class ComposerFooter : UserControl
{
    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(
            nameof(State), typeof(LayerState), typeof(ComposerFooter),
            new PropertyMetadata(LayerState.Clean, OnStateChanged));

    public static readonly DependencyProperty PromptProperty =
        DependencyProperty.Register(
            nameof(Prompt), typeof(string), typeof(ComposerFooter),
            new PropertyMetadata(string.Empty, OnPromptChanged));

    public LayerState State
    {
        get => (LayerState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public string Prompt
    {
        get => (string)GetValue(PromptProperty);
        set => SetValue(PromptProperty, value);
    }

    private bool _suppressPromptCallback;

    public ComposerFooter()
    {
        this.InitializeComponent();
        this.Loaded             += (_, _) => { RenderChips(); ApplyState(); SyncLeadQuestion(); };
        this.DataContextChanged += (_, _) => { RenderChips(); ApplyState(); SyncLeadQuestion(); };
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ComposerFooter f)
        {
            f.RenderChips();
            f.ApplyState();
            f.SyncLeadQuestion();
        }
    }

    private static void OnPromptChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ComposerFooter f && !f._suppressPromptCallback)
        {
            f._suppressPromptCallback = true;
            f.PromptInput.Text = (string?)e.NewValue ?? string.Empty;
            f._suppressPromptCallback = false;
        }
    }

    private LayerKind ActiveLayerKind()
    {
        var dc = DataContext;
        if (dc is null) return LayerKind.Intent;
        var idx = (dc.GetType().GetProperty("ActiveIndex")?.GetValue(dc) as int?) ?? 0;
        var clamped = Math.Clamp(idx, 0, Composer.Models.Layers.All.Length - 1);
        return Composer.Models.Layers.All[clamped].Kind;
    }

    private void ApplyState()
    {
        var kind = ActiveLayerKind();
        // Eyebrow word from ComposerStatus per brief.
        EyebrowText.Text = $"COMPOSER · {ComposerStatus.ForLayerState(State)}";

        // Italic hint next to the primary button — reframes the click per
        // the current state. Matches ShellModel.ActivePrimaryHint copy.
        PrimaryHintText.Text = State switch
        {
            LayerState.Clean      => "accepting the recommendation",
            LayerState.Dirty      => "with your edits",
            LayerState.Previewing => "the AI's pass",
            _                     => string.Empty,
        };

        // First-layer label is "Continue →" — there's nothing to lock when
        // the user is just confirming the recommendation. Intent is layer 0
        // (matches the prototype's cold-launch surface and softened first-
        // layer label per composer-context-engine.jsx line 36).
        var firstLayerCleanLabel = kind == LayerKind.Intent
            ? "Continue →"
            : "Lock and continue →";

        switch (State)
        {
            case LayerState.Clean:
                PrimaryButton.Content = firstLayerCleanLabel;
                PrimaryButton.Style = (Style)Application.Current.Resources["InkButtonStyle"];
                DiscardEditsButton.Visibility   = Visibility.Collapsed;
                DiscardPreviewButton.Visibility = Visibility.Collapsed;
                AckLine.Visibility = Visibility.Collapsed;
                break;
            case LayerState.Dirty:
                PrimaryButton.Content = "Generate preview →";
                PrimaryButton.Style = (Style)Application.Current.Resources["AmberButtonStyle"];
                DiscardEditsButton.Visibility   = Visibility.Visible;
                DiscardPreviewButton.Visibility = Visibility.Collapsed;
                AckLine.Visibility = Visibility.Collapsed;
                break;
            case LayerState.Previewing:
                PrimaryButton.Content = "Accept and lock →";
                PrimaryButton.Style = (Style)Application.Current.Resources["InkButtonStyle"];
                DiscardEditsButton.Visibility   = Visibility.Collapsed;
                DiscardPreviewButton.Visibility = Visibility.Visible;
                ApplyAck(kind);
                break;
            case LayerState.Locked:
                PrimaryButton.Content = firstLayerCleanLabel;
                PrimaryButton.Style = (Style)Application.Current.Resources["InkButtonStyle"];
                DiscardEditsButton.Visibility   = Visibility.Collapsed;
                DiscardPreviewButton.Visibility = Visibility.Collapsed;
                AckLine.Visibility = Visibility.Collapsed;
                break;
        }
    }

    /// <summary>Read PreviewAcks[kind] off the model and surface the
    /// quote above the primary action. Hides if the ack is empty.</summary>
    private void ApplyAck(LayerKind kind)
    {
        var dc = DataContext;
        if (dc is null) { AckLine.Visibility = Visibility.Collapsed; return; }
        var acksProp = dc.GetType().GetProperty("PreviewAcks")?.GetValue(dc);
        var dict = Composer.Views.MvuxValueReader.Unwrap<
            System.Collections.Immutable.IImmutableDictionary<LayerKind, string>>(acksProp);
        if (dict is null && acksProp is System.Collections.Immutable.IImmutableDictionary<LayerKind, string> direct)
            dict = direct;
        if (dict is null || !dict.TryGetValue(kind, out var quote) || string.IsNullOrWhiteSpace(quote))
        {
            AckLine.Visibility = Visibility.Collapsed;
            return;
        }
        AckText.Text = $"You asked: “{quote}” — here's what changes if I apply that.";
        AckLine.Visibility = Visibility.Visible;
    }

    /// <summary>Populate the chip row from ComposerModel.SuggestionChips for
    /// the active layer. Click sets the prompt + marks dirty + focuses.</summary>
    private void RenderChips()
    {
        ChipsRow.Children.Clear();
        var kind = ActiveLayerKind();
        if (!ComposerModel.SuggestionChips.TryGetValue(kind, out var chips)) return;

        var monoFont = (FontFamily)Application.Current.Resources["MonoFontFamily"];
        foreach (var chip in chips)
        {
            var btn = new Button
            {
                Content = chip,
                FontFamily = monoFont,
                FontSize = 11,
                FontWeight = Microsoft.UI.Text.FontWeights.Medium,
                CharacterSpacing = 40,
                Background = (Brush)Application.Current.Resources["TransparentBrush"],
                BorderBrush = (Brush)Application.Current.Resources["HairlineBrush"],
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10, 5, 10, 5),
                Foreground = (Brush)Application.Current.Resources["Ink2Brush"],
                Tag = chip,
            };
            btn.Click += OnChipClick;
            ChipsRow.Children.Add(btn);
        }
    }

    private void OnChipClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string chip }) return;
        // Set the prompt textbox + write through to the model. Caret moves
        // to the end so the user can extend the chip text immediately.
        _suppressPromptCallback = true;
        PromptInput.Text = chip;
        PromptInput.SelectionStart = chip.Length;
        _suppressPromptCallback = false;
        PromptInput.Focus(FocusState.Programmatic);

        var page = FindParent<Page>(this);
        MvuxCommandInvoker.Invoke(page?.DataContext, "SetActivePrompt", chip);
    }

    private void OnPromptKeyDown(object sender, KeyRoutedEventArgs e)
    {
        // Esc in Previewing → DiscardPreview (per v11 interaction brief).
        if (e.Key == VirtualKey.Escape && State == LayerState.Previewing)
        {
            e.Handled = true;
            var page = FindParent<Page>(this);
            MvuxCommandInvoker.Invoke(page?.DataContext, "DiscardPreview");
            return;
        }

        // Cmd/Ctrl+Enter → primary action.
        if (e.Key != VirtualKey.Enter) return;
        var ctrl = Microsoft.UI.Input.InputKeyboardSource
            .GetKeyStateForCurrentThread(VirtualKey.Control);
        if ((ctrl & Windows.UI.Core.CoreVirtualKeyStates.Down) != Windows.UI.Core.CoreVirtualKeyStates.Down)
            return;
        e.Handled = true;
        TriggerPrimary();
    }

    private void OnPromptTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressPromptCallback) return;
        var page = FindParent<Page>(this);
        MvuxCommandInvoker.Invoke(page?.DataContext, "SetActivePrompt", PromptInput.Text);
    }

    private void OnPrimaryClick(object sender, RoutedEventArgs e) => TriggerPrimary();

    private void TriggerPrimary()
    {
        var page = FindParent<Page>(this);
        if (page?.DataContext is null) return;

        var resolved = ResolveCurrentState(page.DataContext);
        if (resolved == LayerState.Clean && !string.IsNullOrWhiteSpace(PromptInput.Text))
            resolved = LayerState.Dirty;

        var commandName = resolved switch
        {
            LayerState.Clean       => "LockAndContinue",
            LayerState.Dirty       => "GeneratePreview",
            LayerState.Previewing  => "AcceptAndLock",
            LayerState.Locked      => "LockAndContinue",
            _                      => "LockAndContinue",
        };
        MvuxCommandInvoker.Invoke(page.DataContext, commandName);
        if (resolved == LayerState.Clean || resolved == LayerState.Previewing)
        {
            _suppressPromptCallback = true;
            PromptInput.Text = string.Empty;
            _suppressPromptCallback = false;
        }
    }

    private static LayerState ResolveCurrentState(object dc)
    {
        var t = dc.GetType();
        var idx = (t.GetProperty("ActiveIndex")?.GetValue(dc) as int?) ?? 0;
        var clamped = Math.Clamp(idx, 0, Composer.Models.Layers.All.Length - 1);
        var activeKind = Composer.Models.Layers.All[clamped].Kind;

        var statesObj = t.GetProperty("LayerStates")?.GetValue(dc);
        if (statesObj is System.Collections.Generic.IDictionary<LayerKind, LayerState> states
            && states.TryGetValue(activeKind, out var s))
            return s;
        return LayerState.Clean;
    }

    private void OnDiscardEditsClick(object sender, RoutedEventArgs e)
    {
        var page = FindParent<Page>(this);
        MvuxCommandInvoker.Invoke(page?.DataContext, "DiscardEdits");
        _suppressPromptCallback = true;
        PromptInput.Text = string.Empty;
        _suppressPromptCallback = false;
    }

    private void OnDiscardPreviewClick(object sender, RoutedEventArgs e)
    {
        var page = FindParent<Page>(this);
        MvuxCommandInvoker.Invoke(page?.DataContext, "DiscardPreview");
    }

    /// <summary>Read <c>ActiveLeadQuestion</c> off the bound model and apply
    /// to the lead-question TextBlock. Same reflection pattern as ApplyAck.
    /// Hidden if the layer doesn't define a lead question (e.g., Scaffold
    /// where the footer doesn't render anyway).</summary>
    private void SyncLeadQuestion()
    {
        var dc = DataContext;
        if (dc is null) return;
        var raw = dc.GetType().GetProperty("ActiveLeadQuestion")?.GetValue(dc);
        var text = raw switch
        {
            string s => s,
            null     => null,
            _        => Composer.Views.MvuxValueReader.Unwrap<string>(raw),
        };
        if (string.IsNullOrWhiteSpace(text))
        {
            LeadQuestionText.Visibility = Visibility.Collapsed;
            LeadQuestionText.Text = string.Empty;
        }
        else
        {
            LeadQuestionText.Text = text!;
            LeadQuestionText.Visibility = Visibility.Visible;
        }
    }

    private static T? FindParent<T>(DependencyObject child) where T : class
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent is not null && parent is not T)
            parent = VisualTreeHelper.GetParent(parent);
        return parent as T;
    }
}
