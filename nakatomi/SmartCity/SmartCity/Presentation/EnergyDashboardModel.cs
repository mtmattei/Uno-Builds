namespace SmartCity.Presentation;

/// <summary>
/// MVUX surface for the Predictive AI → Building dashboard. Exposes the energy feeds, the
/// simulated active-floor state (two-way bound to the twin so the left column populates from the
/// same source), and the Apply / Dismiss commands. Data comes from <see cref="IEnergyService"/>.
/// </summary>
public partial record EnergyDashboardModel(IEnergyService Energy)
{
    private const int FlaggedFloorIndex = 3; // 14F sits at index 3 of the 17F..11F list

    // Building is the only wired scale in this build; the selector is present but inert otherwise.
    public IState<BuildingScale> Scale => State<BuildingScale>.Value(this, () => BuildingScale.Building);

    // ── Simulation / selection ────────────────────────────────────────────────────────────────
    // The twin advances this on its render cadence (two-way) and clicking a floor sets it; the
    // active-floor panel + chart highlight read from it. Starts on the flagged optimization floor.
    public IState<int> ActiveFloorIndex => State<int>.Value(this, () => FlaggedFloorIndex);

    // ── Source feeds ──────────────────────────────────────────────────────────────────────────
    public IFeed<IImmutableList<FloorReading>> Floors => Feed.Async(Energy.GetFloorsAsync);
    private IFeed<IImmutableList<HourlyConsumption>> Hourly => Feed.Async(Energy.GetHourlyAsync);
    public IFeed<SavingsSummary> Savings => Feed.Async(Energy.GetSavingsAsync);

    /// <summary>Bar-chart series + Applied flag in one value so the chart binds both from Data.</summary>
    public IFeed<HourlyView> HourlyChart =>
        Feed.Combine(Hourly, Applied).Select(t => new HourlyView(t.Item1, t.Item2));
    private IFeed<ConsumptionRollup> Rollup => Feed.Async(Energy.GetRollupAsync);
    private IFeed<AiSuggestion> RawSuggestion =>
        Feed.Async(async ct => (await Energy.GetSuggestionAsync(ct))!);

    // ── Apply / Dismiss state ─────────────────────────────────────────────────────────────────
    public IState<bool> Applied => State<bool>.Value(this, () => false);
    private IState<bool> Dismissed => State<bool>.Value(this, () => false);

    /// <summary>The floor currently lit by the simulation (or clicked) — drives the live left-column panel.</summary>
    public IFeed<FloorView> ActiveFloor =>
        Feed.Combine(Floors, ActiveFloorIndex)
            .Select(t => FloorView.From(t.Item1[Math.Clamp(t.Item2, 0, t.Item1.Count - 1)]));

    /// <summary>Suggestion card content; filters to None once dismissed (FeedView shows the empty tile).</summary>
    public IFeed<AiSuggestion> Suggestion =>
        Feed.Combine(RawSuggestion, Dismissed)
            .Where(t => !t.Item2)
            .Select(t => t.Item1);

    /// <summary>KPI tiles resolved against Applied: headline shows optimized values post-apply.</summary>
    public IFeed<RollupView> Kpis =>
        Feed.Combine(Rollup, Applied)
            .Select(t => Project(t.Item1, t.Item2));

    // Headline number = current (matches the reference tiles); optimized shown beneath with the +25%
    // chip. Applied flips the headline to the optimized value so the tiles animate post-apply.
    private static RollupView Project(ConsumptionRollup r, bool applied) => new(
        Tile("Daily consumption", r.Daily, applied),
        Tile("Weekly consumption", r.Weekly, applied),
        Tile("Monthly consumption", r.Monthly, applied),
        Tile("Yearly consumption", r.Yearly, applied));

    private static KpiTile Tile(string title, Metric m, bool applied) =>
        new(title, applied ? m.Optimized : m.Current, m.Optimized, Format.OptimizationPct);

    // ── Commands (auto-generated on the bindable proxy) ───────────────────────────────────────
    /// <summary>Commit the optimization — KPIs + chart move to their optimized values.</summary>
    public async ValueTask ApplySuggestion(CancellationToken ct) => await Applied.UpdateAsync(_ => true, ct);

    /// <summary>Clear the suggestion card.</summary>
    public async ValueTask DismissSuggestion(CancellationToken ct) => await Dismissed.UpdateAsync(_ => true, ct);

    /// <summary>Select a floor by list index (0 = 17F … 6 = 11F); pauses on it until the sim advances.</summary>
    public async ValueTask SelectFloor(int index, CancellationToken ct) =>
        await ActiveFloorIndex.UpdateAsync(_ => index, ct);
}
