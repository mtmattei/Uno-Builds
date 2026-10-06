namespace SmartCity.Services;

/// <summary>
/// In-memory seed for the Predictive AI → Building screen. Numbers mirror the reference
/// design (332→204 daily, 1 437→1 204 weekly, 10.4k→8.1k monthly, 154.3k→125.2k yearly;
/// 4.3k$ / 2.4k kWh / 135 CO₂; floor 14 flagged). Decision 3: mock service behind MVUX feeds.
/// </summary>
public class MockEnergyService : IEnergyService
{
    public ValueTask<IImmutableList<FloorReading>> GetFloorsAsync(CancellationToken ct = default)
    {
        // 11F–17F, top floor first to match the right-edge ruler. 14F is the flagged target.
        var floors = ImmutableArray.Create(
            new FloorReading(17, "17F", 298, 241, false),
            new FloorReading(16, "16F", 274, 223, false),
            new FloorReading(15, "15F", 311, 252, false),
            new FloorReading(14, "14F", 332, 204, true),
            new FloorReading(13, "13F", 289, 235, false),
            new FloorReading(12, "12F", 305, 248, false),
            new FloorReading(11, "11F", 281, 229, false));

        return new ValueTask<IImmutableList<FloorReading>>(floors);
    }

    public ValueTask<IImmutableList<HourlyConsumption>> GetHourlyAsync(CancellationToken ct = default)
    {
        var series = ImmutableArray.Create(
            new HourlyConsumption("06:00", 220, 165),
            new HourlyConsumption("08:00", 144, 110),
            new HourlyConsumption("10:00", 284, 212),
            new HourlyConsumption("12:00", 348, 248),
            new HourlyConsumption("14:00", 262, 196),
            new HourlyConsumption("16:00", 151, 118),
            new HourlyConsumption("18:00", 291, 214),
            new HourlyConsumption("20:00", 198, 150),
            new HourlyConsumption("Now", 168, 126));

        return new ValueTask<IImmutableList<HourlyConsumption>>(series);
    }

    public ValueTask<ConsumptionRollup> GetRollupAsync(CancellationToken ct = default)
    {
        var rollup = new ConsumptionRollup(
            Daily: new Metric(332, 204),
            Weekly: new Metric(1437, 1204),
            Monthly: new Metric(10400, 8100),
            Yearly: new Metric(154300, 125200));

        return new ValueTask<ConsumptionRollup>(rollup);
    }

    public ValueTask<AiSuggestion?> GetSuggestionAsync(CancellationToken ct = default)
    {
        var suggestion = new AiSuggestion(
            Title: "Optimize ventilation at 14F in the office sector",
            Body: "Over the last 2 weeks the office sector was unoccupied 70% of the time during " +
                  "non-business hours. By implementing dynamic ventilation, there is a potential " +
                  "25% reduction in energy consumption.",
            ReductionPct: 0.25,
            TargetFloor: 14);

        return new ValueTask<AiSuggestion?>(suggestion);
    }

    public ValueTask<SavingsSummary> GetSavingsAsync(CancellationToken ct = default)
    {
        var trend = new[]
        {
            new TrendPoint("1 Dec", 2600),
            new TrendPoint("5 Dec", 3400),
            new TrendPoint("10 Dec", 2900),
            new TrendPoint("15 Dec", 5200),
            new TrendPoint("20 Dec", 4100),
            new TrendPoint("25 Dec", 6300),
            new TrendPoint("30 Dec", 4800),
        };

        var savings = new SavingsSummary(
            CostSaved: 4300,
            EnergySaved: 2400,
            Co2Avoided: 135,
            Trend: trend);

        return new ValueTask<SavingsSummary>(savings);
    }
}
