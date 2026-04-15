namespace ClaudeDash.Services;

public interface IChatService
{
    /// <summary>
    /// Send a message and stream back the response text chunk by chunk.
    /// </summary>
    IAsyncEnumerable<string> StreamResponseAsync(
        List<ChatMessage> conversationHistory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Whether the API key is configured.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Set the API key at runtime.
    /// </summary>
    void SetApiKey(string apiKey);
}
