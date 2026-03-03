namespace FormaEspresso.Models;

public static class MenuData
{
    public static readonly EspressoItem[] Items =
    [
        new EspressoItem(
            Id: "solo",
            Name: "Solo",
            Volume: "30ml",
            Intensity: 1,
            Price: 4.50m,
            Note: "Pure. Singular.",
            Description: "A single shot of precision. The essence of our craft in its most concentrated form.",
            TimeSeconds: 45
        ),
        new EspressoItem(
            Id: "doppio",
            Name: "Doppio",
            Volume: "60ml",
            Intensity: 2,
            Price: 5.50m,
            Note: "Doubled intention.",
            Description: "Two shots pulled as one. For moments that demand more presence.",
            TimeSeconds: 50
        ),
        new EspressoItem(
            Id: "ristretto",
            Name: "Ristretto",
            Volume: "20ml",
            Intensity: 3,
            Price: 5.00m,
            Note: "Essence, distilled.",
            Description: "A restricted pour. Maximum flavor extracted in minimum volume.",
            TimeSeconds: 25
        ),
        new EspressoItem(
            Id: "lungo",
            Name: "Lungo",
            Volume: "110ml",
            Intensity: 1,
            Price: 5.00m,
            Note: "Extended.",
            Description: "A long draw through the grounds. Contemplative, nuanced, complete.",
            TimeSeconds: 60
        )
    ];
}
