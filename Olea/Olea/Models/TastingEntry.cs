namespace Olea.Models;

public partial record TastingEntry(
    string Id,
    string Name,
    string Origin,
    string Cultivar,
    string HarvestDate,
    string TastingDate,
    int Rating,
    ImmutableList<FlavorNote> Flavors,
    IntensityProfile Intensities,
    string Notes);
