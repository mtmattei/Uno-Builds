using DiscordGitHubBridge.GitHub;

namespace DiscordGitHubBridge.Tests;

public class IssueFactoryTests
{
    private readonly IssueFactory _factory = new();

    [Fact]
    public void Body_contains_starter_footer_and_backlink()
    {
        var thread = TestData.Thread();

        var issue = _factory.Create(thread, ["from-discord"]);

        Assert.Equal("Button does not render on Android", issue.Title);
        Assert.StartsWith("Steps: open the page, tap the button. Nothing happens.", issue.Body);
        Assert.Contains("Reported by: `@matt`", issue.Body);
        Assert.Contains("Source: Discord", issue.Body);
        Assert.EndsWith("Discord thread: https://discord.com/channels/1182775715242967050/1550000000000000001", issue.Body);
        Assert.Equal(["from-discord"], issue.Labels);
    }

    [Fact]
    public void Missing_starter_message_uses_placeholder()
    {
        var issue = _factory.Create(TestData.Thread(content: null), []);

        Assert.StartsWith(IssueFactory.MissingStarterPlaceholder, issue.Body);
        Assert.Contains("Discord thread:", issue.Body);
    }

    [Fact]
    public void Attachments_are_listed()
    {
        var issue = _factory.Create(TestData.Thread(content: "", attachments: ["https://cdn.discordapp.com/a.png"]), []);

        Assert.Contains("**Attachments**\n- https://cdn.discordapp.com/a.png", issue.Body);
        Assert.StartsWith(IssueFactory.MissingStarterPlaceholder, issue.Body);
    }

    [Theory]
    [InlineData("ping @octocat please", "ping `@octocat` please")]
    [InlineData("@everyone look", "`@everyone` look")]
    [InlineData("mail me at dev@example.com", "mail me at dev@example.com")]
    [InlineData("already `@safe` here", "already `@safe` here")]
    [InlineData("@user-name and @Other_1", "`@user-name` and `@Other_1`")]
    public void Mentions_are_neutralized(string input, string expected)
    {
        Assert.Equal(expected, IssueFactory.NeutralizeMentions(input));
    }

    [Fact]
    public void Author_handle_is_wrapped_and_backticks_stripped()
    {
        var issue = _factory.Create(TestData.Thread(author: "we`ird"), []);

        Assert.Contains("Reported by: `@weird`", issue.Body);
    }

    [Fact]
    public void Long_title_is_capped()
    {
        var issue = _factory.Create(TestData.Thread(title: new string('x', 300)), []);

        Assert.Equal(IssueFactory.MaxTitleLength, issue.Title.Length);
    }

    [Fact]
    public void Empty_title_falls_back_to_thread_id()
    {
        var issue = _factory.Create(TestData.Thread(title: "   "), []);

        Assert.Equal("Discord thread 1550000000000000001", issue.Title);
    }

    [Fact]
    public void Long_body_is_truncated_but_footer_survives()
    {
        var issue = _factory.Create(TestData.Thread(content: new string('y', 70_000)), []);

        Assert.True(issue.Body.Length <= IssueFactory.MaxBodyLength);
        Assert.Contains("_[truncated]_", issue.Body);
        Assert.EndsWith("Discord thread: https://discord.com/channels/1182775715242967050/1550000000000000001", issue.Body);
    }
}
