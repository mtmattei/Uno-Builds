using System.Globalization;

namespace SmartCity.Models;

/// <summary>
/// Display formatting for the dashboard. WinUI <c>{Binding}</c> has no <c>StringFormat</c>, so the
/// MVUX projections format here and the XAML binds plain strings.
/// </summary>
public static class Format
{
    private static readonly CultureInfo Ci = CultureInfo.InvariantCulture;

    /// <summary>
    /// Headline optimization figure the AI suggestion advertises. The reference UI labels every
    /// savings chip "+25%" (the suggestion's claim), so the chips use this rather than the per-metric
    /// computed reduction — the optimized *values* are still the real numbers.
    /// </summary>
    public const double OptimizationPct = 0.25;

    /// <summary>Compact magnitude: 332 → "332", 10400 → "10.4k", 154300 → "154.3k".</summary>
    public static string Compact(double v) =>
        v >= 1000 ? (v / 1000d).ToString("0.#", Ci) + "k" : v.ToString("0", Ci);

    public static string Kwh(double v) => Compact(v) + " kWh";

    /// <summary>Savings chip: 0.25 → "+25%", ≤0 → "0%".</summary>
    public static string Pct(double reduction) =>
        reduction <= 0 ? "0%" : "+" + (reduction * 100).ToString("0", Ci) + "%";
}
