# 〰️ Liveline — *Real-time animated line-chart control*

> A reusable SkiaSharp line-chart control for Uno Platform that smoothly lerps between incoming values, auto-scales its Y axis, and idles when the data settles.

<img src="../screenshots/Liveline/liveline-chart.png" alt="Liveline real-time chart" width="560" />

## What you get
A **control library**, not just an app — `src/Liveline/` ships as `OutputType=Library`, and `samples/Liveline.Demo/` wires every property to a live control panel. This is the chart engine behind **Meridian**.

## Highlights
- **`SKCanvasElement` rendering** — gradient area fill, tracking line, value badge, and a momentum indicator, all on a Skia surface.
- **Driven by `CompositionTarget.Rendering`** — animates toward new values via `LerpSpeed`, then **stops invalidating once values converge** (idle-friendly).
- **Allocation-light steady state** — paints/fonts are reused; native Skia resources release on `Unloaded` and recreate on re-`Loaded`, so it's safe to navigate away and back.
- **Drop-in API** — `Data` (`IList<LivelinePoint>`), `Value`, `Theme`, plus `ShowGrid` / `ShowBadge` / `ShowFill` / `Momentum` / `IsLoading` / `IsPaused` toggles.
- **Demo host** wired with the full Uno.Extensions stack (`IHostBuilder`, region nav, MVVM) so it behaves like a real sample.

## Stack & platforms
Reusable library + MVVM demo host · Uno.Sdk 6.5.36+ · `net10.0-desktop` · `net10.0-browserwasm`

## Run it
```powershell
# Open Liveline.sln and run the Liveline.Demo project
dotnet run --project samples/Liveline.Demo/Liveline.Demo/Liveline.Demo.csproj -f net10.0-desktop
```
