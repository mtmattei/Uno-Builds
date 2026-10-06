using FreewriteUno.InlineAi.Models;
using Microsoft.UI.Xaml.Controls;

namespace FreewriteUno.InlineAi.Presentation.Controls;

/// <summary>
/// Single-purpose imperative seam (Architecture Brief Stop Check, Option 1): reads the active
/// <see cref="TextBox"/> selection — its text and source range — so the host can anchor the pill/card
/// and so Apply can target the original characters. Nothing else lives here. The selection's bounding
/// rect is measured separately by the host, since <see cref="TextBox"/> exposes no selection geometry.
/// <para>
/// The range is read against the live TextBox text, which in Freewrite includes the leading "\n\n"
/// padding prefix — so Apply (TextBox.Select on the same live text) lands on the right characters
/// without any prefix adjustment.
/// </para>
/// </summary>
public static class SelectionAnchor
{
    /// <summary>Minimum selection length (chars) before the pill is offered (Interaction Brief: under ~2 chars → no pill).</summary>
    public const int MinSelectionLength = 2;

    /// <summary>Reads the current selection. Returns false when there is no meaningful selection.</summary>
    public static bool TryRead(TextBox editor, out SelectionContext context)
    {
        context = SelectionContext.Empty;

        var start = editor.SelectionStart;
        var length = editor.SelectionLength;
        if (length < MinSelectionLength)
        {
            return false;
        }

        var text = editor.SelectedText;
        if (string.IsNullOrWhiteSpace(text) || text.Trim().Length < MinSelectionLength)
        {
            return false;
        }

        context = new SelectionContext(text, start, length);
        return true;
    }
}
