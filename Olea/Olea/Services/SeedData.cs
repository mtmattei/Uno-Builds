namespace Olea.Services;

public static class SeedData
{
    public static ImmutableList<TastingEntry> Entries { get; } = ImmutableList.Create(
        new TastingEntry(
            Id: Guid.NewGuid().ToString(),
            Name: "Castello di Ama",
            Origin: "Tuscany, Italy",
            Cultivar: "Frantoio",
            HarvestDate: "2024-10",
            TastingDate: "2025-01-15",
            Rating: 5,
            Flavors: ImmutableList.Create(
                new FlavorNote("Green Apple", "#7BA85A"),
                new FlavorNote("Black Pepper", "#A04A3A"),
                new FlavorNote("Basil", "#4E7B5E")),
            Intensities: new IntensityProfile(7, 5, 8),
            Notes: "Incredible complexity. Opens with bright green apple, transitions to fresh basil, finishes with a long peppery burn."),
        new TastingEntry(
            Id: Guid.NewGuid().ToString(),
            Name: "Laconiko Reserve",
            Origin: "Laconia, Greece",
            Cultivar: "Koroneiki",
            HarvestDate: "2024-11",
            TastingDate: "2025-01-22",
            Rating: 4,
            Flavors: ImmutableList.Create(
                new FlavorNote("Citrus", "#7DAD4E"),
                new FlavorNote("Almond", "#B08B6B"),
                new FlavorNote("Chamomile", "#B69AC8")),
            Intensities: new IntensityProfile(8, 3, 4),
            Notes: "Silky smooth with bright citrus on the nose. Delicate floral finish. Perfect for drizzling over fish."),
        new TastingEntry(
            Id: Guid.NewGuid().ToString(),
            Name: "Oro del Desierto",
            Origin: "Almeria, Spain",
            Cultivar: "Picual",
            HarvestDate: "2024-10",
            TastingDate: "2025-02-03",
            Rating: 4,
            Flavors: ImmutableList.Create(
                new FlavorNote("Tomato Leaf", "#5E8B3E"),
                new FlavorNote("Arugula", "#C86A5A")),
            Intensities: new IntensityProfile(6, 6, 7),
            Notes: "Robust and assertive. Tomato leaf aroma dominates, balanced by a pleasant bitter green olive character."),
        new TastingEntry(
            Id: Guid.NewGuid().ToString(),
            Name: "California Olive Ranch",
            Origin: "California, USA",
            Cultivar: "Arbequina",
            HarvestDate: "2024-11",
            TastingDate: "2025-02-10",
            Rating: 3,
            Flavors: ImmutableList.Create(
                new FlavorNote("Banana", "#8CB56A"),
                new FlavorNote("Cream", "#DEAF66")),
            Intensities: new IntensityProfile(7, 2, 2),
            Notes: "Mild and approachable. Ripe banana notes with a creamy, buttery mouthfeel. Good everyday oil."));

    public static ImmutableList<FlavorCategory> FlavorWheel { get; } = ImmutableList.Create(
        new FlavorCategory("Fruity", "#6B9B4A", ImmutableList.Create(
            new FlavorNote("Green Apple", "#7BA85A"),
            new FlavorNote("Tomato Leaf", "#5E8B3E"),
            new FlavorNote("Banana", "#8CB56A"),
            new FlavorNote("Citrus", "#7DAD4E"))),
        new FlavorCategory("Floral", "#9B7BB5", ImmutableList.Create(
            new FlavorNote("Artichoke", "#A88BC2"),
            new FlavorNote("Chamomile", "#B69AC8"),
            new FlavorNote("Lavender", "#8E6BA8"))),
        new FlavorCategory("Herbal", "#5A8B6A", ImmutableList.Create(
            new FlavorNote("Fresh Grass", "#6A9B7A"),
            new FlavorNote("Basil", "#4E7B5E"),
            new FlavorNote("Mint", "#5A9B78"),
            new FlavorNote("Rosemary", "#4A7058"))),
        new FlavorCategory("Nutty", "#A67B5B", ImmutableList.Create(
            new FlavorNote("Almond", "#B08B6B"),
            new FlavorNote("Walnut", "#9A6B4B"),
            new FlavorNote("Pine Nut", "#B89878"))),
        new FlavorCategory("Peppery", "#B85A4A", ImmutableList.Create(
            new FlavorNote("Black Pepper", "#A04A3A"),
            new FlavorNote("Arugula", "#C86A5A"),
            new FlavorNote("Chili", "#D4756A"))),
        new FlavorCategory("Bitter", "#C49B3A", ImmutableList.Create(
            new FlavorNote("Radicchio", "#B48B2A"),
            new FlavorNote("Green Olive", "#D4AB4A"),
            new FlavorNote("Dark Choc.", "#A47B2A"))),
        new FlavorCategory("Buttery", "#D4A85A", ImmutableList.Create(
            new FlavorNote("Cream", "#DEAF66"),
            new FlavorNote("Ripe Fruit", "#C89B4E"))),
        new FlavorCategory("Woody", "#7A6B5A", ImmutableList.Create(
            new FlavorNote("Cedar", "#8A7B6A"),
            new FlavorNote("Hay", "#9A8B78"),
            new FlavorNote("Tobacco", "#6A5B4A"))));

    public static ImmutableList<OleaRegion> Regions { get; } = ImmutableList.Create(
        new OleaRegion("\U0001F1EE\U0001F1F9", "Tuscany", "Central Italy",
            "Bold, peppery oils with fresh herbaceous notes. Known for early harvest Frantoio olives that produce complex, award-winning oils.",
            ImmutableList.Create("Frantoio", "Moraiolo", "Leccino")),
        new OleaRegion("\U0001F1EC\U0001F1F7", "Peloponnese", "Southern Greece",
            "Smooth, buttery Koroneiki oils with ripe fruit character. The warm climate produces oils with a gentle, rounded profile.",
            ImmutableList.Create("Koroneiki", "Athinolia", "Manaki")),
        new OleaRegion("\U0001F1EA\U0001F1F8", "Andalusia", "Southern Spain",
            "The world's largest producing region. Picual oils are prized for their stability and robust, slightly bitter character.",
            ImmutableList.Create("Picual", "Hojiblanca", "Arbequina")),
        new OleaRegion("\U0001F1FA\U0001F1F8", "California", "West Coast, USA",
            "New World oils with clean, bright flavors. Mission olives give mild, buttery oil while Arbequina adds fruity complexity.",
            ImmutableList.Create("Mission", "Arbequina", "Arbosana")),
        new OleaRegion("\U0001F1F9\U0001F1F3", "Tunisia", "North Africa",
            "Ancient olive traditions producing robust, full-bodied oils. Chetoui olives yield intensely peppery, herbaceous oils.",
            ImmutableList.Create("Chetoui", "Chemlali")),
        new OleaRegion("\U0001F1F5\U0001F1F9", "Alentejo", "Southern Portugal",
            "Emerging region with distinctive Galega oils. Warm climate produces smooth, fruity oils with hints of almond and dried herbs.",
            ImmutableList.Create("Galega", "Cobran\u00e7osa", "Cordovil")));
}
