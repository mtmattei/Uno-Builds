using System.Runtime.CompilerServices;
using System.Text;
using FreewriteUno.InlineAi.Models;

namespace FreewriteUno.InlineAi.Services;

/// <summary>
/// Deterministic in-memory <see cref="IAiService"/> for the prototype and for manual demoing.
/// Produces a canned response per <see cref="AiIntent"/> derived from the selection, then streams it
/// word-by-word with a small delay to mimic a real token stream.
/// <para>
/// Demo failure path: a prompt containing "fail" throws, exercising the inline error + Retry flow.
/// </para>
/// </summary>
public sealed class FakeAiService : IAiService
{
    // Per-chunk delay tuned so streaming reads as deliberate without feeling slow. Settable for tests.
    private readonly TimeSpan _chunkDelay;

    public FakeAiService() : this(TimeSpan.FromMilliseconds(28)) { }

    public FakeAiService(TimeSpan chunkDelay) => _chunkDelay = chunkDelay;

    public async IAsyncEnumerable<string> StreamAsync(
        AiRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (LooksLikeFailure(request))
        {
            // Small head-start so the pending indicator is visible before the turn fails.
            await Task.Delay(_chunkDelay * 4, cancellationToken);
            throw new InvalidOperationException("Simulated service failure.");
        }

        var response = Compose(request);

        foreach (var chunk in Tokenize(response))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(_chunkDelay, cancellationToken);
            yield return chunk;
        }
    }

    private static bool LooksLikeFailure(AiRequest request) =>
        request.Prompt?.Contains("fail", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>Builds the full response text for a request. Public-shaped logic kept pure for predictability.</summary>
    private static string Compose(AiRequest request)
    {
        var source = request.Selection.Text.Trim();

        return request.Intent switch
        {
            AiIntent.Rewrite => Polish(source),
            AiIntent.Shorten => Shorten(source),
            AiIntent.ChangeTone => Retone(source, request.Tone ?? ToneOption.Friendly),
            AiIntent.Summarize => $"In short: {FirstWords(source, 12)}.",
            AiIntent.Explain => $"This passage is saying that {Decapitalize(FirstWords(source, 16))} — in other words, it sets up the main idea before the detail that follows.",
            AiIntent.Ask => string.IsNullOrWhiteSpace(request.Prompt)
                ? $"Here's a take on the selection: {FirstWords(source, 14)} reads clearly, though you could tighten the opening."
                : $"Good question. Regarding \"{Truncate(request.Prompt!, 48)}\": the selected passage supports it well, and you could lead with the key point for more impact.",
            _ => source,
        };
    }

    private static string Polish(string s)
    {
        var collapsed = CollapseSpaces(s);
        var capitalized = Capitalize(collapsed);
        return EnsureTerminalPunctuation(capitalized);
    }

    private static string Shorten(string s)
    {
        var firstSentence = s.Split('.', '!', '?')[0].Trim();
        var clipped = FirstWords(string.IsNullOrEmpty(firstSentence) ? s : firstSentence, 10);
        return EnsureTerminalPunctuation(Capitalize(clipped));
    }

    private static string Retone(string s, ToneOption tone)
    {
        var core = CollapseSpaces(s).TrimEnd('.', '!', '?', ' ');
        var toned = tone switch
        {
            ToneOption.Casual => $"Honestly, {Decapitalize(core)} — and that's the gist of it",
            ToneOption.Formal => $"It is worth noting that {Decapitalize(core)}",
            ToneOption.Confident => $"{Capitalize(core)}. No question about it",
            ToneOption.Friendly => $"Hey — {Decapitalize(core)}, which is great to keep in mind",
            _ => core,
        };
        return EnsureTerminalPunctuation(toned);
    }

    // ----- small deterministic string helpers -----

    private static IEnumerable<string> Tokenize(string text)
    {
        // Yield word + trailing space as one chunk so the accumulated string stays well-formed.
        var sb = new StringBuilder();
        foreach (var ch in text)
        {
            sb.Append(ch);
            if (ch == ' ')
            {
                yield return sb.ToString();
                sb.Clear();
            }
        }
        if (sb.Length > 0)
        {
            yield return sb.ToString();
        }
    }

    private static string CollapseSpaces(string s) =>
        string.Join(' ', s.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private static string FirstWords(string s, int count)
    {
        var words = CollapseSpaces(s).Split(' ');
        return string.Join(' ', words.Take(count));
    }

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];

    private static string Decapitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s[1..];

    private static string EnsureTerminalPunctuation(string s) =>
        string.IsNullOrEmpty(s) || ".!?".Contains(s[^1]) ? s : s + ".";

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max].TrimEnd() + "…";
}
