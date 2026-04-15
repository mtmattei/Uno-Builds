namespace ClaudeDash.Models;

public enum VerdictLevel { Ready, ReadyWithWarnings, Blocked }

public record VerdictState(
    VerdictLevel Level = VerdictLevel.Ready,
    string Summary = "",
    ImmutableList<string>? Reasons = null)
{
    public ImmutableList<string> Reasons { get; init; } = Reasons ?? ImmutableList<string>.Empty;
}
