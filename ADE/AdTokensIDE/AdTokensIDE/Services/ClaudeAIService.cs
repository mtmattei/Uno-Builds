using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace AdTokensIDE.Services;

public class ClaudeAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";

    public ClaudeAIService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
    }

    public async IAsyncEnumerable<string> StreamResponseAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            // Fallback to mock responses if no API key
            // Slower delay (120ms) to showcase ads longer
            foreach (var chunk in GetMockResponse(prompt))
            {
                await Task.Delay(120, ct);
                yield return chunk;
            }
            yield break;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        var body = new
        {
            model = "claude-sonnet-4-20250514",
            max_tokens = 1024,
            stream = true,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            yield return $"[Error: {response.StatusCode}]";
            yield break;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrEmpty(line) || !line.StartsWith("data: "))
                continue;

            var json = line[6..];
            if (json == "[DONE]")
                break;

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("type", out var typeEl) &&
                typeEl.GetString() == "content_block_delta" &&
                root.TryGetProperty("delta", out var delta) &&
                delta.TryGetProperty("text", out var text))
            {
                yield return text.GetString() ?? string.Empty;
            }
        }
    }

    public int EstimateTokenCount(string text)
    {
        // Rough estimate: ~4 chars per token
        return (int)Math.Ceiling(text.Length / 4.0);
    }

    private static IEnumerable<string> GetMockResponse(string prompt)
    {
        var response = prompt.ToLowerInvariant() switch
        {
            var p when p.Contains("mvvm") =>
                "Great question! In Uno Platform, you can implement MVVM with CommunityToolkit.Mvvm. " +
                "This is a powerful pattern that separates your UI from your business logic.\n\n" +
                "Here's a comprehensive guide to get you started:\n\n" +
                "**Step 1: Install the NuGet Package**\n" +
                "First, add CommunityToolkit.Mvvm to your project via NuGet Package Manager.\n\n" +
                "**Step 2: Create Your ViewModel**\n" +
                "Inherit your ViewModel class from ObservableObject. This base class provides " +
                "all the INotifyPropertyChanged implementation you need.\n\n" +
                "**Step 3: Define Observable Properties**\n" +
                "Use the [ObservableProperty] attribute on private fields. The source generator " +
                "will automatically create public properties with change notification.\n\n" +
                "**Step 4: Create Commands**\n" +
                "Use [RelayCommand] attribute on methods to generate ICommand properties. " +
                "This eliminates boilerplate code for command implementation.\n\n" +
                "**Step 5: Bind in XAML**\n" +
                "Use {x:Bind} for compile-time binding which provides better performance " +
                "and catches errors at build time rather than runtime.\n\n" +
                "Would you like me to show you a complete working example with code snippets?",

            var p when p.Contains("hello") || p.Contains("hi") =>
                "Hello there! I'm your friendly AI assistant, proudly powered by our generous sponsors. " +
                "Welcome to AdTokens IDE, where every response is brought to you by the finest " +
                "in developer-focused advertising!\n\n" +
                "I can help you with:\n" +
                "- Writing and debugging code\n" +
                "- Explaining programming concepts\n" +
                "- Architectural decisions\n" +
                "- Best practices and patterns\n" +
                "- And much more!\n\n" +
                "While I generate responses, you'll see rotating ads from our sponsors. " +
                "Think of it as a brief moment of zen while your tokens are being carefully crafted.\n\n" +
                "So, what would you like to explore today? I'm here to help!\n\n" +
                "(This response was sponsored by NullPointer Coffee - Debug your code at 3 AM!)",

            var p when p.Contains("uno") || p.Contains("platform") =>
                "Uno Platform is a fantastic choice for cross-platform development! " +
                "Let me give you a comprehensive overview.\n\n" +
                "**What is Uno Platform?**\n" +
                "Uno Platform enables you to build native mobile, desktop, and web applications " +
                "using C# and XAML from a single codebase. It implements the WinUI API, allowing " +
                "you to leverage your existing Windows development skills.\n\n" +
                "**Supported Platforms:**\n" +
                "- Windows (WinUI 3)\n" +
                "- iOS and Android\n" +
                "- macOS and Linux\n" +
                "- WebAssembly (runs in any modern browser)\n\n" +
                "**Key Features:**\n" +
                "1. Pixel-perfect UI across all platforms\n" +
                "2. Hot Reload for rapid development\n" +
                "3. Native performance with Skia rendering\n" +
                "4. Extensive control library including Material Design\n" +
                "5. Strong MVVM support with modern tooling\n\n" +
                "**Getting Started:**\n" +
                "Install the Uno Platform extension for Visual Studio or use the dotnet CLI " +
                "with 'dotnet new unoapp' to create your first project.\n\n" +
                "Would you like me to explain any specific aspect in more detail?",

            _ =>
                "Thank you for your question! Let me provide a thorough response.\n\n" +
                "I understand you're asking about: \"" + prompt + "\"\n\n" +
                "Here's my comprehensive analysis and recommendations:\n\n" +
                "**Understanding the Problem**\n" +
                "Before diving into solutions, it's important to fully understand what you're " +
                "trying to achieve. Let me break this down into manageable components.\n\n" +
                "**Recommended Approach**\n" +
                "1. First, clearly define your requirements and constraints. What are the " +
                "must-haves versus nice-to-haves?\n\n" +
                "2. Next, research existing solutions and patterns. There's often no need to " +
                "reinvent the wheel when established best practices exist.\n\n" +
                "3. Plan your implementation with a focus on maintainability. Write code that " +
                "your future self will thank you for.\n\n" +
                "4. Implement incrementally with testing at each step. This catches issues early " +
                "when they're easier to fix.\n\n" +
                "5. Finally, review and refactor. Clean code is happy code!\n\n" +
                "**Additional Resources**\n" +
                "I recommend checking the official documentation and community forums for " +
                "more specific guidance on your particular use case.\n\n" +
                "Would you like me to elaborate on any of these points or provide specific " +
                "code examples? I'm happy to dive deeper into any aspect that interests you!"
        };

        // Yield word by word for streaming effect
        foreach (var word in response.Split(' '))
        {
            yield return word + " ";
        }
    }
}
