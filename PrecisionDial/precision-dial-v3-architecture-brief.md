# PrecisionDial v3 — Architecture Delta Brief

> The dial's visual identity has changed. Dashed arc segments replace the solid arc and segment bar. The value orbits the arc endpoint. A second variant — the radial menu — shares the same physical knob with a cone-of-light indicator. This brief documents every change since v2.

**New:** Radial menu variant, cone-of-light, size scaling  
**Changed:** Arc rendering, value display, rendering pipeline  
**Removed:** Segment bar, static value label

---

## 01 — Change Summary

| System | Status | What Changed |
|---|---|---|
| Arc Rendering | `CHANGED` | Solid arc + segment bar replaced by dashed arc segments. Each segment is an individual path with progressive opacity. |
| Value Display | `CHANGED` | Setpoint/value now orbits the arc endpoint. Static label below removed. |
| Segment Bar | `REMOVED` | The 20-segment horizontal bar below the readout is gone. The dashed arc IS the bar. |
| Radial Menu Variant | `NEW` | Same knob body, bezel, knurl, brushed metal. Arc segments map 1:1 to menu items. Discrete detent snapping. |
| Cone-of-Light Indicator | `NEW` | Menu variant replaces the line indicator with a wedge-shaped luminance zone — three blur passes of a single path, plus clipped brightened metal grain. |
| Size Scaling System | `NEW` | All geometry is relative to the size prop. Segment count, detent count, font sizes, and feature visibility thresholds adapt per size. |
| Accent Theming | `NEW` | Accent color is parameterized. All amber references derive from one color input. |

---

## 02 — File Structure Delta

Green = new file. Amber = modified from v2. Strikethrough = removed.

```
Controls/PrecisionDial/
    PrecisionDial.cs                          [CHANGED]
        + DialMode enum (Value, Menu), MenuItems DP, SelectedIndex DP
    PrecisionDial.Properties.cs               [CHANGED]
        + DialMode, MenuItems, SelectedIndex, SelectedItem, SegmentCount DPs
    PrecisionDialCanvas.cs                    [CHANGED]
        + DashedArcRenderer replaces ArcRenderer
        + OrbitingValueRenderer
        + ConeLightRenderer

    ~~Renderers/ArcRenderer.cs~~              [REMOVED]
        Replaced by DashedArcRenderer
    Renderers/DashedArcRenderer.cs            [NEW]
        Individual segment paths with progressive opacity
    Renderers/OrbitingValueRenderer.cs        [NEW]
        Text positioned at endpoint of last active segment
    Renderers/ConeLightRenderer.cs            [NEW]
        Three-pass blurred wedge + clipped metal grain (menu mode only)
    Renderers/MenuIconRenderer.cs             [NEW]
        Icon text positioned at segment midpoints along outer ring
    Scaling/DialSizeProfile.cs                [NEW]
        Computes segments, detents, font sizes, feature flags from size

    Theme/PrecisionDialResources.xaml         [CHANGED]
        + Segment gap, cone blur radii, size threshold tokens
```

---

## 03 — API Surface Delta

### New Dependency Properties

| Property | Type | Default | Description |
|---|---|---|---|
| DialMode | `DialMode` | Value | Switches between continuous value dial and discrete menu selector. |
| MenuItems | `IList<DialMenuItem>` | null | Items for menu mode. Each has Label, Icon (string glyph), and Tag (object). |
| SelectedIndex | `int` | 0 | Currently selected menu item index. Two-way bindable. |
| SelectedItem | `DialMenuItem` | null | Read-only. The currently selected item object. |
| SegmentCount | `int` | -1 | Override segment count. -1 = auto-scale from control size. |

### New Events

| Event | Args | Description |
|---|---|---|
| SelectionConfirmed | `MenuSelectionEventArgs` | Fires on pointer release in menu mode. Contains SelectedIndex and SelectedItem. |

### New Types

```csharp
public enum DialMode { Value, Menu }

public sealed class DialMenuItem
{
    public string Label { get; set; }
    public string Icon { get; set; }  // Font glyph or emoji
    public object? Tag { get; set; }
}
```

---

## 04 — Dashed Arc Rendering

**v2:** Single continuous `SKPath.AddArc` stroke for the active range + separate 20-segment horizontal bar below the readout.

**v3:** The arc IS the segment bar. N individual arc segment paths (default: auto-scaled from control size, ~1 segment per 7dp). Each segment is a separate `SKPath` with rounded caps. A 2.2° gap between segments creates the dashed effect.

### Segment Geometry

```csharp
// DashedArcRenderer.cs — precomputed once when segment count or size changes
private void ComputeSegments(int count, float arcSweep, float startAngle, float arcR)
{
    const float gapDeg = 2.2f;
    var totalGap = gapDeg * (count - 1);
    var segDeg = (arcSweep - totalGap) / count;

    _segments = new SegmentData[count];
    for (int i = 0; i < count; i++)
    {
        var s = startAngle + i * (segDeg + gapDeg);
        var e = s + segDeg;
        _segments[i] = new(s, e, (s + e) / 2f);
    }
}
```

### Progressive Rendering

| Segment State | Stroke Width | Color | Opacity |
|---|---|---|---|
| Inactive | 1.5dp (scaled) | White 3% | 1.0 |
| Active (0–80%) | 3dp (scaled) | AccentColor | 0.30 → 0.90 (linear over position) |
| Active (80–100%) | 3dp (scaled) | Warmer accent shift | 0.60 → 1.0 |

### Glow Layer

The last 5 active segments also render through `SKMaskFilter.CreateBlur()` at low opacity to produce a trailing warmth behind the reading position. The blur radius scales with velocity (v2 speed factor).

---

## 05 — Orbiting Value

**v2:** Static value text below the dial, labeled "Setpoint" with a caption.

**v3:** The value is positioned at the endpoint of the last active arc segment, offset outward by ~6.5% of the dial size. As the value changes, the number physically travels around the circumference.

### Position Calculation

```csharp
// OrbitingValueRenderer.cs
public void Draw(SKCanvas canvas, SegmentData[] segs, int activeCount,
    float cx, float cy, float arcR, float size, double value, SKColor accent)
{
    // Position at end of last active segment (or arc start if 0)
    var angleDeg = activeCount > 0
        ? segs[Math.Min(activeCount, segs.Length) - 1].EndAngle
        : segs[0].StartAngle;

    var angleRad = angleDeg * MathF.PI / 180f;
    var textR = arcR + size * 0.065f;
    var x = cx + MathF.Cos(angleRad) * textR;
    var y = cy + MathF.Sin(angleRad) * textR;

    _paint.TextSize = Math.Max(8f, size * 0.04f);
    _paint.Color = accent.WithAlpha((byte)(140 + normalized * 115));

    canvas.DrawText(value.ToString(size >= 100 ? "F1" : "F0"), x, y, _paint);
}
```

### Visibility Threshold

The orbiting value hides below 56dp. At that size the number would collide with the arc segments. The large readout below (if present) serves as fallback.

---

## 06 — Radial Menu Variant

The radial menu is the same physical control with `DialMode="Menu"`. The knob body, bezel, knurling, brushed metal, caustic highlight, and spring physics are identical. What changes:

| Aspect | Behavior |
|---|---|
| **Arc Segments** | N = `MenuItems.Count`. Each segment maps 1:1 to a menu item. Selected = full accent, confirmed = 25% accent, inactive = 4% white. |
| **Interaction** | Discrete detent snapping. Rotation quantizes to item boundaries. Each crossing triggers a click. No continuous float values. |
| **Icons** | Each menu item's Icon glyph is drawn at the midpoint of its arc segment, offset outward. Selected icon scales up. Hides below 60dp. |
| **Indicator** | Cone of light (replaces line). A luminance wedge, wide at knob edge, tapers to center. See section 07. |

### Rotation Calculation

**Critical fix from prototyping:** Rotation must derive from the segment's actual midpoint angle, not a linear interpolation across the full arc. Segments have gaps, so a normalized 0→1 mapping lands the indicator at gap boundaries instead of segment centers.

```csharp
// Correct: rotation from precomputed segment midpoint
var rotationDeg = _segments[selectedIndex].MidAngle + 90f;

// Wrong: linear interpolation hits gap boundaries
// var rotationDeg = (selectedIndex / (count-1f)) * arcSweep - arcSweep/2;
```

### Selection Flow

During drag: `SelectedIndex` updates in real-time as the user crosses segment boundaries. On pointer release: `SelectionConfirmed` fires with the current index. Haptic and audio feedback fire on each crossing — same system as value detents.

---

## 07 — Cone-of-Light Indicator

The menu dial's indicator is not a shape — it's a region of the knob surface that appears warm. No hard edges, no fill, no border lines. One SVG path, rendered three times through different blur filters.

### Geometry

A single wedge path: arc at the knob perimeter (half-angle = segment half-angle × 1.6), two straight edges tapering to a small arc at 10dp from center. This path is never drawn without a blur filter.

```csharp
// ConeLightRenderer.cs
private SKPath BuildConePath(float cx, float cy, float knobR, float halfAngleDeg)
{
    // Drawn pointing "up" (-90°), rotated by the knob group transform
    var le = (-90f - halfAngleDeg) * DEG_TO_RAD;
    var re = (-90f + halfAngleDeg) * DEG_TO_RAD;
    var lt = (-90f - 3f) * DEG_TO_RAD;
    var rt = (-90f + 3f) * DEG_TO_RAD;
    const float tipR = 10f;

    var path = new SKPath();
    path.MoveTo(cx + MathF.Cos(le) * knobR, cy + MathF.Sin(le) * knobR);
    path.ArcTo(/* knobR arc from left edge to right edge */);
    path.LineTo(cx + MathF.Cos(rt) * tipR, cy + MathF.Sin(rt) * tipR);
    path.ArcTo(/* tipR arc from right to left */);
    path.Close();
    return path;
}
```

### Three-Pass Rendering

| Pass | Blur σ | Fill Opacity | Purpose |
|---|---|---|---|
| 1 — Deep diffuse | 18dp (scaled) | 9% accent | Wide subsurface warmth. Bleeds well past cone boundaries. |
| 2 — Medium body | 10dp (scaled) | 6% accent | Gives directional shape without hard edges. |
| 3 — Tight core | 5dp (scaled) | 4% accent | Concentration toward cone center. Overlap of all 3 passes = brightest area. |

### Clipped Metal Grain

The same brushed-metal radial lines that exist across the entire knob face are re-drawn *inside* the cone path via `SKCanvas.ClipPath()`, but in amber at 2% opacity instead of white at 0.6%. This makes the metal texture itself appear to catch warm light — the grain brightens inside the cone region. No additional shapes, no overlays.

```csharp
// Inside RenderOverride, after the three blur passes:
canvas.Save();
canvas.ClipPath(_conePath);

for (int i = 0; i < 72; i++)
{
    var a = (i / 72f) * MathF.PI * 2;
    _grainPaint.Color = accentColor.WithAlpha(
        (byte)(5 + MathF.Sin(a * 3) * 2.5f));  // ~2% amber
    canvas.DrawLine(
        cx + MathF.Cos(a) * knobR * .2f, cy + MathF.Sin(a) * knobR * .2f,
        cx + MathF.Cos(a) * knobR * .97f, cy + MathF.Sin(a) * knobR * .97f,
        _grainPaint);
}

canvas.Restore();
```

### Center Convergence

A soft 16dp circle at the knob center, 3.5% accent opacity through the heaviest blur. The light gathers inward.

> **Why three passes of the same path instead of three different shapes?** The previous approach (v3-draft) used nested cone geometries at different widths — wide/medium/narrow. Each had a visible boundary because the unblurred fills drew hard edges at three different radii, producing the "double cone" artifact. Using one path at three blur levels creates a smooth continuous falloff with no internal boundaries.

---

## 08 — Size Scaling System

All dial geometry is relative to the `Width`/`Height` of the control. A `DialSizeProfile` computes runtime parameters from the available size.

### Scaling Rules (from testbench validation)

| Feature | Formula / Threshold | Notes |
|---|---|---|
| Arc radius | size × 0.43 | Proportional |
| Knob radius | size × 0.35 | Proportional |
| Bezel radius | size × 0.36 | Proportional |
| Segment count (auto) | max(8, round(size / 7)) | 40px → 8 segs, 240px → 34 segs |
| Detent count (auto) | max(4, round(size / 10)) | 40px → 4 detents, 240px → 24 detents |
| Knurl line count | round(size × 0.9) | Proportional to circumference |
| Indicator stroke width | max(1.0, size × 0.007) | Minimum 1dp |
| Orbiting value font | max(8, size × 0.04) | Scales but floors at 8dp |
| Orbiting value | Visible ≥ 56dp | Hidden below — collides with arc |
| Brushed metal lines | Visible ≥ 60dp | Hidden below — not enough pixels |
| Menu icons | Visible ≥ 60dp | Hidden below |
| Readout label below | Visible ≥ 72dp | Hidden below |
| Menu item label below | Visible ≥ 96dp | Hidden below |

### Implementation

```csharp
// DialSizeProfile.cs
public sealed class DialSizeProfile
{
    public float Size { get; }
    public float ArcR => Size * 0.43f;
    public float KnobR => Size * 0.35f;
    public float BezelR => Size * 0.36f;
    public int AutoSegments => Math.Max(8, (int)MathF.Round(Size / 7f));
    public int AutoDetents => Math.Max(4, (int)MathF.Round(Size / 10f));
    public int KnurlCount => (int)MathF.Round(Size * 0.9f);
    public float ValueFontSize => MathF.Max(8f, Size * 0.04f);
    public bool ShowOrbitingValue => Size >= 56f;
    public bool ShowBrushedMetal => Size >= 60f;
    public bool ShowMenuIcons => Size >= 60f;
    public bool ShowReadout => Size >= 72f;
    public bool ShowMenuLabel => Size >= 96f;

    public DialSizeProfile(float size) => Size = size;
}
```

---

## 09 — Accent Theming

The `AccentBrush` DP (from v1) now drives every colored element in both variants. The SkiaSharp renderers resolve it to an `SKColor` once per frame and derive all opacity variants from it. No hardcoded amber anywhere in the render pipeline.

### Derived Colors (computed from AccentBrush)

| Usage | Derivation |
|---|---|
| Active segment fill | Accent at 30–100% opacity (progressive) |
| Hot zone segments | Accent shifted warmer (+20 hue, +10% saturation) |
| Orbiting value text | Accent at 55–100% opacity (proportional to value) |
| Indicator line | Accent at 60–100% opacity |
| Cone blur passes | Accent at 4–9% opacity |
| Clipped metal grain | Accent at ~2% opacity |
| Selected arc segment (menu) | Accent at 85% |
| Confirmed arc segment (menu) | Accent at 25% |

---

## 10 — Updated Rendering Pipeline

The full layer stack for both variants. Layers marked **NEW** or **CHANGED** are v3 additions.

### Value Dial — Layer Stack

| # | Layer | Status |
|---|---|---|
| 1 | Dashed arc segments (inactive) | `CHANGED` |
| 2 | Dashed arc segments (active, progressive) | `CHANGED` |
| 3 | Active segment glow (last 5, blurred) | `CHANGED` |
| 4 | Detent tick marks (progressive) | from v2 |
| 5 | Knurled edge ring | — |
| 6 | Outer bezel | — |
| 7 | Knob body + brushed metal (rotated) | — |
| 8 | Line indicator + glow (rotated) | — |
| 9 | Caustic highlight (fixed) | — |
| 10 | Center dimple | — |
| 11 | Pulse ring | — |
| 12 | Particles | — |
| 13 | Orbiting value text | `NEW` |

### Menu Dial — Layer Stack

| # | Layer | Status |
|---|---|---|
| 1 | Menu arc segments (inactive/selected/confirmed) | `NEW` |
| 2 | Selected segment glow (blurred) | `NEW` |
| 3 | Menu icons at segment midpoints | `NEW` |
| 4 | Knurled edge ring | — |
| 5 | Outer bezel | — |
| 6 | Knob body + brushed metal (rotated) | — |
| 7 | Cone-of-light pass 1 — deep diffuse (rotated) | `NEW` |
| 8 | Cone-of-light pass 2 — medium (rotated) | `NEW` |
| 9 | Cone-of-light pass 3 — tight core (rotated) | `NEW` |
| 10 | Clipped brightened metal grain (rotated) | `NEW` |
| 11 | Center convergence glow | `NEW` |
| 12 | Caustic highlight (fixed) | — |
| 13 | Center dimple | — |

---

## 11 — Design Token Delta

| Token | Value | Usage |
|---|---|---|
| DialSegmentGapDeg | 2.2° | Gap between dashed arc segments |
| DialMenuSegmentGapDeg | 2.4° | Slightly wider gap for menu (fewer, larger segments) |
| DialConeBlurSigma1 | 18dp | Deep diffuse pass blur radius |
| DialConeBlurSigma2 | 10dp | Medium pass blur radius |
| DialConeBlurSigma3 | 5dp | Tight core pass blur radius |
| DialConeOpacity1 | 0.09 | Deep diffuse fill opacity |
| DialConeOpacity2 | 0.06 | Medium fill opacity |
| DialConeOpacity3 | 0.04 | Tight core fill opacity |
| DialConeWidthMultiplier | 1.6 | Cone half-angle = segment half-angle × this |
| DialClippedGrainOpacity | 0.02 | Amber grain lines inside cone |
| DialOrbitingValueMinSize | 56dp | Below this, orbiting value is hidden |
| DialBrushedMetalMinSize | 60dp | Below this, metal lines are hidden |
| DialMenuIconMinSize | 60dp | Below this, menu icons are hidden |
| DialReadoutMinSize | 72dp | Below this, large readout below is hidden |

---

## 12 — Updated Implementation Roadmap

| Phase | Scope | Notes |
|---|---|---|
| 1–2 | v1 + v2 core | Unchanged. Ship the base control first. |
| 2.1–2.2 | Angular input + physics | Unchanged from v2. |
| 3 | Design tokens | Expanded with v3 tokens above. |
| 3.1 | Visual enhancements (v2) | Unchanged. |
| 3.2 | Particles (v2) | Unchanged. |
| **3.3** | `CHANGED` **Dashed arc + orbiting value** | Replace ArcRenderer with DashedArcRenderer. Add OrbitingValueRenderer. Remove SegmentBarRenderer. Remove static value label. This changes the visual identity — ship as a deliberate version bump. |
| **3.4** | `NEW` **Size scaling system** | DialSizeProfile. Feature-flag all visual elements by size threshold. Test at 40, 56, 72, 96, 128, 180, 240dp. |
| 4 | Audio feedback (v2) | Unchanged. |
| 4.1 | Rolling readout (v2) | Unchanged. |
| **4.2** | `NEW` **Radial menu variant** | DialMode DP, MenuItems DP, SelectedIndex DP, SelectionConfirmed event. Discrete detent snapping logic. MenuIconRenderer. Segment-midpoint rotation fix. |
| **4.3** | `NEW` **Cone-of-light indicator** | ConeLightRenderer. Single path, three blur passes. Clipped metal grain. This is menu-mode only — the value dial keeps its line indicator. |
| 5 | Accessibility | Menu mode needs `ISelectionProvider` in addition to `IRangeValueProvider`. |
| 6 | Sample app | Expanded: audio mixer (value dials) + settings page (menu dial) + testbench page showing all sizes and accents. |

> **Shipping strategy:** Phase 3.3 (dashed arc) is a visual breaking change — the dial looks fundamentally different. It should ship as a deliberate version bump, not a patch. The radial menu variant (4.2–4.3) is purely additive — it adds a mode, never modifies the value-dial behavior.

---

*PrecisionDial v3 — Dashed Arc · Orbiting Value · Radial Menu · Cone of Light · Size Scaling*
