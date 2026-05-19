using System.ComponentModel;
using Composer.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Composer.Views.Layers;

/// <summary>
/// Layer 3 — UX. Horizontal strip of screen tiles. Read-only in brief 02 —
/// edits flow through composer prompt only.
/// </summary>
public sealed partial class UXFlowStrip : UserControl
{
    private INotifyPropertyChanged? _vm;

    public UXFlowStrip()
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
        if (e.PropertyName == "UX")
            DispatcherQueue?.TryEnqueue(Render);
    }

    private void Render()
    {
        var flow = Composer.Views.MvuxValueReader.Read<UXFlow>(DataContext, "UX")
                   ?? UXFlow.Default;
        TileStack.Children.Clear();

        // Prototype UXCanvas (jsx 1265–1267): filename derives from the flow
        // name slug — "dispatch a job" → "dispatch-flow.md". Meta is the
        // dot-prefixed uppercase counter. Caption is italic right-aligned.
        var slug = SlugifyFlowName(flow.Name);
        FlowNameText.Text    = $"{slug}-flow.md";
        MetaText.Text        = $"·  {flow.Screens.Length} SCREENS";
        FlowCaptionText.Text = flow.Name;

        var monoFont   = (FontFamily)Application.Current.Resources["MonoFontFamily"];
        var serifFont  = (FontFamily)Application.Current.Resources["SerifLightItalicFontFamily"];

        for (var i = 0; i < flow.Screens.Length; i++)
        {
            var screen = flow.Screens[i];
            var tile = new Border
            {
                Width = 130,
                Height = 170,
                Background = AppBrushes.Paper2,
                BorderBrush = AppBrushes.Hairline,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
            };
            var inner = new StackPanel { Spacing = 6, Margin = new Thickness(10) };
            inner.Children.Add(new TextBlock
            {
                Text = $"SCREEN {i + 1:D2}",
                FontFamily = monoFont,
                FontSize = 10,
                CharacterSpacing = 160,
                Foreground = AppBrushes.Ink3,
            });
            inner.Children.Add(new TextBlock
            {
                Text = screen.Name,
                FontFamily = serifFont,
                FontSize = 14,
                Foreground = AppBrushes.Ink,
                TextWrapping = TextWrapping.Wrap,
            });

            // Mock-block stack — quick visual representation of the screen UI.
            var blocks = new StackPanel { Spacing = 4, Margin = new Thickness(0, 6, 0, 6) };
            foreach (var b in screen.MockBlocks)
            {
                var w = b switch
                {
                    UIBlock.Full           => 110.0,
                    UIBlock.ThreeQuarters  => 80.0,
                    UIBlock.Half           => 60.0,
                    UIBlock.Quarter        => 30.0,
                    _ => 110.0,
                };
                blocks.Children.Add(new Border
                {
                    Width = w,
                    Height = 4,
                    Background = AppBrushes.Hairline,
                    HorizontalAlignment = HorizontalAlignment.Left,
                });
            }
            inner.Children.Add(blocks);

            inner.Children.Add(new TextBlock
            {
                Text = screen.Note,
                FontFamily = monoFont,
                FontSize = 10,
                Foreground = AppBrushes.Ink3,
                TextWrapping = TextWrapping.Wrap,
            });

            tile.Child = inner;
            TileStack.Children.Add(tile);

            if (i < flow.Screens.Length - 1)
            {
                TileStack.Children.Add(new TextBlock
                {
                    Text = "→",
                    FontFamily = monoFont,
                    FontSize = 14,
                    Foreground = AppBrushes.Ink4,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }
        }
    }

    private static string SlugifyFlowName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "primary";
        var lower = name.Trim().ToLowerInvariant();
        var sb = new System.Text.StringBuilder(lower.Length);
        var lastWasDash = true;
        foreach (var ch in lower)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
                lastWasDash = false;
            }
            else if (!lastWasDash && sb.Length > 0)
            {
                sb.Append('-');
                lastWasDash = true;
            }
        }
        var s = sb.ToString().TrimEnd('-');
        return string.IsNullOrEmpty(s) ? "primary" : s;
    }
}
