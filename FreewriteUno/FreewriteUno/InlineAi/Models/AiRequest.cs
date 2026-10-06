namespace FreewriteUno.InlineAi.Models;

/// <summary>
/// A single turn sent to <c>IAiService</c>. Carries the user's intent, the held selection,
/// the optional free-text prompt, and a tone when the intent is <see cref="AiIntent.ChangeTone"/>.
/// </summary>
public record AiRequest(
    AiIntent Intent,
    SelectionContext Selection,
    string? Prompt = null,
    ToneOption? Tone = null);
