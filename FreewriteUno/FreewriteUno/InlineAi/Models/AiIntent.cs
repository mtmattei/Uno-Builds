namespace FreewriteUno.InlineAi.Models;

/// <summary>
/// What the user asked the AI to do. Drives both the request the service receives
/// and whether the resulting prose is an applicable rewrite (<see cref="AiIntentExtensions.ProducesRewrite"/>).
/// </summary>
public enum AiIntent
{
    /// <summary>Free-text question typed into the composer.</summary>
    Ask,
    Rewrite,
    Summarize,
    Explain,
    Shorten,
    ChangeTone,
}

public static class AiIntentExtensions
{
    /// <summary>
    /// Rewrite-style intents return prose that can be applied back over the selection.
    /// Informational intents (Summarize / Explain) do not.
    /// </summary>
    public static bool ProducesRewrite(this AiIntent intent) => intent switch
    {
        AiIntent.Rewrite => true,
        AiIntent.Shorten => true,
        AiIntent.ChangeTone => true,
        _ => false,
    };
}
