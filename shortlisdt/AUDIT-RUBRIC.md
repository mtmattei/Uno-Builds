# Uno Sample App Audit Rubric

Used to grade sample apps in this folder (`Caffe`, `CrmDashboard`, `Pens`, etc.) for showcase readiness. Keep it short, keep it honest.

## How to score

Score each category **0–4**:

| Score | Meaning |
|------:|---------|
| 0 | Missing or broken |
| 1 | Below standard — needs rework |
| 2 | Acceptable — works but unpolished |
| 3 | Good — ship-ready |
| 4 | Exemplary — worth referencing as a pattern |

Weighted total → letter grade.

| Grade | Range | Meaning |
|------:|------:|---------|
| A | 85–100 | Showcase-ready |
| B | 70–84 | Solid sample, minor polish |
| C | 55–69 | Useful but rough — fix before promoting |
| D | 40–54 | Demo-only, not for the gallery |
| F | < 40 | Broken / re-scope |

## Categories

### 1. Build & Runtime Health — 20%
- `dotnet build` is clean on `net10.0-desktop` (and other declared TFMs)
- App launches without first-run errors
- No bundled SDK/version mismatches (Uno.Sdk, SkiaSharp, Lottie)
- `global.json` and `UnoFeatures` reflect actual usage

### 2. Architecture — 15%
- Pattern matches the page's needs (MVUX for async/reactive, MVVM only for imperative pages)
- `IHostBuilder` + DI; no `new Service()` in code-behind
- Navigation via regions (`Navigation.Request`, `Region.Attached`) — no code-behind frame nav
- Services have correct lifetimes; no static singletons hiding state

### 3. XAML Quality — 15%
- Binding style matches the pattern: `{Binding}` for MVUX surfaces, `x:Bind` for plain DataContext
- No hardcoded hex colors or magic numbers in page XAML. Brand colors live in `Themes/` (Material overrides or a clearly named custom theme).
- `FontSize` allowed on `FontIcon` / `SymbolIcon` (glyph sizing is the API). Not allowed on `TextBlock` / `Run` / `TextBox` — use type-scale styles.
- Resources scoped sensibly (`App.xaml` / `Themes/`), not duplicated per page
- No dead XAML, commented-out blocks, or unused styles

### 4. Visual & UX Polish — 15%
- Material theme applied; type scale used consistently
- `SafeArea` + `AutoLayout` (or equivalent) where the app must adapt
- Responsive across mobile + desktop breakpoints
- Loading / empty / error states present (FeedView or equivalent)

### 5. Platform Behavior — 10%
- Declares ≥2 TFMs (Desktop + at least one of Android/iOS/WASM/Browser). A single-TFM sample is capped at score 2 in this category.
- Runs on every declared target — no platform leaks (Windows-only APIs in shared code, etc.)
- Platform-specific code isolated via file suffixes or runtime checks

### 6. Performance — 10%
- Lists virtualize (`ItemsRepeater` / virtualizing `ItemsControl`)
- Async work off the UI thread; no `.Result` / `.Wait()` on UI
- No obvious jank: scrolling, theme switch, first paint
- Image assets sized sensibly, not full-res PNGs

### 7. Code Quality — 5%
- Clear naming; one concept per file
- No dead code, TODOs without owners, or commented experiments
- Records used for immutable data; `with` for updates (MVUX)
- Accessibility basics: `AutomationProperties.Name` on key controls, focus visuals

### 8. Documentation — 10%
- `README.md` at app root: what it shows, how to run, target platforms
- Showcase angle is one sentence the audience can repeat
- Known limitations called out (preview SDK pins, missing platforms)

## Scoring sheet (copy per app)

```
App: <name>
Reviewer: <name>
Date: 2026-MM-DD
Commit: <sha>

Category                       Score (0-4)   Weight   Weighted
Build & Runtime Health         _ / 4         20       _ / 20
Architecture                   _ / 4         15       _ / 15
XAML Quality                   _ / 4         15       _ / 15
Visual & UX Polish             _ / 4         15       _ / 15
Platform Behavior              _ / 4         10       _ / 10
Performance                    _ / 4         10       _ / 10
Code Quality                   _ / 4          5       _ /  5
Documentation                  _ / 4         10       _ / 10
                                                      -------
Total                                                 _ / 100
Grade: _

Top 3 issues:
1.
2.
3.

Quickest wins:
-
-

Showcase verdict: [Gallery | Fix-then-gallery | Internal demo only | Re-scope]
```

## Notes

- A category scoring **0** caps the overall grade at **C** — broken basics can't be averaged away.
- Weights are calibrated for *showcase samples*, not production apps. If you grade a production codebase with this, raise Performance and Code Quality, lower Visual Polish.
- Re-audit after any major Uno.Sdk bump.
