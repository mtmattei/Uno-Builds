# Composer Context Engine — Design Brief

**Version:** v8 (consolidated)
**Status:** canonical reference. Supersedes the legacy `ARCHITECTURE.md`, `DESIGN.md`, `INTERACTIONS.md`, `README.md`, `implementation-plan.md` in this folder, plus all eleven `update-context-engine-*` delta briefs.
**Audience:** anyone implementing, evaluating, or extending the Composer Context Engine. Self-contained — no prerequisite reading.
**Reference prototype:** `composer-context-engine.jsx` (React, ~1764 lines) demonstrates every behavior described here. Treat the prototype as the executable specification; this document is the design intent and the porting target for Uno Platform XAML.

---

## 1. Thesis

The Composer Context Engine is **a workspace where a conversation crystallizes into a build system.**

Most code generation tools accept a prompt and emit a project. The Engine inverts that flow: the user composes a project across **eight named context layers**, each layer locks in turn, each lock writes a file, and the accumulated context becomes the agent's brief for actually building the app.

The thesis is that good app generation is a **layering** problem, not a prompting problem. A 1500-word prompt asking for "a field-service scheduling app" leaves the AI guessing about navigation patterns, state management, color tokens, error states, persistence, and build phasing. A composition stack makes each of those decisions explicit, in a deliberate order, with the user's input shaping the AI's recommendations at each step.

What the user gets at the end isn't just a scaffold — it's nine markdown files (one per layer plus a synthesized `prompt-context.md`) that constitute a complete brief the agent can execute against. The composition is the artifact.

### The eight layers

```
01 Intent          What the app is for                    README.md
02 UX              How users move through it               ux-flows.md
03 Architecture    How it is shaped                        architecture.md
04 Design System   How it feels                            design-system.md
05 Interactions    Every state of every flow               interaction-spec.md
06 Data            Shapes and contracts                    data-contracts.md
07 Implementation  Phased build plan                       implementation-plan.md
08 Scaffold        Runnable starting point                 scaffold.command
                                                           prompt-context.md (synthesized)
```

Order matters. Each layer's locked context becomes available to subsequent layers. UX cannot meaningfully lock before Intent (it needs to know what the app is). Architecture cannot meaningfully lock before UX (it needs to know what screens exist). Design cannot meaningfully lock before UX + Architecture (it needs to know the canvases it's styling and the components it's coloring). And so on.

---

## 2. Workspace shell

Three-column layout, with the left and right rails hidden on the very first screen and animated in once the user advances past Intent.

```
┌──── Composition Stack ────┬──────── Active Canvas ────────┬───── Files Rail ─────┐
│  260px sticky sidebar      │  flex 1, max 880px           │  260px sticky        │
│                            │                               │                       │
│  All 8 layers visible      │  Top: progress indicator      │  All 9 files         │
│  Click locked to revisit   │  Project name + Reset        │  Status per file     │
│  Active = amber 2px border │  Locked context cards above   │  Pulses for active   │
│  Locked = ink 2px border   │  Active layer canvas          │  Glows for drafted   │
│  Future = 0.5 opacity      │  Composer footer              │                       │
│                            │  Future preview cards below   │                       │
└────────────────────────────┴───────────────────────────────┴───────────────────────┘
```

### 2.1 Focused first screen

When `activeIndex === 0 && lockedIds.size === 0`, the layout collapses to a centered single column:

- Both rails are width 0 with opacity 0 (not just hidden — fully sized away)
- The active canvas centers on the page with `justify-content: center`
- The active canvas's `max-width` tightens from 880 → 720
- The active canvas's `padding-top` expands from 32 → 64

The user lands on a focused intent canvas with nothing competing for attention. They make one decision (what is this app?) and commit. The workspace then opens up.

### 2.2 Animated rails

When the user clicks `Lock and continue →` on Intent, six properties transition simultaneously over 480ms:

| Element / Property                        | From → To       | Duration | Easing       | Delay  |
|-------------------------------------------|----------------|---------|--------------|--------|
| `CompositionStack.width`                  | 0 → 260        | 480ms   | ease-out-quint| 0      |
| `CompositionStack.opacity`                | 0 → 1          | 320ms   | ease-in-out  | 160ms  |
| `FilesRail.width`                         | 0 → 260        | 480ms   | ease-out-quint| 0      |
| `FilesRail.opacity`                       | 0 → 1          | 320ms   | ease-in-out  | 160ms  |
| `FilesRail.padding-left`                  | 0 → 24         | 480ms   | ease-out-quint| 0      |
| `ActiveCanvas.max-width`                  | 720 → 880      | 480ms   | ease-out-quint| 0      |

The 160ms delay on rail content opacity is critical: rails *slide* into place first, then their contents fade in. Without the delay, the rails feel like they're "appearing." With it, they "open up."

The cubic-bezier curve `(0.16, 1, 0.3, 1)` produces overshoot-soft deceleration. Generous landing at the end. Reads as deliberate revealing rather than mechanical sliding.

Reset reverses all animations cleanly using the inverse storyboard.

### 2.3 Progress indicator

A single 1px hairline at the top of the center column with an amber-filled segment showing fraction complete. Below the line, on one row: layer name (Eyebrow, ink3) on the left, counter (`02 / 08`, mono ink4) on the right.

```
[━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━]
[████████████████████████ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─]   ← amber filled segment

ARCHITECTURE                                                       03 / 08
```

Width = `(activeIndex + 1) / 8 × 100%`. Animates over 480ms with the same ease-out-quint as the rail reveal. No dots, no per-layer labels, no chrome.

### 2.4 Future preview cards

Below the active canvas (only when rails are visible), every layer with `index > activeIndex` renders as a flat one-line preview card:

```
┌─ ARCHITECTURE · upcoming ─────────────────────────────────┐
│ how it is shaped                                            │
└─────────────────────────────────────────────────────────────┘     ← 0.32 opacity

┌─ DESIGN SYSTEM · upcoming ──────────────────────────────────┐
│ how it feels                                                 │
└──────────────────────────────────────────────────────────────┘    ← 0.24 opacity
…
```

Opacity calculation: `Math.max(0.05, 0.40 - (distance × 0.08))`.

| Distance | Opacity |
|----------|---------|
| 1        | 0.32    |
| 2        | 0.24    |
| 3        | 0.16    |
| 4        | 0.08    |
| 5+       | 0.05    |

Each card transitions opacity over 480ms with ease-out-quint when activeIndex changes. The result is a slow, calm progressive clarification as the user moves forward — layers don't pop into view, they fade in over half a second.

Cards are read-only (`pointerEvents: 'none'`). They render the layer's `hint` text in italic Inter behind a dashed hairline border with no background fill.

### 2.5 Locked context cards

When a layer locks, it compresses into a flat one-line card stacked above the active canvas:

```
┌─ ✓ INTENT · LOCKED                                        Revisit ↗ ┐
│ "Field-service scheduling, for mobile-first technicians.              │
│  Receive jobs, schedule, dispatch."                                   │
│ ────────────────────────────────────────────────────────────────────  │
│ APP TYPE       Field-service scheduling                                │
│ PRIMARY USER   Mobile-first technicians                                │
│ WORKFLOW       Receive jobs, schedule, dispatch                        │
│ PLATFORMS      Web, iOS, Android                                       │
└────────────────────────────────────────────────────────────────────────┘
```

- No card frame. The card is a hairline-divider-bounded zone (`borderTop: 1px solid hairline`).
- No background fill. Sits on the page's paper background directly.
- `BlockHandle` (`⋮⋮`) appears in the left gutter on hover (Notion-style drag affordance — visual only in v8, not yet wired to drag events).
- Header row: `✓ {LayerLabel}` Eyebrow + `· locked` Eyebrow + `Revisit ↗` button right-aligned.
- Body: italic Inter summary derived from the layer's locked state.
- Detail grid: 2-column grid of 4 key facts — Eyebrow labels (90px min-width) with Mono values.

By the time the user reaches Layer 8, all 7 prior layers stack visibly above the active canvas. The cumulative context is always readable.

### 2.6 Revisit behavior

Clicking `Revisit ↗` on any locked card sets `activeIndex` to that layer. The layer's state goes from `locked` back to `clean` (values preserved — revisiting is non-destructive).

When the user leaves a revisited layer (by locking again), it re-locks. Layers further down the stack remain locked. Their context cards remain in place. Their files stay drafted.

---

## 3. Per-layer state machine

Each layer has its own state, isolated from other layers. Four values:

```csharp
public enum LayerState
{
    Clean,        // No edits. Lock-and-continue is the action.
    Dirty,        // User edited canvas or typed in prompt. Generate-preview is the action.
    Previewing,   // AI rendered proposed redraw. Accept-or-discard.
    Locked,       // Terminal. Layer settled, file drafted, advanced past.
}
```

### 3.1 State transitions

```
                          ┌──────── lock-and-continue ───────────┐
                          │                                       ▼
         (default)        │                                    [locked]
       ┌────────────►  [clean] ──── edit canvas ────►  [dirty]
       │                  ▲ ▲                            │
       │                  │ │                            │ generate-preview
       │  ┌─ revisit ─────┘ │                            │
       │  │                 └──── discard-edits ─────────┤
       │  │                 ┌──── discard-preview ───────┤
       │  │                 │                            ▼
       │  │             [previewing] ◄── edit canvas ──[stays dirty]
       │  │                 │
       │  │                 └─── accept-and-lock ──────► [locked]
```

### 3.2 Triggers for going dirty

- Any change to the layer's own canvas state (editing `IntentCard.appType`, changing `DesignTokens.action`, etc.)
- Typing non-empty text in the layer's composer prompt textarea

Both are valid signals. The dirty state is **per-layer-scoped** — editing the Design canvas marks Design dirty, not Intent.

### 3.3 Snapshots and rollback

When a layer transitions clean → dirty, capture a snapshot of the layer's relevant state (intent values, design tokens, etc.). On `discard-edits` or `discard-preview`, restore from snapshot and clear the prompt.

```csharp
public record LayerSnapshot(
    Intent? IntentValues,
    DesignTokens? DesignValues,
    ArchitectureBlueprint? ArchValues,
    InteractionsMatrix? InteractionsValues,
    // … one nullable field per layer
);
```

Only the relevant slice is populated for each layer's snapshot.

### 3.4 Generate-preview behavior

```csharp
public ValueTask GeneratePreview()
{
    var id = ActiveLayer.Kind;
    var result = await _previewService.GeneratePreviewAsync(
        kind: id,
        currentValues: GetCurrentValues(id),
        userPrompt: GetPrompt(id),
        lockedContextSummaries: BuildLockedContext());

    _previewValues[id] = result.ProposedValues;
    _previewSummary[id] = result.Summary;
    SetLayerState(id, LayerState.Previewing);
}
```

The canvas re-renders using preview values when `LayerState == Previewing`. Diff signals (amber tint, "was X" annotations) are visible only in this state.

```csharp
public interface ILayerPreviewService
{
    Task<LayerPreviewResult> GeneratePreviewAsync(
        LayerKind kind,
        object currentValues,
        string userPrompt,
        IReadOnlyDictionary<LayerKind, string> lockedContextSummaries,
        CancellationToken ct = default);
}

public record LayerPreviewResult(
    object ProposedValues,    // typed per-layer; cast at consumption site
    string Summary);          // one-line italic-serif headline
```

Registered in DI at app startup. **Identity-preview fallback when no API key configured**: proposed values mirror dirty values exactly, summary reads *"Showing your edits as proposed."* This is critical — `Generate preview →` must always do something visible, even without an AI configured.

### 3.5 Accept-and-lock

```csharp
public async ValueTask AcceptAndLock()
{
    var id = ActiveLayer.Kind;
    if (_previewValues.TryGetValue(id, out var proposed))
        ApplyProposedValues(id, proposed);
    _previewValues.Remove(id);
    _previewSummary.Remove(id);
    _snapshots.Remove(id);
    SetLayerState(id, LayerState.Locked);
    ClearPrompt(id);
    AdvanceIfPossible();
}
```

Lock-and-continue (clean path) is the same except no proposed values to adopt — values are already canonical.

---

## 4. Editorial design system

Inter + JetBrains Mono. Hairlines as architecture. Color discipline. Marginalia annotations.

### 4.1 Typography

| Use                   | Family        | Size  | Weight | Notes                                |
|-----------------------|--------------|-------|--------|--------------------------------------|
| Display headline      | Inter        | 26px  | 500    | -0.015em tracking, no italic         |
| Heading sample        | JetBrains    | 15px  | 500    | Mono signals system-generated text   |
| Body / paragraph      | Inter        | 13-14px | 400  | line-height 1.55                     |
| Eyebrow / labels      | Inter        | 10-11px | 500  | uppercase, 0.04em tracking           |
| File path / code      | JetBrains    | 11-12px | 400-500 | -0.01em tracking                  |
| Hex codes             | JetBrains    | 12px  | 400    | inline value display                 |
| Caption metadata      | Inter        | 9-10px | 500   | uppercase, ink4                      |

JetBrains Mono is used **only** for code, file paths, hex values, keyboard shortcuts. Never for UI labels, even ones that feel "system-y." The discipline is what makes the workspace read as editorial-developer rather than terminal-clone.

### 4.2 Color palette

```
paper      #ffffff
paper2     #fbfbfb
paper3     #f5f5f5
paper4     #0a0a0a   (dark code blocks)

ink        #1a1a1a   (primary text)
ink2       #3a3a3a   (secondary text)
ink3       #737373   (tertiary text, italic body)
ink4       #a3a3a3   (metadata, captions)
ink5       #d4d4d4   (very low-emphasis chrome, BlockHandle)

hairline   #ececec   (primary section dividers)
hairline2  #f0f0f0   (nested row dividers, recede behind hairline)
hairlineDk #1f1f1f   (borders inside dark code blocks)

indigo     #3d3dff   (State (MVUX) module + writing file dot — ONLY)
amber      #c89c3f   (active marker + dirty/preview badge — ONLY)
amberSoft  #fdf8ef   (proposed-tint background for changed values)
```

Phase tints are scoped to the Implementation canvas only:
```
phaseRed    #b04534    SCAFFOLD
phaseBlue   #3d6f9a    SHELL
phasePurple #7d4fa0    DOMAIN
phasePink   #b4567d    SCREENS
phaseGreen  #6f8068    STATES
phaseAmber  #c89c3f    POLISH
```

Interactions canvas state colors are scoped to that canvas only:
```
Default state:  ink (#1a1a1a)
Loading state:  indigo (#3d3dff) — the "system working" color
Success state:  #7a9b6e (sage)
Empty state:    ink3 (#737373) — recessed, low-emphasis
Error state:    #b04534 (coral)
Offline state:  amber (#c89c3f)
```

### 4.3 Editorial primitives

Six reusable primitives. Every canvas uses them. Defined once in the codebase, referenced everywhere.

#### `Eyebrow` — caps metadata labels

```jsx
<span style={{
  fontFamily: SANS, fontSize: 11, fontWeight: 500,
  letterSpacing: '0.04em', textTransform: 'uppercase',
  color: ink3,
}}>
  Why this fits
</span>
```

Used for: layer indices, file labels, state badges, field names, file row statuses, section headers.

#### `Mono` — inline code values

```jsx
<span style={{
  fontFamily: MONO, fontSize: 12, fontWeight: 400,
  color: ink2, letterSpacing: '-0.01em',
}}>
  #c89c3f
</span>
```

Used for: hex codes, file paths, field type names, layer index in stack item.

#### `Body` — paragraph prose

```jsx
<span style={{
  fontFamily: SANS, fontSize: 14, fontWeight: 400,
  color: ink2, lineHeight: 1.55,
}}>
  Three entities — Job, Technician, Schedule — with explicit nullability.
</span>
```

Used for: descriptions, summaries, item titles, locked-card prose.

#### `SectionHeader` — canvas chrome

Replaces card-style canvas headers. Hairline divider only, no background fill, no rounded corners.

```jsx
<div style={{
  display: 'flex', alignItems: 'baseline',
  justifyContent: 'space-between',
  paddingBottom: 12, marginBottom: 16,
  borderBottom: `1px solid ${hairline}`,
}}>
  <Mono color={ink} size={12} weight={500}>{filename}</Mono>
  <Eyebrow size={10} color={badgeColor}>{badge}</Eyebrow>
  {action}
</div>
```

Filenames render lowercase: `blueprint.svg`, `interaction-states.md`, `ColorPaletteOverride.xaml`. The Mono rendering already provides typographic distinction; uppercasing both fights the eye.

#### `Annotation` — marginalia

Replaces the boxed `WhyThis` and `AgentPrompt` callouts from earlier prototypes. Italic Inter behind a 1px left rule.

```jsx
<div style={{
  paddingLeft: 16, marginTop: 18,
  borderLeft: `1px solid ${hairline}`,
}}>
  <Eyebrow size={10} color={ink3} weight={500}>
    Why this fits
  </Eyebrow>
  <span style={{
    fontFamily: SANS, fontSize: 13, fontStyle: 'italic',
    color: ink3, lineHeight: 1.55,
  }}>
    {voice === 'quote' ? <>"{children}"</> : children}
  </span>
</div>
```

Two voices controlled by `voice` prop:
- `voice="rationale"` (default): plain italic Inter, no quote marks. Used for `Why this X` annotations.
- `voice="quote"`: italic Inter wrapped in `"…"`. Used for `Agent prompt` content.

Reads as a developer's margin note — not a called-out warning.

#### `CodeBlock` — generated artifact preview

Light or dark theme, XAML or tree language.

```jsx
<CodeBlock
  label="ColorPaletteOverride.xaml"
  source="GENERATED FROM TOKEN MAP"
  code={xaml}
  language="xaml"      // 'xaml' | 'tree'
  theme="dark"         // 'dark' | 'light'
/>
```

Visual hierarchy:
- **Dark blocks** = "what the agent will write" (XAML, code, scaffold commands)
- **Light blocks** = "what the project will look like" (solution tree, file structures)

Both are valid in the same canvas; they signal different kinds of artifact.

XAML language tints: red elements, amber attributes, sage strings, ink3 italic comments.
Tree language only colors `# comments` (italic ink3 on light, italic sage on dark).

#### `BlockHandle` — Notion-style drag affordance

Small `⋮⋮` glyph in the left gutter (-22px offset) on hover. Visual only in v8 — drag events not wired. Used by `LockedContextCard`; can extend to other repeated rows.

### 4.4 Layout primitives

- **Hairlines as architecture**: sections are demarcated by 1px hairline dividers, not card frames with rounded corners and shadows. The visual rhythm is `paddingBottom: 12px` + `borderBottom: 1px solid hairline` + `marginBottom: 16px` between sections.
- **No card chrome around canvases**: canvas content sits on the page's paper background. `SectionHeader` is the only top-of-canvas chrome.
- **Tabular grids over cards**: phase plans, color tokens, data contracts render as tabular grids (`gridTemplateColumns: '40px 140px 1fr 240px'` etc.) — not as cards.
- **32px gutters between layers**: between locked context cards, the active canvas, and future preview cards.

### 4.5 Animation timing

| Concern                      | Duration | Easing                              |
|------------------------------|---------|-------------------------------------|
| Rail reveal width            | 480ms   | cubic-bezier(0.16, 1, 0.3, 1)       |
| Rail content opacity         | 320ms   | ease-in-out (160ms delay)           |
| Future card opacity          | 480ms   | cubic-bezier(0.16, 1, 0.3, 1)       |
| Progress indicator width     | 480ms   | cubic-bezier(0.16, 1, 0.3, 1)       |
| Hover visual property        | 200-240ms | ease                              |
| Button hover                 | 140ms   | ease                                |
| Background tint change       | 220ms   | ease                                |

Consistency matters more than absolute values. The 480ms ease-out-quint is the "workspace opening" rhythm; the 200-240ms ease is the "responsive interaction" rhythm.

---

## 5. Visualization patterns (v8 enrichment)

The Architecture and Interactions canvases use richer SVG-driven visualizations than the other six layers. Both share five underlying patterns.

### 5.1 Hand-drawn wobble via SVG turbulence filter

```jsx
<defs>
  <filter id="rough-arch" x="-3%" y="-3%" width="106%" height="106%">
    <feTurbulence type="fractalNoise" baseFrequency="0.022" numOctaves="2" seed="3" />
    <feDisplacementMap in="SourceGraphic" in2="turb" scale="1.5" />
  </filter>
  <filter id="rough-arch-bold" x="-3%" y="-3%" width="106%" height="106%">
    <feTurbulence type="fractalNoise" baseFrequency="0.018" numOctaves="2" seed="5" />
    <feDisplacementMap in="SourceGraphic" in2="turb2" scale="2" />
  </filter>
</defs>
```

Conventions:
- Two filter variants per canvas: default (scale 1.3-1.5) and bold (scale 1.8-2.0). Hovering swaps to bold.
- Distinct seeds per canvas — Architecture uses `seed="3"` and `"5"`, Interactions uses `"7"` and `"11"`. Same hand on different sheets of paper.
- Apply filter only to rect/path, never to `<text>`. Text inherits displacement and becomes illegible. Text is a sibling of the filtered rect with the same translate.
- Filter region `x="-3%" y="-3%" width="106%" height="106%"` provides 3% bleed so edges don't clip.

### 5.2 Connected-set hover computation

```jsx
const [hovered, setHovered] = useState(null);

const connected = useMemo(() => {
  if (!hovered) return null;
  const nodes = new Set([hovered]);
  const links = new Set();
  EDGES.forEach((e) => {
    if (e.from === hovered || e.to === hovered) {
      links.add(`${e.from}-${e.to}`);
      nodes.add(e.from);
      nodes.add(e.to);
    }
  });
  return { nodes, links };
}, [hovered]);
```

### 5.3 Three-state ternary for visual properties

```jsx
const isConn = isLinkConnected(edge);
const opacity = !connected ? 0.65 : (isConn ? 1 : 0.12);
const stroke  = !connected ? ink3 : (isConn ? ink : ink4);
const strokeWidth = isConn && connected ? 1.8 : 1;
```

Three states:
- **No hover** (`!connected`): default visual state — middle opacity, default stroke
- **Hovered + connected**: emphasized — full opacity, bold stroke
- **Hovered + non-connected**: dimmed — low opacity (0.12 for edges, 0.25 for nodes)

The conditional ensures the diagram has a "resting state" when nothing is hovered. Without it, the diagram would always look like nothing is connected.

### 5.4 Z-order: edges first, nodes on top

```jsx
<rect fill="url(#gridPattern)" />     // backdrop
{EDGES.map(...)}                      // edges (lines + labels)
{NODES.map(...)}                      // nodes (rects + labels + badges)
```

Nodes sit on top of edges. Edge labels sit on top of edge lines (via paper-colored masking rect). The click target is always the topmost visible element.

### 5.5 Rich detail panel

Below the SVG, in a 14px-vertical-padded zone with `minHeight: 64`:

```jsx
{hoveredItem ? (
  <>
    <div style={{ display: 'flex', gap: 12, marginBottom: 4 }}>
      <Eyebrow size={10} color={ink2} weight={600}>
        {hoveredItem.label}
      </Eyebrow>
      <Eyebrow size={9} color={ink4}>
        {/* per-canvas metadata: "4 files · 2 connections" or "trace transitions" */}
      </Eyebrow>
    </div>
    <Body color={ink2} size={13} style={{ fontStyle: 'italic' }}>
      {hoveredItem.description}
    </Body>
  </>
) : (
  <Body color={ink4} size={13} style={{ fontStyle: 'italic' }}>
    {restingPrompt}
  </Body>
)}
```

`minHeight` prevents layout shift as the user hovers in and out.

---

## 6. Layer 01 — Intent

**Purpose:** capture what the app is for in four named fields, plus an optional notes textarea.

**Canvas: `IntentCard`**

```
[intent.md · EDITING]

App type           Field-service scheduling
─────────────────────────────────────────────────────────────────────
Primary user       Mobile-first technicians
─────────────────────────────────────────────────────────────────────
Workflow           Receive jobs, schedule, dispatch
─────────────────────────────────────────────────────────────────────
Platforms          Web, iOS, Android
```

Tabular grid: `gridTemplateColumns: '120px 1fr 80px'`. Eyebrow labels (120px column), Inter 14px values (1fr column), `Proposed` annotation column (80px column, only renders during preview).

**MVUX state:**

```csharp
public record IntentValues(
    string AppType,
    string PrimaryUser,
    string Workflow,
    string Platforms,
    string Notes);

public IState<IntentValues> Intent => State<IntentValues>.Value(this, () => new(
    AppType:     "Field-service scheduling",
    PrimaryUser: "Mobile-first technicians",
    Workflow:    "Receive jobs, schedule, dispatch",
    Platforms:   "Web, iOS, Android",
    Notes:       ""));
```

The four primary fields are pre-populated with the recommended defaults. The user can edit (mark dirty) or accept all by clicking `Lock and continue →`.

**Edit triggers:** any field change marks Intent dirty. Composer prompt non-empty marks Intent dirty.

**Diff visualization:** during `previewing`, fields that differ between current and proposed get amberSoft background tint and a `Proposed` badge in the right column.

**Composer hooks:**
- Title: *"What are we building?"*
- Subtitle: *"Fill what you know. I'll infer the rest as we go."*
- Lead question (clean): *"If I summarize the intent right now, the agent has enough to scaffold a meaningful skeleton. Anything else worth adding before locking?"*
- Suggestion chips: `Mobile-first`, `Offline-first`, `No backend yet`

**Annotations:**

> *Why this intent: Field-service is a focused vertical with clear stakes — jobs, schedules, offline conditions. Naming the workflow here means the agent can stop guessing and start scaffolding domain types from §06 with confidence.*

> *Agent prompt: "Treat this as the canonical app description. Future layers should reference these terms verbatim — Job, Technician, Dispatch — not synonyms."*

**Locked card:**

```
✓ INTENT · LOCKED                                                Revisit ↗
"Field-service scheduling, for mobile-first technicians. Receive jobs, schedule, dispatch."
─────────────────────────────────────────────────────────────────────────
APP TYPE       Field-service scheduling
PRIMARY USER   Mobile-first technicians
WORKFLOW       Receive jobs, schedule, dispatch
PLATFORMS      Web, iOS, Android
```

**File output (`README.md`):**

```markdown
# {AppName}

{IntentValues.AppType} for {IntentValues.PrimaryUser}.

## What this app does

{IntentValues.Workflow}

## Platforms

{IntentValues.Platforms}

{If Notes is non-empty: ## Additional context\n\n{IntentValues.Notes}}
```

---

## 7. Layer 02 — UX

**Purpose:** capture how users move through the app, expressed as a horizontal flow strip of 5 screens.

**Canvas: `UXFlowStrip`**

```
[dispatch-flow.md · 5 SCREENS]

┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐
│SCREEN 1 │ →  │SCREEN 2 │ →  │SCREEN 3 │ →  │SCREEN 4 │ →  │SCREEN 5 │
│Dashboard│    │Job detl │    │Schedule │    │Dispatch │    │Confirm. │
│ ▭▭▭     │    │ ▭▭      │    │ ▭▭▭     │    │ ▭       │    │ ▭▭      │
│ ▭▭      │    │ ▭▭▭     │    │ ▭▭      │    │ ▭▭      │    │ ▭▭▭     │
│ ▭▭▭     │    │ ▭▭      │    │ ▭▭▭     │    │ ▭▭▭     │    │ ▭       │
│Today's  │    │Status,  │    │Drag-to- │    │Confirm  │    │Synced   │
│jobs     │    │location │    │reorder  │    │+ assign │    │or queued│
└─────────┘    └─────────┘    └─────────┘    └─────────┘    └─────────┘
```

Each tile: 116px wide, paper2 background, hairline border, 6px border-radius. Top: `SCREEN N` Eyebrow. Middle: screen name (Inter 13px weight 500). Mock UI blocks (3 horizontal hairlines at varying widths). Bottom: Mono note.

Between tiles: `→` arrow in Inter 14px ink4.

**MVUX state:**

```csharp
public record UXFlow(
    string Name,
    IImmutableList<ScreenDef> Screens);

public record ScreenDef(
    string Name,
    string Note,
    IImmutableList<UIBlock> MockBlocks);
```

**Edit triggers:** composer prompt only in v8. Direct manipulation (click-rename, drag-reorder, add screen) is future scope.

**Composer hooks:**
- Title: *"How do users move through it?"*
- Subtitle: *"Five screens for the primary dispatch flow. Architecture in §03 will pick the navigation primitive."*
- Lead question: *"Drag-to-reorder schedule, or list-with-time-pickers? Stay on screen after dispatch, or return to dashboard?"*
- Suggestions: `Drag-to-reorder`, `Return to dashboard`, `Modal confirmation`

**Annotations:**

> *Why this flow: Five steps maps to a five-second mental model. Confirmation isn't a modal — it's a real terminal screen so the offline-queued case has somewhere to live without surprising the user.*

> *Agent prompt: "Each screen represents a discrete user state. The architecture in §03 will pick the navigation primitive — for now, treat the flow as the canonical mental model regardless of how it gets wired."*

**Locked card:**

```
✓ UX · LOCKED                                                    Revisit ↗
Five-screen dispatch flow with confirmation as terminal state.
─────────────────────────────────────────────────────────────────────────
SCREENS         5
PRIMARY FLOW    Dispatch
EMPTY STATES    4
ERROR STATES    2
```

**File output (`ux-flows.md`):** section per flow listing screens in order, each screen with name + note + which states matter on it.

---

## 8. Layer 03 — Architecture

**Purpose:** capture the module shape of the app — what folders exist, what binds to what, what persists where. Rendered as a hand-drawn SVG blueprint with hover-to-explore highlighting.

**Canvas: `ArchitectureBlueprint`**

```
[blueprint.svg · LOCKED CONTEXT                          [↻ Regenerate]]

┌────────┐         ╲binds╱         ┌──────────────┐         ╲consumes╱     ┌──────────┐
│ Pages  │────────────────────────►│ State (MVUX) │────────────────────────►│   HTTP   │
└────────┘                         └──────────────┘                         └──────────┘
    │                                       │
 requests                               consumes
    │                                       │
    ▼                                       ▼
┌────────────┐                       ┌──────────────┐         ╲persists╱    ┌──────────┐
│ Navigation │                       │   Services   │────────────────────────►│ Storage  │
└────────────┘                       └──────────────┘                         └──────────┘

[hover detail panel: State (MVUX) · 6 files · 3 connections
                    Feeds, States, Selection]
```

SVG canvas: 800×340 viewBox. Hairline grid backdrop. Six modules in a 3×2 grid (top row y=90, bottom row y=230). Five labeled edges with dashed hairline strokes and italic Inter labels masked over the line.

**Hand-drawn wobble:** all rects and lines use `filter="url(#rough-arch)"` with `feTurbulence` + `feDisplacementMap`. Hovered modules switch to `filter="url(#rough-arch-bold)"` with stronger displacement.

**Hover-to-explore:**
- `useMemo` computes connected modules + edges for the hovered module
- Connected: full opacity, ink stroke at width 1.8, edge label gains weight 600 + ink fill
- Non-connected: opacity 0.25 for modules, 0.12 for edges

**File count badge:** when a module is hovered, a small `{N}f` pill appears at top-right of the rect, color-matched to the module color.

**MVUX state:**

```csharp
public record ArchitectureBlueprint(
    IImmutableList<ModuleDef> Modules,
    IImmutableList<EdgeDef> Edges);

public record ModuleDef(
    string Id,
    string Label,
    Point Position,
    Brush Color,
    string Description,
    int Files);

public record EdgeDef(
    string FromId,
    string ToId,
    string Label);
```

Default modules: Pages (ink), Navigation (ink), State (MVUX) (indigo), Services (ink2), HTTP (Kiota) (ink2), Storage (ink2). Default edges: Pages→MVUX (binds), Pages→Navigation (requests), MVUX→Services (consumes), Services→HTTP (calls), Services→Storage (persists).

**The `↻ Regenerate diagram` action** calls `ILayerPreviewService.GeneratePreviewAsync(LayerKind.Architecture, ...)` directly, bypassing the dirty-state check (explicit re-roll request).

**Edit triggers:** composer prompt only in v8. Drag/click-rename/edge editing are future scope. The state shape (`Position`, `Modules`, `Edges` as immutable lists) supports them when those land.

**Solution tree:** below the SVG, a separate `CodeBlock` with `language="tree"` + `theme="light"` renders the implied folder structure:

```
FieldDispatch/
├── FieldDispatch/                   # shared
│   ├── Models/
│   ├── Presentation/                # MVUX feeds
│   ├── Services/
│   ├── Views/
│   └── App.xaml
├── FieldDispatch.Mobile/             # iOS · Android
├── FieldDispatch.Desktop/            # WinAppSDK · Mac
├── FieldDispatch.Skia.Wpf/
├── FieldDispatch.Wasm/
└── FieldDispatch.Tests/
```

Light theme = paper background, italic ink3 comments. Reads as reference material, not as code in an editor window.

**Composer hooks:**
- Title: *"How is this app shaped?"*
- Subtitle: *"A blueprint of how modules connect, and the solution tree they imply."*
- Lead question: *"Two open questions before locking — does scheduling logic live in a service, or stay inside State (MVUX)? And do technicians authenticate, or is access role-less?"*
- Suggestions: `Region-based navigation`, `Single technician role`, `Offline-first`

**Annotations:**

> *Why this fits: MVUX gives both views identical reactive feeds with offline-first state — no MVVM ceremony, no Rx plumbing. Navigation regions let Shell and TabBar animate independently, which the dispatch flow in §02 depends on.*

> *Agent prompt: "When generating XAML, use uen:Region.Attached for navigation surfaces and bind ItemsSource to JobsModel.Jobs (an IFeed). Never construct ViewModels in code-behind."*

**Locked card:**

```
✓ ARCHITECTURE · LOCKED                                         Revisit ↗
Pages bind State (MVUX); Services consume HTTP and Storage.
─────────────────────────────────────────────────────────────────────────
MODULES        6
CONNECTIONS    5
PATTERN        MVUX
PERSISTENCE    Offline-first
```

**File output (`architecture.md`):**

```markdown
# Architecture

## Pattern
MVUX

## Modules

### Pages
Route surfaces — Calendar, Jobs, Technicians

Connects to:
- State (MVUX) — binds
- Navigation — requests

### Navigation
Region-based routes

### State (MVUX)
Feeds, States, Selection

Connects to:
- Services — consumes

[... one section per module ...]

## Connections summary

| From          | To              | Relationship |
|---------------|-----------------|--------------|
| Pages         | State (MVUX)    | binds        |
| Pages         | Navigation      | requests     |
| State (MVUX)  | Services        | consumes     |
| Services      | HTTP (Kiota)    | calls        |
| Services      | Storage         | persists     |

## Solution structure

```
{ Synthesized solution tree, same as canvas display }
```

## Why this fits

{ Italic-serif rationale from WhyThis block }

## Agent guidance

{ AgentPrompt content — load-bearing rules for downstream agent generation }
```

---

## 9. Layer 04 — Design System

**Purpose:** capture color tokens, typography scale, and live `ColorPaletteOverride.xaml`. Most-edited canvas of the eight.

**Canvas: `DesignTokenGrid`**

```
[design-tokens.md · EDITING]

Color tokens
┌──┬─────────────┬──────────┬──────────────┐
│██│ Surface     │ #0C0D0F  │              │
├──┼─────────────┼──────────┼──────────────┤
│██│ Action      │ #C89C3F  │              │  ← 1.5px amber border on Action swatch
├──┼─────────────┼──────────┼──────────────┤
│██│ Info        │ #7AB3DF  │              │
├──┼─────────────┼──────────┼──────────────┤
│██│ Success     │ #9BBF9D  │              │
├──┼─────────────┼──────────┼──────────────┤
│██│ Warn        │ #E87F6D  │              │
├──┼─────────────┼──────────┼──────────────┤
│██│ Panel       │ #16181C  │              │
├──┼─────────────┼──────────┼──────────────┤
│██│ Tag         │ #B288C4  │              │
├──┼─────────────┼──────────┼──────────────┤
│██│ Locked      │ #0C0D0F  │              │
└──┴─────────────┴──────────┴──────────────┘

Type scale
Display    Today's schedule                              26px
Heading    Job #4471 · Boiler service                    15px
Body       Tap a job to assign a technician.             13px
Caption    ARRIVED · 09:14 SYNCED                        10px

┌─ Primary ─────────────┐  ┌─ Tab Bar · Toolkit ──────┐
│ [Assign tech] [View map]│  │ │Today│Week│Map│        │
└────────────────────────┘  └───────────────────────────┘

[ColorPaletteOverride.xaml · GENERATED FROM TOKEN MAP]
<dark code block with synthesized XAML>
```

**Tabular color grid:** `gridTemplateColumns: '24px 1fr 100px 80px'` — swatch / name / hex / proposed-was column. Hairline2 row dividers between rows. The Action swatch has a 1.5px amber border (visual self-reference).

**Type scale grid:** `gridTemplateColumns: '60px 1fr 50px'` — Eyebrow label / live sample / size. Real app strings, not lorem ipsum:
- Display: "Today's schedule" (Inter 26px weight 500, -0.015em tracking)
- Heading: "Job #4471 · Boiler service" (JetBrains Mono 15px weight 500 — system identifier voice)
- Body: "Tap a job to assign a technician. Conflicts surface inline." (Inter 13px ink2)
- Caption: "ARRIVED · 09:14 SYNCED" (Eyebrow 10px ink3)

**Mini control gallery:** two boxes side by side (1px hairline border, no fill, 6px border-radius):
- PRIMARY: live `Assign tech` pill button (Action color background, paper4 foreground for 4.5:1 contrast) + ghost `View map` button
- TAB BAR · TOOLKIT: three segments `Today / Week / Map` (active = Action color background, paper4 foreground)

**MVUX state:**

```csharp
public record DesignTokens(
    Color Surface,    // app background
    Color Action,     // primary CTA — only saturated hue
    Color Info,       // informational accents
    Color Success,    // confirmations
    Color Warn,       // error/warn → Material ErrorColor
    Color Panel,      // elevated dark surface → Material SurfaceColor (Dark)
    Color Tag,        // secondary chips
    Color Locked,     // disabled state surface
    string BodyFont); // "Inter" / "Newsreader" / "Fraunces"

public IState<DesignTokens> Design => …
```

**Edit triggers:**
- Color picker (`<input type="color">` in React; in XAML: `Button` with `ColorPicker` flyout) → on change, mark dirty + update live mirror
- Body font dropdown → on change, mark dirty + update type scale rendering live
- Composer prompt → mark dirty

**Live `ColorPaletteOverride.xaml` synthesis:**

```csharp
public IFeed<string> ColorPaletteOverrideXaml => Design.Select(BuildXaml);

private static string BuildXaml(DesignTokens t)
{
    string Hex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    return $$"""
    <ResourceDictionary
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

      <ResourceDictionary.ThemeDictionaries>

        <ResourceDictionary x:Key="Light">
          <Color x:Key="PrimaryColor">#1A1A1A</Color>
          <Color x:Key="OnPrimaryColor">#FAFAFA</Color>
          <Color x:Key="SecondaryColor">{{Hex(t.Action)}}</Color>
          <Color x:Key="OnSecondaryColor">#1A1A1A</Color>
          <Color x:Key="SurfaceColor">#FAFAFA</Color>
          <Color x:Key="OnSurfaceColor">#1A1A1A</Color>
          <Color x:Key="BackgroundColor">#FFFFFF</Color>
          <Color x:Key="ErrorColor">{{Hex(t.Warn)}}</Color>
        </ResourceDictionary>

        <ResourceDictionary x:Key="Dark">
          <Color x:Key="PrimaryColor">#FAFAFA</Color>
          <Color x:Key="OnPrimaryColor">{{Hex(t.Surface)}}</Color>
          <Color x:Key="SecondaryColor">{{Hex(t.Action)}}</Color>
          <Color x:Key="OnSecondaryColor">#1A1A1A</Color>
          <Color x:Key="SurfaceColor">{{Hex(t.Panel)}}</Color>
          <Color x:Key="OnSurfaceColor">#FAFAFA</Color>
          <Color x:Key="BackgroundColor">{{Hex(t.Surface)}}</Color>
          <Color x:Key="ErrorColor">{{Hex(t.Warn)}}</Color>
        </ResourceDictionary>

      </ResourceDictionary.ThemeDictionaries>
    </ResourceDictionary>
    """;
}
```

**Token → Material slot mapping:**

| Design token | Material Light slot                    | Material Dark slot                     |
|--------------|----------------------------------------|----------------------------------------|
| Action       | SecondaryColor                         | SecondaryColor (consistent across themes) |
| Warn         | ErrorColor                             | ErrorColor (consistent across themes)  |
| Surface      | (Light uses #FAFAFA default)           | BackgroundColor                        |
| Panel        | (Light uses #FAFAFA default)           | SurfaceColor                           |

Info, Success, Tag, Locked don't fit canonical Material slots — they live as additional semantic resources in a separate `ThemeColorOverrides.xaml` file (future work, scope flag).

The XAML mirror updates immediately on any token edit. No `Generate Preview` needed for the live mirror — only for the lock cycle.

**Diff visualization:**
- Changed swatches get amberSoft background (`#fdf8ef`) + `was {oldHex}` Eyebrow in the right column
- Changed body font shows `Was {oldFont}` Eyebrow in the type scale section
- All inputs disabled (read-only divs replace `<input type="color">`)

**Composer hooks:**
- Title: *"How should it feel?"*
- Subtitle: *"Tokens, type scale on real sample copy, and the ColorPaletteOverride.xaml the agent will write."*
- Lead question: *"The palette is low-chroma for outdoor visibility. Want a brand override on Action, or stay with amber?"*
- Suggestions: `Stay with amber`, `Use a brand color`, `Show alternatives`

**Annotations:**

> *Why this palette: Outdoor sun + a phone in a truck mount means low chroma, high contrast. Amber is the only saturated hue and is reserved for the next action — never decoration.*

> *Agent prompt: "Apply tokens via ThemeResource — never hex. Reference SurfaceBrush, OnSurfaceBrush, SecondaryBrush. Light and Dark theme dictionaries let the same XAML render correctly in both themes."*

**Locked card:**

```
✓ DESIGN SYSTEM · LOCKED                                         Revisit ↗
Inter body, action #C89C3F, low-chroma palette.
─────────────────────────────────────────────────────────────────────────
BODY           Inter
ACTION         #C89C3F
TYPE SCALE     4 levels
SPACING        4px grid
```

**File output (`design-system.md`):** four sections — Color tokens table (token + hex + Material slot + themes), Typography (font families + 4-level scale with sizes + use case), Spacing rhythm (4-point grid: 4, 8, 12, 16, 24, 32, 48), Color override XAML (literal copy of synthesized file), plus rationale + agent guidance.

---

## 10. Layer 05 — Interactions

**Purpose:** capture every state of every flow, expressed as a state transition diagram. The 6-state contract (Default / Loading / Empty / Error / Success / Offline) is fixed and load-bearing — it maps exactly to VisualStateManager state names in generated XAML.

**Canvas: `StateTransitionDiagram`**

```
[interaction-states.md · 3 FLOWS · 6 STATES        [Create job][Sign in][Sync]]

           ┌─submit─►            ┌─resolves─►
   ┌───────────┐         ┌───────────────┐         ┌───────────┐
   │  Default  │         │    Loading    │         │  Success  │
   │   (ink)   │         │   (indigo)    │         │  (sage)   │
   └───────────┘         └───────────────┘         └───────────┘
        │                        │  ╲                    │
        │                  ┌─no data╲                    │
   disconnect              │         ╲rejects        reset
        │                  ▼          ▼                  │
        ▼              ┌─────┐    ┌─────┐                │ (long arc above)
   ┌───────────┐       │Empty│    │Error│                │
   │  Offline  │       └─────┘    └─────┘                │
   │  (amber)  │                     │                   │
   └───────────┘                  retry                  │
        │                            │                   │
        └──reconnect─►            ──►Loading             │
                                                         ▼
                                                     [back to Default]
```

SVG canvas: 800×340 viewBox. Hairline grid backdrop. Six pill-shaped states (`rx={22}` on a 44-tall rect) in a 3×2 grid:
- **Top row** (y=100): Default (130), Loading (400), Success (670) — primary user-facing states
- **Bottom row** (y=240): Offline (130), Empty (400), Error (670) — exceptional states

Eight directional curved transitions with arrowhead markers (via `<marker id="arrow-int" orient="auto-start-reverse">`).

**Hand-drawn wobble:** same turbulence-filter pattern as Architecture, with `seed="7"` and `seed="11"` for distinct visual character.

**Hover-to-explore:**
- Hovering any state highlights its incoming and outgoing transitions
- Connected: full opacity, ink stroke at width 1.8, bold arrow marker, italic edge labels gain weight 600 + ink fill
- Non-connected: state opacity 0.25, transition opacity 0.1

**Dual interaction model:**
- `hoveredState`: set on mouse enter, cleared on mouse leave. Drives path highlighting and detail panel preview.
- `activeState`: set on click, persists. Drives the pulsing dot affordance.

The detail panel shows whichever is more recent semantically:
- If hovering: hovered description; left rule turns amber; hint reads `hover preview · click to select`
- If not hovering: active description; left rule stays hairline; hint reads `hover any state to trace its transitions`

**Pulsing dot on active state:** 3.5px-radius amber circle at top-right of the active state pill, with `<animate>` pulsing opacity 1↔0.35 over 1.6s. Renders only when `isActive && !isHov` — hover takes visual priority.

**MVUX state:**

```csharp
public enum StateKind
{
    Default,    // baseline / first paint
    Loading,    // async work in progress
    Empty,      // no data yet, but not an error
    Error,      // user or system error, recoverable
    Success,    // confirmation, terminal positive
    Offline,    // connection unavailable, queued or read-only
}

public record InteractionsMatrix(
    IImmutableList<InteractionFlow> Flows);

public record InteractionFlow(
    string Id,
    string Label,
    IImmutableList<StateDef> States);

public record StateDef(
    StateKind Kind,
    string Description);

public IState<InteractionsMatrix> Interactions => …
public IState<string> ActiveFlowId => …       // local view state
public IState<StateKind> ActiveStateKind => … // local view state
```

The 6-state set is **fixed**. Every flow lists every state. The contract feeds the agent's downstream VSM generation — state names map exactly to VSM state names in generated XAML.

**3 default flows:** `create-job`, `sign-in`, `sync`. Switching the flow tab resets `ActiveStateKind` to `Default`.

**Per-flow specialization:** in v8, the diagram shape is shared across all 3 flows. Per-flow positions/transitions is future scope. The current canvas shows the canonical 6-state graph all flows must implement; the AI generates flow-appropriate descriptions when the layer locks.

**Edit triggers:** composer prompt only. No direct pill renaming/reordering — the 6-state contract is fixed.

**Composer hooks:**
- Title: *"What about every state, every flow?"*
- Subtitle: *"For each flow, six states. Click around to see what each one means."*
- Lead question: *"Offline state — queue silently, or always show a sync-pending banner?"*
- Suggestions: `Queue silently`, `Always show banner`, `Banner only when queue has items`

**Annotations:**

> *Why this matters: Offline-first means six states aren't optional — they're load-bearing. A technician in a basement with no signal needs the queued state to feel as resolved as success.*

> *Agent prompt: "Every screen must implement all six states. Use VisualStateManager groups named exactly: Default, Loading, Empty, Error, Success, Offline."*

**Locked card:**

```
✓ INTERACTIONS · LOCKED                                          Revisit ↗
Six states across three flows; offline-first throughout.
─────────────────────────────────────────────────────────────────────────
FLOWS              3
STATES/FLOW        6
OFFLINE            Yes
PERMISSION STATES  Yes
```

**Agent contract — VSM by name:**

The spec → XAML contract is **by name**. The agent generates VSM groups whose state names match the spec exactly. Implementation pattern uses Uno Toolkit's `VisualStateManagerExtensions`:

```xml
<Page xmlns:utu="using:Uno.Toolkit.UI"
      utu:VisualStateManager.States="{Binding CurrentState}">

    <VisualStateManager.VisualStateGroups>
        <VisualStateGroup x:Name="CreateJobStateGroup">
            <VisualState x:Name="Default" />
            <VisualState x:Name="Loading">
                <Storyboard>
                    <!-- skeleton row visuals -->
                </Storyboard>
            </VisualState>
            <VisualState x:Name="Empty" />
            <VisualState x:Name="Error" />
            <VisualState x:Name="Success" />
            <VisualState x:Name="Offline" />
        </VisualStateGroup>
    </VisualStateManager.VisualStateGroups>

    <!-- screen content -->
</Page>
```

The MVUX model exposes `CurrentState` as `IState<string>`. `VisualStateManagerExtensions.States` resolves the named state binding-driven, no procedural `GoToState` calls.

**VSM group naming convention:** `{FlowIdPascalCased}StateGroup`:
- `create-job` → `CreateJobStateGroup`
- `sign-in` → `SignInStateGroup`
- `sync` → `SyncStateGroup`

**File output (`interaction-spec.md`):** one section per flow, one subsection per state with shape:

```markdown
### Default
**Description:** Empty calendar; primary CTA visible.
**VSM group:** CreateJobStateGroup
**VSM state name:** Default

What the user sees: empty calendar grid with the "+" CTA in its primary position.
What the user can do: tap the CTA to start a new job; tap a date to scope creation to that day.
What the system does: render baseline calendar from cached data if available, else show skeleton.
Data required: calendar shape (week/month), today's date, current technician's available days.
```

The four-paragraph elaboration ("What the user sees / can do / system does / data required") is generated by the AI from the description string at lock time. Optional richer model would carry these as first-class fields on `StateDef` — currently lazy-derived.

---

## 11. Layer 06 — Data

**Purpose:** capture entity records with explicit fields and nullability. Renders as a vertical list of entity sections, each with a tabular field grid.

**Canvas: `DataContractGrid`**

```
[data-contracts.md · 3 RECORDS]

Job              record
─────────────────────────────────────────────────────────────────────
id                            string
title                         string
status                        enum
scheduledAt                   datetime
technicianId                  string
notes                         string?

Technician       record
─────────────────────────────────────────────────────────────────────
id                            string
name                          string
skills                        string[]
available                     bool

Schedule         record
─────────────────────────────────────────────────────────────────────
date                          date
jobs                          Job[]
conflicts                     Conflict[]

[Models/Job.cs · GENERATED FROM CONTRACTS]
public partial record Job(
  string Id,
  string Title,
  JobStatus Status,
  DateTime ScheduledAt,
  string TechnicianId,
  string? Notes);
```

Each entity section: hairline-divided zones (`paddingTop: 14, paddingBottom: 14, borderBottom: 1px solid hairline`). Header: Mono name (13px weight 600) + `record` Eyebrow. Field grid: `gridTemplateColumns: '160px 1fr'` — Mono ink2 field name + Mono indigo type.

The indigo type column is the only place outside Architecture where indigo surfaces. It signals "this is a type, not a value."

**MVUX state:**

```csharp
public record DataContracts(
    IImmutableList<EntityDef> Entities);

public record EntityDef(
    string Name,
    EntityKind Kind,                      // Record / Class / Struct
    IImmutableList<FieldDef> Fields);

public record FieldDef(
    string Name,
    string TypeText);                     // "datetime", "string?", "Job[]"
```

**Edit triggers:** composer prompt only.

**Composer hooks:**
- Title: *"What shapes does the data take?"*
- Subtitle: *"Three entities with explicit fields and nullability."*
- Lead question: *"Audit trail on jobs, or latest status only? GeoPoint as record struct, or two doubles?"*
- Suggestions: `Latest status only`, `Audit trail`, `GeoPoint as record`

**Annotations:**

> *Why this shape: Records, not classes — immutability + structural equality is what MVUX feeds need to detect change cheaply. Nullability is explicit because a missing Notes field shouldn't be a bug.*

> *Agent prompt: "Use C# records for all domain types. Mutations go through commands on the model, not setters. The '?' in field types is load-bearing — preserve it."*

**Locked card:**

```
✓ DATA · LOCKED                                                  Revisit ↗
Three entities — Job, Technician, Schedule — with explicit nullability.
─────────────────────────────────────────────────────────────────────────
ENTITIES        3
FIELDS          17
RECORDS         3
COLLECTIONS     3
```

**File output (`data-contracts.md`):** entity per section with fields rendered as a Markdown table. Plus a `Models/Job.cs` synthesized C# record file (one per entity).

---

## 12. Layer 07 — Implementation

**Purpose:** capture the phased build plan as a tabular grid — files, dependencies, agent prompts per phase.

**Canvas: `ImplementationPhaseGrid`**

```
[implementation-plan.md · 6 PHASES]

P1   Scaffold      Multi-head Uno solution wired with         "Run dotnet new
     Solution      MVUX + Uno.Extensions.                      unoapp. Verify each
     skeleton      + FieldDispatch.sln                         head compiles."
                   + Directory.Packages.props
                   + 4 platform heads
─────────────────────────────────────────────────────────────────────────
P2   Shell         Two regions: top-level Shell and inner     "Wire deeplink for
     Shell &       TabBar. Routes registered.                  jobs/{id}."
     navigation    + Views/Shell.xaml
                   + MainPage.xaml
                   + Navigation/RouteMap.cs
─────────────────────────────────────────────────────────────────────────
P3   Domain        WorkOrder, Technician, Conflict.            "Records immutable.
     Models &      JobsModel with IFeed + IListFeed.            Mutations through
     feeds         + Models/WorkOrder.cs                        commands on the
                   + Models/Technician.cs                       model."
                   + Presentation/JobsModel.cs
─────────────────────────────────────────────────────────────────────────
P4   Screens       Today, Week, Map, Job-detail. Tokens       "Drop into Hot Design
     Screens &     from §04 — no hardcoded colors.              after compile —
     bindings      + Views/TodayPage.xaml                       verify spacing scale."
                   + Views/WeekPage.xaml
                   + Views/JobDetailPage.xaml
─────────────────────────────────────────────────────────────────────────
P5   States        All six states from §05 wired into VSM     "Test offline state
     Interaction   groups per screen.                           with airplane mode;
     states        + Per-screen VSM definitions                 queued indicator
                   + Views/States/*.xaml                        must surface."
─────────────────────────────────────────────────────────────────────────
P6   Polish        UI tests, contrast verification,            "Each screen:
     Tests &       performance budgets, bundle profiling.       keyboard nav,
     a11y          + Tests/*.UI.cs                              contrast 4.5:1,
                   + perf harness                               sub-100ms first
                   + A11y audit                                 paint."
```

Tabular grid: `gridTemplateColumns: '40px 140px 1fr 240px'`. Hairline-row separators. Phase eyebrow (`P{N}` in phase color), title block, description + file list, agent prompt with thin left rule.

**MVUX state:**

```csharp
public record BuildPlan(
    IImmutableList<PhaseDef> Phases);

public record PhaseDef(
    int Number,
    string Label,                              // "Scaffold", "Shell", etc.
    string Title,                              // "Solution skeleton"
    string Description,
    IImmutableList<string> Files,
    string AgentPrompt,
    string After);                             // "—" for first phase
```

Phase colors map to `phaseRed / phaseBlue / phasePurple / phasePink / phaseGreen / phaseAmber` from the palette.

**Edit triggers:** composer prompt only.

**Composer hooks:**
- Title: *"How does it get built?"*
- Subtitle: *"Six phases as a tabular plan — files, dependencies, agent prompts."*
- Lead question: *"Strictly linear, or can Domain and Screens parallelize?"*
- Suggestions: `Strictly linear`, `Parallelize`, `Add a tooling phase`

**Annotations:**

> *Why this phasing: Linear with one parallel option (Domain + Screens) keeps the agent from rebuilding scaffolding when a domain change cascades. Each phase has a verifiable acceptance step before the next is unlocked.*

**Locked card:**

```
✓ IMPLEMENTATION · LOCKED                                        Revisit ↗
Six phases, scaffold → polish, with explicit dependencies.
─────────────────────────────────────────────────────────────────────────
PHASES          6
ACCEPTANCE      Per phase
VERIFICATION    Per phase
ORDER           Linear
```

**File output (`implementation-plan.md`):** phase per section, each section listing files + dependencies + acceptance criteria + verification step (the rich version is AI-generated; the local template is the skeleton).

---

## 13. Layer 08 — Scaffold

**Purpose:** terminal-style command block + bundle download. Final layer — no canvas-specific edit triggers, just the deliverables.

**Canvas: `ScaffoldTerminal`**

```
[scaffold.command · READY]

┌────────────────────────────────────────────────────────────── [Copy] ─┐
│ dotnet new unoapp \                                                   │
│   -n FieldDispatch \                                                  │
│   --tfm net10.0 \                                                     │
│   --platforms wasm,ios,android \                                      │
│   --markup xaml --presentation mvux --theme material \                │
│   --features config,http,logging,nav,mvux                             │
└────────────────────────────────────────────────────────────────────────┘

[Download bundle ↓] [Open in Rider]    the composition is, for now, complete.
```

Dark code block (paper4 background) with copy button. Below: action row with primary download + ghost open-in-Rider + italic completion caption right-aligned.

**Computed command:**

```csharp
public IFeed<string> ScaffoldCommand =>
    Feed.Combine(Intent, Architecture, Design)
        .Select(BuildCommand);

private string BuildCommand(...) =>
    $"""
    dotnet new unoapp \
      -n {input.intent.AppType.Replace(" ", "")} \
      --tfm net10.0 \
      --platforms {input.intent.Platforms.ToFlags()} \
      --markup xaml \
      --presentation mvux \
      --theme material \
      --features config,http,logging,nav,mvux
    """;
```

The command is generated entirely from prior layers' values — no Scaffold-specific state to edit.

**Edit triggers:** none. The Scaffold layer's `Lock and continue` is replaced by `Download bundle ↓` (which both finalizes the bundle and locks the layer).

**Composer hooks:**
- Title: *"The bundle is ready."*
- Subtitle: *"Every layer locked. Copy the scaffold, or download the full bundle."*
- No composer prompt or suggestion chips.

**Locked card:**

```
✓ SCAFFOLD · LOCKED                                              Revisit ↗
dotnet new unoapp · ready
─────────────────────────────────────────────────────────────────────────
COMMAND         Generated
BUNDLE          9 files
TARGETS         Web, iOS, Android
SDK             Uno.Sdk 6.5.29
```

**File output:** `scaffold.command` (the dotnet new command in shell-executable form) + `prompt-context.md` synthesized from all locked layers' file contents — a single agent-context document the user feeds to their AI of choice when they start building.

---

## 14. Composer footer

Below every active canvas, the composer footer renders the layer's lead question and the state-driven action buttons.

```
─────────────────────────────────────────────────────────────────────
COMPOSER · REFINING

Two open questions before this layer can lock — does scheduling logic
live in a service, or stay inside State (MVUX)?

[Region-based navigation] [Single technician role] [Offline-first]
┌──────────────────────────────────────────────────────────────────┐
│ Refine, or accept what's drawn…                                  │
└──────────────────────────────────────────────────────────────────┘

[Lock and continue →]  accepting the recommendation
```

### 14.1 Three states

| State        | Lead question / context                                        | Actions                                              |
|--------------|---------------------------------------------------------------|------------------------------------------------------|
| Clean        | Layer-specific lead question + suggestion chips + textarea     | `Lock and continue →` (ink primary)                  |
| Dirty        | Same lead question; textarea may have user input              | `Generate preview →` (amber primary) + `Discard edits` ghost |
| Previewing   | "Here's how I'd redraw this layer with your edits applied…"   | `Accept and lock →` (ink primary) + `← Discard preview` ghost |

The amber tone on `Generate preview →` is reserved for the "AI is about to do something" moment. Once previewing, the action returns to the standard ink primary.

### 14.2 Suggestion chips

Empty-state hint when the textarea is empty. Click a chip to populate the textarea (user can edit before submitting). 3-4 chips per layer surfacing the most common decisions the AI would expect.

### 14.3 Layer-aware composer prompt

The textarea is bound to `prompts[currentLayerId]` — each layer has its own draft text that persists across navigation. Editing the prompt for Design while on Design doesn't touch the prompt for Intent. Typing non-empty text marks the layer dirty.

### 14.4 Composer footer typography

- Eyebrow header: `COMPOSER · {state}` in 10px ink2 caps
- Lead question: Inter 14px ink, max-width 600px
- Suggestion chips: Inter 12px weight 500 ink3, hairline border, 5px border-radius
- Textarea: Inter 14px ink, hairline border (focused: ink2 border)
- Action buttons: per `PrimaryButton` and `GhostButton` specs

---

## 15. Files Rail (right column)

```
FILES
Each layer emits files as it locks.

●  README.md                              DRAFTED      ← amber dot, glow
●  ux-flows.md                            WRITING      ← indigo dot, pulsing
○  architecture.md                        PLANNED
○  design-system.md                       PLANNED
○  interaction-spec.md                    PLANNED
○  data-contracts.md                      PLANNED
○  implementation-plan.md                 PLANNED
○  scaffold.command                       PLANNED
○  prompt-context.md                      PLANNED

─────────────────────────────────────────
1 of 8 locked
UX will write ux-flows.md when locked.
```

### 15.1 File row

- Status dot: 7×7px circle, color-coded by state
- File name: Mono 12px, ink (writing/drafted) or ink3 (planned)
- Status badge: Eyebrow 9px, color-matched to dot

| State    | Dot fill           | Visual                                | Meaning                          |
|----------|--------------------|--------------------------------------|----------------------------------|
| Drafted  | amber              | amber 4px box-shadow at 13% alpha     | Layer locked, file written       |
| Writing  | indigo             | indigo 4px box-shadow + pulse 1.6s    | Layer is active, file in progress |
| Planned  | hollow ring (ink4) | row opacity 0.5                       | Future layer, not yet active     |

### 15.2 State transitions

```
planned ──── (layer becomes active) ────► writing ──── (layer locks) ────► drafted
                                          │
                                          └── (layer revisited) ──► writing again
```

When a locked layer is revisited and re-locked, its file briefly returns to writing then back to drafted.

### 15.3 MVUX state

```csharp
public IFeed<IImmutableDictionary<LayerKind, FileStatus>> FileStatuses =>
    Feed.Combine(ActiveLayer, LockedLayers).Select(BuildFileStatuses);

public enum FileStatus { Planned, Writing, Drafted }
```

### 15.4 Status panel at bottom

`{N} of 8 locked` Eyebrow + italic Inter context line:
- If all 8 locked: *"All layers locked. Bundle ready."*
- Otherwise: *"{ActiveLayer.Label} will write {ActiveLayer.File} when locked."*

---

## 16. Composition Stack (left column)

```
COMPOSITION STACK
A conversation that crystallizes into a build system.
─────────────────────────────────────────────────────────────────────
01  INTENT                                       ✓
    "Field-service scheduling, for mobile-first technicians."

02  UX                                           ✓
    5 screens, dispatch flow

03  ARCHITECTURE                            ← active (amber border)
    Pages → MVUX → Services → HTTP, Storage

04  DESIGN SYSTEM                                ← future (0.5 opacity)
    how it feels

05  INTERACTIONS                                 ← future
    every state of every flow

06  DATA                                         ← future
    shapes and contracts

07  IMPLEMENTATION                               ← future
    phased build plan

08  SCAFFOLD                                     ← future
    runnable starting point
```

### 16.1 Stack item

- Index (Mono 11px weight 500): amber if active, ink2 if locked, ink4 if future
- Label (Inter 13px): ink + weight 600 if active, ink + weight 500 if locked, ink3 if future
- ✓ glyph: appears for locked layers
- Summary line: italic Inter 12px ink2 (locked) or ink3 (active/future)
- Border-left: 2px amber if active, 2px ink2 if locked, 2px transparent if future
- Background: paper3 if active, paper2 on hover (clickable rows only), transparent otherwise
- Cursor: pointer if active or locked (clickable for revisit/jump), default for future

### 16.2 Click behavior

- Click an active row: no-op
- Click a locked row: `setActiveIndex(i)` — revisit that layer (state goes locked → clean, values preserved)
- Click a future row: no-op (future layers can't be jumped to until locked or active)

### 16.3 Sticky positioning

`position: sticky, top: 32, alignSelf: 'flex-start', maxHeight: 'calc(100vh - 32px), overflowY: auto'` — the stack stays visible as the center column scrolls.

---

## 17. Acceptance criteria

The complete checklist for a v8-conformant implementation.

### 17.1 Foundation

- [ ] `LayerKind` enum has exactly 8 values in order: Intent, UX, Architecture, DesignSystem, Interactions, Data, Implementation, Scaffold
- [ ] `LayerState` enum has 4 values: Clean, Dirty, Previewing, Locked
- [ ] `MainModel.LayerStates` is an `IState<IImmutableDictionary<LayerKind, LayerState>>` initialized to all-Clean
- [ ] `MainModel.LayerSnapshots` is a private `Dictionary<LayerKind, LayerSnapshot>` populated lazily on dirty
- [ ] `Layers.All` static immutable array provides metadata (label, file, hint) — single source of truth
- [ ] `ILayerPreviewService` is registered in DI; falls through to identity preview when no API key configured

### 17.2 Workspace shell

- [ ] `RailsVisible` is an `IFeed<bool>` computed from `ActiveIndex` and `LockedLayers`
- [ ] When `RailsVisible == false`, both rails have width 0 + opacity 0
- [ ] When `RailsVisible == false`, the workspace shell is centered, active canvas max-width 720, padding-top 64
- [ ] Rail width transitions 0 → 260 over 480ms with cubic-bezier(0.16, 1, 0.3, 1)
- [ ] Rail content opacity transitions 0 → 1 over 320ms with 160ms delay
- [ ] Reset reverses rail animations cleanly via the inverse storyboard
- [ ] Progress indicator is a 1px hairline + amber-fill segment + counter `02 / 08`
- [ ] Progress segment width transitions over 480ms with the same curve as rails

### 17.3 Per-layer behavior

- [ ] Editing a layer's canvas value marks that layer dirty
- [ ] Typing non-empty text in the composer prompt marks the active layer dirty
- [ ] Snapshot is captured on clean → dirty transition; restored on discard
- [ ] `GeneratePreview()` calls `ILayerPreviewService` and stores results in `_previewValues[layerId]` + `_previewSummary[layerId]`
- [ ] Each canvas reads from preview values when state is Previewing; renders amber-tint diff signals
- [ ] `AcceptAndLock()` adopts preview values, clears them, transitions to Locked, advances activeIndex
- [ ] Each layer's locked card derives summary + 4-fact details from live MVUX state (not fixtures)

### 17.4 Files Rail

- [ ] 8 layer files + `prompt-context.md` row rendered in order
- [ ] File status dot uses amber (drafted), indigo (writing), hollow ring (planned)
- [ ] Writing dot pulses 1.6s ease-in-out infinite via Storyboard
- [ ] Drafted dot has amber 4px box-shadow at 13% alpha
- [ ] Status panel at bottom shows `{N} of 8 locked` + context line

### 17.5 Composition Stack

- [ ] All 8 layers render with state-coded styling (active amber border, locked ink2 border + ✓, future 0.5 opacity)
- [ ] Click locked or active row jumps to that layer
- [ ] Click on locked row sets state back to Clean (values preserved)
- [ ] Sticky position keeps the stack visible during center-column scroll

### 17.6 Editorial design system

- [ ] Inter and JetBrains Mono are loaded as `SANS` and `MONO`
- [ ] Color palette uses v8 values from §4.2
- [ ] `Eyebrow`, `Mono`, `Body`, `SectionHeader`, `Annotation`, `CodeBlock`, `BlockHandle` primitives exist
- [ ] Filenames in SectionHeader render lowercase (e.g., `blueprint.svg`, not `BLUEPRINT.SVG`)
- [ ] Annotation primitive replaces all v6 WhyThis + AgentPrompt boxes
- [ ] No card chrome around canvases — hairline dividers only
- [ ] Locked context cards render without card chrome

### 17.7 Layer 01 — Intent

- [ ] `IntentValues` record with 5 fields (AppType, PrimaryUser, Workflow, Platforms, Notes)
- [ ] Tabular grid with `gridTemplateColumns: '120px 1fr 80px'`
- [ ] Field changes mark layer dirty
- [ ] Diff visualization: amberSoft background + Proposed badge in column 3 for changed fields

### 17.8 Layer 02 — UX

- [ ] `UXFlow` + `ScreenDef` records
- [ ] 5 screen tiles (116px wide) with Eyebrow + name + mock blocks + note
- [ ] Read-only in v8 (composer prompt only edits)

### 17.9 Layer 03 — Architecture

- [ ] `ArchitectureBlueprint` record with `Modules` and `Edges` immutable lists
- [ ] `ModuleDef` includes `Description` and `Files: int`
- [ ] SVG canvas: 800×340 viewBox, hairline grid backdrop
- [ ] 6 modules in 3×2 grid, 5 dashed edges with italic Inter labels
- [ ] `rough-arch` and `rough-arch-bold` filters in SVG defs (seeds 3 and 5)
- [ ] Module rectangles use `filter="url(#rough-arch)"`, swap to bold on hover
- [ ] Hover-to-explore: connected modules + edges keep full opacity, others dim to 0.25 / 0.12
- [ ] File count badge `{N}f` appears at top-right of hovered module
- [ ] Detail panel shows `{label} · {N} files · {N} connections` + description on hover
- [ ] Solution tree below SVG uses `CodeBlock` with `theme="light" language="tree"`
- [ ] `↻ Regenerate` action calls preview generation directly

### 17.10 Layer 04 — Design System

- [ ] `DesignTokens` record with 8 colors + `BodyFont` (defaulting to "Inter")
- [ ] Tabular color grid: `gridTemplateColumns: '24px 1fr 100px 80px'`
- [ ] Action swatch has 1.5px amber border (visual self-reference)
- [ ] Type scale grid: `gridTemplateColumns: '60px 1fr 50px'`
- [ ] Display sample uses Inter 26px weight 500 with -0.015em tracking (no italic)
- [ ] Heading sample uses JetBrains Mono 15px weight 500
- [ ] Mini control gallery: PRIMARY box + TAB BAR · TOOLKIT box, 1px hairline border, no fill
- [ ] Action button uses paper4 foreground for 4.5:1 contrast against amber
- [ ] `ColorPaletteOverride.xaml` is synthesized live from tokens with Light + Dark dictionaries
- [ ] Diff tint uses amberSoft (#fdf8ef)

### 17.11 Layer 05 — Interactions

- [ ] `StateKind` enum with exactly 6 values
- [ ] `InteractionsMatrix`, `InteractionFlow`, `StateDef` records
- [ ] 3 default flows × 6 states each
- [ ] `STATES` constant: 6 entries with `pos`, `label`, `color`, `description` per state
- [ ] State positions form 3×2 grid (top y=100, bottom y=240)
- [ ] State colors: Default ink, Loading indigo, Success #7a9b6e, Empty ink3, Error #b04534, Offline amber
- [ ] `TRANSITIONS` constant: 8 entries with explicit `path`, `labelX`, `labelY`
- [ ] `rough-int` and `rough-int-bold` filters (seeds 7 and 11)
- [ ] `arrow-int` and `arrow-int-bold` markers with `orient="auto-start-reverse"`
- [ ] State pills use `rx={22}` (fully rounded on short axis)
- [ ] `hoveredState` and `activeState` are separate state hooks
- [ ] Detail panel left rule turns amber when hover-previewing (hovered ≠ active)
- [ ] Pulsing dot renders only when `isActive && !isHov`, opacity 1↔0.35 over 1.6s
- [ ] VSM group naming follows `{FlowIdPascalCased}StateGroup`
- [ ] VSM state names match `StateKind` enum values exactly

### 17.12 Layer 06 — Data

- [ ] `DataContracts`, `EntityDef`, `FieldDef` records
- [ ] Hairline-divided entity sections with header (Mono name + record Eyebrow)
- [ ] Field grid: `gridTemplateColumns: '160px 1fr'`
- [ ] Type column rendered in indigo Mono
- [ ] `Models/Job.cs` synthesized C# record CodeBlock below

### 17.13 Layer 07 — Implementation

- [ ] `BuildPlan`, `PhaseDef` records
- [ ] Tabular grid: `gridTemplateColumns: '40px 140px 1fr 240px'`
- [ ] Phase eyebrow uses phase color (red/blue/purple/pink/green/amber)
- [ ] Agent prompt rendered in italic Inter behind thin left rule

### 17.14 Layer 08 — Scaffold

- [ ] Computed scaffold command from Intent + Architecture + Design state
- [ ] Dark code block (paper4 background) with copy button
- [ ] Action row: Download bundle ↓ (primary) + Open in Rider (ghost) + italic completion caption
- [ ] No composer prompt or suggestion chips
- [ ] `prompt-context.md` synthesized from all locked layer file contents at scaffold lock

### 17.15 Composer footer

- [ ] State-driven action buttons: Lock and continue (clean, ink), Generate preview (dirty, amber), Accept and lock (previewing, ink) + Discard (ghost)
- [ ] Per-layer prompt persists in `prompts[layerId]` across navigation
- [ ] Suggestion chips show only when textarea is empty
- [ ] Lead question replaced by preview-context line when state is Previewing

### 17.16 Future preview cards

- [ ] Render only when `RailsVisible == true`
- [ ] Opacity = `Math.max(0.05, 0.40 - (distance × 0.08))`
- [ ] Opacity transitions over 480ms with ease-out-quint when activeIndex changes
- [ ] `pointerEvents: 'none'` (read-only)
- [ ] No card chrome — dashed hairline border + paper background only

---

## 18. Out of scope / future work

What v8 explicitly does NOT cover, organized by domain.

### 18.1 AI integration

- The prototype's `generatePreview()` is mocked (identity copy of dirty values). Real AI integration via `ILayerPreviewService.GeneratePreviewAsync` is the canonical extension point. Wire to Claude / GPT / etc. via the Anthropic / OpenAI SDKs.
- Cross-layer awareness: the AI receives `lockedContextSummaries` but the prototype doesn't use them. Real integration would feed Intent + UX summaries into Architecture's recommendations, etc.

### 18.2 Direct manipulation in canvases

- **Architecture**: drag modules, click-to-rename, edge editing — state shape supports them, no UI wired
- **UX**: drag-to-reorder screens, click-to-rename, add/remove screens
- **Interactions**: state pill renaming, transition rerouting, per-flow specialization (current diagram is shared across all 3 flows)
- **Data**: add/remove entities, edit field types directly
- **Implementation**: drag phases to reorder, edit file lists inline
- **Design**: spacing token editor, type scale size editor, full Material Theme Builder DSP integration

### 18.3 Persistence

- No persisted state across sessions in v8. In-flight compositions are lost on app restart.
- Production would need a versioned local-storage schema with serialization for `LayerStates`, `Snapshots`, all per-layer MVUX state, plus the file rail status map.

### 18.4 Visualization enrichments

- **Real rough.js integration** — current SVG turbulence filter is a simulation. Real rough.js (via WebView2 hybrid or Win2D composition shaders) would give richer hand-drawn aesthetics with multiple parallel strokes, hatched fills, and explicit imperfection.
- **Animated state transition playback** — a "play happy path" button that animates Default → Loading → Success along the transition diagram
- **Computed transition path routing** — current Interactions transitions use hand-tuned SVG paths. A graph routing algorithm (Dijkstra-like with anchor points) could compute paths from state positions.
- **Module file lists in detail panel** — current Architecture detail panel shows `{N} files` count. Future could list actual file names.

### 18.5 Layers without v8-style enrichment

The Architecture and Interactions canvases got rich SVG diagrams in v8. Other layers could benefit from similar treatment:

- **Data** — entity-relationship diagram with foreign keys, connection arrows, type indicators
- **UX** — real screen mockups in tiles instead of generic block placeholders, with a navigation graph showing transitions between flows
- **Implementation** — Gantt-style phase timeline with dependencies as arrows
- **Design** — Material Theme Builder integration, contrast pair visualizations, palette accessibility audit

### 18.6 Cross-layer dependency enforcement

The user can technically advance through layers in any order. Sequential flow is convention, not enforcement. A future addition could gate `Lock and continue →` on "all required fields filled" per layer, or add explicit validation per layer-lock.

### 18.7 Localization

The composer prompt strings, lead questions, suggestion chips, and annotation copy are all English. The `interaction-spec.md` template includes a `Localization` field that's currently unused. Production would localize all UI text via `x:Uid` resource keys and have the AI generate localized output files based on user locale.

### 18.8 Cross-tab state sharing

If multiple browser tabs / app windows have the same composition open, edits in one don't propagate to others. Real-time collab via SignalR or similar is far future.

### 18.9 Export formats

v8 outputs nine markdown files + one shell command. Future export options:
- ZIP bundle download
- Direct push to GitHub repo
- Copy to clipboard as a single concatenated context blob (the synthesized `prompt-context.md` is close but the export path could be smoother)
- Integration with Claude Code, Cursor, Copilot, etc. — write the bundle directly into the user's project root

---

## 19. Reference: Uno Platform integration

Specific Uno-isms baked into the Engine's output and the agent prompts.

### 19.1 Uno.Sdk version

`6.5.29` is the current latest stable. Hardcoded in `Directory.Packages.props` of generated projects. The Engine could fetch the latest from NuGet at scaffold lock time — currently uses a constant.

### 19.2 UnoFeatures

The scaffold command's `--features` flag maps to `<UnoFeatures>` entries in the project's csproj. v8 always includes:

```
config, http, logging, nav, mvux
```

Maps to:
```xml
<UnoFeatures>
  Material;
  Toolkit;
  Hosting;
  Configuration;
  Http;
  HttpKiota;
  Logging;
  Mvux;
  Navigation;
</UnoFeatures>
```

Other UnoFeatures available based on user prompts during composition:
- Authentication / AuthenticationMsal / AuthenticationOidc — when user mentions auth
- Localization — when user mentions multiple languages
- ThemeService — always for design-system handling
- Storage — when offline-first is mentioned
- Maps — when location features are mentioned
- MediaPlayerElement — when media playback is mentioned

### 19.3 Architecture defaults

Tagged as REC (recommended) in the Engine's defaults:
- **Pattern**: MVUX (over MVVM)
- **Markup**: XAML (over C# Markup)
- **Renderer**: Skia (over Native)
- **HTTP**: Kiota (over Refit)
- **Navigation**: Region-based (uen:Region.Attached)
- **Theme**: Material (over Cupertino, Fluent)

### 19.4 Toolkit Controls used

The Engine's UI references these Uno Toolkit controls in agent prompts:
- `Card` (with explicit style)
- `ChipGroup` for suggestion chips
- `NavigationBar` for top-level shell
- `AutoLayout` for canvas content layout
- `TabBar` for sub-navigation
- `DrawerControl` for offline state banners
- `LoadingView` for loading state visuals
- `ResponsiveView` for adaptive layouts
- `SafeArea` for mobile screen edges
- `ShadowContainer` for elevated cards

The agent prompt for Architecture explicitly says: *"Use Toolkit Controls in preference to building custom equivalents."*

### 19.5 Single Project structure

Generated projects use Uno's Single Project pattern: one `{AppName}.csproj` with multiple TFMs (`net10.0-windows10.0.19041.0`, `net10.0-ios18.0`, `net10.0-android35.0`, `net10.0-maccatalyst18.0`, `net10.0-desktop`, `net10.0-browserwasm`).

Platform-specific code lives in `Platforms/{Platform}/` folders.

Optional companion projects:
- `{AppName}.Api` (ASP.NET Core) — only when user mentions a backend
- `{AppName}.UITests` — always included for the Polish phase

---

## 20. Reference: prototype source

The reference React prototype is `composer-context-engine.jsx` in the parent directory. Approximate structure:

```
Lines 1-50      : version comment + tokens + LAYERS array + easings
Lines 50-260    : editorial primitives (Eyebrow, Mono, Body, buttons, BlockHandle, SectionHeader, Annotation, CodeBlock)
Lines 260-470   : workspace shell parts (ProgressIndicator, StackItem, CompositionStack, LockedContextCard, FuturePreviewCard, ActiveLayerHeader)
Lines 498-560   : IntentCanvas
Lines 561-614   : UXCanvas
Lines 615-820   : MODULES + EDGES + ArchitectureCanvas (with rough filter, hover-to-explore, file count badges)
Lines 825-995   : DesignCanvas (tabular color grid, type scale, control gallery, ColorPaletteOverride.xaml synthesis)
Lines 997-1230  : STATES + TRANSITIONS + InteractionsCanvas (state transition diagram with arrows)
Lines 1230-1290 : DataCanvas
Lines 1290-1345 : ImplementationCanvas
Lines 1345-1380 : ScaffoldCanvas
Lines 1380-1470 : ComposerFooter
Lines 1470-1530 : FilesRail
Lines 1530-end  : main CompositionEngine export with state management + render orchestration
```

Total: 1764 lines. All braces, parens, brackets balanced (verified via awk).

---

## 21. Glossary

- **Layer**: one of the 8 composition steps (Intent, UX, Architecture, etc.)
- **Canvas**: the active layer's rendered UI in the center column
- **Stack**: the left-rail Composition Stack showing all 8 layers
- **Rail**: either the left Composition Stack or the right Files Rail
- **Locked context card**: the compressed summary card a layer becomes after locking
- **Future preview card**: the dimmed read-only card showing an upcoming layer's hint
- **Composer footer**: the prompt + action area at the bottom of every active canvas
- **Annotation**: a marginalia note (italic Inter behind a thin left rule), either rationale ("Why this X") or quote ("Agent prompt")
- **Eyebrow**: small caps metadata label, Inter 10-11px weight 500, 0.04em tracking, uppercase
- **MVUX**: Uno Platform's reactive state pattern (Models, Views, Updates, eXperiences)
- **VSM**: VisualStateManager — XAML's mechanism for visual state transitions
- **Hover-to-explore**: the v8 interaction pattern where hovering an SVG element highlights its connected elements and dims unrelated ones
- **Three-state ternary**: the visual property pattern — `!hovered ? default : (isConnected ? emphasized : dimmed)`
- **Connected-set**: the `useMemo`-computed `Set` of element IDs connected to the currently hovered element

---

## End of brief

Length: ~2050 lines. Self-contained. All eleven delta briefs (`update-context-engine-01-foundations.md` through `update-context-engine-11-interactions-rich.md`) are superseded by this single document for design intent and implementation planning. The deltas remain useful as historical record of the design's evolution.

The reference prototype in `composer-context-engine.jsx` is the executable spec. When this brief and the prototype disagree, the prototype wins — this brief is the design intent that the prototype implements.
