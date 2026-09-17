using DiscordGitHubBridge.Bridge;

namespace DiscordGitHubBridge.Tests;

public class ThreadRuleEvaluatorTests
{
    private static ThreadRuleEvaluator Evaluator() => new(TestData.Options(TestData.DefaultForum()));

    [Fact]
    public void Disallowed_forum_is_not_configured()
    {
        var result = Evaluator().Evaluate(TestData.Thread(parent: TestData.OtherForum));

        Assert.Equal(RuleDecision.NotConfigured, result.Decision);
        Assert.Empty(result.Labels);
    }

    [Fact]
    public void Allowed_forum_with_trigger_tag_qualifies()
    {
        var result = Evaluator().Evaluate(TestData.Thread());

        Assert.Equal(RuleDecision.Qualifies, result.Decision);
        Assert.Equal(["from-discord"], result.Labels);
    }

    [Fact]
    public void Missing_trigger_tag_is_ignored()
    {
        var result = Evaluator().Evaluate(TestData.Thread(tags: ["Bug"]));

        Assert.Equal(RuleDecision.Ignored, result.Decision);
        Assert.Contains("RequiredTags", result.Reason);
    }

    [Fact]
    public void Ignored_tag_wins_over_trigger_tag()
    {
        var result = Evaluator().Evaluate(TestData.Thread(tags: ["Track on GitHub", "Duplicate"]));

        Assert.Equal(RuleDecision.Ignored, result.Decision);
        Assert.Contains("Duplicate", result.Reason);
    }

    [Fact]
    public void Tag_matching_is_case_insensitive_and_trims()
    {
        var result = Evaluator().Evaluate(TestData.Thread(tags: ["  track ON github "]));

        Assert.Equal(RuleDecision.Qualifies, result.Decision);
    }

    [Fact]
    public void Multiple_tags_map_to_labels_in_stable_order_without_duplicates()
    {
        var forum = TestData.DefaultForum();
        forum.DefaultLabels = ["from-discord", "bug"];
        var evaluator = new ThreadRuleEvaluator(TestData.Options(forum));

        var result = evaluator.Evaluate(TestData.Thread(tags: ["Track on GitHub", "Feature Request", "BUG", "Unmapped"]));

        Assert.Equal(RuleDecision.Qualifies, result.Decision);
        Assert.Equal(["from-discord", "bug", "enhancement"], result.Labels);
    }

    [Fact]
    public void Empty_required_tags_means_any_post_qualifies()
    {
        var forum = TestData.DefaultForum();
        forum.RequiredTags = [];
        var evaluator = new ThreadRuleEvaluator(TestData.Options(forum));

        var result = evaluator.Evaluate(TestData.Thread(tags: []));

        Assert.Equal(RuleDecision.Qualifies, result.Decision);
        Assert.Equal(["from-discord"], result.Labels);
    }

    [Theory]
    [InlineData(new string[0], new[] { "Track on GitHub" }, true)]
    [InlineData(new[] { "Bug" }, new[] { "Bug", "track on github" }, true)]
    [InlineData(new[] { "Track on GitHub" }, new[] { "Track on GitHub", "Bug" }, false)]
    [InlineData(new[] { "Track on GitHub" }, new string[0], false)]
    [InlineData(new string[0], new[] { "Bug" }, false)]
    public void Trigger_tag_added_detects_only_new_trigger(string[] before, string[] after, bool expected)
    {
        Assert.Equal(expected, Evaluator().TriggerTagWasAdded(TestData.Forum, before, after));
    }

    [Fact]
    public void Trigger_tag_added_is_false_for_unconfigured_forum()
    {
        Assert.False(Evaluator().TriggerTagWasAdded(TestData.OtherForum, [], ["Track on GitHub"]));
    }
}
