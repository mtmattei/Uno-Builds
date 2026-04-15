using System.Text;
using System.Text.RegularExpressions;
using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record ChatModel
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatModel> _logger;

    public ChatModel(IChatService chatService, ILogger<ChatModel> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public IState<bool> IsConfigured => State.Value(this, () => _chatService.IsConfigured);
    public IState<string> InputText => State.Value(this, () => "");
    public IState<bool> IsStreaming => State.Value(this, () => false);
    public IState<string> ApiKeyInput => State.Value(this, () => "");

    public IListState<ChatMessage> Messages => ListState.Value(this, () =>
        ImmutableList.Create(new ChatMessage(
            Role: ChatRole.Assistant,
            Text: "Hey. I'm your ClaudeDash assistant. Ask me anything about your dev environment, sessions, projects, or configs.")));

    public async ValueTask SendMessage(string text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var userMsg = new ChatMessage(Role: ChatRole.User, Text: text);
        await Messages.AddAsync(userMsg, ct);

        await IsStreaming.Set(true, ct);

        try
        {
            var allMessages = (await Messages) ?? ImmutableList<ChatMessage>.Empty;
            var sb = new StringBuilder();

            await foreach (var chunk in _chatService.StreamResponseAsync(allMessages.ToList(), ct))
            {
                sb.Append(chunk);
            }

            var response = sb.ToString();
            var cards = ParseContentCards(response);
            var assistantMsg = new ChatMessage(Role: ChatRole.Assistant, Text: response, Cards: cards);
            await Messages.AddAsync(assistantMsg, ct);
        }
        catch (OperationCanceledException)
        {
            await Messages.AddAsync(new ChatMessage(Role: ChatRole.Assistant, Text: "[Cancelled]"), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Chat stream error");
            await Messages.AddAsync(new ChatMessage(Role: ChatRole.Assistant, Text: $"Error: {ex.Message}"), ct);
        }
        finally
        {
            await IsStreaming.Set(false, ct);
        }
    }

    public async ValueTask SetApiKey(string key, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        _chatService.SetApiKey(key);
        await IsConfigured.Set(_chatService.IsConfigured, ct);
        await ApiKeyInput.Set("", ct);
    }

    public async ValueTask ClearHistory(CancellationToken ct)
    {
        // Clear and re-add welcome
        await Messages.UpdateAsync(_ => ImmutableList.Create(
            new ChatMessage(Role: ChatRole.Assistant, Text: "Conversation cleared. What would you like to know?")), ct);
    }

    private static ImmutableList<ContentCard> ParseContentCards(string text)
    {
        var builder = ImmutableList.CreateBuilder<ContentCard>();

        var codeBlockPattern = new Regex(@"```(\w*)\n([\s\S]*?)```", RegexOptions.Multiline);
        foreach (Match match in codeBlockPattern.Matches(text))
        {
            builder.Add(new ContentCard(
                Type: ContentCardType.CodeBlock,
                Language: string.IsNullOrEmpty(match.Groups[1].Value) ? null : match.Groups[1].Value,
                Body: match.Groups[2].Value.TrimEnd()));
        }

        var filePattern = new Regex(@"`((?:[~/]|[A-Z]:\\)[^`]+\.\w+)`");
        foreach (Match match in filePattern.Matches(text))
        {
            var path = match.Groups[1].Value;
            builder.Add(new ContentCard(
                Type: ContentCardType.FileReference,
                Title: System.IO.Path.GetFileName(path),
                Body: path,
                FilePath: path));
        }

        return builder.ToImmutable();
    }
}
