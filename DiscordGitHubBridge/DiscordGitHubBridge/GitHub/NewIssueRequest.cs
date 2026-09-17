namespace DiscordGitHubBridge.GitHub;

public sealed record NewIssueRequest(string Title, string Body, IReadOnlyList<string> Labels);

public sealed record CreatedIssue(long Number, string HtmlUrl);
