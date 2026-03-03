namespace AdTokensIDE.Services;

public interface IAIService
{
    IAsyncEnumerable<string> StreamResponseAsync(string prompt, CancellationToken ct = default);
    int EstimateTokenCount(string text);
}
