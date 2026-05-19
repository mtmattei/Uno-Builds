using System;
using System.Linq;
using System.Text.RegularExpressions;
using Composer.Models;

namespace Composer.Services;

/// <summary>
/// Regex-rule context deriver. 13 keyword groups → entity noun. Verbatim
/// from <c>docs/ARCHITECTURE-BRIEF-from-scratch.md</c> §10.2 — keep the
/// rule order identical so the same intent produces the same domain noun
/// the brief's expected derivations target.
///
/// Singleton lifetime: no per-call state, pre-compiled regexes.
/// </summary>
public sealed class ContextDeriver : IContextDeriver
{
    private static readonly (Regex Match, string Noun)[] Rules = new[]
    {
        (new Regex(@"habit|streak",                            RegexOptions.IgnoreCase | RegexOptions.Compiled), "habit"),
        (new Regex(@"recipe|cook|meal",                        RegexOptions.IgnoreCase | RegexOptions.Compiled), "recipe"),
        (new Regex(@"workout|exercise|fitness",                RegexOptions.IgnoreCase | RegexOptions.Compiled), "workout"),
        (new Regex(@"trade|portfolio|invest|stock",            RegexOptions.IgnoreCase | RegexOptions.Compiled), "trade"),
        (new Regex(@"task|todo|backlog",                       RegexOptions.IgnoreCase | RegexOptions.Compiled), "task"),
        (new Regex(@"note|journal|diary",                      RegexOptions.IgnoreCase | RegexOptions.Compiled), "note"),
        (new Regex(@"appointment|booking|reserv",              RegexOptions.IgnoreCase | RegexOptions.Compiled), "appointment"),
        (new Regex(@"patient|medical|health|clinic",           RegexOptions.IgnoreCase | RegexOptions.Compiled), "patient"),
        (new Regex(@"invoice|billing|payment",                 RegexOptions.IgnoreCase | RegexOptions.Compiled), "invoice"),
        (new Regex(@"order|purchase|cart",                     RegexOptions.IgnoreCase | RegexOptions.Compiled), "order"),
        (new Regex(@"ticket|incident|issue",                   RegexOptions.IgnoreCase | RegexOptions.Compiled), "ticket"),
        (new Regex(@"lesson|class|course",                     RegexOptions.IgnoreCase | RegexOptions.Compiled), "lesson"),
        (new Regex(@"dispatch|field-service|job|service-call", RegexOptions.IgnoreCase | RegexOptions.Compiled), "job"),
    };

    private static readonly Regex OfflineFirst =
        new(@"offline|local|queue|sync\s+later|no\s+backend", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MobileFirst =
        new(@"mobile|phone|tablet|on-the-go", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex NonAlnum =
        new("[^a-zA-Z0-9 ]", RegexOptions.Compiled);

    public DerivedContext Derive(IntentValues intent)
    {
        var blob = string.Join(' ',
            new[] { intent?.AppType, intent?.Workflow, intent?.PrimaryUser }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

        var matched = Rules.FirstOrDefault(r => r.Match.IsMatch(blob));
        var noun = matched.Noun ?? "item";
        var title = char.ToUpper(noun[0]) + noun[1..];
        var plural = noun.EndsWith('s') ? noun : noun + "s";

        var cleaned = NonAlnum.Replace(intent?.AppType ?? "App", "");
        var appName = string.Concat(cleaned
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => char.ToUpper(w[0]) + w[1..]));
        if (string.IsNullOrWhiteSpace(appName)) appName = "App";

        var userParts = (intent?.PrimaryUser ?? "users").ToLowerInvariant().Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var userNoun = userParts.Length > 0 ? userParts[^1] : "users";

        return new DerivedContext(
            AppName:        appName,
            EntityNoun:     noun,
            EntityTitle:    title,
            EntityPlural:   plural,
            UserNoun:       userNoun,
            IsOfflineFirst: OfflineFirst.IsMatch(blob),
            IsMobileFirst:  MobileFirst.IsMatch(blob),
            // Brief's IsFieldService check: matches the default example app
            // type verbatim. Used by Architecture/UX to keep the canvas in
            // archetype-mode until the user names something specific.
            IsFieldService: intent?.AppType == IntentValues.Default.AppType);
    }
}
