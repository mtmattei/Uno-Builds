using ClaudeDash.Models;

namespace ClaudeDash.Controls;

public sealed partial class VerdictBar : UserControl
{
    private static readonly Color GreenFg = ColorHelper.FromArgb(255, 110, 231, 183);   // #6EE7B7
    private static readonly Color GreenBg = ColorHelper.FromArgb(255, 22, 38, 35);      // #162623
    private static readonly Color GreenBd = ColorHelper.FromArgb(255, 27, 67, 55);      // #1B4337

    private static readonly Color AmberFg = ColorHelper.FromArgb(255, 252, 211, 77);    // #FCD34D
    private static readonly Color AmberBg = ColorHelper.FromArgb(255, 38, 33, 23);      // #262117
    private static readonly Color AmberBd = ColorHelper.FromArgb(255, 65, 53, 25);      // #413519

    private static readonly Color RedFg = ColorHelper.FromArgb(255, 252, 165, 165);     // #FCA5A5
    private static readonly Color RedBg = ColorHelper.FromArgb(255, 37, 27, 29);        // #251B1D
    private static readonly Color RedBd = ColorHelper.FromArgb(255, 65, 38, 40);        // #412628

    public VerdictBar()
    {
        this.InitializeComponent();
    }

    public void Bind(VerdictState verdict)
    {
        Color fg, bg, bd;
        string glyph;

        switch (verdict.Level)
        {
            case VerdictLevel.Ready:
                fg = GreenFg; bg = GreenBg; bd = GreenBd;
                glyph = "\uE73E"; // checkmark
                break;
            case VerdictLevel.ReadyWithWarnings:
                fg = AmberFg; bg = AmberBg; bd = AmberBd;
                glyph = "\uE7BA"; // warning
                break;
            case VerdictLevel.Blocked:
                fg = RedFg; bg = RedBg; bd = RedBd;
                glyph = "\uE711"; // stop
                break;
            default:
                fg = GreenFg; bg = GreenBg; bd = GreenBd;
                glyph = "\uE73E";
                break;
        }

        RootBorder.Background = new SolidColorBrush(bg);
        RootBorder.BorderBrush = new SolidColorBrush(bd);
        StatusIcon.Glyph = glyph;
        StatusIcon.Foreground = new SolidColorBrush(fg);
        SummaryText.Text = verdict.Summary;
        SummaryText.Foreground = new SolidColorBrush(fg);

        // Reasons as dot-separated text on second line
        if (verdict.Reasons.Count > 0)
        {
            var dimFg = ColorHelper.FromArgb(180, fg.R, fg.G, fg.B);
            ReasonsText.Text = string.Join(" \u00b7 ", verdict.Reasons.Take(3));
            ReasonsText.Foreground = new SolidColorBrush(dimFg);
            ReasonsText.Visibility = Visibility.Visible;
        }
        else
        {
            ReasonsText.Visibility = Visibility.Collapsed;
        }
    }
}
