using System.ComponentModel;
using Composer.Models;
using Composer.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Composer.Views.Layers;

/// <summary>
/// Layer 4 — Design System. Color swatches + font dropdown + spacing rhythm.
/// Font dropdown is editable; color swatches are display-only in brief 02
/// (composer prompt drives palette changes — full color picker is a future
/// brief per §5).
/// </summary>
public sealed partial class DesignTokenGrid : UserControl
{
    private INotifyPropertyChanged? _vm;
    private bool _suppressFontCallback;

    private static readonly int[] SpacingRhythm = { 4, 8, 12, 16, 24, 32, 48 };

    /// <summary>Type scale rows. Sample copy is the field-service archetype
    /// per brief 05 §2 — real strings the user might see in their app, not
    /// lorem ipsum, so they can judge the type choices in context.</summary>
    private static readonly (string Step, double Size, string Sample, bool UseMono)[] TypeScale =
    {
        ("Display 32",  32, "Today's schedule",                                                false),
        ("Heading 19",  19, "Job #4471 · Boiler service",                                      true),
        ("Body 16",     16, "Tap a job to assign a technician. Conflicts surface inline.",     false),
        ("Caption 11",  11, "ARRIVED · 09:14 SYNCED",                                          true),
    };

    /// <summary>Order matches the brief's 4×2 grid (4 columns × 2 rows).</summary>
    private static readonly string[] TokenOrder =
        { "Surface", "Action", "Info", "Success", "Warn", "Panel", "Tag", "Locked" };

    public DesignTokenGrid()
    {
        this.InitializeComponent();
        this.DataContextChanged += (_, args) => Attach(args.NewValue);
        this.Unloaded            += (_, _)    => Detach();
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
        if (e.PropertyName is "Design" or "AppName" or "AppDescription"
            or "LayerStates"
            or "ReferenceScreenshotCount" or "ReferenceScreenshotPaths")
            DispatcherQueue?.TryEnqueue(Render);
    }

    private DesignTokens? ReadDesign() =>
        Composer.Views.MvuxValueReader.Read<DesignTokens>(DataContext, "Design");

    /// <summary>True when the DesignSystem layer is in `previewing` state.
    /// Reads the LayerStates dict via reflection so the canvas can flip into
    /// diff mode when the user has staged a preview.</summary>
    private bool IsPreviewing()
    {
        var dc = DataContext;
        if (dc is null) return false;
        var statesProp = Composer.Views.MvuxValueReader.ReadRaw(dc, "LayerStates");
        if (statesProp is null) return false;

        // The bindable proxy returns the dictionary directly OR wraps it; we
        // only care whether DesignSystem maps to Previewing.
        var dict = Composer.Views.MvuxValueReader.Unwrap<System.Collections.Immutable.IImmutableDictionary<LayerKind, LayerState>>(statesProp);
        if (dict is null && statesProp is System.Collections.Immutable.IImmutableDictionary<LayerKind, LayerState> direct)
            dict = direct;
        if (dict is null) return false;
        return dict.TryGetValue(LayerKind.DesignSystem, out var s) && s == LayerState.Previewing;
    }

    /// <summary>Read the staged preview tokens via the model's GetPreviewValues
    /// method. Returns null when nothing is staged.</summary>
    private DesignTokens? ReadProposedTokens()
    {
        var raw = Composer.Views.MvuxValueReader.InvokeMethod(DataContext, "GetPreviewValues", LayerKind.DesignSystem);
        return raw as DesignTokens
            ?? Composer.Views.MvuxValueReader.Unwrap<DesignTokens>(raw);
    }

    private void ApplyVisionIndicator()
    {
        var dc = DataContext;
        var count = (Composer.Views.MvuxValueReader.ReadRaw(dc, "ReferenceScreenshotCount") as int?) ?? 0;
        if (count > 0)
        {
            VisionInputBadge.Visibility = Visibility.Visible;
            VisionInputBadgeText.Text = count == 1 ? "VISION · 1 SHOT" : $"VISION · {count} SHOTS";
            CaptionText.Text = "tuned to your description and reference screenshots";
        }
        else
        {
            VisionInputBadge.Visibility = Visibility.Collapsed;
            CaptionText.Text = "tuned to your description";
        }
    }

    private void ApplyReferenceShots()
    {
        ReferenceShotsStack.Children.Clear();
        var dc = DataContext;
        if (dc is null) { ReferenceShotsBlock.Visibility = Visibility.Collapsed; return; }

        var raw = Composer.Views.MvuxValueReader.ReadRaw(dc, "ReferenceScreenshotPaths");
        var paths = raw switch
        {
            System.Collections.Immutable.ImmutableArray<string> arr => (System.Collections.Generic.IEnumerable<string>)arr,
            System.Collections.Generic.IEnumerable<string> e        => e,
            _                                                       => System.Array.Empty<string>(),
        };

        var any = false;
        foreach (var path in paths)
        {
            if (string.IsNullOrWhiteSpace(path)) continue;
            ReferenceShotsStack.Children.Add(BuildThumbnail(path));
            any = true;
        }
        ReferenceShotsBlock.Visibility = any ? Visibility.Visible : Visibility.Collapsed;
    }

    private FrameworkElement BuildThumbnail(string path)
    {
        var border = new Border
        {
            Width = 96,
            Height = 72,
            Background = AppBrushes.Paper2,
            BorderBrush = AppBrushes.Hairline,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
        };
        ToolTipService.SetToolTip(border, path);

        try
        {
            if (System.IO.File.Exists(path))
            {
                var img = new Image
                {
                    Stretch = Stretch.UniformToFill,
                    Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new System.Uri(path)),
                };
                border.Child = img;
                return border;
            }
        }
        catch { /* fall through to "not found" placeholder */ }

        // Placeholder for invalid / missing paths so the user sees the
        // file-not-found case explicitly.
        border.BorderBrush = AppBrushes.Ink4;
        border.Child = new TextBlock
        {
            Text = "MISSING",
            FontFamily = (FontFamily)Application.Current.Resources["MonoFontFamily"],
            FontSize = 10,
            CharacterSpacing = 160,
            Foreground = AppBrushes.Ink4,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        return border;
    }

    /// <summary>Map the AI's font name to an actually-renderable FontFamily.
    /// Only Fraunces is bundled in Assets/Fonts; Geist / Newsreader / Inter
    /// map to system-fallback families that render distinctly enough that
    /// the dropdown shows a real visual change.</summary>
    private static FontFamily ResolveBodyFont(string name) => name switch
    {
        "Fraunces"   => (FontFamily)Application.Current.Resources["SerifLightFontFamily"],
        "Newsreader" => new FontFamily("Cambria, Georgia, serif"),
        "Inter"      => new FontFamily("Segoe UI, Inter, Arial, sans-serif"),
        "Geist"      => new FontFamily("Geist, Inter, Segoe UI Variable Display, Segoe UI, sans-serif"),
        "Geist Mono" => new FontFamily("Geist Mono, JetBrains Mono, Cascadia Mono, Consolas, monospace"),
        _            => (FontFamily)Application.Current.Resources["SerifLightFontFamily"],
    };

    private string ResolveSampleText()
    {
        var dc = DataContext;
        if (dc is null) return "Set the tone";
        var t = dc.GetType();
        var description = (t.GetProperty("AppDescription")?.GetValue(dc) as string ?? string.Empty).Trim();
        if (!string.IsNullOrEmpty(description))
        {
            // Use the first ~5 words of the description so the type scale
            // reads like the actual app's copy — not the placeholder.
            var words = description.Split(' ');
            var take = System.Math.Min(words.Length, 5);
            return string.Join(' ', words, 0, take);
        }
        var name = (t.GetProperty("AppName")?.GetValue(dc) as string ?? string.Empty).Trim();
        return string.IsNullOrEmpty(name) ? "Set the tone" : name;
    }

    private void Render()
    {
        var current = ReadDesign() ?? DesignTokens.Default;
        var previewing = IsPreviewing();
        var proposed = previewing ? ReadProposedTokens() : null;

        // In `previewing` state we display the proposed tokens (the "after"
        // values) while showing diff signals on changed swatches. In all
        // other states we display the live tokens.
        var d = proposed ?? current;

        ApplyVisionIndicator();
        ApplyReferenceShots();

        // ── Color swatches: 4×2 grid of ColorPicker flyout buttons.
        // Brief 05 §2/§3 — direct edit triggers MarkDirty.
        ColorGrid.Children.Clear();
        for (var i = 0; i < TokenOrder.Length; i++)
        {
            var name = TokenOrder[i];
            var color = ReadTokenColor(d, name);
            var oldColor = previewing ? (Color?)ReadTokenColor(current, name) : null;
            var changed = oldColor.HasValue && !ColorEquals(oldColor.Value, color);

            var col = i % 4;
            var row = i / 4;
            var swatch = BuildSwatch(name, color, isAction: name == "Action",
                                     readOnly: previewing,
                                     wasColor: changed ? oldColor : null);
            Grid.SetColumn(swatch, col);
            Grid.SetRow(swatch, row);
            swatch.Margin = new Thickness(
                left:   col == 0 ? 0 : 8,
                top:    row == 0 ? 0 : 12,
                right:  0,
                bottom: 0);
            ColorGrid.Children.Add(swatch);
        }

        // ── Type scale: archetype sample copy per brief 05 §2.
        TypeScaleStack.Children.Clear();
        var bodyFont = ResolveBodyFont(d.BodyFont);
        var monoFont = (FontFamily)Application.Current.Resources["MonoFontFamily"];
        foreach (var (step, size, sample, useMono) in TypeScale)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(96) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var stepLabel = new TextBlock
            {
                Text = step,
                FontFamily = monoFont,
                FontSize = 10,
                CharacterSpacing = 160,
                Foreground = AppBrushes.Ink3,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(stepLabel, 0);
            grid.Children.Add(stepLabel);

            var sampleBlock = new TextBlock
            {
                Text = sample,
                FontFamily = useMono ? monoFont : bodyFont,
                FontSize = size,
                Foreground = AppBrushes.Ink,
                TextTrimming = TextTrimming.CharacterEllipsis,
                CharacterSpacing = useMono ? 140 : 0,
            };
            Grid.SetColumn(sampleBlock, 1);
            grid.Children.Add(sampleBlock);

            TypeScaleStack.Children.Add(grid);
        }

        // ── Spacing rhythm
        SpacingRow.Children.Clear();
        foreach (var s in SpacingRhythm)
        {
            var stack = new StackPanel { Spacing = 4 };
            stack.Children.Add(new Border
            {
                Width = s,
                Height = 24,
                Background = AppBrushes.Ink2,
            });
            stack.Children.Add(new TextBlock
            {
                Text = s.ToString(),
                FontFamily = monoFont,
                FontSize = 10,
                CharacterSpacing = 160,
                Foreground = AppBrushes.Ink3,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            SpacingRow.Children.Add(stack);
        }

        // ── Live control gallery — Primary button + ghost button + Tab Bar.
        var actionBrush = new SolidColorBrush(d.Action);
        var actionContrast = ContrastForeground(d.Action); // ink or paper based on luminance
        PrimaryButtonHost.Background = actionBrush;
        PrimaryButtonText.Foreground = actionContrast;

        TabBarRow.Children.Clear();
        var segments = new[] { ("TODAY", true), ("WEEK", false), ("MAP", false) };
        for (var i = 0; i < segments.Length; i++)
        {
            var (label, active) = segments[i];
            TabBarRow.Children.Add(new Border
            {
                Background = active ? actionBrush : (Brush)AppBrushes.Paper,
                BorderBrush = AppBrushes.Hairline,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10, 6, 10, 6),
                Margin  = i == 0 ? new Thickness(0) : new Thickness(-1, 0, 0, 0),
                Child = new TextBlock
                {
                    Text = label,
                    FontFamily = monoFont,
                    FontSize = 10,
                    CharacterSpacing = 140,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = active ? actionContrast : AppBrushes.Ink2,
                },
            });
        }

        // ── Live ColorPaletteOverride.xaml mirror — dark editor-window
        // styling per brief 05 §4 (this is "what the agent will write",
        // not printed appendix).
        ColorOverrideMirror.Apply(
            fileLabel:   "COLORPALETTEOVERRIDE.XAML",
            sourceLabel: "GENERATED FROM TOKEN MAP",
            code:        Composer.Models.LayerMarkdownTemplates.BuildColorPaletteOverrideXaml(d),
            language:    "xaml",
            theme:       "dark");

        // ── Font combo — order matches DesignTokenGrid.xaml ComboBoxItem order.
        // In previewing state with a changed font, swap the dropdown for a
        // "WAS {oldFont}" amber badge (brief 05 §6).
        var fontChanged = previewing && proposed is not null
                          && !string.Equals(current.BodyFont, proposed.BodyFont, System.StringComparison.Ordinal);
        if (fontChanged)
        {
            FontCombo.Visibility   = Visibility.Collapsed;
            FontWasBadge.Visibility = Visibility.Visible;
            FontWasBadge.Text       = $"WAS {current.BodyFont.ToUpperInvariant()}";
        }
        else
        {
            FontCombo.Visibility   = Visibility.Visible;
            FontWasBadge.Visibility = Visibility.Collapsed;
            FontCombo.IsEnabled = !previewing;

            _suppressFontCallback = true;
            FontCombo.SelectedIndex = d.BodyFont switch
            {
                "Fraunces"   => 0,
                "Newsreader" => 1,
                "Inter"      => 2,
                "Geist"      => 3,
                "Geist Mono" => 4,
                _            => 0,
            };
            _suppressFontCallback = false;
        }
    }

    private static Color ReadTokenColor(DesignTokens d, string name) => name switch
    {
        "Surface" => d.Surface,
        "Action"  => d.Action,
        "Info"    => d.Info,
        "Success" => d.Success,
        "Warn"    => d.Warn,
        "Panel"   => d.Panel,
        "Tag"     => d.Tag,
        "Locked"  => d.Locked,
        _         => d.Surface,
    };

    private static DesignTokens WithToken(DesignTokens d, string name, Color v) => name switch
    {
        "Surface" => d with { Surface = v },
        "Action"  => d with { Action  = v },
        "Info"    => d with { Info    = v },
        "Success" => d with { Success = v },
        "Warn"    => d with { Warn    = v },
        "Panel"   => d with { Panel   = v },
        "Tag"     => d with { Tag     = v },
        "Locked"  => d with { Locked  = v },
        _         => d,
    };

    /// <summary>Build a swatch entry — a Button with the token color as its
    /// background, opening a ColorPicker flyout when clicked. The Action
    /// swatch gets a 2px amber border (the visual self-reference per
    /// brief 05 §2). Edits mark the layer dirty via SetDesign.
    /// When <paramref name="readOnly"/> the flyout is omitted (previewing
    /// state — the user is reviewing, not editing). When
    /// <paramref name="wasColor"/> is non-null the swatch wraps in an
    /// amber-tint backdrop with a `was #XXXXXX` annotation underneath
    /// (diff signal per brief 05 §6).</summary>
    private FrameworkElement BuildSwatch(string name, Color color, bool isAction,
                                          bool readOnly = false, Color? wasColor = null)
    {
        var stack = new StackPanel { Spacing = 6 };

        var swatchButton = new Button
        {
            Background = new SolidColorBrush(color),
            BorderBrush  = isAction ? AppBrushes.Amber : AppBrushes.Hairline,
            BorderThickness = new Thickness(isAction ? 2 : 1),
            CornerRadius = new CornerRadius(4),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(0),
            Height = 56,
            IsEnabled = !readOnly,
        };

        if (!readOnly)
        {
            var picker = new ColorPicker
            {
                IsAlphaEnabled = false,
                IsAlphaSliderVisible = false,
                IsAlphaTextInputVisible = false,
                IsColorChannelTextInputVisible = true,
                IsHexInputVisible = true,
                IsMoreButtonVisible = false,
                IsColorSliderVisible = true,
                IsColorPreviewVisible = true,
                ColorSpectrumShape = ColorSpectrumShape.Box,
                Color = color,
            };
            picker.ColorChanged += (s, e) => OnTokenColorChanged(name, e.NewColor);
            swatchButton.Flyout = new Flyout { Content = picker };
            ToolTipService.SetToolTip(swatchButton, $"{name} · click to edit");
        }

        stack.Children.Add(swatchButton);

        var labelGrid = new Grid();
        labelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        labelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var label = new TextBlock
        {
            Text = name.ToUpperInvariant(),
            FontFamily = (FontFamily)Application.Current.Resources["MonoFontFamily"],
            FontSize = 10,
            CharacterSpacing = 160,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = AppBrushes.Ink2,
        };
        Grid.SetColumn(label, 0);
        labelGrid.Children.Add(label);

        var hex = new TextBlock
        {
            Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}",
            FontFamily = (FontFamily)Application.Current.Resources["MonoFontFamily"],
            FontSize = 10,
            CharacterSpacing = 160,
            Foreground = AppBrushes.Ink4,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(8, 0, 0, 0),
        };
        Grid.SetColumn(hex, 1);
        labelGrid.Children.Add(hex);

        stack.Children.Add(labelGrid);

        if (wasColor.HasValue)
        {
            var oldHex = $"was #{wasColor.Value.R:X2}{wasColor.Value.G:X2}{wasColor.Value.B:X2}";
            stack.Children.Add(new TextBlock
            {
                Text = oldHex.ToUpperInvariant(),
                FontFamily = (FontFamily)Application.Current.Resources["MonoFontFamily"],
                FontSize = 9,
                CharacterSpacing = 180,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = AppBrushes.Amber,
            });

            // Wrap the entire stack in an amber-tint backdrop so the
            // changed swatch reads as "the AI moved this one." Brief 05 §6.
            return new Border
            {
                Background = (Brush)Application.Current.Resources["AmberSoftBrush"],
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6),
                Child = stack,
            };
        }

        return stack;
    }

    private static bool ColorEquals(Color a, Color b)
        => a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;

    private void OnTokenColorChanged(string tokenName, Color newColor)
    {
        var current = ReadDesign() ?? DesignTokens.Default;
        var updated = WithToken(current, tokenName, newColor);
        var page = FindParent<Page>(this);
        MvuxCommandInvoker.Invoke(page?.DataContext, "SetDesign", updated);
    }

    /// <summary>Pick paper or ink as the foreground that contrasts with the
    /// supplied background color. Uses relative luminance (perceptual
    /// approximation) — light backgrounds get ink, dark get paper.</summary>
    private static Brush ContrastForeground(Color bg)
    {
        // Rec. 709 luminance, sufficient for the design canvas's small
        // sample text. Threshold chosen so amber (#d4a959, ~0.7) lands on
        // ink and darker mids fall on paper.
        var r = bg.R / 255.0;
        var g = bg.G / 255.0;
        var b = bg.B / 255.0;
        var l = 0.2126 * r + 0.7152 * g + 0.0722 * b;
        return l > 0.55 ? AppBrushes.Ink : AppBrushes.Paper;
    }

    private void OnFontChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressFontCallback) return;
        if (FontCombo.SelectedItem is not ComboBoxItem item) return;
        var font = item.Content?.ToString() ?? "Fraunces";

        var current = ReadDesign() ?? DesignTokens.Default;
        var updated = current with { BodyFont = font };

        var page = FindParent<Page>(this);
        MvuxCommandInvoker.Invoke(page?.DataContext, "SetDesign", updated);
    }

    private static T? FindParent<T>(DependencyObject child) where T : class
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent is not null && parent is not T)
            parent = VisualTreeHelper.GetParent(parent);
        return parent as T;
    }
}
