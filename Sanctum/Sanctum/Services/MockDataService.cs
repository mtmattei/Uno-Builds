using Sanctum.Models;

namespace Sanctum.Services;

/// <summary>
/// Provides mock data for development and fallback scenarios
/// </summary>
public class MockDataService : IMockDataService
{
    public SanityPlan GenerateSanityPlan(UserPreferences preferences)
    {
        return new SanityPlan
        {
            Manifesto = "Your digital sanctuary awaits. We have crafted a personalized protocol to help you reclaim your attention and find peace in the noise.",
            Rules =
            [
                "Social feeds will be batched and delivered twice daily at 9 AM and 6 PM",
                "Email notifications will be consolidated into a morning digest",
                "Focus sessions will automatically enable Do Not Disturb mode"
            ]
        };
    }

    public string GenerateSmartSynthesis(AppMode mode)
    {
        return mode switch
        {
            AppMode.Focus => "You have 3 batched notifications. Calendar clear until 4 PM.",
            AppMode.Recover => "Take a breath. No urgent items require your attention right now.",
            _ => "5 items filtered today. Your next focus window is at 2 PM."
        };
    }

    public List<FeedItem> GenerateSignalFeed()
    {
        return
        [
            new FeedItem
            {
                Source = "THE VERGE",
                Tag = "Technology",
                Title = "The future of calm computing is here",
                Summary = "A new wave of apps prioritizes mental wellness over engagement metrics, signaling a shift in how we think about digital tools."
            },
            new FeedItem
            {
                Source = "WIRED",
                Tag = "Productivity",
                Title = "Why single-tasking is making a comeback",
                Summary = "Research shows that focused work sessions outperform multitasking by a factor of three in knowledge work."
            },
            new FeedItem
            {
                Source = "FAST COMPANY",
                Tag = "Wellness",
                Title = "The anti-notification movement gains momentum",
                Summary = "Leading tech companies are rethinking their approach to user attention and mental health."
            }
        ];
    }

    public List<DigestItem> GenerateDailyDigest()
    {
        return
        [
            new DigestItem
            {
                Icon = "\uE715",
                Title = "Team standup in 30 minutes",
                Timestamp = "9:30 AM",
                Summary = "Daily sync with the product team",
                IsUrgent = true
            },
            new DigestItem
            {
                Icon = "\uE119",
                Title = "3 emails from colleagues",
                Timestamp = "Earlier today",
                Summary = "Project updates and feedback requests",
                IsUrgent = false
            },
            new DigestItem
            {
                Icon = "\uE8BD",
                Title = "Slack: 12 messages batched",
                Timestamp = "Last 2 hours",
                Summary = "General discussion in team channels",
                IsUrgent = false
            }
        ];
    }

    public List<GoalOption> GetGoalOptions()
    {
        return
        [
            new GoalOption
            {
                Id = "deep-focus",
                Title = "Deep Focus",
                Description = "Eliminate distractions during work sessions",
                Icon = "\uE8BE"
            },
            new GoalOption
            {
                Id = "restorative-sleep",
                Title = "Restorative Sleep",
                Description = "Wind down with evening notification limits",
                Icon = "\uEC46"
            },
            new GoalOption
            {
                Id = "digital-calm",
                Title = "Digital Calm",
                Description = "Reduce anxiety from constant connectivity",
                Icon = "\uE9E9"
            }
        ];
    }

    public List<SourceOption> GetSourceOptions()
    {
        return
        [
            new SourceOption
            {
                Id = "social",
                Title = "Social Feeds",
                Icon = "\uE902"
            },
            new SourceOption
            {
                Id = "email",
                Title = "Email",
                Icon = "\uE715"
            },
            new SourceOption
            {
                Id = "news",
                Title = "News",
                Icon = "\uE12A"
            },
            new SourceOption
            {
                Id = "chat",
                Title = "Instant Chat",
                Icon = "\uE8BD"
            }
        ];
    }

    public List<SourceControlItem> GetSourceControlItems()
    {
        return
        [
            new SourceControlItem { Icon = "\uE902", Label = "Social Media", Status = SourceStatus.Batched },
            new SourceControlItem { Icon = "\uE715", Label = "Email", Status = SourceStatus.Allowed },
            new SourceControlItem { Icon = "\uE12A", Label = "News Apps", Status = SourceStatus.Muted },
            new SourceControlItem { Icon = "\uE8BD", Label = "Messaging", Status = SourceStatus.Batched }
        ];
    }
}

public interface IMockDataService
{
    SanityPlan GenerateSanityPlan(UserPreferences preferences);
    string GenerateSmartSynthesis(AppMode mode);
    List<FeedItem> GenerateSignalFeed();
    List<DigestItem> GenerateDailyDigest();
    List<GoalOption> GetGoalOptions();
    List<SourceOption> GetSourceOptions();
    List<SourceControlItem> GetSourceControlItems();
}
