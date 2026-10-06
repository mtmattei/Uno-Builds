namespace SmartCity.Services;

/// <summary>
/// Backing data for the Predictive AI → Building dashboard. Async signatures keep the
/// MVUX feeds genuinely asynchronous even though the data is local mock content.
/// </summary>
public interface IEnergyService
{
    /// <summary>Per-floor readings for the building twin (11F–17F; 14F flagged).</summary>
    ValueTask<IImmutableList<FloorReading>> GetFloorsAsync(CancellationToken ct = default);

    /// <summary>Hourly bar-chart series: current vs after-optimization.</summary>
    ValueTask<IImmutableList<HourlyConsumption>> GetHourlyAsync(CancellationToken ct = default);

    /// <summary>Daily / Weekly / Monthly / Yearly KPI roll-ups.</summary>
    ValueTask<ConsumptionRollup> GetRollupAsync(CancellationToken ct = default);

    /// <summary>The current AI optimization suggestion, or <c>null</c> when none is suggested.</summary>
    ValueTask<AiSuggestion?> GetSuggestionAsync(CancellationToken ct = default);

    /// <summary>Monthly savings roll-up plus the trend series.</summary>
    ValueTask<SavingsSummary> GetSavingsAsync(CancellationToken ct = default);
}
