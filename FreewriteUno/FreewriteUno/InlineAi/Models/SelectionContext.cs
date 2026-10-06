namespace FreewriteUno.InlineAi.Models;

/// <summary>
/// The highlighted text plus the source range it came from. Set when the chat opens
/// and used by Apply to write a rewrite back over the original characters.
/// <para>
/// In Freewrite the editor body carries a leading "\n\n" padding prefix;
/// <see cref="Start"/>/<see cref="Length"/> are stored against the live TextBox text
/// (prefix included) so Apply via TextBox.Select lands on the right characters.
/// </para>
/// </summary>
public record SelectionContext(string Text, int Start, int Length)
{
    public static SelectionContext Empty { get; } = new(string.Empty, 0, 0);

    public bool IsEmpty => Length <= 0 || string.IsNullOrEmpty(Text);
}
