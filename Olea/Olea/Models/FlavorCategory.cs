namespace Olea.Models;

public record FlavorCategory(string Name, string Color, ImmutableList<FlavorNote> Children);
