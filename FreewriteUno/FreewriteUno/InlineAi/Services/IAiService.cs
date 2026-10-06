using FreewriteUno.InlineAi.Models;

namespace FreewriteUno.InlineAi.Services;

/// <summary>
/// Backend boundary for the inline chat. Streams the assistant response token-by-token so the
/// view can fill text in as it arrives. The prototype is backed by <see cref="FakeAiService"/>;
/// the real implementation swaps in behind the same contract (SSE / chunked HTTP adapter).
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Streams the assistant prose for a request. Each yielded string is a chunk to append to the
    /// in-flight message. Honors cancellation between chunks. May throw to signal a failed turn.
    /// </summary>
    IAsyncEnumerable<string> StreamAsync(AiRequest request, CancellationToken cancellationToken = default);
}
