using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Composer.Models;
using Microsoft.Extensions.Options;
using Windows.UI;

namespace Composer.Services;

/// <summary>
/// Anthropic-backed layer preview + initial-seed service. Both entry points
/// dispatch by <see cref="LayerKind"/> to a layer-specific JSON contract,
/// send to Sonnet via the Refit <see cref="IAnthropicClient"/>, and parse the
/// proposed values back into the layer's typed record. UX and DesignSystem
/// route through the vision endpoint when reference screenshots are supplied
/// so Sonnet can see the user's reference images.
///
/// Identity fallback (no API key, network error, parse failure): preview
/// returns the current values unchanged with "Showing your edits as proposed.";
/// initial-seed returns null so the caller falls back to the layer's
/// hardcoded <c>Default</c>.
/// </summary>
public sealed class LayerPreviewService : ILayerPreviewService
{
    private readonly IAnthropicClient _api;
    private readonly IOptions<AnthropicConfig> _options;

    private AnthropicConfig Cfg => _options.Value;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
    };

    public LayerPreviewService(IAnthropicClient api, IOptions<AnthropicConfig> options)
    {
        _api = api;
        _options = options;
    }

    // ────────────────────────────────────────────────────────────────────
    //  GenerateInitialAsync — workspace-entry seeding
    // ────────────────────────────────────────────────────────────────────

    public async Task<object?> GenerateInitialAsync(
        LayerKind kind,
        string appName,
        string description,
        string platforms,
        string runtime,
        ImmutableArray<string> screenshotPaths,
        CancellationToken ct = default)
    {
        var key = Cfg.ApiKey;
        if (string.IsNullOrWhiteSpace(key)) return null;

        try
        {
            var (system, prompt) = BuildSeedPrompt(kind, appName, description, platforms, runtime);
            if (system is null) return null;

            string? text;
            var useVision = !screenshotPaths.IsDefaultOrEmpty
                            && (kind == LayerKind.UX || kind == LayerKind.DesignSystem);

            if (useVision)
            {
                text = await SendVisionAsync(system, prompt, screenshotPaths, key, ct).ConfigureAwait(false);
            }
            else
            {
                var req = new MessagesRequest(
                    model: Cfg.Model,
                    max_tokens: 2000,
                    messages: new[] { new MessagesContent("user", prompt) },
                    system: system,
                    temperature: 0.4);
                var resp = await _api.CreateMessageAsync(req, key, Cfg.Version, ct).ConfigureAwait(false);
                text = resp?.content?.FirstOrDefault(b => b.type == "text")?.text;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
#if DEBUG
                System.Console.WriteLine($"[Composer] Seed for {kind}: empty AI response");
#endif
                return null;
            }
            var json = ExtractJsonObject(text);
            if (string.IsNullOrEmpty(json))
            {
#if DEBUG
                System.Console.WriteLine($"[Composer] Seed for {kind}: no JSON object found in response. Raw text head: {text.Substring(0, System.Math.Min(text.Length, 240))}");
#endif
                return null;
            }

            var parsed = ParseSeed(kind, json);
#if DEBUG
            if (parsed is null)
                System.Console.WriteLine($"[Composer] Seed for {kind}: parse returned null. JSON head: {json.Substring(0, System.Math.Min(json.Length, 240))}");
#endif
            return parsed;
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> SendVisionAsync(
        string system, string prompt, ImmutableArray<string> paths, string key, CancellationToken ct)
    {
        // Build the multimodal content array — one image block per readable
        // path, then the text prompt at the end.
        var content = new List<object>();
        var attached = 0;
        var skipped = new List<string>();
        foreach (var path in paths)
        {
            try
            {
                if (!File.Exists(path))
                {
                    skipped.Add($"{path} (not found)");
                    continue;
                }
                var bytes = await File.ReadAllBytesAsync(path, ct).ConfigureAwait(false);
                var b64 = Convert.ToBase64String(bytes);
                var mime = Path.GetExtension(path).ToLowerInvariant() switch
                {
                    ".png"  => "image/png",
                    ".gif"  => "image/gif",
                    ".webp" => "image/webp",
                    _       => "image/jpeg",
                };
                content.Add(new
                {
                    type = "image",
                    source = new { type = "base64", media_type = mime, data = b64 },
                });
                attached++;
#if DEBUG
                System.Console.WriteLine($"[Composer] Vision: attached {path} ({bytes.Length:N0} bytes, {mime})");
#endif
            }
            catch (System.Exception ex)
            {
                skipped.Add($"{path} ({ex.GetType().Name})");
            }
        }
        content.Add(new { type = "text", text = prompt });

#if DEBUG
        System.Console.WriteLine($"[Composer] Vision call: {attached} image(s) attached, {skipped.Count} skipped.");
        foreach (var s in skipped) System.Console.WriteLine($"[Composer]   skipped: {s}");
#endif

        var visionReq = new
        {
            model = Cfg.Model,
            max_tokens = 2000,
            system,
            temperature = 0.4,
            messages = new[] { new { role = "user", content = content.ToArray() } },
        };

        var resp = await _api.CreateVisionMessageAsync(visionReq, key, Cfg.Version, ct).ConfigureAwait(false);
        return resp?.content?.FirstOrDefault(b => b.type == "text")?.text;
    }

    private static (string? system, string user) BuildSeedPrompt(
        LayerKind kind, string appName, string description, string platforms, string runtime)
    {
        var ctx = $"App: {appName}\nDescription: {description}\nPlatforms: {platforms}\nRuntime: {runtime}";

        return kind switch
        {
            LayerKind.UX => (
                """
                You design primary user flows for Uno Platform apps. Given an app name, one-sentence
                description, target platforms, and runtime (and optionally reference screenshots),
                design the primary user flow as 4–6 screens.

                Reply with ONLY this JSON shape (no preamble, no commentary):
                {
                  "Name": "<short flow name>",
                  "Screens": [
                    {
                      "Name": "<short screen name>",
                      "Note": "<one-line user goal>",
                      "MockBlocks": ["Full"|"ThreeQuarters"|"Half"|"Quarter", ...]
                    }
                  ]
                }

                MockBlocks should have 3–4 entries representing the rough block layout of the screen
                (a Full block is a hero/banner, a Half is a side-by-side card, etc.). If reference
                screenshots are supplied, infer the block layout from them.
                """,
                ctx),

            LayerKind.Architecture => (
                """
                You design Uno Platform architecture blueprints. Given an app, produce a module
                blueprint with 4–7 modules across 3 columns and 5–7 connecting edges.

                Layout: position modules in three columns at X = 20, 290, 560. Y values either 70
                or 220 (two rows). Module width is 180 — keep modules within X 20..740. ColorKey
                must be one of: "ink", "ink2", "indigo", "amber". Use "indigo" for the central
                state/data layer and "ink2" for backing services.

                Reply with ONLY this JSON (no preamble):
                {
                  "Modules": [
                    {
                      "Id": "<lowercase id>",
                      "Label": "<title-cased label>",
                      "Position": { "X": <number>, "Y": <number> },
                      "ColorKey": "ink"|"ink2"|"indigo"|"amber",
                      "Description": "<one-line responsibility>"
                    }
                  ],
                  "Edges": [
                    { "FromId": "<id>", "ToId": "<id>", "Label": "<short verb>" }
                  ]
                }
                """,
                ctx),

            LayerKind.DesignSystem => (
                """
                You curate design tokens for Uno Platform apps. Given an app (and optionally
                reference screenshots), produce a token set whose accent colour reflects the
                app's tone. Keep Primary near-black, Surface near-white. Pick a body font from:
                Fraunces, Newsreader, Inter.

                Reply with ONLY this JSON (no preamble) — colours as hex like #18181B:
                {
                  "Primary":  "#RRGGBB",
                  "Surface":  "#RRGGBB",
                  "Accent":   "#RRGGBB",
                  "Critical": "#RRGGBB",
                  "Success":  "#RRGGBB",
                  "BodyFont": "Fraunces"|"Newsreader"|"Inter"
                }
                """,
                ctx),

            LayerKind.Interactions => (
                """
                You define interaction state behaviours for Uno Platform apps. Given an app,
                define 2–4 named flows. Every flow has the SAME six states (default, loading,
                empty, error, success, offline). Tailor each state's Description to what the
                user sees AND what the system does, specific to that flow's purpose.

                Reply with ONLY this JSON (no preamble):
                {
                  "Flows": [
                    {
                      "Id": "<lowercase-kebab>",
                      "Label": "<flow label>",
                      "States": [
                        { "Id": "default", "Label": "Default", "Description": "<text>" },
                        { "Id": "loading", "Label": "Loading", "Description": "<text>" },
                        { "Id": "empty",   "Label": "Empty",   "Description": "<text>" },
                        { "Id": "error",   "Label": "Error",   "Description": "<text>" },
                        { "Id": "success", "Label": "Success", "Description": "<text>" },
                        { "Id": "offline", "Label": "Offline", "Description": "<text>" }
                      ]
                    }
                  ]
                }
                """,
                ctx),

            LayerKind.Data => (
                """
                You shape data contracts for Uno Platform apps. Given an app, define 2–4 record
                entities with explicit fields and nullability. Use string, int, double, bool,
                date, datetime, GeoPoint, enum, "string?", "int?", "Type[]" forms etc.

                Reply with ONLY this JSON (no preamble):
                {
                  "Entities": [
                    {
                      "Name": "<PascalCaseEntityName>",
                      "Kind": "Record",
                      "Fields": [
                        { "Name": "<camelCase>", "TypeText": "<type spec>" }
                      ]
                    }
                  ]
                }
                """,
                ctx),

            LayerKind.Implementation => (
                """
                You sequence Uno Platform builds. Given an app, define 5–7 sequential phases.

                Reply with ONLY this JSON (no preamble):
                {
                  "Phases": [
                    {
                      "Number": <int>,
                      "Label": "<short label>",
                      "Files": ["<file or directory>", ...],
                      "After": "<previous phase label or '—' for the first>"
                    }
                  ]
                }
                """,
                ctx),

            _ => (null, ctx),
        };
    }

    private static object? ParseSeed(LayerKind kind, string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return kind switch
        {
            LayerKind.Intent         => ParseIntent(root),
            LayerKind.UX             => ParseUX(root),
            LayerKind.Architecture   => ParseArchitecture(root),
            LayerKind.DesignSystem   => ParseDesign(root),
            LayerKind.Interactions   => ParseInteractions(root),
            LayerKind.Data           => ParseData(root),
            LayerKind.Implementation => ParseImplementation(root),
            _ => null,
        };
    }

    private static IntentValues? ParseIntent(JsonElement root)
    {
        var d = IntentValues.Default;
        var appType     = GetStr(root, "AppType")     ?? d.AppType;
        var primaryUser = GetStr(root, "PrimaryUser") ?? d.PrimaryUser;
        var workflow    = GetStr(root, "Workflow")    ?? d.Workflow;
        var platforms   = GetStr(root, "Platforms")   ?? d.Platforms;
        var notes       = GetStr(root, "Notes")       ?? d.Notes;
        return new IntentValues(appType, primaryUser, workflow, platforms, notes);
    }

    /// <summary>Case-insensitive property lookup — Sonnet sometimes returns
    /// camelCase even when the schema is PascalCase, and vice versa. Walk
    /// the object's properties and match on OrdinalIgnoreCase.</summary>
    private static bool TryGet(JsonElement el, string name, out JsonElement value)
    {
        if (el.ValueKind == JsonValueKind.Object)
        {
            foreach (var p in el.EnumerateObject())
            {
                if (string.Equals(p.Name, name, System.StringComparison.OrdinalIgnoreCase))
                {
                    value = p.Value;
                    return true;
                }
            }
        }
        value = default;
        return false;
    }

    private static UXFlow? ParseUX(JsonElement root)
    {
        if (!TryGet(root, "Name", out var nameEl)
            || !TryGet(root, "Screens", out var screensEl)
            || screensEl.ValueKind != JsonValueKind.Array)
            return null;

        var screens = ImmutableArray.CreateBuilder<ScreenDef>(screensEl.GetArrayLength());
        foreach (var s in screensEl.EnumerateArray())
        {
            var screenName = TryGet(s, "Name", out var sn) ? sn.GetString() ?? "" : "";
            var note       = TryGet(s, "Note", out var no) ? no.GetString() ?? "" : "";
            var blocks     = ImmutableArray.CreateBuilder<UIBlock>();
            if (TryGet(s, "MockBlocks", out var mb) && mb.ValueKind == JsonValueKind.Array)
            {
                foreach (var b in mb.EnumerateArray())
                {
                    var v = b.GetString();
                    blocks.Add(v switch
                    {
                        "Full"           => UIBlock.Full,
                        "ThreeQuarters"  => UIBlock.ThreeQuarters,
                        "Half"           => UIBlock.Half,
                        "Quarter"        => UIBlock.Quarter,
                        _                => UIBlock.Full,
                    });
                }
            }
            screens.Add(new ScreenDef(screenName, note, blocks.ToImmutable()));
        }

        return new UXFlow(nameEl.GetString() ?? "Primary flow", screens.ToImmutable());
    }

    private static ArchitectureBlueprint? ParseArchitecture(JsonElement root)
    {
        if (!TryGet(root, "Modules", out var modulesEl)
            || !TryGet(root, "Edges", out var edgesEl)
            || modulesEl.ValueKind != JsonValueKind.Array
            || edgesEl.ValueKind   != JsonValueKind.Array)
            return null;

        var mods = ImmutableArray.CreateBuilder<ModuleDef>(modulesEl.GetArrayLength());
        foreach (var m in modulesEl.EnumerateArray())
        {
            var id    = TryGet(m, "Id",          out var i) ? i.GetString() ?? "" : "";
            var label = TryGet(m, "Label",       out var l) ? l.GetString() ?? "" : "";
            var color = TryGet(m, "ColorKey",    out var c) ? c.GetString() ?? "ink" : "ink";
            var desc  = TryGet(m, "Description", out var d) ? d.GetString() ?? "" : "";

            double x = 20, y = 70;
            if (TryGet(m, "Position", out var pos) && pos.ValueKind == JsonValueKind.Object)
            {
                if (TryGet(pos, "X", out var xe)) x = xe.GetDouble();
                if (TryGet(pos, "Y", out var ye)) y = ye.GetDouble();
            }

            mods.Add(new ModuleDef(id, label, new Windows.Foundation.Point(x, y), color, desc));
        }

        var eds = ImmutableArray.CreateBuilder<EdgeDef>(edgesEl.GetArrayLength());
        foreach (var e in edgesEl.EnumerateArray())
        {
            var from  = TryGet(e, "FromId", out var f) ? f.GetString() ?? "" : "";
            var to    = TryGet(e, "ToId",   out var t) ? t.GetString() ?? "" : "";
            var label = TryGet(e, "Label",  out var l) ? l.GetString() ?? "" : "";
            eds.Add(new EdgeDef(from, to, label));
        }

        return new ArchitectureBlueprint(mods.ToImmutable(), eds.ToImmutable());
    }

    private static DesignTokens? ParseDesign(JsonElement root)
    {
        var d = DesignTokens.Default;
        var surface = ParseHex(GetStr(root, "Surface"), d.Surface);
        var action  = ParseHex(GetStr(root, "Action"),  d.Action);
        var info    = ParseHex(GetStr(root, "Info"),    d.Info);
        var success = ParseHex(GetStr(root, "Success"), d.Success);
        var warn    = ParseHex(GetStr(root, "Warn"),    d.Warn);
        var panel   = ParseHex(GetStr(root, "Panel"),   d.Panel);
        var tag     = ParseHex(GetStr(root, "Tag"),     d.Tag);
        var locked  = ParseHex(GetStr(root, "Locked"),  d.Locked);
        var font    = GetStr(root, "BodyFont");
        var bodyFont = font switch
        {
            "Newsreader" or "Inter" or "Fraunces" or "Geist" or "Geist Mono" => font,
            _ => "Fraunces",
        };
        return new DesignTokens(surface, action, info, success, warn, panel, tag, locked, bodyFont!);
    }

    private static InteractionsMatrix? ParseInteractions(JsonElement root)
    {
        // The AI sometimes returns the flows directly as a top-level array,
        // sometimes wrapped in {"Flows": [...]}.
        JsonElement flowsEl;
        if (root.ValueKind == JsonValueKind.Array)
        {
            flowsEl = root;
        }
        else if (TryGet(root, "Flows", out var f1)) flowsEl = f1;
        else if (TryGet(root, "flows", out var f2)) flowsEl = f2;
        else return null;

        if (flowsEl.ValueKind != JsonValueKind.Array) return null;

        var flows = ImmutableArray.CreateBuilder<InteractionFlow>(flowsEl.GetArrayLength());
        foreach (var f in flowsEl.EnumerateArray())
        {
            var id    = TryGet(f, "Id",    out var ie) ? ie.GetString() ?? "" : "";
            var label = TryGet(f, "Label", out var le) ? le.GetString() ?? "" : "";

            // Build a description-by-kind map from the AI's State entries,
            // then materialise the canonical 6 states in fixed order so the
            // contract from brief 06 §1 is preserved regardless of what the
            // AI returned.
            var byKind = new Dictionary<StateKind, string>();
            if (TryGet(f, "States", out var ss) && ss.ValueKind == JsonValueKind.Array)
            {
                foreach (var s in ss.EnumerateArray())
                {
                    var skind  = TryGet(s, "Kind",        out var ske) ? ske.GetString() ?? "" : "";
                    var sid    = TryGet(s, "Id",          out var sie) ? sie.GetString() ?? "" : "";
                    var sdesc  = TryGet(s, "Description", out var sde) ? sde.GetString() ?? "" : "";
                    var kind = ResolveStateKind(skind, sid);
                    if (kind is { } k) byKind[k] = sdesc;
                }
            }
            if (byKind.Count == 0) continue;

            var states = ImmutableArray.CreateBuilder<StateDef>(InteractionFlow.StandardStates.Length);
            foreach (var k in InteractionFlow.StandardStates)
                states.Add(new StateDef(k, byKind.TryGetValue(k, out var d) ? d : ""));

            flows.Add(new InteractionFlow(id, label, states.ToImmutable()));
        }
        return flows.Count > 0 ? new InteractionsMatrix(flows.ToImmutable()) : null;
    }

    private static StateKind? ResolveStateKind(string kindText, string idText)
    {
        var s = (kindText.Length > 0 ? kindText : idText).Trim().ToLowerInvariant();
        return s switch
        {
            "default"  => StateKind.Default,
            "loading"  => StateKind.Loading,
            "empty"    => StateKind.Empty,
            "error"    => StateKind.Error,
            "success"  => StateKind.Success,
            "offline"  => StateKind.Offline,
            _          => null,
        };
    }

    private static DataContracts? ParseData(JsonElement root)
    {
        if (!TryGet(root, "Entities", out var entsEl) || entsEl.ValueKind != JsonValueKind.Array)
            return null;

        var ents = ImmutableArray.CreateBuilder<EntityDef>(entsEl.GetArrayLength());
        foreach (var e in entsEl.EnumerateArray())
        {
            var name = TryGet(e, "Name", out var n) ? n.GetString() ?? "" : "";
            var kindText = TryGet(e, "Kind", out var k) ? k.GetString() ?? "Record" : "Record";
            var kind = kindText switch
            {
                "Class"  => EntityKind.Class,
                "Struct" => EntityKind.Struct,
                _        => EntityKind.Record,
            };

            var fields = ImmutableArray.CreateBuilder<FieldDef>();
            if (TryGet(e, "Fields", out var fs) && fs.ValueKind == JsonValueKind.Array)
            {
                foreach (var f in fs.EnumerateArray())
                {
                    var fn = TryGet(f, "Name",     out var fne) ? fne.GetString() ?? "" : "";
                    var ft = TryGet(f, "TypeText", out var fte) ? fte.GetString() ?? "" : "";
                    fields.Add(new FieldDef(fn, ft));
                }
            }

            ents.Add(new EntityDef(name, kind, fields.ToImmutable()));
        }

        return new DataContracts(ents.ToImmutable());
    }

    private static BuildPlan? ParseImplementation(JsonElement root)
    {
        if (!TryGet(root, "Phases", out var phasesEl) || phasesEl.ValueKind != JsonValueKind.Array)
            return null;

        var phases = ImmutableArray.CreateBuilder<PhaseDef>(phasesEl.GetArrayLength());
        foreach (var p in phasesEl.EnumerateArray())
        {
            var num   = TryGet(p, "Number", out var ne) ? ne.GetInt32()        : phases.Count + 1;
            var label = TryGet(p, "Label",  out var le) ? le.GetString() ?? "" : "";
            var after = TryGet(p, "After",  out var ae) ? ae.GetString() ?? "" : "—";

            var files = ImmutableArray.CreateBuilder<string>();
            if (TryGet(p, "Files", out var fs) && fs.ValueKind == JsonValueKind.Array)
                foreach (var f in fs.EnumerateArray())
                    files.Add(f.GetString() ?? "");

            phases.Add(new PhaseDef(num, label, files.ToImmutable(), after));
        }

        return new BuildPlan(phases.ToImmutable());
    }

    private static string? GetStr(JsonElement root, string name) =>
        TryGet(root, name, out var el) && el.ValueKind == JsonValueKind.String
            ? el.GetString()
            : null;

    private static Color ParseHex(string? hex, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(hex)) return fallback;
        var s = hex.Trim().TrimStart('#');
        if (s.Length is not (6 or 8)) return fallback;
        try
        {
            var off = s.Length == 8 ? 2 : 0;
            byte a = s.Length == 8 ? byte.Parse(s.Substring(0, 2), NumberStyles.HexNumber) : (byte)0xFF;
            byte r = byte.Parse(s.Substring(off + 0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(s.Substring(off + 2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(s.Substring(off + 4, 2), NumberStyles.HexNumber);
            return Color.FromArgb(a, r, g, b);
        }
        catch { return fallback; }
    }

    // ────────────────────────────────────────────────────────────────────
    //  GeneratePreviewAsync — refine on user prompt
    // ────────────────────────────────────────────────────────────────────

    public async Task<LayerPreviewResult> GeneratePreviewAsync(
        LayerKind kind,
        object currentValues,
        string userPrompt,
        IReadOnlyDictionary<LayerKind, string> lockedContextSummaries,
        CancellationToken ct = default)
    {
        var key = Cfg.ApiKey;
        if (string.IsNullOrWhiteSpace(key))
            return Identity(currentValues);

        try
        {
            var layer = Layers.Get(kind);
            var valueType = currentValues.GetType();
            var currentJson = JsonSerializer.Serialize(currentValues, valueType, JsonOpts);

            var lockedBlock = lockedContextSummaries.Count == 0
                ? "(none yet — this is one of the earlier layers)"
                : string.Join("\n", lockedContextSummaries.Select(kv => $"- {Layers.Get(kv.Key).Label}: {kv.Value}"));

            var system =
                $$"""
                You are refining the {{layer.Label}} layer of a Uno Platform app composition.

                Layer purpose: {{layer.Subtitle}}

                Locked context from prior layers:
                {{lockedBlock}}

                You will receive the current state of the {{layer.Label}} layer as JSON, and a user prompt
                describing how they want it changed. Return JSON of the SAME shape with your proposed
                refinement, plus a one-line summary suitable for display in italic serif type.

                Respond with EXACTLY this JSON structure and nothing else:
                {"proposed": <same shape as input>, "summary": "<one-line headline>"}
                """;

            var prompt =
                $"""
                Current state:
                {currentJson}

                User prompt:
                {userPrompt}
                """;

            var req = new MessagesRequest(
                model: Cfg.Model,
                max_tokens: 2000,
                messages: new[] { new MessagesContent("user", prompt) },
                system: system,
                temperature: 0.3);

            var resp = await _api.CreateMessageAsync(req, key, Cfg.Version, ct);
            var text = resp?.content?.FirstOrDefault(b => b.type == "text")?.text;
            if (string.IsNullOrWhiteSpace(text))
                return Identity(currentValues);

            var json = ExtractJsonObject(text);
            if (string.IsNullOrEmpty(json))
                return Identity(currentValues);

            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("proposed", out var proposedEl))
                return Identity(currentValues);

            var proposedObj = JsonSerializer.Deserialize(proposedEl.GetRawText(), valueType, JsonOpts);
            if (proposedObj is null)
                return Identity(currentValues);

            var summary = doc.RootElement.TryGetProperty("summary", out var sumEl) && sumEl.ValueKind == JsonValueKind.String
                ? sumEl.GetString() ?? "Preview ready."
                : "Preview ready.";

            return new LayerPreviewResult(proposedObj, summary);
        }
        catch
        {
            return Identity(currentValues);
        }
    }

    private static LayerPreviewResult Identity(object currentValues) =>
        new(currentValues, "Showing your edits as proposed.");

    /// <summary>Extracts the first balanced JSON object from a text response.</summary>
    private static string ExtractJsonObject(string text)
    {
        var depth = 0;
        var start = -1;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '{')
            {
                if (depth == 0) start = i;
                depth++;
            }
            else if (c == '}')
            {
                depth--;
                if (depth == 0 && start >= 0)
                    return text.Substring(start, i - start + 1);
            }
        }
        return string.Empty;
    }
}
