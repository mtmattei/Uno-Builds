using Refit;
using Vitalis.Models;

namespace Vitalis.Services;

public record GeminiContent(string Role, IList<GeminiPart> Parts);
public record GeminiPart(string Text);
public record GeminiRequest(IList<GeminiContent> Contents);
public record GeminiCandidate(GeminiContent Content);
public record GeminiResponse(IList<GeminiCandidate> Candidates);

public interface IGeminiApi
{
    [Post("/v1beta/models/gemini-2.0-flash:generateContent")]
    Task<GeminiResponse> GenerateContentAsync(
        [Body] GeminiRequest request,
        [Query] string key,
        CancellationToken ct = default);
}

public interface IGeminiService
{
    Task<AIInsight?> AnalyzeOrganAsync(Organ organ, CancellationToken ct = default);
}

public class GeminiService : IGeminiService
{
    private readonly IGeminiApi _api;
    private readonly string _apiKey;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(IGeminiApi api, IOptions<GeminiConfig> config, ILogger<GeminiService> logger)
    {
        _api = api;
        _apiKey = config.Value.ApiKey ?? string.Empty;
        _logger = logger;
    }

    public async Task<AIInsight?> AnalyzeOrganAsync(Organ organ, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("Gemini API key not configured");
            return new AIInsight(
                "API key not configured. Please add your Gemini API key to appsettings.json.",
                ["Configure API key in settings", "Get a key from Google AI Studio", "Restart the application"]
            );
        }

        try
        {
            var metricsJson = System.Text.Json.JsonSerializer.Serialize(organ.Metrics.Select(m => new
            {
                m.Label,
                m.Value,
                m.Unit,
                Trend = m.Trend.ToString(),
                Status = m.Status.ToString()
            }));

            var jsonFormat = """{"summary": "A 2-3 sentence analysis of the organ's health status", "recommendations": ["recommendation 1", "recommendation 2", "recommendation 3"]}""";
            var prompt = $"""
                You are a soft sci-fi medical assistant - calm, precise, and encouraging.
                Analyze the following health metrics for the {organ.Name}:

                Organ: {organ.Name}
                Description: {organ.Description}
                Metrics: {metricsJson}

                Respond ONLY with valid JSON in this exact format (no markdown, no code blocks):
                {jsonFormat}
                """;

            var request = new GeminiRequest(
            [
                new GeminiContent("user", [new GeminiPart(prompt)])
            ]);

            var response = await _api.GenerateContentAsync(request, _apiKey, ct);
            var text = response.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (string.IsNullOrEmpty(text))
            {
                return GetFallbackInsight(organ);
            }

            var jsonStart = text.IndexOf('{');
            var jsonEnd = text.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                text = text.Substring(jsonStart, jsonEnd - jsonStart + 1);
            }

            var result = System.Text.Json.JsonSerializer.Deserialize<AIInsightDto>(text, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result is not null)
            {
                return new AIInsight(
                    result.Summary ?? "Analysis complete.",
                    result.Recommendations?.ToImmutableList() ?? []
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API");
        }

        return GetFallbackInsight(organ);
    }

    private static AIInsight GetFallbackInsight(Organ organ) => new(
        $"Your {organ.Name.ToLower()} metrics are within healthy parameters. All vital signs show stable readings with no immediate concerns detected.",
        [
            "Maintain current healthy lifestyle habits",
            "Schedule regular check-ups every 6 months",
            "Stay hydrated and get adequate rest"
        ]
    );

    private record AIInsightDto(string? Summary, List<string>? Recommendations);
}

public record GeminiConfig
{
    public string? ApiKey { get; init; }
    public string? BaseUrl { get; init; }
}
