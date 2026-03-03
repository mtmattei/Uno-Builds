namespace Olea.Models;

public record OleaRegion(
    string Flag,
    string Name,
    string Area,
    string Description,
    ImmutableList<string> Cultivars);
