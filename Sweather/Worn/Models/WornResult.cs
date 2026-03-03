namespace Worn.Models;

public record WornResult(
    string Tagline,
    string Headline,
    string Description,
    TierId Tier,
    ImmutableList<OutfitItem> Outfit,
    ImmutableList<FabricTag> FabricTags,
    ImmutableList<HourlyMoment> Hourly,
    ImmutableList<DayForecast> Daily,
    ImmutableList<Alert> Alerts,
    NudgeBar Nudge,
    ImmutableList<string> HeadlineRotation,
    string LocationName
);

public record OutfitItem(
    string Emoji,
    string Name,
    string Desc,
    NecessityLevel Necessity
)
{
    public int LayerIndex { get; init; }

    public string LayerLabel => $"L{LayerIndex}";

    public string NecessityLabel => Necessity switch
    {
        NecessityLevel.Survival => "survival",
        NecessityLevel.NonNegotiable => "non-negotiable",
        NecessityLevel.MustHave => "must-have",
        NecessityLevel.SmartPick => "smart pick",
        NecessityLevel.GoTo => "go-to",
        NecessityLevel.SafeBet => "safe bet",
        NecessityLevel.EasyPick => "easy pick",
        NecessityLevel.NiceTouch => "nice touch",
        NecessityLevel.Maybe => "maybe",
        _ => "maybe"
    };
}

public record FabricTag(string Label, string Color);

public record HourlyMoment(
    string Time,
    string DisplayTime,
    string Emoji,
    string Garment,
    string Vibe,
    TierId TierId,
    bool IsRaining,
    bool IsSnowing,
    bool IsNow,
    string SwatchColor,
    bool IsPast,
    bool HasTransition,
    string TransitionLabel,
    double SwayAngle
);

public record DayForecast(
    string Date,
    string DayAbbrev,
    string Headline,
    string Emoji,
    TierId TierId,
    ImmutableList<string> Fabrics,
    string SwatchColor,
    string Tip,
    string BackLabel,
    string BackFabricDetail,
    bool IsToday
);

public record Alert(string Icon, string Title, string Desc);

public record NudgeBar(string Icon, string Summary, string Tag);
