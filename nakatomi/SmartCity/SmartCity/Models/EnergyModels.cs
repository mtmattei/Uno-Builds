namespace SmartCity.Models;

/// <summary>Scale selector levels. Only <see cref="Building"/> is wired in this build.</summary>
public enum BuildingScale
{
    City,
    District,
    Street,
    Building,
}

/// <summary>One floor of the building twin. Floor 14 is the flagged optimization target.</summary>
public record FloorReading(int Floor, string Label, double CurrentKwh, double OptimizedKwh, bool IsFlagged);

/// <summary>Display projection of the live (lit) floor for the left-column panel and the twin tooltip.</summary>
public record FloorView(string Title, string Label, string CurrentText, string OptimizedText, string ReductionText, string StatusText, bool IsFlagged)
{
    public static FloorView From(FloorReading f) => new(
        $"{f.Floor} Floor",
        f.Label,
        Format.Kwh(f.CurrentKwh),
        Format.Kwh(f.OptimizedKwh),
        Format.Pct(Format.OptimizationPct),
        f.IsFlagged ? "Optimization target" : "Nominal",
        f.IsFlagged);
}

/// <summary>A single bar in the energy-consumption chart (current vs after-optimization).</summary>
public record HourlyConsumption(string TimeLabel, double CurrentKwh, double OptimizedKwh);

/// <summary>Bar-chart series bundled with the Applied flag so the chart reads both from one feed value.</summary>
public record HourlyView(IImmutableList<HourlyConsumption> Series, bool Applied);

/// <summary>Current vs optimized consumption for one rollup window. Reduction is computed, never stored (Decision 4).</summary>
public record Metric(double Current, double Optimized)
{
    public double ReductionPct => Current <= 0 ? 0 : (Current - Optimized) / Current;
}

/// <summary>Daily / Weekly / Monthly / Yearly KPI roll-ups.</summary>
public record ConsumptionRollup(Metric Daily, Metric Weekly, Metric Monthly, Metric Yearly);

/// <summary>The blue AI suggestion card content.</summary>
public record AiSuggestion(string Title, string Body, double ReductionPct, int TargetFloor);

/// <summary>One point on the monthly-savings trend line.</summary>
public record TrendPoint(string DateLabel, double Value);

/// <summary>Monthly savings roll-up: cost, energy, CO₂ avoided, plus the trend series.</summary>
public record SavingsSummary(double CostSaved, double EnergySaved, double Co2Avoided, TrendPoint[] Trend)
{
    public string CostText => "$" + Format.Compact(CostSaved);
    public string EnergyText => Format.Kwh(EnergySaved);
    public string Co2Text => Format.Compact(Co2Avoided) + " kg";
}

/// <summary>One KPI tile: headline kWh (current, or optimized once applied), the optimized value, and the savings chip.</summary>
public record KpiTile(string Title, double Kwh, double Optimized, double ReductionPct)
{
    public string KwhText => Format.Kwh(Kwh);
    public string OptimizedText => Format.Kwh(Optimized);
    public string ReductionText => Format.Pct(ReductionPct);
}

/// <summary>The four KPI tiles, resolved for the current Applied state (Decision 4 reduction computed).</summary>
public record RollupView(KpiTile Daily, KpiTile Weekly, KpiTile Monthly, KpiTile Yearly);
