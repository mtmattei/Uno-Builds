# ReticleLab — Reticle Loader

A HUD-style loading ring built as a **keyed retemplate of the stock WinUI `ProgressRing`**. It uses only a `Style` and a `ControlTemplate`: no subclass, no code in the control, no Lottie. The single-page sample drives it through three modes: Scan, Lock and Idle.

The spec is in [`RETICLE-LOADER-SPEC.md`](RETICLE-LOADER-SPEC.md).

```xml
<ProgressRing Style="{StaticResource ReticleRingStyle}"
              IsActive="{x:Bind ViewModel.IsActive, Mode=OneWay}"
              IsIndeterminate="{x:Bind ViewModel.IsIndeterminate, Mode=OneWay}"
              Value="100"
              AutomationProperties.Name="Reticle loader" />
```

## Run

```powershell
dotnet build ReticleLab/ReticleLab.csproj -f net10.0-desktop
dotnet run --project ReticleLab/ReticleLab.csproj -f net10.0-desktop
```

- Uno.Sdk `6.7.30` (pinned in `global.json`), .NET 10.
- Targets: `net10.0-desktop`, `net10.0-browserwasm`, `net10.0-android`.
- In Debug, set `APP_NO_HOTDESIGN=1` to skip `UseStudio()` when no DevServer is reachable, such as in headless runs.

## The pattern

Uno's `ProgressRing` drives three `CommonStates`. They map 1:1 onto the modes:

| Mode | `IsActive` | `IsIndeterminate` | VSM state | What the template does |
|---|---|---|---|---|
| Scan | true | true | `Active` | Four layers counter-rotate with `Forever` animations (28 s, 16 s ccw, 2.8 s, 7 s) |
| Lock | true | false | `DeterminateActive` | Layers ease home, the reticle rotates 45° and scales 0.5 into a plus, the ring thickens and pulses once, the lock dot fades in |
| Idle | false | unchanged | `Inactive` | The Hero dims to 35% and settles home; the Compact fades out |

The template parts `LottiePlayer` and `LayoutRoot` are both optional in Uno (`as` casts, null-safe calls). The custom template has no `AnimatedVisualPlayer`, and no Lottie package is referenced.

### Transitions without `VisualTransition`

Uno's `VisualStateManager` ignores `GeneratedDuration`. It hands off between states with `TurnOverAnimationsTo`: running animations on properties that the next storyboard also targets are **paused at their live value**, and the rest are stopped.

So **every state storyboard animates every animated property**, using keyframes with no `From`. Each state then eases from wherever the previous one left off. Scan→Lock spins the layers down to 0° from their current angles, with no snap. `Storyboard.TargetName`/`TargetProperty` must be spelled identically across states, because that string is what the handoff matches on.

### Binding order

`MainViewModel` sets `IsIndeterminate` from `OnModeChanged`. The toolkit's generated setter runs that before it raises `PropertyChanged(IsActive)`. Idle → Lock therefore goes `Inactive → DeterminateActive` without a frame of `Active`. Idle leaves `IsIndeterminate` unchanged, so Lock → Idle never passes through Scan either.

### Geometry

All Path Data comes from `tools/reticle-geometry.cs` (spec Appendix A), and its output matches the appendix byte-for-byte. Circles are emitted as two circular arcs, because Uno's `Path.Data` parser throws on elliptical arcs. Regenerate rather than hand-edit:

```powershell
dotnet run tools/reticle-geometry.cs
```

## Files

| File | Contents |
|---|---|
| `ReticleLab/Themes/Hud.xaml` | HUD colours (the only hex values), IBM Plex Mono families, text styles, `HudSegmentStyle` |
| `ReticleLab/Themes/ReticleRing.xaml` | `ReticleRingStyle` (Hero, 600-unit canvas) and `ReticleRingCompactStyle` (40-unit canvas) |
| `ReticleLab/Presentation/MainViewModel.cs` | `ReticleMode` → `IsActive` / `IsIndeterminate` / status text |
| `ReticleLab/Presentation/MainPage.xaml` | Segment row, Hero, status dots (StateTriggers), compact row, 600 px `AdaptiveTrigger` |
| `tools/reticle-geometry.cs` | Path Data generator |

## Measured

### CPU

Measured with a Release build on `net10.0-desktop`, **Linux X11 under Xvfb (software GL)**. Each mode got a 15 s settle, then three 5 s samples. The figures are percent of one core.

| Mode | Sample 1 | Sample 2 | Sample 3 |
|---|---|---|---|
| Scan | 226.4 | 222.0 | 225.1 |
| Lock | 0.0 | 0.2 | 0.0 |
| Idle | 0.0 | 0.0 | 0.2 |
| Scan (again) | 217.8 | 227.6 | 226.0 |

- **Lock and Idle drop to baseline.** Leaving `Active` stops the `Forever` storyboards (spike R3), so the D1 fallback (a subclass with a timer) is not needed.
- **Scan's absolute cost is inflated by software rasterisation.** Xvfb repaints the whole window on the CPU every frame. The same pump on a GPU-backed Windows desktop was measured at about ⅓ of a core. Re-measure Scan on real hardware before quoting a number.

### Stroke widths per size tier

These are in rendered px. The spec targets roughly 0.5 to 3 px.

| Tier | Size | Hairline | Standard | Heavy | Lock ring |
|---|---|---|---|---|---|
| Hero | 240 | 0.52 | 0.70 | 1.04 | 1.76 |
| Hero | 320 | 0.69 | 0.93 | 1.39 | 2.35 |
| Hero | 480 | 1.04 | 1.40 | 2.08 | **3.52** |

| Tier | Size | Track | Arcs |
|---|---|---|---|
| Compact | 24 | 0.60 | 0.75 |
| Compact | 40 | 1.00 | 1.25 |
| Compact | 64 | 1.60 | 2.00 |

The Lock ring at a 480 px Hero is the only stroke over 3 px.

## Verification status

| Check | Result |
|---|---|
| Build: desktop and WASM, Debug and Release | 0 errors, 0 warnings, no `Uno0001` |
| Desktop: Scan / Lock / Idle, pointer-driven | Pass |
| Mid-flight Scan→Lock (frame burst) | Eases from the live angle; the pulse expands and fades; settles by about 700 ms |
| Adaptive layout below 600 px | Pass: 240 px Hero, 16 px padding |
| WASM (Chromium): Scan / Lock / Idle | Pass |
| Keyboard | Tab reaches each pill and Space selects. The focus ring follows the pill radius |
| Android, automation peer / Narrator, App MCP | **Not run.** This build ran in a Linux container with no Android workload or App MCP |

## Open items

- **Arrow-key group navigation.** `XYFocusKeyboardNavigation="Enabled"` had no effect on this Uno build. A `KeyDown` handler calling `FocusManager.TryMoveFocus` did not move focus either. The pills are individual tab stops for now.
- **Reduced motion** reads `UISettings.AnimationsEnabled` once, at startup. Uno implements it on Android only, and that path is not verified here.
