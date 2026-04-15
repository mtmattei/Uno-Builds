using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using Path = System.IO.Path;

namespace ClaudeDash.Services;

public class ChatService : IChatService
{
    private readonly IClaudeConfigService _configService;
    private readonly IBackgroundScannerService _scanner;
    private readonly ILogger<ChatService> _logger;
    private readonly HttpClient _httpClient;

    private string? _apiKey;

    private const string ApiUrl = "https://api.anthropic.com/v1/messages";
    private const string Model = "claude-sonnet-4-6";
    private const string ApiVersion = "2023-06-01";

    public bool IsConfigured => !string.IsNullOrEmpty(_apiKey);

    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claudedash");
    private static readonly string ConfigFile = Path.Combine(ConfigDir, "config.json");

    public ChatService(
        IClaudeConfigService configService,
        IBackgroundScannerService scanner,
        ILogger<ChatService> logger)
    {
        _configService = configService;
        _scanner = scanner;
        _logger = logger;
        _httpClient = new HttpClient();

        // Load API key: env var takes priority, then persisted config
        _apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        if (string.IsNullOrEmpty(_apiKey))
            _apiKey = LoadPersistedApiKey();
    }

    public void SetApiKey(string apiKey)
    {
        _apiKey = apiKey;
        PersistApiKey(apiKey);
    }

    private static string? LoadPersistedApiKey()
    {
        try
        {
            if (!File.Exists(ConfigFile)) return null;
            var json = File.ReadAllText(ConfigFile);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("apiKey", out var keyProp))
                return keyProp.GetString();
        }
        catch { }
        return null;
    }

    private static void PersistApiKey(string apiKey)
    {
        try
        {
            Directory.CreateDirectory(ConfigDir);
            var json = JsonSerializer.Serialize(new { apiKey });
            File.WriteAllText(ConfigFile, json);
        }
        catch { }
    }

    public async IAsyncEnumerable<string> StreamResponseAsync(
        List<ChatMessage> conversationHistory,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            yield return "API key not configured. Set the ANTHROPIC_API_KEY environment variable or enter your key in settings.";
            yield break;
        }

        var systemPrompt = await BuildSystemPromptAsync();
        var messages = BuildMessageArray(conversationHistory);

        var requestBody = new
        {
            model = Model,
            max_tokens = 4096,
            stream = true,
            system = systemPrompt,
            messages
        };

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });

        var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("anthropic-version", ApiVersion);

        HttpResponseMessage? response = null;
        try
        {
            response = await _httpClient.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Claude API error {Status}: {Body}",
                    response.StatusCode, errorBody);
                yield return $"API error ({response.StatusCode}): {errorBody}";
                yield break;
            }

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (line == null) break;

                if (!line.StartsWith("data: ")) continue;
                var data = line["data: ".Length..];

                if (data == "[DONE]") break;

                var chunk = ParseStreamChunk(data);
                if (chunk != null)
                    yield return chunk;
            }
        }
        finally
        {
            response?.Dispose();
        }
    }

    private static string? ParseStreamChunk(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var type = root.GetProperty("type").GetString();

            if (type == "content_block_delta")
            {
                var delta = root.GetProperty("delta");
                if (delta.TryGetProperty("text", out var textEl))
                    return textEl.GetString();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static object[] BuildMessageArray(List<ChatMessage> history)
    {
        var messages = new List<object>();
        foreach (var msg in history)
        {
            // Skip system messages, streaming placeholders, and empty content
            if (msg.Role == ChatRole.System) continue;
            if (msg.IsStreaming) continue;
            if (string.IsNullOrWhiteSpace(msg.Text)) continue;

            messages.Add(new
            {
                role = msg.Role == ChatRole.User ? "user" : "assistant",
                content = msg.Text
            });
        }

        // Claude API requires first message to be "user" - trim leading assistant messages
        while (messages.Count > 0)
        {
            var first = (dynamic)messages[0];
            if ((string)first.role == "user") break;
            messages.RemoveAt(0);
        }

        return messages.ToArray();
    }

    private async Task<string> BuildSystemPromptAsync()
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are ClaudeDash Assistant, an AI helper embedded in the ClaudeDash developer dashboard.");
        sb.AppendLine("ClaudeDash is a dev environment dashboard for Uno Platform Studio users.");
        sb.AppendLine();
        sb.AppendLine("Your role:");
        sb.AppendLine("- Answer questions about the user's development environment, Claude Code sessions, projects, and configuration");
        sb.AppendLine("- Provide actionable advice for fixing issues");
        sb.AppendLine("- Help interpret session data, cost analysis, and hygiene reports");
        sb.AppendLine("- Be concise and technical. Use monospace formatting for file paths and commands.");
        sb.AppendLine();
        sb.AppendLine("When referencing data, use markdown formatting:");
        sb.AppendLine("- Use ```language for code blocks");
        sb.AppendLine("- Use **bold** for emphasis");
        sb.AppendLine("- Use bullet lists for multiple items");
        sb.AppendLine();

        // Inject live environment context
        var snapshot = _scanner.LatestSnapshot;
        if (snapshot != null)
        {
            sb.AppendLine("## Current Environment State");
            sb.AppendLine($"- Sessions: {snapshot.SessionCount}");
            sb.AppendLine($"- Projects: {snapshot.ProjectCount}");
            sb.AppendLine($"- MCP Servers: {snapshot.McpServerCount}");
            sb.AppendLine($"- Skills: {snapshot.SkillCount}");
            sb.AppendLine($"- Agents: {snapshot.AgentCount}");
            sb.AppendLine($"- Memory Files: {snapshot.MemoryFileCount}");
            sb.AppendLine($"- Hooks: {snapshot.HookCount}");
            sb.AppendLine($"- Last scan: {snapshot.LastScanTime:u} ({snapshot.ScanDuration.TotalMilliseconds:F0}ms)");
            sb.AppendLine();
        }

        // Add recent sessions context
        try
        {
            var sessions = await _configService.GetRecentSessionsAsync(5);
            if (sessions.Count > 0)
            {
                sb.AppendLine("## Recent Sessions");
                foreach (var s in sessions)
                {
                    var msg = s.FirstUserMessage.Length > 80
                        ? s.FirstUserMessage[..80] + "..."
                        : s.FirstUserMessage;
                    sb.AppendLine($"- [{s.ShortId}] {s.Model} | {s.RepoName} | \"{msg}\"");
                }
                sb.AppendLine();
            }
        }
        catch { }

        // Add MCP servers
        try
        {
            var servers = await _configService.GetMcpServersAsync();
            if (servers.Count > 0)
            {
                sb.AppendLine("## MCP Servers");
                foreach (var s in servers)
                    sb.AppendLine($"- {s.Name}: {s.Command}");
                sb.AppendLine();
            }
        }
        catch { }

        return sb.ToString();
    }
}
