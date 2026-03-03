using System.Globalization;

namespace Worn.Services;

public sealed class WeatherMappingEngine : IWeatherMappingEngine
{
    // ───────────────────────────────────────────────
    //  Tier definitions
    // ───────────────────────────────────────────────

    [Uno.Extensions.Equality.ImplicitKeys(IsEnabled = false)]
    private sealed record TierProfile(
        TierId Id,
        double MinF,
        double MaxF,
        string[] Headlines,
        string Description,
        OutfitItem[] BaseOutfit,
        string SwatchColor,
        string Emoji
    );

    private static readonly TierProfile[] Tiers =
    [
        // ── Scorcher (95+) ──
        new(
            TierId.Scorcher, 95, double.MaxValue,
            [
                "It's giving heatwave, bestie.",
                "SPF is your entire personality today.",
                "Melt-proof fits only.",
                "The sun chose violence.",
                "Your outfit? Barely there. By design.",
                "Dress like the sidewalk could fry an egg.",
                "The pavement is angry. Dress accordingly.",
                "Heat advisory? More like outfit advisory.",
                "Too hot to think. Here's what to wear.",
                "Minimalism isn't a trend today, it's survival.",
                "Your wardrobe just got a lot simpler.",
                "Fabric? As little as legally allowed.",
                "If it's not breathable, it's not coming.",
                "The forecast is: barely dressed."
            ],
            "The air is thick and the asphalt is angry. Strip it down to the absolute essentials, " +
            "reach for the lightest fabrics you own, and keep a cold drink within arm's reach. " +
            "This is survival-chic territory.",
            [
                new("🩳", "Linen shorts", "Loose-cut linen in a neutral tone", NecessityLevel.NonNegotiable),
                new("👕", "Tank top", "Breathable, moisture-wicking tank", NecessityLevel.NonNegotiable),
                new("🩴", "Slides", "Open-toe ventilation for your feet", NecessityLevel.GoTo),
                new("🧴", "Sunscreen", "SPF 50+ broad-spectrum, non-negotiable", NecessityLevel.MustHave),
                new("🕶️", "Sunglasses", "UV-blocking polarized lenses", NecessityLevel.MustHave),
                new("👒", "Wide brim hat", "Full-coverage sun protection", NecessityLevel.SmartPick),
                new("💧", "Water bottle", "Hydrate like your life depends on it", NecessityLevel.MustHave),
                new("👗", "Linen dress", "Maxi or midi, loose and flowing", NecessityLevel.GoTo),
                new("👡", "Strappy sandals", "Open, airy, lets your feet breathe", NecessityLevel.EasyPick),
                new("👚", "Cropped blouse", "Lightweight crop with airflow in mind", NecessityLevel.EasyPick)
            ],
            "#E8573A",
            "🩳"
        ),

        // ── Hot (85-94) ──
        new(
            TierId.Hot, 85, 94.99,
            [
                "Keep it breezy, keep it cute.",
                "Less is literally more today.",
                "Light layers and cold brew energy.",
                "Dress like you have iced coffee plans.",
                "Summer called, and it meant business.",
                "Linen season is officially open.",
                "Your closet's summer edit, activated.",
                "Wear less, stress less.",
                "Heat-friendly fits for humans with taste.",
                "It's a sandals-and-sunglasses kind of day.",
                "Breezy fabrics, zero overthinking.",
                "The air is warm and your outfit should match.",
                "Cotton, linen, done."
            ],
            "It's proper hot out there but not punishing. Think airy fabrics, relaxed silhouettes, " +
            "and nothing that clings. You want to look effortless because effort itself is sweating today.",
            [
                new("👗", "Sundress / linen set", "Flowy, relaxed, and seasonally perfect", NecessityLevel.GoTo),
                new("👕", "Breathable tee", "Cotton or linen crew in a soft wash", NecessityLevel.SafeBet),
                new("🩳", "Shorts", "Mid-length, easy movement", NecessityLevel.GoTo),
                new("👡", "Sandals", "Strappy or slip-on, your pick", NecessityLevel.EasyPick),
                new("🕶️", "Sunglasses", "Your face deserves shade", NecessityLevel.SmartPick),
                new("👒", "Straw hat", "Sun coverage with vacation energy", NecessityLevel.NiceTouch),
                new("👘", "Kimono wrap", "Light open-front layer with effortless drape", NecessityLevel.NiceTouch),
                new("👚", "Breezy blouse", "Flowy sleeves, relaxed fit, airy cotton", NecessityLevel.EasyPick),
                new("🩴", "Flip-flops", "Maximum ventilation for casual days", NecessityLevel.Maybe)
            ],
            "#F0944D",
            "👗"
        ),

        // ── Warm (75-84) ──
        new(
            TierId.Warm, 75, 84.99,
            [
                "Golden hour dressing, all day.",
                "T-shirt weather with main-character energy.",
                "Warm but make it fashion.",
                "The sweet spot. Dress accordingly.",
                "Outfit of the day? Easy mode.",
                "Your closet is playing on easy difficulty.",
                "Everything works today. Pick your favorite.",
                "The kind of day where style is effortless.",
                "Casual perfection weather.",
                "Roll up your sleeves and enjoy it.",
                "This temperature was made for good outfits.",
                "No coat, no stress, no problem.",
                "Throw on your best and walk out the door."
            ],
            "This is the temperature sweet spot where everything in your closet works. " +
            "A good tee, your favorite bottoms, and clean sneakers will carry you through the whole day. " +
            "No overthinking required.",
            [
                new("👕", "Classic tee", "Well-fitted cotton crew or V-neck", NecessityLevel.GoTo),
                new("👖", "Chinos or jeans", "Light-wash or khaki for the season", NecessityLevel.SafeBet),
                new("👟", "Sneakers", "Clean white or canvas low-tops", NecessityLevel.EasyPick),
                new("🕶️", "Sunglasses", "Finishing touch, always", NecessityLevel.NiceTouch),
                new("👗", "Wrap dress", "Versatile and flattering for any occasion", NecessityLevel.GoTo),
                new("🥿", "Flats", "Comfortable, minimal, goes with everything", NecessityLevel.EasyPick),
                new("👜", "Tote bag", "Light canvas or leather - carry layers just in case", NecessityLevel.NiceTouch),
                new("👘", "Open-front kimono", "A breezy layer that adds texture without warmth", NecessityLevel.Maybe)
            ],
            "#E8B84D",
            "👕"
        ),

        // ── Pleasant (65-74) ──
        new(
            TierId.Pleasant, 65, 74.99,
            [
                "Peak vibes. Peak wardrobe.",
                "The weather is flirting with you.",
                "Cardigan-optional perfection.",
                "This is what closets dream about.",
                "Effortless. Truly.",
                "Layer if you want. Or don't. Both work.",
                "The Goldilocks zone of getting dressed.",
                "Your whole wardrobe just became relevant.",
                "Perfect weather for your favorite outfit.",
                "Sleeve length? Dealer's choice.",
                "Walk-everywhere, wear-anything weather.",
                "The kind of day outfits were invented for.",
                "Grab what you love. It'll work."
            ],
            "Every outfit works in this range - it's the Goldilocks zone. " +
            "A light layer draped over your shoulders looks intentional, and you can go sleeveless if the mood hits. " +
            "This is your fashion playground.",
            [
                new("👔", "Button-down or blouse", "Rolled sleeves, relaxed collar", NecessityLevel.GoTo),
                new("👖", "Jeans or trousers", "Your best-fitting pair", NecessityLevel.SafeBet),
                new("👞", "Loafers", "Polished leather, comfortable sole", NecessityLevel.EasyPick),
                new("🧥", "Light cardigan", "Toss it over your shoulders for the aesthetic", NecessityLevel.NiceTouch),
                new("👗", "Midi skirt", "A-line or pleated, season-spanning staple", NecessityLevel.GoTo),
                new("🩰", "Ballet flats", "Sleek, minimal, pairs with dresses and jeans", NecessityLevel.EasyPick),
                new("👜", "Structured handbag", "Leather or canvas, ties the look together", NecessityLevel.NiceTouch),
                new("💍", "Statement ring", "One piece that elevates the whole outfit", NecessityLevel.Maybe),
                new("🧥", "Linen blazer", "Unstructured, open-front, instant polish", NecessityLevel.NiceTouch)
            ],
            "#7EAE7B",
            "👔"
        ),

        // ── Light Jacket (55-64) ──
        new(
            TierId.LightJacket, 55, 64.99,
            [
                "Jacket weather has entered the chat.",
                "Layer one: activated.",
                "That transitional fit energy.",
                "Your denim jacket's moment to shine.",
                "Bring a layer. Thank yourself later.",
                "The air has opinions. Bring a jacket.",
                "Not cold, not warm. Perfectly in between.",
                "Morning says jacket. Afternoon says maybe not.",
                "This is the day layers were designed for.",
                "A light outer layer does all the work.",
                "Crisp air, sharp fit.",
                "Your bomber jacket has been waiting for this.",
                "Shacket season, if you will."
            ],
            "There's a crispness in the air that says bring a jacket but don't go overboard. " +
            "A denim jacket, a bomber, or a light overshirt gives you flexibility without bulk. " +
            "Morning might be cool, but afternoon could warm up.",
            [
                new("🧥", "Light jacket", "Denim, bomber, or shacket", NecessityLevel.MustHave),
                new("👕", "Long-sleeve tee", "Midweight cotton, great base layer", NecessityLevel.GoTo),
                new("👖", "Jeans or chinos", "Full-length, your go-to pair", NecessityLevel.SafeBet),
                new("👟", "Sneakers", "Closed-toe, comfortable for walking", NecessityLevel.EasyPick),
                new("🧥", "Overshirt", "Flannel or heavy cotton, works open or buttoned", NecessityLevel.GoTo),
                new("👞", "Desert boots", "Suede ankle boots with crepe sole", NecessityLevel.EasyPick),
                new("👖", "Corduroy trousers", "Textured, warm-toned, autumn-ready", NecessityLevel.SafeBet),
                new("🧣", "Light scarf", "Thin cotton or silk, more style than warmth", NecessityLevel.NiceTouch),
                new("🎒", "Day pack", "Small backpack for your extra layer and essentials", NecessityLevel.Maybe)
            ],
            "#5B9EA6",
            "🧥"
        ),

        // ── Sweater Weather (45-54) ──
        new(
            TierId.SweaterWeather, 45, 54.99,
            [
                "Cozy is the entire vibe.",
                "Knit picks and warm sips.",
                "Sweater weather is a lifestyle.",
                "Layer game: intermediate mode.",
                "Time for textures and warm tones.",
                "Chunky knits and zero regrets.",
                "The kind of cold that feels like a hug in wool.",
                "Your coziest sweater just got called up.",
                "Knitwear, coffee, and crisp air. Perfect.",
                "This is the weather autumn songs are about.",
                "Everything looks better with a sweater.",
                "Turtleneck energy, fully engaged.",
                "Wrap yourself in something warm and soft.",
                "Cashmere weather. Or at least cashmere-adjacent."
            ],
            "This is the temperature that launched a thousand Instagram flatlays. " +
            "Break out the chunky knits, pair with your coziest pants, and lean into the autumn-all-year aesthetic. " +
            "A scarf is never wrong here.",
            [
                new("🧶", "Knit sweater", "Chunky crew or turtleneck in a warm tone", NecessityLevel.MustHave),
                new("👕", "Base layer tee", "Fitted long-sleeve underneath", NecessityLevel.GoTo),
                new("👖", "Jeans or cords", "Heavier weight denim or corduroy", NecessityLevel.SafeBet),
                new("🧣", "Scarf", "Adds warmth and instant style points", NecessityLevel.SmartPick),
                new("👢", "Ankle boots", "Sturdy, warm, and seasonally on point", NecessityLevel.EasyPick),
                new("🧶", "Cable-knit cardigan", "Open-front, oversized, pairs with everything", NecessityLevel.GoTo),
                new("🥾", "Hiking boots", "Rugged, warm, and rain-ready", NecessityLevel.EasyPick),
                new("🧢", "Beanie", "Keeps your head warm, adds personality", NecessityLevel.NiceTouch),
                new("👜", "Leather satchel", "Structured bag that handles the elements", NecessityLevel.NiceTouch),
                new("👓", "Clear-frame glasses", "Fog-friendly, adds a bookish charm", NecessityLevel.Maybe)
            ],
            "#8B7D6B",
            "🧶"
        ),

        // ── Coat Up (32-44) ──
        new(
            TierId.CoatUp, 32, 44.99,
            [
                "Coat check: mandatory.",
                "Your outerwear is the outfit now.",
                "Bundle-lite. Looking sharp, staying warm.",
                "Cold enough to commit to a coat.",
                "Winter-adjacent dressing.",
                "The coat makes the outfit today.",
                "Layer like you mean it.",
                "Your coat closet's time to shine.",
                "Cold air, warm wool, sharp look.",
                "This is proper coat weather. No debate.",
                "Button up, zip up, go.",
                "Structured warmth is the move.",
                "Your heaviest jacket, your best scarf.",
                "Frost on the ground, style on the sleeve."
            ],
            "It is properly cold and your coat is doing the heavy lifting. " +
            "Think warm layers underneath, a structured coat on top, and accessories that earn their keep. " +
            "Gloves and a beanie are smart moves, not optional extras.",
            [
                new("🧥", "Winter coat", "Wool, puffer, or insulated parka", NecessityLevel.NonNegotiable),
                new("🧶", "Sweater or fleece", "Midweight insulating layer", NecessityLevel.MustHave),
                new("👕", "Thermal base layer", "Fitted long-sleeve thermal", NecessityLevel.GoTo),
                new("👖", "Warm trousers", "Lined pants or heavy denim", NecessityLevel.SafeBet),
                new("🧤", "Gloves", "Touchscreen-compatible for real life", NecessityLevel.SmartPick),
                new("🥾", "Insulated boots", "Warm, waterproof-ready footwear", NecessityLevel.EasyPick),
                new("🧣", "Wool scarf", "Thick-knit, wraps twice for extra warmth", NecessityLevel.SmartPick),
                new("🧢", "Beanie or ear warmers", "Ears need love in this cold", NecessityLevel.NiceTouch),
                new("🧥", "Puffer vest", "Layer under a coat for extra core warmth", NecessityLevel.EasyPick),
                new("🎒", "Insulated backpack", "Keeps an extra layer and hand warmers close", NecessityLevel.Maybe)
            ],
            "#5A6978",
            "🧥"
        ),

        // ── Bundle (15-31) ──
        new(
            TierId.Bundle, 15, 31.99,
            [
                "Full send on layers.",
                "Dress like you mean it. It's cold.",
                "Warmth is the only trend today.",
                "Your warmest everything, please.",
                "Fashion? Survival comes first.",
                "The cold is not messing around today.",
                "Every layer counts. Literally.",
                "Your thermals were made for this moment.",
                "Think warm. Dress warmer.",
                "It's the kind of cold that bites.",
                "Bundle up or stay in. Those are the options.",
                "Fleece, wool, down. Stack them all.",
                "If you can't feel your face, add more.",
                "The wind chill has entered the conversation."
            ],
            "Every inch of exposed skin is a regret waiting to happen. " +
            "This calls for your heaviest coat, thermal everything underneath, and accessories that cover ears, neck, and hands. " +
            "Looking good is secondary to not freezing.",
            [
                new("🧥", "Heavy parka", "Down-filled or heavily insulated", NecessityLevel.NonNegotiable),
                new("🧶", "Thick sweater", "Wool or heavy fleece layer", NecessityLevel.NonNegotiable),
                new("👕", "Thermal base layer", "Merino wool or synthetic thermal", NecessityLevel.MustHave),
                new("👖", "Insulated pants", "Lined or thermal-weight bottoms", NecessityLevel.MustHave),
                new("🧣", "Scarf + beanie", "Cover your neck and ears", NecessityLevel.SmartPick),
                new("🧤", "Heavy gloves", "Insulated, windproof gloves", NecessityLevel.SmartPick),
                new("🧦", "Wool socks", "Merino wool, moisture-wicking warmth", NecessityLevel.GoTo),
                new("🥾", "Winter boots", "Insulated, waterproof, high-ankle support", NecessityLevel.MustHave),
                new("🧥", "Fleece mid-layer", "Zip-up fleece between base and coat", NecessityLevel.GoTo),
                new("🥽", "Snow goggles", "Eye protection in blowing snow and ice", NecessityLevel.NiceTouch)
            ],
            "#3E5064",
            "🧤"
        ),

        // ── Survival (below 15) ──
        new(
            TierId.Survival, double.MinValue, 14.99,
            [
                "Do not go outside unless you must.",
                "Layer everything you own. Then add more.",
                "Frostbite is not a look.",
                "Survival mode: fully engaged.",
                "If you can stay in, stay in.",
                "This is not outfit weather. This is armor weather.",
                "Skin exposure time: zero minutes.",
                "The cold will hurt. Dress like it.",
                "Every gap in your layers is a liability.",
                "Warmth over everything. We mean everything.",
                "Forget fashion. Embrace insulation.",
                "Your heaviest coat is your lightest option.",
                "If your nose goes numb, you're underdressed."
            ],
            "This is dangerous cold. Exposed skin can get frostbitten in minutes. " +
            "If you absolutely must go out, wear every thermal layer you have, cover all skin, " +
            "and limit your time outside. Fashion is irrelevant. Warmth is everything.",
            [
                new("🧥", "Arctic parka", "Rated for extreme cold, full coverage", NecessityLevel.Survival),
                new("🧶", "Double fleece layer", "Two insulating mid-layers minimum", NecessityLevel.Survival),
                new("👕", "Heavy thermal base", "Full-body merino or expedition-weight thermal", NecessityLevel.NonNegotiable),
                new("👖", "Insulated snow pants", "Wind- and waterproof over thermals", NecessityLevel.NonNegotiable),
                new("🧣", "Balaclava + scarf", "Full face and neck coverage", NecessityLevel.MustHave),
                new("🧤", "Expedition gloves", "Multi-layer insulated mittens or gloves", NecessityLevel.MustHave),
                new("🧦", "Expedition socks", "Double-layer wool, moisture barrier", NecessityLevel.MustHave),
                new("🥾", "Extreme cold boots", "Rated to -40, sealed ankle cuffs", NecessityLevel.NonNegotiable),
                new("🥽", "Ski goggles", "Wind and ice protection for your eyes", NecessityLevel.SmartPick),
                new("🧥", "Neck gaiter", "Fleece-lined, covers chin to nose", NecessityLevel.SmartPick)
            ],
            "#2C3A4A",
            "🧣"
        )
    ];

    // ───────────────────────────────────────────────
    //  Rain / snow headline overrides
    // ───────────────────────────────────────────────

    private static readonly string[] RainHeadlines =
    [
        "Umbrella is the accessory today.",
        "Wet-weather chic, activated.",
        "Rain check? Nope. Rain fit.",
        "Waterproof everything, please.",
        "Splashproof and still stylish.",
        "Puddles don't stand a chance.",
        "The sky is crying. Your outfit shouldn't be.",
        "Embrace the drizzle in style.",
        "Raincoat vibes, coffee shop dreams.",
        "Dark fabrics and dry feet. That's the plan.",
        "Water-resistant is a personality trait today.",
        "Grab the umbrella. Skip the regret."
    ];

    private static readonly string[] SnowHeadlines =
    [
        "Snow day dressing: engage.",
        "Flurries call for serious layers.",
        "Winter wonderland, winter wardrobe.",
        "Snow is falling, boots are calling.",
        "Powder-proof your whole fit.",
        "Snowflakes on your coat, warmth in your layers.",
        "The world is white. Your boots should be waterproof.",
        "Let it snow. But let you stay warm.",
        "Every snowflake is a reminder to layer up.",
        "Fresh snow, warm socks, sturdy boots.",
        "Bundle for the beauty of it all.",
        "Snow on the ground, wool on the body."
    ];

    private static readonly string[] WindHeadlines =
    [
        "The wind has strong opinions today.",
        "Zip up. The gusts aren't playing.",
        "Hold onto your hat. Literally.",
        "Windbreaker weather, no negotiation.",
        "Breezy? More like gusty with attitude.",
        "Batten down the layers.",
        "Your scarf is pulling double duty today.",
        "Tuck everything in. The wind will find gaps.",
        "If it's loose, it's gone. Layer tight.",
        "The wind is the outfit's worst enemy today."
    ];

    private static readonly string[] FogHeadlines =
    [
        "Moody skies, cozy layers.",
        "Visibility is low. Your outfit game isn't.",
        "Foggy mornings call for warm textures.",
        "Mist and layers go hand in hand.",
        "The world is soft-focus today. Dress to match.",
        "Fog rolls in, knits come out."
    ];

    // ───────────────────────────────────────────────
    //  Hourly weather-code to emoji
    // ───────────────────────────────────────────────

    private static string WeatherCodeToEmoji(int code) => code switch
    {
        0 => "☀️",
        1 or 2 => "🌤️",
        3 => "☁️",
        45 or 48 => "🌫️",
        51 or 53 or 55 => "🌦️",
        56 or 57 => "🌧️",
        61 or 63 or 65 => "🌧️",
        66 or 67 => "🌧️",
        71 or 73 or 75 => "🌨️",
        77 => "🌨️",
        80 or 81 or 82 => "🌧️",
        85 or 86 => "🌨️",
        95 => "⛈️",
        96 or 99 => "⛈️",
        _ => "🌡️"
    };

    // ───────────────────────────────────────────────
    //  Public entry point
    // ───────────────────────────────────────────────

    public WornResult Process(CurrentWeather current, IList<HourlyWeather> hourly, IList<DailyWeather> daily, string locationName)
    {
        var tier = ResolveTier(current.ApparentTemp);
        var tagline = BuildTagline(locationName);
        var baseOutfit = tier.BaseOutfit.ToList();
        var modifiers = EvaluateModifiers(current);

        ApplyModifiers(baseOutfit, modifiers);
        DeduplicateOutfit(baseOutfit);

        // Assign layer indices
        for (int i = 0; i < baseOutfit.Count; i++)
        {
            baseOutfit[i] = baseOutfit[i] with { LayerIndex = i + 1 };
        }

        var headline = PickHeadline(tier, modifiers);
        var headlineRotation = BuildHeadlineRotation(headline, tier, modifiers);
        var fabricTagsList = BuildFabricTags(current);
        // Prepend layer count as first tag
        fabricTagsList.Insert(0, new FabricTag($"{baseOutfit.Count} layer day", "#C4654A"));
        var fabricTags = fabricTagsList.ToImmutableList();
        var hourlyMoments = BuildHourly(hourly, current).ToImmutableList();
        var dailyForecasts = BuildDaily(daily).ToImmutableList();
        var alerts = BuildAlerts(daily).ToImmutableList();
        var nudge = BuildNudge(hourlyMoments);

        return new WornResult(
            tagline,
            headline,
            tier.Description,
            tier.Id,
            baseOutfit.ToImmutableList(),
            fabricTags,
            hourlyMoments,
            dailyForecasts,
            alerts,
            nudge,
            headlineRotation,
            locationName
        );
    }

    // ───────────────────────────────────────────────
    //  Tier resolution
    // ───────────────────────────────────────────────

    private static TierProfile ResolveTier(double apparentTempF)
    {
        foreach (var tier in Tiers)
        {
            if (apparentTempF >= tier.MinF && apparentTempF <= tier.MaxF)
            {
                return tier;
            }
        }

        // Fallback - should never happen given the ranges
        return Tiers[^1];
    }

    // ───────────────────────────────────────────────
    //  Tagline - time-of-day contextual label
    // ───────────────────────────────────────────────

    private static readonly string[][] MorningTaglines =
    [
        ["{0} morning mood", "{0} sunrise style", "early {0} essentials",
         "{0} AM, outfit ready", "good morning, {0}"]
    ];

    private static readonly string[][] AfternoonTaglines =
    [
        ["{0} afternoon vibes", "{0} midday memo", "your {0} outfit update",
         "{0} PM pick", "afternoon edition, {0}"]
    ];

    private static readonly string[][] EveningTaglines =
    [
        ["{0} evening energy", "{0} golden hour fit", "sunset {0} style",
         "{0} evening layers", "winding down {0}"]
    ];

    private static readonly string[] LateNightTaglines =
    [
        "late night layers", "after-hours outfit", "midnight wardrobe",
        "the night shift look", "burning the midnight wool"
    ];

    private static string BuildTagline(string locationName)
    {
        var now = DateTime.Now;
        var dayName = now.ToString("dddd", CultureInfo.InvariantCulture);
        var hour = now.Hour;
        var suffix = string.IsNullOrWhiteSpace(locationName) ? "" : $" in {locationName}";

        if (hour >= 5 && hour <= 11)
        {
            var pool = MorningTaglines[0];
            return string.Format(pool[Random.Shared.Next(pool.Length)], dayName) + suffix;
        }

        if (hour >= 12 && hour <= 16)
        {
            var pool = AfternoonTaglines[0];
            return string.Format(pool[Random.Shared.Next(pool.Length)], dayName) + suffix;
        }

        if (hour >= 17 && hour <= 20)
        {
            var pool = EveningTaglines[0];
            return string.Format(pool[Random.Shared.Next(pool.Length)], dayName) + suffix;
        }

        return LateNightTaglines[Random.Shared.Next(LateNightTaglines.Length)] + suffix;
    }

    // ───────────────────────────────────────────────
    //  Headline selection with overrides
    // ───────────────────────────────────────────────

    private static string PickHeadline(TierProfile tier, ModifierSet modifiers)
    {
        // Snow overrides rain, rain overrides wind, wind overrides fog, fog overrides default
        if (modifiers.IsSnowActive)
        {
            return SnowHeadlines[Random.Shared.Next(SnowHeadlines.Length)];
        }

        if (modifiers.IsHeavyRain)
        {
            return RainHeadlines[Random.Shared.Next(RainHeadlines.Length)];
        }

        if (modifiers.IsWindy)
        {
            return WindHeadlines[Random.Shared.Next(WindHeadlines.Length)];
        }

        if (modifiers.IsFoggy)
        {
            return FogHeadlines[Random.Shared.Next(FogHeadlines.Length)];
        }

        return tier.Headlines[Random.Shared.Next(tier.Headlines.Length)];
    }

    private static ImmutableList<string> BuildHeadlineRotation(string primary, TierProfile tier, ModifierSet modifiers)
    {
        // Pick the source pool based on the same override logic as PickHeadline
        string[] pool;
        if (modifiers.IsSnowActive) pool = SnowHeadlines;
        else if (modifiers.IsHeavyRain) pool = RainHeadlines;
        else if (modifiers.IsWindy) pool = WindHeadlines;
        else if (modifiers.IsFoggy) pool = FogHeadlines;
        else pool = tier.Headlines;

        // Shuffle and pick up to 4 unique headlines (primary first)
        var candidates = pool.Where(h => h != primary).OrderBy(_ => Random.Shared.Next()).Take(3).ToList();
        candidates.Insert(0, primary);
        return candidates.ToImmutableList();
    }

    // ───────────────────────────────────────────────
    //  Modifier evaluation
    // ───────────────────────────────────────────────

    private sealed record ModifierSet(
        bool IsLightRain,
        bool IsMediumRain,
        bool IsHeavyRain,
        bool IsSnowActive,
        bool IsWindy,
        bool IsHumid,
        bool IsUvHigh,
        bool IsUvVeryHigh,
        bool IsUvExtreme,
        bool IsClearSky,
        bool IsFoggy
    );

    private static ModifierSet EvaluateModifiers(CurrentWeather c)
    {
        var prob = c.PrecipProbability;
        var rain = c.Rain;
        var snow = c.Snowfall;
        var wind = c.WindSpeed;
        var gusts = c.WindGusts;
        var humidity = c.Humidity;
        var uv = c.UvIndex;
        var cloud = c.CloudCover;
        var wc = c.WeatherCode;

        bool isHeavyRain = prob > 80 || rain > 5;
        bool isMediumRain = !isHeavyRain && (prob >= 60 || (rain >= 1 && rain <= 5));
        bool isLightRain = !isHeavyRain && !isMediumRain && prob >= 40;

        // Fog: weather code 45 (fog) or 48 (depositing rime fog), or very low visibility
        bool isFoggy = wc is 45 or 48 || c.Visibility < 1000;

        return new ModifierSet(
            IsLightRain: isLightRain,
            IsMediumRain: isMediumRain,
            IsHeavyRain: isHeavyRain,
            IsSnowActive: snow > 0,
            IsWindy: wind > 20 || gusts > 30,
            IsHumid: humidity > 75,
            IsUvHigh: uv >= 6 && uv <= 7,
            IsUvVeryHigh: uv >= 8 && uv <= 9,
            IsUvExtreme: uv >= 10,
            IsClearSky: cloud < 30 && uv < 6,
            IsFoggy: isFoggy
        );
    }

    // ───────────────────────────────────────────────
    //  Apply modifiers to outfit
    // ───────────────────────────────────────────────

    private static void ApplyModifiers(List<OutfitItem> outfit, ModifierSet m)
    {
        // Rain layers
        if (m.IsHeavyRain)
        {
            outfit.Add(new("☂️", "Sturdy umbrella", "Full-size, wind-resistant umbrella", NecessityLevel.NonNegotiable));
            outfit.Add(new("🧥", "Waterproof jacket", "Sealed seams, hooded rain shell", NecessityLevel.MustHave));
            outfit.Add(new("👢", "Rain boots", "Waterproof, mid-calf coverage", NecessityLevel.SmartPick));
        }
        else if (m.IsMediumRain)
        {
            outfit.Add(new("☂️", "Umbrella", "Compact but reliable umbrella", NecessityLevel.MustHave));
            outfit.Add(new("🧥", "Waterproof jacket", "Lightweight rain shell with hood", NecessityLevel.SmartPick));
        }
        else if (m.IsLightRain)
        {
            outfit.Add(new("☂️", "Compact umbrella", "Toss it in your bag, just in case", NecessityLevel.NiceTouch));
        }

        // Snow
        if (m.IsSnowActive)
        {
            outfit.Add(new("👢", "Waterproof boots", "Insulated, sealed against snow and slush", NecessityLevel.MustHave));
            outfit.Add(new("🧦", "Thermal socks", "Merino wool for all-day warmth", NecessityLevel.SmartPick));
        }

        // Wind
        if (m.IsWindy)
        {
            outfit.Add(new("🧥", "Windbreaker", "Lightweight but wind-blocking shell", NecessityLevel.SmartPick));
            outfit.Add(new("🧣", "Scarf", "Keeps wind off your neck and chest", NecessityLevel.NiceTouch));
        }

        // UV layers (mutually exclusive tiers, highest wins)
        if (m.IsUvExtreme)
        {
            outfit.Add(new("🧴", "Sunscreen", "SPF 50+ reapply every two hours", NecessityLevel.NonNegotiable));
            outfit.Add(new("👒", "Wide brim hat", "Maximum face and neck coverage", NecessityLevel.MustHave));
        }
        else if (m.IsUvVeryHigh)
        {
            outfit.Add(new("🕶️", "Sunglasses", "Polarized UV-blocking lenses", NecessityLevel.MustHave));
            outfit.Add(new("👒", "Wide brim hat", "Serious sun coverage", NecessityLevel.SmartPick));
            outfit.Add(new("🧴", "Sunscreen", "SPF 30+ at minimum", NecessityLevel.SmartPick));
        }
        else if (m.IsUvHigh)
        {
            outfit.Add(new("🕶️", "Sunglasses", "UV protection is non-negotiable", NecessityLevel.SmartPick));
            outfit.Add(new("🧢", "Hat", "Brimmed cap for face coverage", NecessityLevel.NiceTouch));
        }

        // Clear sky (only if no UV modifiers already added sunglasses)
        if (m.IsClearSky && !m.IsUvHigh && !m.IsUvVeryHigh && !m.IsUvExtreme)
        {
            outfit.Add(new("🕶️", "Sunglasses", "Clear skies call for shades", NecessityLevel.NiceTouch));
        }

        // Fog
        if (m.IsFoggy)
        {
            outfit.Add(new("👓", "Clear glasses", "Shield your eyes from damp fog - even non-prescription", NecessityLevel.NiceTouch));
            outfit.Add(new("🎒", "Reflective bag", "Low visibility? Be seen. A bright accessory helps", NecessityLevel.Maybe));
        }

        // High humidity (without rain)
        if (m.IsHumid && !m.IsLightRain && !m.IsMediumRain && !m.IsHeavyRain)
        {
            outfit.Add(new("👕", "Moisture-wicking tee", "Synthetic blend that dries fast in humid air", NecessityLevel.NiceTouch));
        }
    }

    // ───────────────────────────────────────────────
    //  Deduplication by Name
    // ───────────────────────────────────────────────

    private static void DeduplicateOutfit(List<OutfitItem> outfit)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        outfit.RemoveAll(item => !seen.Add(item.Name));
    }

    // ───────────────────────────────────────────────
    //  Fabric tags
    // ───────────────────────────────────────────────

    private static List<FabricTag> BuildFabricTags(CurrentWeather c)
    {
        var tags = new List<FabricTag>();
        var temp = c.ApparentTemp;
        var humidity = c.Humidity;
        var wind = c.WindSpeed;
        var uv = c.UvIndex;
        var prob = c.PrecipProbability;
        bool anyRain = c.IsRaining || c.Rain > 0;
        bool anySnow = c.IsSnowing || c.Snowfall > 0;

        // cotton-friendly: no rain AND humidity <70% AND temp 55-85
        if (!anyRain && humidity < 70 && temp >= 55 && temp <= 85)
        {
            tags.Add(new("cotton-friendly", "#8B9E7E"));
        }

        // moisture-wicking: humidity >75% OR temp >85
        if (humidity > 75 || temp > 85)
        {
            tags.Add(new("moisture-wicking", "#4A5E78"));
        }

        // waterproof: precipProb >50% OR rain/snow active
        if (prob > 50 || anyRain || anySnow)
        {
            tags.Add(new("waterproof", "#4A5E78"));
        }

        // hair-safe: wind <12mph AND no rain
        if (wind < 12 && !anyRain)
        {
            tags.Add(new("hair-safe", "#8B9E7E"));
        }

        // closed-toe: temp <65 OR rain OR snow
        if (temp < 65 || anyRain || anySnow)
        {
            tags.Add(new("closed-toe", "#3A3632"));
        }

        // sunscreen-worthy: uv >=6
        if (uv >= 6)
        {
            tags.Add(new("sunscreen-worthy", "#C9A96E"));
        }

        // windproof: wind >20mph
        if (wind > 20)
        {
            tags.Add(new("windproof", "#6B7B8D"));
        }

        // breathable: temp >75
        if (temp > 75)
        {
            tags.Add(new("breathable", "#8B9E7E"));
        }

        // open-toe OK: temp >=70 AND no rain AND no snow
        if (temp >= 70 && !anyRain && !anySnow)
        {
            tags.Add(new("open-toe OK", "#C9A96E"));
        }

        return tags;
    }

    // ───────────────────────────────────────────────
    //  Hourly moments
    // ───────────────────────────────────────────────

    private static List<HourlyMoment> BuildHourly(IList<HourlyWeather> hourly, CurrentWeather current)
    {
        var moments = new List<HourlyMoment>();
        var now = DateTime.Now;
        var currentHourStr = now.ToString("yyyy-MM-ddTHH:00", CultureInfo.InvariantCulture);
        var todayMidnight = now.Date.ToString("yyyy-MM-ddT00:00", CultureInfo.InvariantCulture);

        // Find today's midnight index
        int midnightIndex = 0;
        for (int i = 0; i < hourly.Count; i++)
        {
            if (string.Compare(hourly[i].Time, todayMidnight, StringComparison.Ordinal) >= 0)
            {
                midnightIndex = i;
                break;
            }
        }

        TierId? previousTier = null;

        // Loop 24 hours from midnight to 11 PM
        for (int i = midnightIndex; i < hourly.Count && moments.Count < 24; i++)
        {
            var h = hourly[i];
            var tier = ResolveTier(h.ApparentTemp);
            bool isPast = string.Compare(h.Time, currentHourStr, StringComparison.Ordinal) < 0;
            bool isNow = string.Compare(h.Time, currentHourStr, StringComparison.Ordinal) == 0;

            // Determine the transition vibe
            string vibe;
            if (isNow)
            {
                vibe = "Right now";
            }
            else if (previousTier.HasValue && tier.Id != previousTier.Value)
            {
                vibe = GetTransitionLabel(previousTier.Value, tier.Id, h.IsRaining);
            }
            else
            {
                vibe = GetDefaultVibe(tier.Id);
            }

            // Transition annotation
            bool hasTransition = previousTier.HasValue && tier.Id != previousTier.Value;
            string transitionLabel = hasTransition
                ? GetClotheslineTransitionLabel(previousTier!.Value, tier.Id, h.IsRaining, h.IsSnowing)
                : "";

            // Parse display time
            string displayTime = FormatDisplayTime(h.Time);

            bool isFoggy = h.WeatherCode is 45 or 48 || h.Visibility < 1000;

            // Sway angle scaled by wind speed:
            // Base: -5 to +5, wind multiplier ramps from 1.0 (calm) to 2.5 (30+ mph)
            double windMultiplier = 1.0 + Math.Clamp(current.WindSpeed / 20.0, 0, 1.5);
            double swayAngle = (Random.Shared.NextDouble() * 10.0 - 5.0) * windMultiplier;

            moments.Add(new HourlyMoment(
                Time: h.Time,
                DisplayTime: displayTime,
                Emoji: GetClothingEmojiForTier(tier.Id, h.IsRaining, h.IsSnowing, moments.Count, isFoggy),
                Garment: GetGarmentForTier(tier.Id),
                Vibe: vibe,
                TierId: tier.Id,
                IsRaining: h.IsRaining,
                IsSnowing: h.IsSnowing,
                IsNow: isNow,
                SwatchColor: tier.SwatchColor,
                IsPast: isPast,
                HasTransition: hasTransition,
                TransitionLabel: transitionLabel,
                SwayAngle: swayAngle
            ));

            previousTier = tier.Id;
        }

        return moments;
    }

    private static string FormatDisplayTime(string isoTime)
    {
        // Expected format: "2025-01-15T14:00"
        if (DateTime.TryParseExact(isoTime, "yyyy-MM-ddTHH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            // "7 AM", "12 PM", etc.
            return dt.ToString("h tt", CultureInfo.InvariantCulture).TrimStart('0');
        }

        return isoTime;
    }

    private static string GetTransitionLabel(TierId previous, TierId next, bool isRaining)
    {
        if (isRaining)
        {
            return "Umbrella Up";
        }

        // Lower enum value = hotter. If next is lower (hotter), shed a layer.
        return next < previous ? "Shed a Layer" : "Re-Layer";
    }

    private static readonly string[] WarmingLabels =
        ["lose the jacket", "lighten up", "ditch the layers", "strip it down"];

    private static readonly string[] CoolingLabels =
        ["jacket up", "sweater time", "bundle up", "add a layer"];

    private static string GetClotheslineTransitionLabel(TierId previous, TierId next, bool isRaining, bool isSnowing)
    {
        if (isSnowing) return "grab the snow gear";
        if (isRaining) return "grab the umbrella";

        // Lower enum = hotter tier
        if (next < previous)
            return WarmingLabels[Random.Shared.Next(WarmingLabels.Length)];

        return CoolingLabels[Random.Shared.Next(CoolingLabels.Length)];
    }

    private static NudgeBar BuildNudge(IList<HourlyMoment> hourlyMoments)
    {
        int transitions = 0;
        for (int i = 1; i < hourlyMoments.Count; i++)
        {
            if (hourlyMoments[i].TierId != hourlyMoments[i - 1].TierId)
                transitions++;
        }

        return transitions switch
        {
            0 => new NudgeBar("👕", "Steady outfit day - one look carries you through", "STEADY"),
            1 => new NudgeBar("🧥", "One outfit change ahead - bring an extra layer", "SIMPLE"),
            2 => new NudgeBar("🧥", "Two outfit changes - dress in layers", "LAYERED"),
            _ => new NudgeBar("🌡️", "Volatile day ahead - pack options and plan for swaps", "VOLATILE")
        };
    }

    private static readonly Dictionary<TierId, string[]> VibePools = new()
    {
        [TierId.Scorcher] = ["Scorching", "Blazing", "Sizzling", "Sweltering"],
        [TierId.Hot] = ["Toasty", "Balmy", "Sultry", "Steamy"],
        [TierId.Warm] = ["Easy breezy", "Golden", "Mellow", "Sunny vibes"],
        [TierId.Pleasant] = ["Perfect", "Lovely", "Ideal", "Just right"],
        [TierId.LightJacket] = ["Crisp", "Fresh", "Bracing", "Snappy"],
        [TierId.SweaterWeather] = ["Cozy", "Snug", "Toasty knit", "Woolly"],
        [TierId.CoatUp] = ["Brisk", "Biting", "Sharp cold", "Frosty"],
        [TierId.Bundle] = ["Frigid", "Icy", "Bone-cold", "Harsh"],
        [TierId.Survival] = ["Dangerous", "Brutal", "Extreme", "Stay inside"]
    };

    private static string GetDefaultVibe(TierId tier)
    {
        if (VibePools.TryGetValue(tier, out var pool))
        {
            return pool[Random.Shared.Next(pool.Length)];
        }

        return "Steady";
    }

    private static readonly Dictionary<TierId, string[]> GarmentPools = new()
    {
        [TierId.Scorcher] = ["Tank top", "Linen shorts", "Crop top", "Flowy sundress", "Slides"],
        [TierId.Hot] = ["Sundress", "Linen set", "Camp shirt", "Cotton shorts", "Breathable tee"],
        [TierId.Warm] = ["Classic tee", "Polo shirt", "Wrap dress", "Light chinos", "Canvas sneakers"],
        [TierId.Pleasant] = ["Button-down", "Henley", "Midi skirt", "Linen blazer", "Light cardigan"],
        [TierId.LightJacket] = ["Light jacket", "Denim jacket", "Bomber", "Shacket", "Overshirt"],
        [TierId.SweaterWeather] = ["Knit sweater", "Turtleneck", "Cable cardigan", "Fleece pullover", "Wool vest"],
        [TierId.CoatUp] = ["Winter coat", "Wool overcoat", "Puffer jacket", "Peacoat", "Insulated parka"],
        [TierId.Bundle] = ["Heavy parka", "Down coat", "Fleece mid-layer", "Insulated vest", "Thermal base"],
        [TierId.Survival] = ["Arctic parka", "Expedition shell", "Balaclava", "Snow pants", "Insulated boots"]
    };

    private static string GetGarmentForTier(TierId tier)
    {
        if (GarmentPools.TryGetValue(tier, out var pool))
        {
            return pool[Random.Shared.Next(pool.Length)];
        }

        return "Layers";
    }

    private static readonly Dictionary<TierId, string[]> ClothingEmojiPools = new()
    {
        [TierId.Scorcher] = ["🩳", "👙", "🩴", "👕", "🧢", "👗", "🩱", "👡", "👚", "🎀", "👒", "🕶️"],
        [TierId.Hot] = ["👗", "🩳", "👕", "🩴", "👒", "🧢", "👙", "👡", "👚", "👘", "🕶️", "🎀"],
        [TierId.Warm] = ["👕", "👖", "👟", "👗", "🕶️", "👔", "👚", "🥿", "👘", "🩰", "👜", "💍"],
        [TierId.Pleasant] = ["👔", "👖", "👟", "👗", "🧥", "👚", "👢", "🥿", "🩰", "👞", "👜", "💍"],
        [TierId.LightJacket] = ["🧥", "👖", "👢", "🧣", "👕", "👔", "🧢", "👞", "🎒", "👜", "👚", "🥿"],
        [TierId.SweaterWeather] = ["🧶", "🧣", "👢", "👖", "🧤", "🧥", "👕", "👞", "🎒", "🥾", "👜", "👓"],
        [TierId.CoatUp] = ["🧥", "🧤", "🧣", "👢", "🧶", "👖", "🧢", "🥾", "🎒", "👞", "👓", "🧦"],
        [TierId.Bundle] = ["🧤", "🧣", "🧥", "👢", "🧶", "🧦", "🧢", "🥾", "🎒", "👓", "🥽", "👖"],
        [TierId.Survival] = ["🧣", "🧤", "🧥", "👢", "🧶", "🧦", "🧢", "🥾", "🥽", "🎒", "👓", "👖"]
    };

    private static readonly string[] RainEmojis = ["☂️", "🧥", "👢", "🥾", "🧥", "👜", "🎒", "☂️", "👢"];
    private static readonly string[] SnowEmojis = ["🧣", "🧤", "👢", "🧥", "🧶", "🥾", "🥽", "🧦", "🧢"];
    private static readonly string[] FogEmojis = ["👓", "🧥", "🧣", "👢", "🧶", "🎒", "👖", "👞"];

    private static string GetClothingEmojiForTier(TierId tier, bool isRaining, bool isSnowing, int index = 0, bool isFoggy = false)
    {
        if (isSnowing) return SnowEmojis[index % SnowEmojis.Length];
        if (isRaining) return RainEmojis[index % RainEmojis.Length];
        if (isFoggy) return FogEmojis[index % FogEmojis.Length];

        if (ClothingEmojiPools.TryGetValue(tier, out var pool))
        {
            return pool[index % pool.Length];
        }

        return "👕";
    }

    // ───────────────────────────────────────────────
    //  Daily forecasts
    // ───────────────────────────────────────────────

    private static List<DayForecast> BuildDaily(IList<DailyWeather> daily)
    {
        var forecasts = new List<DayForecast>();

        for (int i = 0; i < daily.Count && forecasts.Count < 7; i++)
        {
            var d = daily[i];
            bool isToday = i == 0;

            var tier = ResolveTier(d.ApparentTempMid);
            var headline = tier.Headlines[Random.Shared.Next(tier.Headlines.Length)];

            // Build fabric list for this day
            var fabrics = BuildDailyFabrics(d, tier.Id);

            // Tips - editorial styling notes
            var tip = GetDayTip(tier.Id, d);

            // Back label
            var backLabel = GetBackLabel(tier.Id, d);

            // Back fabric detail
            var backFabricDetail = GetBackFabricDetail(tier.Id, d);

            // Day abbreviation
            string dayAbbrev;
            if (DateTime.TryParse(d.Date, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                dayAbbrev = parsedDate.ToString("ddd", CultureInfo.InvariantCulture).ToUpperInvariant();
            }
            else
            {
                dayAbbrev = "---";
            }

            bool isDayFoggy = d.WeatherCode is 45 or 48;

            forecasts.Add(new DayForecast(
                Date: d.Date,
                DayAbbrev: dayAbbrev,
                Headline: headline,
                Emoji: GetClothingEmojiForTier(tier.Id, d.IsRainy, d.IsSnowy, i, isDayFoggy),
                TierId: tier.Id,
                Fabrics: fabrics.ToImmutableList(),
                SwatchColor: tier.SwatchColor,
                Tip: tip,
                BackLabel: backLabel,
                BackFabricDetail: backFabricDetail,
                IsToday: isToday
            ));
        }

        return forecasts;
    }

    private static List<string> BuildDailyFabrics(DailyWeather d, TierId tier)
    {
        var fabrics = new List<string>();
        var midTemp = d.ApparentTempMid;

        // cotton-friendly
        if (!d.IsRainy && midTemp >= 55 && midTemp <= 85)
        {
            fabrics.Add("cotton-friendly");
        }

        // moisture-wicking
        if (midTemp > 85)
        {
            fabrics.Add("moisture-wicking");
        }

        // waterproof
        if (d.PrecipProbabilityMax > 50 || d.IsRainy || d.IsSnowy)
        {
            fabrics.Add("waterproof");
        }

        // closed-toe
        if (midTemp < 65 || d.IsRainy || d.IsSnowy)
        {
            fabrics.Add("closed-toe");
        }

        // sunscreen-worthy
        if (d.UvIndexMax >= 6)
        {
            fabrics.Add("sunscreen-worthy");
        }

        // windproof
        if (d.WindSpeedMax > 20)
        {
            fabrics.Add("windproof");
        }

        // breathable
        if (midTemp > 75)
        {
            fabrics.Add("breathable");
        }

        // open-toe OK
        if (midTemp >= 70 && !d.IsRainy && !d.IsSnowy)
        {
            fabrics.Add("open-toe OK");
        }

        return fabrics;
    }

    private static readonly string[] SnowTips =
    [
        "Avoid exposed skin. Double up on every layer.",
        "Waterproof boots are essential. Tuck pants into socks.",
        "Snow-proof your shoes and bring an extra pair of dry socks.",
        "Waterproof boots and a warm coat will see you through the flurries.",
        "Dark-soled boots hide salt stains. Your future self will thank you.",
        "Merino wool socks dry faster than cotton if your feet get damp.",
        "Swap laces for zip-up boots - easier with cold fingers.",
        "A bright-colored outer layer helps visibility in heavy snow."
    ];

    private static readonly string[] RainTips =
    [
        "A light rain shell keeps you dry without overheating.",
        "Bring a compact umbrella and stick to darker fabrics that hide water marks.",
        "Waterproof outer layer is a must. Umbrella in bag, always.",
        "Skip suede and canvas today. Stick to leather or synthetics.",
        "Roll your pants up a notch to dodge puddle splash-back.",
        "A trench coat looks intentional even when it's just practical.",
        "Waxed cotton has old-school rain resistance and ages beautifully.",
        "Layer a thin rain shell under a blazer for dry, polished commuting."
    ];

    private static readonly Dictionary<TierId, string[]> DayTipPools = new()
    {
        [TierId.Scorcher] =
        [
            "Stick to light colors that reflect heat. Linen is your best friend.",
            "Wet a bandana and drape it around your neck for instant relief.",
            "White and cream tones keep you cooler than you'd expect.",
            "Avoid dark denim today. It absorbs heat and clings when you sweat.",
            "Loose sleeves catch a breeze. Fitted anything traps heat."
        ],
        [TierId.Hot] =
        [
            "Loose fits and breathable fabrics keep you cool without sacrificing style.",
            "Linen wrinkles are a feature, not a flaw. Lean into it.",
            "A breezy midi dress does the work of an entire outfit.",
            "Swap your belt for elastic waistbands - comfort is king in heat.",
            "Cropped pants let air circulate around your ankles."
        ],
        [TierId.Warm] =
        [
            "This is anything-goes weather. Experiment with that outfit you've been saving.",
            "Roll your sleeves for a casual look that adapts to the temperature.",
            "Try a chambray shirt open over a tee - effortless layering.",
            "Canvas sneakers and a good tee are all you need today.",
            "Swap your jeans for chinos. They breathe better and look just as good."
        ],
        [TierId.Pleasant] =
        [
            "Light layers let you adapt as the day shifts. A cardigan ties it together.",
            "Throw a denim jacket over anything and it immediately looks put together.",
            "This is the day to wear that outfit you've been planning.",
            "A silk scarf adds polish without warmth. Perfect for transitional temps.",
            "Try mixing textures: cotton tee under a linen blazer is chef's kiss."
        ],
        [TierId.LightJacket] =
        [
            "A structured jacket elevates any basic outfit. Roll the sleeves if it warms up.",
            "Your denim jacket is the MVP of this temperature range.",
            "Try an overshirt buttoned halfway for that relaxed layered look.",
            "Pair boots with cuffed jeans for an effortless autumn silhouette.",
            "A scarf draped loosely adds style without bulk."
        ],
        [TierId.SweaterWeather] =
        [
            "Texture is everything - try mixing knits with denim for that effortless fall look.",
            "A chunky turtleneck eliminates the need for any accessories above the chest.",
            "Layer a thin cardigan under a coat for double insulation without bulk.",
            "Corduroy pairs beautifully with cable knit. Texture on texture.",
            "A beanie isn't just warm, it ties a whole outfit together."
        ],
        [TierId.CoatUp] =
        [
            "Your coat is the statement piece. Choose one that works with everything underneath.",
            "Tonal outfits - all one color family - look elevated under a structured coat.",
            "Tuck your scarf into your coat lapels for a cleaner silhouette.",
            "A belt over a long coat cinches the waist and adds structure.",
            "Layering a vest under your coat adds core warmth without arm bulk."
        ],
        [TierId.Bundle] =
        [
            "Tuck your scarf into your coat for a polished, wind-sealed look.",
            "Thermal base layers are invisible but make the biggest difference.",
            "Mittens are warmer than gloves - save dexterity for indoors.",
            "A fleece-lined hoodie under a parka is the warmth cheat code.",
            "Double up on socks only if your shoes have room. Tight shoes mean cold feet."
        ],
        [TierId.Survival] =
        [
            "Minimize time outside. If you must go, cover every inch of skin.",
            "Petroleum jelly on exposed skin (nose, cheeks) prevents windburn.",
            "Breathe through a scarf to warm the air before it hits your lungs.",
            "Hand warmers in your pockets are a small investment with huge returns.",
            "Skip metal jewelry - it conducts cold directly to your skin."
        ]
    };

    private static string GetDayTip(TierId tier, DailyWeather d)
    {
        if (d.IsSnowy) return SnowTips[Random.Shared.Next(SnowTips.Length)];
        if (d.IsRainy) return RainTips[Random.Shared.Next(RainTips.Length)];

        if (DayTipPools.TryGetValue(tier, out var pool))
            return pool[Random.Shared.Next(pool.Length)];

        return "Dress in layers and adjust as you go.";
    }

    private static string GetBackLabel(TierId tier, DailyWeather d)
    {
        if (d.IsSnowy) return "Snow Tip";
        if (d.IsRainy) return "Rain Tip";
        if (tier == TierId.Survival) return "Survival Tip";
        return "Styling Note";
    }

    private static readonly string[] SnowFabricDetails =
    [
        "Prioritize waterproof, insulated fabrics. Merino wool base layers wick moisture while keeping you warm.",
        "Gore-Tex shells over fleece mid-layers give you wind and water protection without overheating.",
        "Synthetic down dries faster than natural down if it gets damp. A smart pick in wet snow.",
        "Wool-blend socks with sealed-seam boots keep feet warm even in slush."
    ];

    private static readonly string[] RainFabricDetails =
    [
        "Go for water-resistant shells and quick-dry fabrics. Avoid cotton - it holds moisture and gets heavy.",
        "Nylon and polyester shells shed water naturally. Save the wool for underneath.",
        "Waxed cotton jackets develop character over time and handle drizzle beautifully.",
        "Quick-dry synthetics underneath a shell mean you recover fast if you get caught out."
    ];

    private static readonly Dictionary<TierId, string[]> FabricDetailPools = new()
    {
        [TierId.Scorcher] =
        [
            "Linen and lightweight cotton are your MVPs. Avoid synthetics that trap heat.",
            "Bamboo-blend fabrics feel silky and cool against the skin. Worth trying.",
            "Seersucker's puckered weave creates airflow pockets against the body. Genius for heat.",
            "Loose-weave cotton gauze lets air circulate. The breezier the better."
        ],
        [TierId.Hot] =
        [
            "Breathable cotton blends and moisture-wicking fabrics keep you fresh all day.",
            "Rayon and viscose drape beautifully and breathe better than polyester.",
            "Tencel (lyocell) is smooth, cool, and more sustainable than traditional synthetics.",
            "Jersey knit in cotton or modal gives stretch and breathability in one."
        ],
        [TierId.Warm] =
        [
            "Cotton, chambray, and light jersey - all great picks for this range.",
            "Poplin shirts feel crisp and look polished without weighing you down.",
            "French terry is the Goldilocks fabric here - not too warm, not too light.",
            "Light linen-cotton blends give you linen's airiness without all the wrinkles."
        ],
        [TierId.Pleasant] =
        [
            "Almost any fabric works. Layer with cotton tees and light knits.",
            "Ponte knit blazers stretch and breathe. Perfect for this transitional range.",
            "Cotton chinos in mid-weight are the do-everything bottom for this temperature.",
            "Silk scarves add a layer of polish without any real warmth penalty."
        ],
        [TierId.LightJacket] =
        [
            "Denim, canvas, and mid-weight cotton make great outer layers.",
            "Flannel shirts work as both a mid-layer and a standalone. Double duty.",
            "Brushed cotton has a soft hand feel that adds comfort without heaviness.",
            "Moleskin fabric has a suede-like feel with real warmth. A hidden gem."
        ],
        [TierId.SweaterWeather] =
        [
            "Wool, cashmere blends, and heavier knits add warmth and texture.",
            "Merino wool regulates temperature naturally and resists odor. Layer it everywhere.",
            "Cable-knit patterns trap air pockets, which is what actually keeps you warm.",
            "Alpaca wool is lighter than sheep's wool but warmer. The upgrade pick."
        ],
        [TierId.CoatUp] =
        [
            "Wool coats, down-fill, and fleece-lined pieces are your warmth essentials.",
            "Boiled wool is denser than knit wool and naturally wind-resistant.",
            "Quilted linings in your coat add insulation without visible bulk.",
            "Sherpa fleece under a shell gives you warmth that punches above its weight."
        ],
        [TierId.Bundle] =
        [
            "Merino wool base layers under heavy fleece and an insulated outer shell.",
            "Synthetic insulation (Primaloft, Thinsulate) stays warm even when damp.",
            "Layering thin fabrics traps more air than one thick layer. Three thin beats one thick.",
            "Wind-blocking membranes in your outer layer make the biggest difference in perceived cold."
        ],
        [TierId.Survival] =
        [
            "Expedition-weight thermals, windproof shells, and insulated waterproof everything.",
            "Avoid cotton entirely. It loses all insulation when damp. Synthetics and wool only.",
            "Vapor barrier liners (VBLs) inside your gloves and boots prevent moisture buildup in extreme cold.",
            "Balaclava-grade fleece covers your face without restricting breathing. Essential below zero."
        ]
    };

    private static string GetBackFabricDetail(TierId tier, DailyWeather d)
    {
        if (d.IsSnowy) return SnowFabricDetails[Random.Shared.Next(SnowFabricDetails.Length)];
        if (d.IsRainy) return RainFabricDetails[Random.Shared.Next(RainFabricDetails.Length)];

        if (FabricDetailPools.TryGetValue(tier, out var pool))
            return pool[Random.Shared.Next(pool.Length)];

        return "Layer smart - mix weights and textures for comfort and style.";
    }

    // ───────────────────────────────────────────────
    //  Alerts
    // ───────────────────────────────────────────────

    private static List<Alert> BuildAlerts(IList<DailyWeather> daily)
    {
        // Check future days only (skip index 0 which is today)
        Alert? highestPriorityAlert = null;
        int highestPriority = int.MaxValue;

        for (int i = 1; i < daily.Count; i++)
        {
            var d = daily[i];
            string dayName = "that day";
            if (DateTime.TryParse(d.Date, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                dayName = parsedDate.ToString("dddd", CultureInfo.InvariantCulture);
            }

            // Priority 1: Snow (weatherCode 71-77)
            if (d.WeatherCode >= 71 && d.WeatherCode <= 77 && highestPriority > 1)
            {
                highestPriority = 1;
                highestPriorityAlert = new Alert(
                    "\U0001f328\ufe0f",
                    $"Snow Advisory for {dayName}",
                    "Snowfall expected. Plan for waterproof boots, extra layers, and slower commutes."
                );
            }

            // Priority 2: Extreme Cold (survival tier)
            var tier = ResolveTier(d.ApparentTempMid);
            if (tier.Id == TierId.Survival && highestPriority > 2)
            {
                highestPriority = 2;
                highestPriorityAlert = new Alert(
                    "\U0001f976",
                    $"Extreme Cold Advisory for {dayName}",
                    "Dangerously cold temperatures ahead. Limit outdoor exposure and layer aggressively."
                );
            }

            // Priority 3: Rain (precipProbMax >70% AND precipSum >5mm)
            if (d.PrecipProbabilityMax > 70 && d.PrecipSum > 5 && highestPriority > 3)
            {
                highestPriority = 3;
                highestPriorityAlert = new Alert(
                    "\u2602\ufe0f",
                    $"Rain Advisory for {dayName}",
                    "Significant rain expected. Pack an umbrella and wear waterproof layers."
                );
            }

            // Priority 4: Extreme Heat (scorcher tier)
            if (tier.Id == TierId.Scorcher && highestPriority > 4)
            {
                highestPriority = 4;
                highestPriorityAlert = new Alert(
                    "\U0001f525",
                    $"Extreme Heat Advisory for {dayName}",
                    "Scorching temperatures incoming. Stay hydrated, wear SPF, and keep it minimal."
                );
            }

            // Priority 5: Wind (windSpeedMax >35mph)
            if (d.WindSpeedMax > 35 && highestPriority > 5)
            {
                highestPriority = 5;
                highestPriorityAlert = new Alert(
                    "\U0001f4a8",
                    $"Wind Advisory for {dayName}",
                    "High winds expected. Secure loose items and add a windproof layer."
                );
            }

            // Priority 6: UV (uvIndexMax >=8)
            if (d.UvIndexMax >= 8 && highestPriority > 6)
            {
                highestPriority = 6;
                highestPriorityAlert = new Alert(
                    "\u2600\ufe0f",
                    $"UV Advisory for {dayName}",
                    "Extreme UV levels ahead. Sunscreen, hat, and sunglasses are non-negotiable."
                );
            }
        }

        var result = new List<Alert>();
        if (highestPriorityAlert is not null)
        {
            result.Add(highestPriorityAlert);
        }

        return result;
    }
}
