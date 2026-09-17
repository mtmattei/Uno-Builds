using System.Text;
using System.Text.RegularExpressions;
using DiscordGitHubBridge.Bridge;

namespace DiscordGitHubBridge.GitHub;

/// <summary>Builds the GitHub issue title and body from a thread snapshot. Pure.</summary>
public sealed partial class IssueFactory
{
    public const int MaxTitleLength = 256;
    public const int MaxBodyLength = 60_000;
    public const string MissingStarterPlaceholder = "_(Starter message unavailable at creation time.)_";
    private const string TruncationMarker = "\n\n_[truncated]_";

    public NewIssueRequest Create(ForumThreadSnapshot thread, IReadOnlyList<string> labels)
    {
        var title = thread.Title.Trim();
        if (title.Length == 0)
        {
            title = $"Discord thread {thread.ThreadId}";
        }
        if (title.Length > MaxTitleLength)
        {
            title = title[..MaxTitleLength];
        }

        var body = new StringBuilder();

        var content = thread.StarterContent?.Trim();
        body.Append(string.IsNullOrEmpty(content) ? MissingStarterPlaceholder : NeutralizeMentions(content));

        if (thread.AttachmentUrls.Count > 0)
        {
            body.Append("\n\n**Attachments**\n");
            foreach (var url in thread.AttachmentUrls)
            {
                body.Append("- ").Append(url).Append('\n');
            }
        }

        body.Append("\n\n---\n");
        body.Append("Reported by: `@").Append(thread.AuthorDisplayName.Replace("`", string.Empty)).Append("`\n");
        body.Append("Source: Discord\n");
        body.Append("Discord thread: ").Append(thread.ThreadUrl);

        var text = body.ToString();
        if (text.Length > MaxBodyLength)
        {
            // Keep the footer intact: trim the starter content instead of the tail.
            var footerStart = text.LastIndexOf("\n\n---\n", StringComparison.Ordinal);
            var footer = text[footerStart..];
            var head = text[..(MaxBodyLength - footer.Length - TruncationMarker.Length)];
            text = head + TruncationMarker + footer;
        }

        return new NewIssueRequest(title, text, labels);
    }

    /// <summary>Wraps @handles in backticks so GitHub does not notify unrelated users who share a name.</summary>
    internal static string NeutralizeMentions(string content) =>
        MentionPattern().Replace(content, static m => $"`{m.Value}`");

    [GeneratedRegex(@"(?<![\w`])@[A-Za-z0-9][\w-]*")]
    private static partial Regex MentionPattern();
}
