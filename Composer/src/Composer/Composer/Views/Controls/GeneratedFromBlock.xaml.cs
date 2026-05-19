using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Composer.Views.Controls;

/// <summary>
/// Code reference block. Pairs with the Architecture canvas's solution-tree
/// synthesis, the Design canvas's ColorPaletteOverride.xaml mirror, and any
/// other "show the file we're about to write" surface.
///
/// Two themes:
///  - <c>light</c> (default): paper-toned printed-appendix look. Reads as
///    reference material.
///  - <c>dark</c>: ink-toned editor-window look. Reads as code-the-agent-
///    will-write.
///
/// Two languages:
///  - <c>tree</c>: only <c># comments</c> get tinted (italic comment color).
///  - <c>xaml</c>: small state-machine tokeniser tints elements / attrs /
///    string literals / XML comments. Good enough for synthesised override
///    files; not a full XAML parser.
///
/// Specs:
///  docs/update-context-engine-04-architecture.md §5
///  docs/update-context-engine-05-design.md §4
/// </summary>
public sealed partial class GeneratedFromBlock : UserControl
{
    public GeneratedFromBlock()
    {
        this.InitializeComponent();
    }

    /// <summary>Set the block's contents. Call again to update.</summary>
    public void Apply(string fileLabel, string sourceLabel, string code,
                      string language = "tree", string theme = "light")
    {
        FileLabelText.Text   = fileLabel ?? string.Empty;
        SourceLabelText.Text = sourceLabel ?? string.Empty;

        var palette = theme == "dark" ? DarkPalette : LightPalette;

        OuterBorder.Background  = palette.SurfaceBrush;
        OuterBorder.BorderBrush = palette.BorderBrush;
        HeaderStrip.Background  = palette.HeaderBrush;
        FileLabelText.Foreground   = palette.HeaderTextBrush;
        SourceLabelText.Foreground = palette.SourceTextBrush;

        LinesStack.Children.Clear();
        if (string.IsNullOrEmpty(code)) return;

        var monoFont = (FontFamily)Application.Current.Resources["MonoFontFamily"];
        var serifIt  = (FontFamily)Application.Current.Resources["SerifLightItalicFontFamily"];

        foreach (var rawLine in code.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.Length == 0 ? " " : rawLine;
            var tb = new TextBlock { FontFamily = monoFont, FontSize = 12 };

            if (language == "tree")
            {
                var hashIdx = line.IndexOf('#');
                if (hashIdx < 0)
                {
                    tb.Text = line;
                    tb.Foreground = palette.CodeBrush;
                }
                else
                {
                    tb.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run
                    {
                        Text = line.Substring(0, hashIdx),
                        Foreground = palette.CodeBrush,
                    });
                    tb.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run
                    {
                        Text = line.Substring(hashIdx),
                        Foreground = palette.CommentBrush,
                        FontFamily = serifIt,
                        FontStyle = Windows.UI.Text.FontStyle.Italic,
                    });
                }
            }
            else if (language == "xaml")
            {
                AppendXamlLine(tb, line, palette);
            }
            else
            {
                tb.Text = line;
                tb.Foreground = palette.CodeBrush;
            }

            LinesStack.Children.Add(tb);
        }
    }

    /// <summary>Tokenise a XAML line into element / attribute / string runs
    /// using a small state machine. Good enough for the synthesised override
    /// files (well-formed XAML, no edge cases like CDATA or processing
    /// instructions); not a full XAML parser.</summary>
    private static void AppendXamlLine(TextBlock tb, string line, Palette p)
    {
        var trimmed = line.TrimStart();
        if (trimmed.StartsWith("<!--"))
        {
            tb.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run
            {
                Text = line,
                Foreground = p.CommentBrush,
                FontStyle = Windows.UI.Text.FontStyle.Italic,
            });
            return;
        }

        var i = 0;
        while (i < line.Length)
        {
            if (line[i] == '<')
            {
                var tagEnd = line.IndexOf('>', i);
                if (tagEnd < 0) tagEnd = line.Length - 1;

                var nameStart = i + 1;
                if (nameStart < line.Length && line[nameStart] == '/') nameStart++;
                var nameEnd = nameStart;
                while (nameEnd < line.Length && nameEnd <= tagEnd
                       && (char.IsLetterOrDigit(line[nameEnd]) || line[nameEnd] == ':' || line[nameEnd] == '.'))
                    nameEnd++;

                tb.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run
                {
                    Text = line.Substring(i, nameStart - i),
                    Foreground = p.PunctBrush,
                });

                if (nameEnd > nameStart)
                {
                    tb.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run
                    {
                        Text = line.Substring(nameStart, nameEnd - nameStart),
                        Foreground = p.ElementBrush,
                    });
                }

                i = nameEnd;

                while (i <= tagEnd)
                {
                    if (i >= line.Length) break;
                    var c = line[i];
                    if (c == '>')
                    {
                        tb.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run
                        {
                            Text = ">",
                            Foreground = p.PunctBrush,
                        });
                        i++;
                        break;
                    }
                    if (c == '/' && i + 1 < line.Length && line[i + 1] == '>')
                    {
                        tb.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run
                        {
                            Text = "/>",
                            Foreground = p.PunctBrush,
                        });
                        i += 2;
                        break;
                    }

                    if (char.IsLetter(c))
                    {
                        var ae = i;
                        while (ae < line.Length && ae <= tagEnd
                               && (char.IsLetterOrDigit(line[ae]) || line[ae] == ':' || line[ae] == '.' || line[ae] == '-'))
                            ae++;
                        tb.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run
                        {
                            Text = line.Substring(i, ae - i),
                            Foreground = p.AttrBrush,
                        });
                        i = ae;
                        continue;
                    }

                    if (c == '"')
                    {
                        var se = line.IndexOf('"', i + 1);
                        if (se < 0) se = line.Length - 1;
                        tb.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run
                        {
                            Text = line.Substring(i, se - i + 1),
                            Foreground = p.StringBrush,
                        });
                        i = se + 1;
                        continue;
                    }

                    tb.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run
                    {
                        Text = line.Substring(i, 1),
                        Foreground = p.CodeBrush,
                    });
                    i++;
                }
            }
            else
            {
                var nextTag = line.IndexOf('<', i);
                if (nextTag < 0) nextTag = line.Length;
                tb.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run
                {
                    Text = line.Substring(i, nextTag - i),
                    Foreground = p.CodeBrush,
                });
                i = nextTag;
            }
        }
    }

    private sealed record Palette(
        Brush SurfaceBrush,
        Brush HeaderBrush,
        Brush BorderBrush,
        Brush HeaderTextBrush,
        Brush SourceTextBrush,
        Brush CodeBrush,
        Brush PunctBrush,
        Brush ElementBrush,
        Brush AttrBrush,
        Brush StringBrush,
        Brush CommentBrush);

    private static readonly Palette LightPalette = new(
        SurfaceBrush:    AppBrushes.Paper2,
        HeaderBrush:     AppBrushes.Paper3,
        BorderBrush:     AppBrushes.Hairline,
        HeaderTextBrush: AppBrushes.Ink,
        SourceTextBrush: AppBrushes.Ink3,
        CodeBrush:       AppBrushes.Ink2,
        PunctBrush:      AppBrushes.Ink3,
        ElementBrush:    new SolidColorBrush(Color.FromArgb(0xFF, 0xa8, 0x3a, 0x25)),
        AttrBrush:       new SolidColorBrush(Color.FromArgb(0xFF, 0xa8, 0x84, 0x1a)),
        StringBrush:     new SolidColorBrush(Color.FromArgb(0xFF, 0x5a, 0x7a, 0x5a)),
        CommentBrush:    AppBrushes.Ink3);

    private static readonly Palette DarkPalette = new(
        SurfaceBrush:    new SolidColorBrush(Color.FromArgb(0xFF, 0x18, 0x18, 0x1B)),
        HeaderBrush:     new SolidColorBrush(Color.FromArgb(0xFF, 0x23, 0x23, 0x2A)),
        BorderBrush:     new SolidColorBrush(Color.FromArgb(0xFF, 0x39, 0x39, 0x40)),
        HeaderTextBrush: new SolidColorBrush(Color.FromArgb(0xFF, 0xFA, 0xFA, 0xFA)),
        SourceTextBrush: new SolidColorBrush(Color.FromArgb(0xFF, 0xA1, 0xA1, 0xAA)),
        CodeBrush:       new SolidColorBrush(Color.FromArgb(0xFF, 0xE4, 0xE2, 0xE6)),
        PunctBrush:      new SolidColorBrush(Color.FromArgb(0xFF, 0xA1, 0xA1, 0xAA)),
        ElementBrush:    new SolidColorBrush(Color.FromArgb(0xFF, 0xF5, 0x9D, 0x82)),
        AttrBrush:       new SolidColorBrush(Color.FromArgb(0xFF, 0xE5, 0xC3, 0x8B)),
        StringBrush:     new SolidColorBrush(Color.FromArgb(0xFF, 0x9B, 0xC5, 0x9B)),
        CommentBrush:    new SolidColorBrush(Color.FromArgb(0xFF, 0xA1, 0xA1, 0xAA)));
}
