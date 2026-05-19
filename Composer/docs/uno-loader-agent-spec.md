# CompositionStackLoader — Agent Integration Spec

A drop-in loading animation for Uno Platform that plays a composition-stack → mobile UI → desktop UI sequence, then fires `AnimationCompleted`. Pure SkiaSharp, no XAML animations, identical pixels on every target (Windows, Mac, iOS, Android, WASM, Linux).

This document is the complete spec for an agent to integrate this control. Follow steps in order.

---

## Prerequisites — verify before starting

The host project must satisfy all of these. If any are missing, stop and surface the gap to the user.

- **Uno Platform 6.4 or later** (released November 2025, first version with GA .NET 10 support)
- **.NET 10 SDK installed** (this control is built for .NET 10 specifically)
- **WinUI-style head** (uses `Microsoft.UI.Xaml`). For UWP-style heads see *Variant: UWP-style heads* at the bottom.
- Target frameworks: standard Uno .NET 10 targets — `net10.0-windows10`, `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-browserwasm`, `net10.0-desktop`

Before doing anything, run:

```bash
dotnet --version
```

Confirm 10.0.x. If older, install the .NET 10 SDK before proceeding.

Then verify Uno version in the project's `Directory.Packages.props` or csproj — `Uno.Sdk` should be `6.4.x` or higher. If on 6.3 or earlier, run the Uno migration guide before integrating this control.

```bash
# Optional but recommended — Uno's environment validator
dotnet tool install -g Uno.Check
uno-check
```

---

## Step 1 — Add the NuGet dependency

Add to the head project (or `Directory.Packages.props` if the solution uses central package management):

```xml
<PackageReference Include="SkiaSharp.Views.Uno.WinUI" Version="3.119.2" />
```

This is the SkiaSharp 3.x branch, which is the line that supports .NET 10. SkiaSharp 2.88.x will not work — it doesn't ship .NET 10 native assets.

If `Directory.Packages.props` exists at the solution root, add the version there and reference without `Version=` in the csproj. Do not duplicate — the build will fail.

After adding, run:

```bash
dotnet restore
```

Verify the package resolved with no errors before proceeding. If restore fails with a native-asset error on a specific platform (e.g. WASM), check that the corresponding `SkiaSharp.NativeAssets.*` package transitive reference resolved — sometimes Uno's project system needs an explicit reference on the platform head.

> **Context worth knowing:** Uno Platform co-maintains SkiaSharp with Microsoft as of mid-2025, so .NET 10 + Uno + SkiaSharp 3.x is the actively-supported path forward. SkiaSharp 4.0 is in preview and not needed for this control.

---

## Step 2 — Create the control file

Create the directory `Controls/` in the project that will host the loader (typically the shared/main project, not the head). Then create `Controls/CompositionStackLoader.cs` with the contents from the file `CompositionStackLoader.cs` (provided alongside this spec).

**Important:** Change the namespace at the top of the file from `YourApp.Controls` to match the host project's root namespace + `.Controls`. For example, if the project is `Acme.MyApp`, the namespace becomes `Acme.MyApp.Controls`.

The file is fully self-contained — no other dependencies, no XAML partner file, no resource dictionary entries.

---

## Step 3 — Wire the loader into the app

There are two integration patterns. Pick based on whether the host has an existing splash/loading page.

### Pattern A — App has no splash page (most common)

Modify the page that's set as the initial content (typically `MainPage.xaml` or whatever `Window.Content` resolves to in `App.xaml.cs`).

Wrap the existing root content in a `Grid` with the loader on top:

```xml
<Page x:Class="Acme.MyApp.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:controls="using:Acme.MyApp.Controls">
    <Grid>
        <!-- Existing app content goes here, in a named Grid -->
        <Grid x:Name="MainContent" Visibility="Collapsed">
            <!-- whatever was already in the page -->
        </Grid>

        <!-- Loader sits on top -->
        <controls:CompositionStackLoader x:Name="Loader"
                                         AnimationCompleted="OnLoaderFinished"
                                         Background="#FAFAFA" />
    </Grid>
</Page>
```

Then in the code-behind:

```csharp
private void OnLoaderFinished(object sender, EventArgs e)
{
    MainContent.Visibility = Visibility.Visible;
    Loader.Visibility = Visibility.Collapsed;
}
```

### Pattern B — App has a dedicated splash/loading page

Replace the page's content (or the loading indicator section of it) with the loader. In the `AnimationCompleted` handler, navigate to the next page:

```csharp
private void OnLoaderFinished(object sender, EventArgs e)
{
    Frame.Navigate(typeof(ShellPage));
}
```

---

## Step 4 — Optional: tie animation to real loading progress

The animation runs for ~18 seconds. If the app's actual initialization finishes earlier (or later), wire `Skip()` to gracefully exit:

```csharp
public MainPage()
{
    InitializeComponent();
    _ = InitializeAppAsync();
}

private async Task InitializeAppAsync()
{
    await DataService.LoadAsync();
    await AuthService.RestoreSessionAsync();
    // ... whatever else needs to happen on boot

    // Tell the loader to fade out gracefully (400ms) and fire AnimationCompleted
    Loader.Skip();
}
```

`Skip()` is a no-op if the animation already completed naturally, so it's safe to call unconditionally.

If real loading is *slower* than 18 seconds, the loader holds the final desktop frame until something calls `Skip()` or until the page unloads. To make the loader loop forever instead of stopping at the end, see the *Customization* section below.

---

## Step 5 — Build and verify

```bash
dotnet build
```

Then run on the platform the user is targeting. The loader should:

1. Reveal each of the 5 layers in sequence (PLAN, ARCHITECTURE, DESIGN SYSTEM, WIRING, FOUNDATION) with labels and selection brackets — ~7.5s total
2. Hold the complete stack briefly — ~1s
3. Morph into a mobile UI (phone with status bar, app bar, chips, content cards, tab bar) — ~1.7s
4. Hold the mobile UI — ~2.2s
5. Reflow into a desktop UI (window with traffic lights, sidebar, toolbar, data table, status bar) — ~1.9s
6. Hold the desktop UI — ~2.2s
7. Return to the stack — ~1.3s
8. Fire `AnimationCompleted`

Total runtime: ~18 seconds.

If anything looks wrong, check the *Troubleshooting* section.

---

## Customization

### Change the total runtime

Edit the `Phases[]` array in `CompositionStackLoader.cs`. Each phase's `DurMs` controls its duration. The reveal phases (indices 0–4) are 1500ms each. To make a 10-second version, halve all durations.

### Make it loop forever

Replace the `Finish()` method:

```csharp
private void Finish()
{
    // Restart instead of completing
    _clock.Restart();
}
```

Or add a `Loop` property:

```csharp
public bool Loop { get; set; } = false;

private void Finish()
{
    if (Loop)
    {
        _clock.Restart();
        return;
    }
    if (_finished) return;
    _finished = true;
    _timer?.Stop();
    AnimationCompleted?.Invoke(this, EventArgs.Empty);
}
```

### Run only the reveal phases

Truncate `Phases[]` to the first 6 entries (indices 0–5: the five layer reveals plus the stack hold). Total runtime drops to ~8.6 seconds and ends on the assembled stack instead of the desktop UI.

### Tighten the typography

The labels use Skia's default mono font as a substitute for Martian Mono. To embed and use the real font:

1. Add the Martian Mono `.ttf` file as an embedded resource in the project
2. In the constructor, load it once and replace the `_monoFont` field:

```csharp
using var stream = typeof(CompositionStackLoader).Assembly
    .GetManifestResourceStream("Acme.MyApp.Assets.MartianMono-Regular.ttf");
var typeface = SKTypeface.FromStream(stream);
_monoFont = new SKFont(typeface, 10);
```

The serif font (Fraunces) used for the figure captions can be loaded the same way.

### Tune jitter on WASM

If the animation stutters during initial app boot on WebAssembly, the `DispatcherTimer` is competing for the main thread. Replace it with `CompositionTarget.Rendering` for frame-locked updates — Uno 6.4's Skia pipeline is now display-synced, so this aligns the loader with the platform's render cadence:

```csharp
private void OnLoaded(object sender, RoutedEventArgs e)
{
    _clock.Restart();
    Microsoft.UI.Xaml.Media.CompositionTarget.Rendering += OnRendering;
}

private void OnUnloaded(object sender, RoutedEventArgs e)
{
    Microsoft.UI.Xaml.Media.CompositionTarget.Rendering -= OnRendering;
    _clock.Stop();
}

private void OnRendering(object sender, object e)
{
    if (_finished) return;
    _canvas.Invalidate();
}
```

Delete the `_timer` field and its references.

---

## Variant: UWP-style heads

If the project uses UWP-style Uno (no `Microsoft.UI.Xaml`, uses `Windows.UI.Xaml` instead):

1. Replace the NuGet package: `SkiaSharp.Views.Uno` instead of `SkiaSharp.Views.Uno.WinUI`
2. In the file, change `using Microsoft.UI.Xaml;` to `using Windows.UI.Xaml;`
3. Change `using Microsoft.UI.Xaml.Controls;` to `using Windows.UI.Xaml.Controls;`
4. Change `using SkiaSharp.Views.Windows;` to `using SkiaSharp.Views.UWP;`

Everything else is identical.

---

## Troubleshooting

### "The type or namespace name 'SKXamlCanvas' could not be found"
The SkiaSharp.Views package didn't restore correctly. Run `dotnet restore --force-evaluate` and rebuild. If on UWP-style head, see *Variant* above.

### Animation runs but nothing visible
Check that the loader has non-zero size in the visual tree. It needs to be in a layout that gives it dimensions — `<Grid>` with no width/height constraints works (it fills available space). If put inside a `<StackPanel>` with `Vertical` orientation and no explicit `Height`, it may collapse.

### Animation visible but text labels missing
Skia couldn't find a fallback mono font on the platform. Embed Martian Mono per the *Customization* section, or change `_monoFont = new SKFont() { Size = 10 }` to use a known-available family: `_monoFont = new SKFont(SKTypeface.FromFamilyName("Consolas"), 10)`.

### Choppy on WASM
See *Tune jitter on WASM* in *Customization*.

### Animation fires `AnimationCompleted` immediately
The `_clock` is being read before `OnLoaded` fires, or the page is being recycled before the animation completes. Don't subscribe to `AnimationCompleted` in a constructor — only after `Loaded`. Don't call `Skip()` before the loader's own `Loaded` event has fired.

### Layers look pixelated
SkiaSharp is rendering at a lower resolution than the display. `SKXamlCanvas` should auto-scale on high-DPI displays — if it doesn't, set `_canvas.IgnorePixelScaling = false` in the constructor (it's the default but worth confirming).

### `AnimationCompleted` never fires
The page was unloaded before the animation finished, which stops the timer without firing the event. Either let the page stay alive for the full ~18s, or call `Skip()` on unload if the host needs to know the animation reached an "end state" of any kind.

---

## What this control is and isn't

**It is:** A self-contained visual splash. Renders identically across all Uno targets. Single C# file, single NuGet dependency.

**It isn't:** A theming-aware system component. Colors are hardcoded to a paper-white / ink-black monochrome palette. If the host app needs dark mode, see *Theming* below.

### Theming (not implemented, easy to add)

To make the loader respect light/dark mode, expose dependency properties for the palette:

```csharp
public static readonly DependencyProperty PaperColorProperty =
    DependencyProperty.Register(nameof(PaperColor), typeof(Color),
        typeof(CompositionStackLoader), new PropertyMetadata(Colors.White));

public Color PaperColor
{
    get => (Color)GetValue(PaperColorProperty);
    set => SetValue(PaperColorProperty, value);
}
```

Replace the `Paper` / `Paper2` / `Ink` / etc. fields with property reads inside `DrawFrame`. Bind them from XAML to theme resources:

```xml
<controls:CompositionStackLoader
    PaperColor="{ThemeResource ApplicationPageBackgroundThemeBrush}"
    InkColor="{ThemeResource SystemBaseHighColor}" />
```

This is left as a future enhancement because the original animation was designed around its specific monochrome palette and doesn't necessarily look right inverted.

---

## Files in this delivery

1. `AGENT_SPEC.md` — this file
2. `CompositionStackLoader.cs` — the control implementation (also provided alongside)

Place `CompositionStackLoader.cs` in the project's `Controls/` folder. This spec file can be discarded after integration or kept as documentation.
