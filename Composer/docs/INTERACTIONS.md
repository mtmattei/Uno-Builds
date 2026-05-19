# Interactions

This document is the source of truth for **every state, transition, and animation** in the Composer. If a control's behavior disagrees with this document, the control is wrong.

## Motion profile

The app uses a single motion profile across all interactions:

- **Easing for entrance and "task complete":** `cubic-bezier(0.16, 1, 0.3, 1)` — fast start, gentle settle. In Uno: `<KeySpline ControlPoint1="0.16,1" ControlPoint2="0.3,1" />`.
- **Easing for hover and minor state changes:** `ease-out` (standard built-in `EasingFunctionBase` with `EasingMode="EaseOut"`).
- **Duration tokens:**
  - `Tight` = 140ms (button press feedback)
  - `Quick` = 180ms (hover transitions, focus border swaps)
  - `Standard` = 220ms (chip color flips, button shape morphs)
  - `Comfortable` = 240ms (chevron rotation)
  - `Reveal` = 280ms (alternatives panel collapse/expand)
  - `Fluent` = 320ms (status glyph fill, body expansion)
  - `Generous` = 360ms (platform chip morph)
  - `Editorial` = 380ms (status glyph check draw, pop animations)
  - `Mount` = 480ms (message mount slide-in)
  - `Page` = 600ms (initial sheet mount)

These should be defined as `Duration` resources in `Themes/Animations.xaml`:

```xml
<Duration x:Key="DurationTight">0:0:0.140</Duration>
<Duration x:Key="DurationQuick">0:0:0.180</Duration>
<!-- ... -->
```

## Animation inventory

### `pageIn` — initial sheet mount

When the app first loads, the inset sheet animates in.

- Property: `Translation` (Y) and `Opacity`.
- From: `Translation="0,12"`, `Opacity="0"`.
- To: `Translation="0,0"`, `Opacity="1"`.
- Duration: 600ms.
- Easing: `cubic-bezier(0.16, 1, 0.3, 1)`.
- Trigger: `Loaded` event of the page root.
- Fires once per session.

### `msgIn` — message mount

When a new `MessageBlock` is added, it slides up and fades in.

- Property: `Translation` (Y) and `Opacity`.
- From: `Translation="0,8"`, `Opacity="0"`.
- To: `Translation="0,0"`, `Opacity="1"`.
- Duration: 480ms.
- Easing: `cubic-bezier(0.16, 1, 0.3, 1)`.
- Trigger: when `JustAddedId == this message's Id`. After 800ms, the model clears `JustAddedId` so the animation doesn't replay on re-render.

In MVUX terms, `JustAddedId` is an `IState<Guid?>`. After every layer transition, it's set to the new message's id. A timer (or `Task.Delay`) resets it after 800ms.

### `dotPulse` — thinking indicator

Three dots in `ThinkingIndicator` pulse continuously while the API is in flight.

- Property: `Opacity` and `ScaleTransform`.
- Keyframes:
  - 0%: `Opacity=0.3`, `Scale=1`
  - 50%: `Opacity=1`, `Scale=1.25`
  - 100%: `Opacity=0.3`, `Scale=1`
- Duration: 1100ms per cycle.
- Easing: `EaseInOut`.
- Repeats: forever while visible.
- Per-dot delays: 0ms, 160ms, 320ms.
- Trigger: visible whenever `IsThinking == true`.

### `dotPop` — emphasis pop

Defined in the prototype's stylesheet for potential future use. Not currently triggered. Specification preserved for completeness:

- Property: `ScaleTransform`.
- Keyframes: 0% scale 1 → 50% scale 1.6 → 100% scale 1.
- Duration: arbitrary (used as a single-shot 300ms emphasis).

### Status glyph: dot → check

The animation that fires when an artifact transitions from `Planned` to `Drafted`. Two layered properties.

**Layer 1: outer circle fills**
- `Fill`: `Transparent` → `InkBrush`.
- `Stroke`: `Ink4Brush` → `InkBrush`.
- Duration: 320ms.
- Easing: `EaseOut`.
- Delay: 0ms.

**Layer 2: inner check draws**
- `StrokeDashOffset` of the inscribed check path: `12` → `0`.
- Duration: 380ms.
- Easing: `cubic-bezier(0.16, 1, 0.3, 1)`.
- Delay: 140ms — **this delay is essential**. The circle fills first, then the check draws over the now-black background. Removing the delay makes the glyph read as a binary swap.

The dash math: the check path's total length is `≈12` units (manually measured). `StrokeDasharray="12"` + animating `StrokeDashOffset` from `12` to `0` reveals the path as if drawn.

### Chevron rotation

Both the artifact card chevron and the Show alternatives chevron.

- Property: `RotateTransform.Angle`.
- From: `0°` (chevron points down, ▽).
- To: `180°` (chevron points up, ▵).
- Duration: 240ms (artifact card) / 220ms (Show alternatives).
- Easing: `EaseOut`.
- Trigger: `Expanded` dependency property changes.

### Alternatives panel collapse/expand

The collapsible region inside the suggestion card containing Alternatives + (on design layer) Asset inputs.

- Property: `MaxHeight` of an outer `Border` wrapping the content, plus `Opacity` of the content.
- Closed: `MaxHeight=0`, `Opacity=0`.
- Open: `MaxHeight=`(natural content height), `Opacity=1`.
- Duration: 280ms.
- Easing: `EaseOut`.
- Trigger: `AlternativesOpen` state changes.

**Implementation note:** Uno doesn't have CSS `grid-template-rows: 0fr ↔ 1fr` semantics. Use one of:
- A `MaxHeight` Storyboard targeting a fixed high value (e.g., 800) for the open state. Acceptable when content height is bounded.
- Measure-based: on first open, measure the content's natural height, then animate to that value. Use a `SizeChanged` handler to recompute on viewport changes.

The `MaxHeight=800` approach is pragmatic — the content here is small (3 alternatives + 3 asset fields max). Don't over-engineer.

### Artifact card body expand

Same pattern as the alternatives panel.

- Property: `MaxHeight` of the body `Border`.
- Closed: `MaxHeight=0`.
- Open: `MaxHeight=`(natural).
- Duration: 320ms.
- Easing: `EaseOut`.
- The `Border` has `ClipToBounds="True"` so children clip during the animation.

### Platform chip morph

Multi-property morph between text-label and icon states.

**Unselected → Selected:**
- `Width` of text run: `(natural)` → `0`.
- `Width` of icon span: `0` → `14`.
- `Opacity` of text run: `1` → `0` (fades during shrink).
- `Opacity` of icon span: `0` → `1` (fades during grow), delayed 100ms.
- `ScaleTransform` of icon: `0.6` → `1`, delayed 100ms.
- `Background`: `Transparent` → `InkBrush`.
- `BorderBrush`: `HairlineBrush` → `InkBrush`.
- `Foreground`: `Ink2Brush` → `PaperBrush`.

**Durations:**
- Width: 360ms `cubic-bezier(0.16, 1, 0.3, 1)`.
- Opacity: 240ms `EaseOut`, 100ms delay (fade in for icon).
- Scale: 320ms `cubic-bezier(0.16, 1, 0.3, 1)`, 100ms delay.
- Color/border: 220ms `EaseOut`.

**Implementation:**
- Use `VisualStateManager` with `Unselected` and `Selected` states inside the chip's template.
- Wire `IsSelected` (DependencyProperty) → `GoToState`.
- Define Storyboards inside each `VisualState`'s `Storyboard`.
- The text-run `Width` collapse is the trickiest — use `DoubleAnimation` on a `Grid.Width` of the text-containing column. Set the column to `Auto` initially and animate to `0`. Alternatively, swap `Visibility` with a `Width` animation on the parent.

### Hover transitions

Three patterns recurring throughout the app.

**Border hover** (alternative button, ghost button, edit pill, runtime chip when unselected):
- `BorderBrush`: `HairlineBrush` → `InkBrush` (or `Ink2Brush` for some variants).
- `Foreground`: corresponding ink → ink.
- 180ms `EaseOut`.

**Background hover** (alternative button):
- `Background`: `PaperBrush` → `Paper3Brush`.
- 180ms `EaseOut`.

**Pre-on-hover** (artifact card body's read-only pre):
- `BorderBrush`: `HairlineBrush` → `Ink3Brush`.
- `Background`: `Paper3Brush` → `Paper2Brush`.
- Edit hint label: `Opacity 0 → 1`.
- 180ms `EaseOut`.

### Send button morph

Empty/disabled → ready transition on the send button.

- `Background`: `Transparent` → `InkBrush`.
- `Foreground`: `Ink4Brush` → `PaperBrush`.
- `BorderBrush`: `HairlineBrush` → `InkBrush`.
- 180ms `EaseOut`.

The morph is driven by two conditions: (a) input is non-empty, AND (b) `IsThinking == false`. Wire as a single boolean state computed from `IFeed<bool> CanSend`.

## State machine: chat layers

The conversation moves through seven layers in order. Each layer corresponds to one composer turn (with one suggestion + alternatives).

```
description → wiring → design → interactions → architecture → plan → done
```

### Per-layer specifications

#### `description`

- **Trigger:** user clicks Begin Composing in the foundation panel.
- **Composer turn:**
  - Body: `"Foundation set for \"{appName}\". Three files drafted."`
  - Callouts: `[{ layer: "Foundation", status: "Locked in", notes: "README.md, CLAUDE.md, scaffold.sh drafted" }]`
  - Post: `"What does {appName} do? A sentence or two will sharpen everything that follows."`
  - **No suggestion panel** — pure free-text question.
- **User response:** free-text input. Submitted via the InputBox.
- **On submit:**
  1. Update `AppDescription` state with the text. This re-fires the foundation effect (README + CLAUDE re-draft with the description).
  2. Append a `USER` `MessageBlock` with the user's text.
  3. Mark the description composer turn as `appliedLabel = userText` so when collapsed, it shows the user's description (truncated to 80 chars).
  4. Fire `GenerateContextualReasonings` (single API call — see "API integration" below).
  5. After 50ms, push the wiring prompt.

#### `wiring`

- **Composer turn:**
  - Body: `"Got it. README and CLAUDE updated with your description."`
  - Post: `"Now to wire the agent."`
  - Suggestion: `"Connect Uno docs and Figma"`, action label `"Apply"`, drafts `.mcp.json` with two servers.
  - Reasoning (default): `"Uno docs gives the agent authoritative API references; Figma lets it pull design tokens directly."` Replaced by `Reasonings["wiring"]` if the contextual API call succeeded.
  - Alternatives:
    - `"Uno docs only"` → drafts `.mcp.json` with one server.
    - `"All four (Uno docs, Figma, GitHub, Filesystem)"` → drafts with four.
    - `"No MCP servers for now"` → drafts `{"mcpServers": {}}`.
  - Free-text hint: `"Or list the servers you want."`
- **On Apply or Alternative:** drafts `.mcp.json`, pushes design prompt.
- **On free text:** routes to API (see "Free-text override" below).

#### `design`

- **Composer turn:**
  - Body: `"Wiring is set."`
  - Callouts: `[{ layer: "Wiring", status: "Locked in", notes: ".mcp.json drafted" }]`
  - Post: `"Design tokens next."`
  - Suggestion: `"Material defaults"`, drafts DESIGN.md with `Material` source.
  - Reasoning (default): `"Material has the broadest control coverage and renders consistently across every Uno target."` Replaceable by `Reasonings["design"]`.
  - Alternative: `"Fluent defaults"` → drafts with `Fluent` source.
  - **Asset inputs** (special panel): three optional fields — Figma URL, Prototype URL, Screenshots. When at least one is non-empty, "Apply with these assets" becomes active and applies a custom `applyAssetsAction`.
  - Free-text hint: `"Or describe a custom system below."`
- **On Apply with assets:** drafts DESIGN.md including the provided URLs in references. The applied label uses the truthy assets, e.g. `"Custom assets (Figma, prototype)"`.

#### `interactions`

- **Composer turn:**
  - Body: `"Design tokens drafted."`
  - Callouts: `[{ layer: "Design system", status: "Locked in", notes: "DESIGN.md drafted" }]`
  - Post: `"Motion and interaction patterns next."`
  - Suggestion: `"Smooth (cubic easing, 180–420ms durations)"` → drafts INTERACTIONS.md with `motion: 'smooth'`.
  - Reasoning (default): `"Smooth easing with mid-length durations works for most apps — fast enough to feel responsive, slow enough to read as deliberate."`
  - Alternatives:
    - `"Snappy (100–220ms durations)"` → `motion: 'snappy'`
    - `"Minimal (system defaults, no custom motion)"` → `motion: 'minimal'`
  - Free-text hint: `"Or describe specific motion or gesture patterns."`

#### `architecture`

- **Composer turn:**
  - Body: `"Interactions set."`
  - Callouts: `[{ layer: "Interactions", status: "Locked in", notes: "INTERACTIONS.md drafted" }]`
  - Post: `"Architecture next."`
  - Suggestion: `"MVUX · XAML · Skia · Kiota · Region nav · Material"` → drafts ARCHITECTURE.md with the recommended stack (plus current `Runtime` and `Platforms` from foundation state).
  - Reasoning (default): `"MVUX with XAML markup and Skia rendering is the recommended stack for new Uno apps in 2025+."`
  - Alternatives:
    - `"MVVM · XAML · Skia · Refit · Frame nav · Fluent"`
    - `"MVUX · C# Markup (no XAML) · Skia · Kiota · Region · Material"`
  - Free-text hint: `"Or describe a custom stack."`

#### `plan`

- **Composer turn:**
  - Body: `"Architecture set."`
  - Callouts: `[{ layer: "Architecture", status: "Locked in", notes: "ARCHITECTURE.md drafted" }]`
  - Post: `"Implementation plan next."`
  - Suggestion: `"Six generic vertical slices"`, action label `"Accept"` → drafts implementation-plan.md with the standard six.
  - Reasoning: `"Six vertical slices cover the typical cross-cutting concerns of any line-of-business app."`
  - Alternative: `"Three minimal slices (shell, list, detail)"` → drafts a shorter plan.
  - Free-text hint: `"Or describe the slices that fit your app."`

#### `done`

- Terminal layer.
- Body: `"Plan accepted. All eight artifacts drafted."`
- Callouts: `[{ layer: "Plan", status: "Locked in", notes: "implementation-plan.md drafted" }]`
- Post: `"Ready to scaffold. Download the bundle below — every file is yours to refine."`
- No suggestion panel.
- The Download bundle button becomes prominent below the artifact panel.

## State model

Every piece of state below corresponds to an `IState<T>` or `IFeed<T>` on `MainModel`.

| Name | Type | Initial | Notes |
|---|---|---|---|
| `AppName` | `IState<string>` | `""` | Foundation field. Required. Trimmed on read. |
| `AppDescription` | `IState<string>` | `""` | Free-text from the description layer. |
| `Platforms` | `IState<ImmutableHashSet<PlatformKind>>` | `{Web, Android, iOS}` | Multi-select foundation field. |
| `Runtime` | `IState<RuntimeKind>` | `Net10` | `.NET 10` or `.NET 9`. |
| `Messages` | `IState<ImmutableList<ChatMessage>>` | `[]` | Conversation transcript. |
| `Artifacts` | `IState<ImmutableDictionary<ArtifactKind, Artifact>>` | All planned, empty content | Live build state. |
| `InputValue` | `IState<string>` | `""` | Current input box value. |
| `IsThinking` | `IState<bool>` | `false` | API call in flight. |
| `JustAddedId` | `IState<Guid?>` | `null` | Drives `msgIn` animation. Cleared 800ms after set. |
| `ErrorMsg` | `IState<string?>` | `null` | API failure banner. |
| `AlternativesOpen` | `IState<bool>` | `false` | Whether the latest composer turn's alternatives panel is open. |
| `Reasonings` | `IState<ImmutableDictionary<LayerKind, string>>` | `{}` | Contextual reasoning per layer, populated post-description. |
| `DesignAssets` | `IState<DesignAssets>` | `("","","")` | Figma URL, Prototype URL, Screenshots. |
| `ExpandedIds` | `IState<ImmutableHashSet<ArtifactKind>>` | `{}` | Which artifact cards are visually expanded. |
| `EditingId` | `IState<ArtifactKind?>` | `null` | Which artifact (if any) is in inline edit mode. |

**Derived feeds** (computed from the above):

```csharp
public IFeed<bool> FoundationReady => AppName.Select(n => !string.IsNullOrWhiteSpace(n))
    .CombineLatest(Platforms.Select(p => p.Count > 0), (a, b) => a && b);

public IFeed<int> DraftedCount => Artifacts.Select(a =>
    a.Values.Count(art => art.Status == ArtifactStatus.Drafted));

public IFeed<bool> CanSend => InputValue.Select(v => !string.IsNullOrWhiteSpace(v))
    .CombineLatest(IsThinking, (a, b) => a && !b);

public IFeed<bool> IsEmpty => Messages.Select(m => m.Count == 0);
```

## Layer-to-artifact map (drives expansion sync)

```csharp
public static readonly ImmutableDictionary<LayerKind, ImmutableArray<ArtifactKind>> LayerToArtifacts =
    new Dictionary<LayerKind, ImmutableArray<ArtifactKind>>
    {
        [LayerKind.Description]  = ImmutableArray.Create(ArtifactKind.Readme, ArtifactKind.Claude),
        [LayerKind.Wiring]       = ImmutableArray.Create(ArtifactKind.Mcp),
        [LayerKind.Design]       = ImmutableArray.Create(ArtifactKind.Design),
        [LayerKind.Interactions] = ImmutableArray.Create(ArtifactKind.Interactions),
        [LayerKind.Architecture] = ImmutableArray.Create(ArtifactKind.Architecture),
        [LayerKind.Plan]         = ImmutableArray.Create(ArtifactKind.Plan),
        [LayerKind.Done]         = ImmutableArray<ArtifactKind>.Empty,
    }.ToImmutableDictionary();
```

**Sync rule:** `ExpandedIds = LayerToArtifacts[CurrentLayer]`.

This drives the artifact panel's expansion to follow the chat's current focus. When the chat is on the wiring question, `.mcp.json` is the expanded card. When the chat advances to design, `.mcp.json` collapses (with check) and DESIGN.md expands (planned, with placeholder body) since DESIGN.md is what the user's next decision will populate.

The replacement is total — when `ExpandedIds` is set, any prior expansions (manual or auto) are cleared. The user can still manually click a drafted card to re-expand it; that manual expansion will be cleared at the next layer transition.

## Interaction patterns

### Foundation completion

- User types in `AppName` TextBox → `AppName` state updates → foundation effect fires → README and CLAUDE re-draft (since both depend on `AppName`).
- User toggles a `PlatformChip` → `Platforms` state updates → foundation effect → README and scaffold.sh re-draft.
- User toggles `RuntimeChip` → `Runtime` updates → CLAUDE and scaffold.sh re-draft.
- All three artifacts (README, CLAUDE, scaffold) transition `Planned → Drafted` together once `AppName` is non-empty AND at least one platform is selected.
- The `StatusGlyph` for each runs the dot→check animation when this happens.

### Begin conversation

- The Begin Composing button is visible only when `FoundationReady && Messages.Count == 0`.
- Click pushes the description prompt into `Messages` (no `USER` turn precedes it).
- Sets `JustAddedId` to the new message's id.
- Sets `ExpandedIds = LayerToArtifacts[Description]` = `{Readme, Claude}`. README and CLAUDE auto-expand.

### Suggestion-and-action

The primary interaction model. On every composer turn after the description, the user has three response paths:

1. **Apply** — accepts the suggestion. Drafts the suggestion's `Updates`, advances chat to `NextLayer`.
2. **Pick alternative** — selects from the alternatives list. Same effect, different content.
3. **Free text** — types in the input box and submits. Routes to the API (see below).

**Apply / Alternative click flow:**
1. Set `AlternativesOpen = false`.
2. Find the latest COMPOSER message in `Messages`. Mutate its `appliedLabel = action.Label` so it'll render as a `CompactComposerTurn` next render.
3. Append the new composer prompt for `action.NextLayer`.
4. Set `JustAddedId` to the new message's id (drives `msgIn`).
5. For each `(ArtifactKind, content)` in `action.Updates`: set that artifact to `(Drafted, content)`. Triggers status glyph animation.
6. Set `ExpandedIds = LayerToArtifacts[action.NextLayer]`. Previous expansion clears.
7. Set `EditingId = null`.

No USER message is appended for Apply / Alternative — **the selection is the reply**. The action's `Label` becomes the `appliedLabel` shown in the now-compact composer turn ("DESIGN SYSTEM · Material defaults").

### Alternatives toggle

- The "Show alternatives" / "Show alternatives & assets" / "Hide options" button toggles `AlternativesOpen`.
- Only the latest composer turn's panel responds — older ones are compact.
- Animation: `MaxHeight` of the wrapping border, 280ms ease-out, plus chevron rotation 220ms.

### Design assets opt-in

A user-supplied alternative path on the design layer.

- Three text inputs in the alternatives panel (revealed when `AlternativesOpen`).
- Each binds two-way to a property of `DesignAssets`.
- "Apply with these assets" button:
  - Disabled (gray) when all three inputs are empty.
  - When at least one is non-empty, becomes active.
  - On click: builds an action dynamically — reading `DesignAssets` state at click time so the user can edit until the moment they click. Action.Label is computed: `"Custom assets (Figma, prototype)"` based on which fields are filled.
  - Drafts DESIGN.md with the URLs included in its body.
  - Same downstream effect as Apply.

**Implementation note:** the React prototype models this as a getter (`applyAssetsAction: () => action`) that closes over current state. In MVUX, equivalent is a `ValueAsync` read at the moment of the command:

```csharp
public async ValueTask ApplyDesignAssetsAsync(CancellationToken ct)
{
    var assets = await DesignAssets;
    var action = BuildDesignActionWithAssets(assets);
    await ApplyActionAsync(action, ct);
}
```

### Inline edit on artifact cards

- Click on an expanded drafted artifact's `<pre>` body → enters edit mode.
- The pre swaps to a TextBox; auto-focus, cursor at end.
- `Background="PaperBrush"`, `BorderBrush="InkBrush"`, mono 10.5px.
- Commit on `Blur`, `Cmd/Ctrl+Enter`, or `Tab`.
- Cancel on `Esc`.
- Commit writes the new content into `Artifacts[id].Content`. Status stays `Drafted`.
- Editing has no effect on the chat — it's a local override of the drafted content. The bundle export uses the latest content regardless of how it was set.

### Go back to a prior decision (Edit pill)

The `Edit` button on a `CompactComposerTurn` truncates the conversation back to that turn.

1. Truncate `Messages` up to (not including) this turn's index.
2. Reset all artifacts whose layer is at or after this turn's layer (via `ArtifactResetsFromLayer` map).
3. Clear `ExpandedIds`, `EditingId`, `AlternativesOpen`.
4. If `layer == Description`, also clear `AppDescription`.
5. After 30ms, push the layer's prompt fresh — it becomes the new latest, with auto-expansion driven by the layer.

```csharp
public static readonly ImmutableDictionary<LayerKind, ImmutableArray<ArtifactKind>> ArtifactResetsFromLayer =
    new Dictionary<LayerKind, ImmutableArray<ArtifactKind>>
    {
        [LayerKind.Description] = ImmutableArray.Create(
            ArtifactKind.Mcp, ArtifactKind.Design, ArtifactKind.Interactions, ArtifactKind.Architecture, ArtifactKind.Plan),
        [LayerKind.Wiring]       = ImmutableArray.Create(
            ArtifactKind.Mcp, ArtifactKind.Design, ArtifactKind.Interactions, ArtifactKind.Architecture, ArtifactKind.Plan),
        [LayerKind.Design]       = ImmutableArray.Create(
            ArtifactKind.Design, ArtifactKind.Interactions, ArtifactKind.Architecture, ArtifactKind.Plan),
        [LayerKind.Interactions] = ImmutableArray.Create(
            ArtifactKind.Interactions, ArtifactKind.Architecture, ArtifactKind.Plan),
        [LayerKind.Architecture] = ImmutableArray.Create(
            ArtifactKind.Architecture, ArtifactKind.Plan),
        [LayerKind.Plan]         = ImmutableArray.Create(ArtifactKind.Plan),
    }.ToImmutableDictionary();
```

Note: foundation artifacts (README, CLAUDE, scaffold) are never reset by going back through chat — they're driven by foundation state, not chat state.

### Free-text override (API path)

When the user types text and hits Send instead of clicking Apply or Alternative.

1. Read `InputValue`, trim, clear input.
2. Append a `USER` `MessageBlock` with the text. Set `JustAddedId`.
3. Determine current layer from the latest COMPOSER message's `Layer`.
4. **Description layer special-case:** save text to `AppDescription` directly, push wiring prompt after 50ms. Fire contextual reasoning generation. **No API call here for the layer transition.** The user's free-text IS the description.
5. **Other layers:** call `IComposerApiService.ContinueAsync(messages, ctx)`. While in flight, set `IsThinking = true`. The thinking indicator displays.
6. On API response:
   - Parse JSON: `{ pre, callouts, post, updates }`.
   - Append a COMPOSER message with the parsed content.
   - For each artifact id in `updates`, set `(Drafted, content)`.
   - After 200ms, push the `nextLayer`'s standard prompt — the conversation continues with a fresh suggestion for the next decision.
   - Set `IsThinking = false`.
7. On API error: set `ErrorMsg`, set `IsThinking = false`.

The 200ms gap between API response and pushing the next prompt is intentional — it lets the user briefly see what their override produced before the chat advances. During this 200ms, `ExpandedIds` is still on the previous layer's targets, so the just-drafted artifact remains expanded.

### Contextual reasoning generation

Single API call after the user submits their description.

- **Trigger:** description submission, immediately after `AppDescription` is set.
- **Endpoint:** `claude-sonnet-4-20250514`, `max_tokens: 800`.
- **System prompt:** `"You're helping a developer build \"{name}\" — {description}. For each of five upcoming technical decisions, write ONE single-sentence justification grounded in the SPECIFIC needs of this app. ..."` (see prototype source for exact text).
- **Response:** JSON `{ wiring, design, interactions, architecture, plan }` — five strings.
- **On success:** set `Reasonings` to the parsed dictionary. Each subsequent composer turn's `SuggestionPanel` reads `Reasonings[layer]` if present, falling back to the static reasoning.
- **On failure:** silent. Static reasoning remains.

The render of reasoning in `SuggestionPanel` should compute live: `var text = Reasonings.GetValueOrDefault(message.Layer) ?? message.StaticReasoning;`. So the swap-in happens automatically once the API resolves, even if the wiring turn was already shown by then.

### Reset

The Reset button in the header.

- Clears `AppName`, `AppDescription`, `InputValue`, `ErrorMsg`, `Reasonings`, `DesignAssets`, `Messages`, `ExpandedIds`, `EditingId`.
- Sets `Platforms` back to `{Web, Android, iOS}`, `Runtime` to `Net10`.
- Resets all `Artifacts` to `(Planned, "")`.
- The page returns to the empty state.

No confirmation dialog. This is intentional — the prototype is exploratory; reset is fast and recoverable (the user just restarts the conversation).

### Download bundle

When `DraftedCount == 8`, the Download bundle button appears.

- Click triggers `IBundleExporter.ExportAsync(Artifacts)`.
- Output: a single markdown file `golden-path-composition.md`.
- Format:
  ```markdown
  # Golden Path Composition

  > Generated by The Composer · {YYYY-MM-DD}
  >
  > This bundle contains 8 files. Save each section under the indicated filename.

  ---

  ## README.md

  ```md
  # MyApp
  ...
  ```

  ---

  ## CLAUDE.md

  ```md
  # Project Instructions
  ...
  ```

  ---

  ...
  ```

- Fence languages: `.md` → `md`, `.json` → `json`, `.sh` → `bash`.
- For each artifact: `## {filename}\n\n` then the fenced content (or `_(not drafted)_` if planned, but this branch shouldn't fire since the button is gated on full draft).
- WASM trigger: invoke browser download via `Microsoft.JSInterop`-equivalent or Uno's `FileSavePicker` (which routes through browser's file save dialog).

## Keyboard interactions

| Key | Context | Behavior |
|---|---|---|
| `Enter` | InputBox | Submit if non-empty AND not thinking. |
| `Shift+Enter` | InputBox | Insert newline (do not submit). |
| `Esc` | Artifact card edit mode | Cancel edit, restore prior content. |
| `Cmd/Ctrl+Enter` | Artifact card edit mode | Commit edit. |
| `Tab` | Artifact card edit mode | Commit edit (via Blur). |

No global shortcuts. No focus-trap modals (there are no modals).

## Accessibility

- Every interactive element has `AutomationProperties.Name` set explicitly.
- `PlatformChip`: `IsToggle=true`, `Name="{x:Bind PlatformName}"`.
- `RuntimeChip`: `Name="{x:Bind RuntimeName}"`, `Toggle.ToggleState` reflects selection.
- ArtifactCard header button: `Name="{x:Bind Filename}"`, `HelpText="Click to expand"` when drafted; `IsEnabled=false` when planned.
- Send button: `Name="Send message"`, `IsEnabled` reflects `CanSend`.
- Apply button: `Name="Apply suggestion"`.
- Suggestion alternative buttons: `Name="{x:Bind Alternative.Label}"`.
- Edit pill: `Name="Edit this decision"`.
- Reset: `Name="Reset composition"`.
- Begin composing: `Name="Begin composing"`.

Color contrast: every text-on-background pair in the palette passes WCAG AA at 4.5:1 for body text and 3:1 for large text. The disabled `Ink4Brush` on `PaperBrush` is at threshold (~3.4:1) — acceptable for placeholder/secondary text but **never used for primary actionable text**.

Touch targets: minimum 44×44 effective area. Chips, buttons, and pills all clear this with their padding. The artifact card header row is the full panel width × ~40px tall — well above threshold.

## Performance considerations

- The transcript is virtually unbounded but in practice will hold ~14 messages max (7 layers × 2 turns each minus the first). Use `ItemsRepeater` without virtualization — overhead isn't worth it.
- The artifact panel is fixed at 8 items. Direct binding, no virtualization.
- Animations all use compositor-eligible properties (`Opacity`, `Translation`, `Scale`, `RotateTransform`). No layout-triggering animations except `MaxHeight`, which is bounded.
- The `MaxHeight` animation on alternatives + body is the only layout-triggering animation. With small bounded content, this is fine.
- `Storyboards` are cached on the visual tree. Don't construct new ones per state change — define them in `VisualState`s.

---

## Complete component state catalog

Every component in the app has a finite set of states. This is the exhaustive list. If a state isn't here, it doesn't exist; if a control's behavior isn't listed, it should match the closest analog here.

### `AppName` TextBox
- **Empty** (default): italic placeholder "name your app", `Foreground=Ink4Brush`. Border `HairlineBrush`.
- **Empty + focused**: same as empty, border `InkBrush`. The italic stays until a non-whitespace character is typed.
- **Typing / valid**: upright serif 16, `Foreground=InkBrush`. Border `InkBrush` while focused, `HairlineBrush` when blurred.
- **Whitespace-only**: treated as empty for `FoundationReady`. The TextBox itself shows whatever was typed (don't strip whitespace from the visual; only the validity check trims).

### `PlatformChip`
- **Unselected** (default): pill outline, text shown, icon collapsed. `BorderBrush=HairlineBrush`, `Foreground=Ink2Brush`.
- **Unselected + hover**: border + text → `InkBrush`. 180ms.
- **Unselected + focused**: same as hover (no separate focus ring; focus and hover converge on the ink swap). `Outline` set to `2px InkBrush, 2px offset` for keyboard users — see "Focus states".
- **Unselected + pressed**: brief `ScaleTransform 0.97`, 100ms, releases on `PointerReleased`.
- **Selected**: 28×28 ink-filled circle, white icon. `Background=InkBrush`.
- **Selected + hover**: no visual change (the chip is already at its terminal state).
- **Mid-morph**: the 360ms transition window. During this, the chip should be `IsHitTestVisible=false` to prevent re-clicks producing animation glitches.
- **Disabled**: not a state used in this app — chips are always interactive when shown.

### `RuntimeChip`
- **Unselected**, **Unselected + hover/focus**, **Selected**: same ink/paper rules as PlatformChip but no morph — only color and border flip (220ms).
- The REC micro-label dims to opacity 0.55 when unselected, 0.7 when selected. The chip is still labeled `.NET 10` in either state — REC is supplementary.

### `Begin` button
- **Hidden**: `Visibility=Collapsed` when `!FoundationReady || Messages.Count > 0`.
- **Visible**: ink-solid primary button, full-width or natural width per layout. `MarginTop=14`.
- **Hover**: subtle `Translation="0,0,2"` lift via `ThemeShadow` (Z=4, opacity 0.04). 180ms.
- **Focused (keyboard)**: 2px `InkBrush` outline at 2px offset.
- **Pressed**: `ScaleTransform 0.98`, 100ms.
- **Disabled**: never disabled when visible. The visibility is the gate.

### `Reset` button (header)
- **Default**: text-only, `Foreground=Ink3Brush`, underline `HairlineBrush` (offset 3).
- **Hover**: `Foreground` and underline → `InkBrush`. 180ms.
- **Focused**: same as hover plus 2px `InkBrush` outline.
- **Pressed**: brief opacity 0.7, 80ms.
- **Disabled**: never.

### `Apply` button (suggestion card primary)
- **Default**: ink-solid pill, paper text. Always interactive when shown.
- **Hover**: subtle `ThemeShadow` lift Z=2. 180ms.
- **Focused**: 2px `InkBrush` outline at 2px offset.
- **Pressed**: `ScaleTransform 0.98`, 140ms.
- **Disabled**: when `IsThinking` is true (an API call from a free-text submit is mid-flight). Visual: `Opacity=0.4`, `IsHitTestVisible=false`. Implemented by binding `IsEnabled` to `!IsThinking`.
- **Submitted (post-click, pre-state-update)**: brief 80ms ScaleTransform 0.98 → 1, then the chat advances. The button itself unmounts as the suggestion turn becomes compact. No spinner.

### `Show alternatives` ghost button
- Same hover/focus/pressed as Apply but ghost styling.
- **Closed**: chevron at 0°, label per content (`Show alternatives`, `Show alternatives & assets`, depending on layer).
- **Open**: chevron at 180°, label `Hide options`.
- **Disabled**: same rule as Apply (when `IsThinking`).

### Alternative button (each row)
- **Default**: paper background, hairline border, full-width. Serif 14, ink text.
- **Hover**: border `Ink2Brush`, background `Paper3Brush`. 180ms.
- **Focused**: 2px `InkBrush` outline at 2px offset.
- **Pressed**: `ScaleTransform 0.99`, 100ms.
- **Disabled**: when `IsThinking`. Same opacity 0.4 rule.

### `AssetField` TextBox (Figma URL, Prototype URL, Screenshots)
- **Empty + blurred**: empty, hairline border, placeholder visible.
- **Empty + focused**: border `InkBrush`. 180ms.
- **Typing / non-empty + focused**: same as focused.
- **Non-empty + blurred**: border `HairlineBrush`, value shown.
- **Disabled**: when `IsThinking`. Opacity 0.4.
- The multiline (Screenshots) field has the same states plus a vertical resize affordance in its bottom-right corner.

### `Apply with these assets` button
- **Disabled / no assets**: `Background=Paper3Brush`, `Foreground=Ink4Brush`, `BorderBrush=HairlineBrush`, `Cursor=NotAllowed`. `IsEnabled=false`.
- **Active / 1+ asset**: same as primary `Apply` button. 220ms transition between the two.
- Hover/focus/pressed mirror Apply when active.

### `InputBox` textarea
- **Empty + blurred**: italic placeholder `Continue the composition…`, `Foreground=Ink4Brush`. Container border `HairlineBrush`.
- **Empty + focused**: container border `InkBrush`. Italic placeholder still shown until a character is typed.
- **Typing / non-empty**: upright serif 16, `Foreground=InkBrush`.
- **Disabled (during API call)**: `IsEnabled=false`, `Opacity=0.6`. Cursor `NotAllowed`. The user cannot edit during `IsThinking`.

### `Send` button
- **Empty + idle**: ghost styling — transparent bg, `Foreground=Ink4Brush`, border `HairlineBrush`. `IsEnabled=false`, cursor `NotAllowed`.
- **Non-empty + idle**: ink-solid primary. `IsEnabled=true`. Send arrow visible.
- **Hover (when active)**: subtle lift Z=2. 180ms.
- **Focused**: 2px outline.
- **Pressed**: `ScaleTransform 0.98`, 140ms.
- **In-flight (`IsThinking`)**: ghost styling. Even if input has text, the button is disabled until the API call resolves.

### Suggestion chips (in InputBox, description turn only)
- **Default**: hairline border, `Foreground=Ink3Brush`. Pill.
- **Hover**: border + text → `InkBrush`. 180ms.
- **Focused**: 2px outline.
- **Pressed**: chip click fills the InputBox value, the chip row vanishes (since input is no longer empty). No visual ack — the InputBox change is the ack.

### `ArtifactCard` header row
- **Planned**: outlined empty circle glyph, filename `Ink3Brush`, status text "Planned" `Ink4Brush`, no chevron. `IsEnabled=false`, cursor `Default`. Not focusable.
- **Drafted (collapsed)**: filled-circle-with-check glyph, filename `InkBrush`, status text "Drafted" `Ink2Brush`, chevron at 0°. Focusable + clickable.
- **Drafted (expanded)**: same as collapsed but chevron at 180°.
- **Drafted + hover**: no visual change on the row itself — the click affordance is the cursor going pointer. (The pre body has its own hover state.) Filename color stays `InkBrush`.
- **Drafted + focused**: 2px `InkBrush` outline at 2px offset around the row.
- **Drafted + pressed**: brief opacity 0.85, 80ms.
- **Mid-status-transition (Planned → Drafted)**: the dot→check animation runs (320ms fill + 380ms check, 140ms delay). During the 760ms total, the row is `IsHitTestVisible=true` but clicks should be ignored visually until the animation settles. Implementation: an `_animating` flag, or just accept that an early click triggers expansion immediately (which actually feels responsive — go with that).

### `ArtifactCard` body
- **Hidden** (default): `MaxHeight=0`, no content rendered (or rendered with `Opacity=0`).
- **Drafted + expanded**: read-only pre styled per DESIGN.md. Hover transitions on border + background.
- **Drafted + expanded + hover (over pre)**: border `Ink3Brush`, background `Paper2Brush`, "CLICK TO EDIT" hint fades in. 180ms.
- **Drafted + expanded + edit mode**: TextBox swap. `Background=PaperBrush`, `BorderBrush=InkBrush`, `MinHeight=120`, vertical resize. Auto-focused, cursor at end.
- **Planned + expanded**: italic placeholder block ("Awaits your decision in the chat."). Dashed border. No hover or focus state — non-interactive.

### `CompactComposerTurn` row
- **Default**: filled-circle-with-check, layer label, applied label (truncated). Edit pill ghost.
- **Hover (over Edit pill only)**: pill border + text → `InkBrush`. The rest of the row is non-interactive.
- **Edit pill focused**: 2px outline.
- **Edit pill pressed**: brief opacity 0.85, 80ms.

### `Download bundle` button
- **Hidden**: `DraftedCount < 8`.
- **Visible**: ink-solid primary, full-width.
- **Hover / focus / pressed**: same as `Apply` button.
- **In-flight (file save in progress)**: brief disabled state with the label changing to "Saving…" via a localized resource key. `IsEnabled=false`. Resolves in <500ms typically; if longer, no spinner — the browser's own save dialog takes over.
- **Post-save**: button returns to default state. No toast, no confirmation message.

---

## Loading states

Three places things can take time. Each has explicit visible behavior.

### Initial app load
- The page sheet animates in via `pageIn` (600ms) on `Loaded`. Until that completes, the page background `Paper3Brush` is visible.
- Variable fonts (Fraunces, Martian Mono) load from the Uno asset bundle and are typically available before the first paint when bundled correctly. If a flash of fallback font occurs, that's a packaging bug — fix by ensuring fonts are referenced as `ms-appx:///Assets/Fonts/...` and not as URLs.
- No skeleton, no spinner. The page either shows or it doesn't.

### Free-text API call (Submit triggers `Api.ContinueAsync`)
- Set `IsThinking=true` immediately on submit.
- Visual changes simultaneously:
  - `InputBox` textarea: `IsEnabled=false`, opacity 0.6.
  - `Send` button: ghost styling, disabled.
  - `Apply` and `Show alternatives` on the latest suggestion: disabled, opacity 0.4.
  - All alternative buttons: disabled.
  - `ThinkingIndicator` mounts at the bottom of the transcript, after the just-appended USER turn. Three pulsing dots (1100ms cycle, 0/160/320ms delays).
- Foundation panel (name, platforms, runtime) is **not** disabled — those keep working. They drive only the foundation artifacts; they don't conflict with an in-flight chat call.
- `Reset` button is **not** disabled. Reset cancels the in-flight call (see "Cancel and abort").
- On success: `IsThinking=false`, indicator unmounts (no exit animation — clean removal), composer turn appears with `msgIn`.
- On error: `IsThinking=false`, error banner appears (see "Error states").

### Reasoning generation (background, non-blocking)
- Fired fire-and-forget after description submit.
- **No visible loading state.** The wiring suggestion shows immediately with static reasoning; when the API resolves, the reasoning text swaps in (no animation — the swap is silent).
- If the user has already advanced past wiring before the call resolves, the swap still happens in any earlier turns *if* they're still rendered. Compact turns don't show reasoning, so no visible difference there.
- Failure is silent. Static reasoning persists.

### Foundation effect (re-drafting README/CLAUDE/scaffold)
- Pure-function, synchronous in render time. No loading state. The artifacts go from `Planned → Drafted` (or update content) instantly.
- The `StatusGlyph` dot→check animation runs on the `Planned → Drafted` transition. Subsequent content edits while the artifact stays `Drafted` produce no animation — the content just updates silently.

### File save (`Download bundle`)
- The button briefly enters its "Saving…" state.
- Browser's file save dialog opens (WASM uses the browser's native save flow via `FileSavePicker`). Once open, control is transferred to the browser; the app waits.
- On dialog dismiss (save or cancel): button returns to default. No toast.

---

## Error states

Errors are surfaced as a dismissible banner above the `InputBox`, never as a modal or alert.

### Banner visual spec (also goes in DESIGN.md)
- Container: full-width within the transcript column's padding. `Background=PaperBrush`, `BorderThickness="1"`, `BorderBrush=InkBrush` (the only place an ink border is used as an emphasis device — errors deserve attention). `CornerRadius=4`. `Padding=14,12`. `MarginBottom=12` so it sits above the input.
- Layout: horizontal, gap `Space12`, vertical center.
  - 14×14 alert glyph: a `Path` rendering a small triangle with a centered exclamation, stroke `1.4`, color `InkBrush`. Inline.
  - Body: serif 14, `InkBrush`, line-height 1.4. The error message text.
  - Right: dismiss button — 16×16 ghost button with an `×` glyph. Hover swaps `Ink3Brush → InkBrush`.
- Mount: `msgIn` animation reused (480ms slide + fade).
- Dismiss: opacity 0 + translateY -4, 240ms ease-out, then unmount.

### Error taxonomy

| Kind | Trigger | Banner copy | After-effect |
|---|---|---|---|
| Network failure | `HttpRequestException`, no response | "Couldn't reach the composer. Check your connection and try again." | Restore prior `InputValue` (the user's text isn't lost). `IsThinking=false`. |
| Auth failure (401) | API key missing/invalid | "API key isn't configured. Set ANTHROPIC_API_KEY and reload." | `IsThinking=false`. The `Send` button is left disabled until the user reloads. |
| Rate limit (429) | API returns 429 | "Rate limited. Wait a moment and try again." | Restore InputValue, `IsThinking=false`. The user can resubmit. |
| Server error (5xx) | API 500-599 | "The composer is having trouble. Try again in a moment." | Restore InputValue, `IsThinking=false`. |
| Malformed response | JSON parse fails | "Couldn't read the composer's response. Try rephrasing." | Restore InputValue, `IsThinking=false`. The user can edit and resubmit. |
| File save failed | `FileSavePicker` returns null or throws | "Couldn't save the bundle. Try again." | Button returns to default, no state change to artifacts. |
| Reasoning generation failed | Background reasoning API call fails | **No banner.** Silent fall back to static reasoning. | None. |

### Banner lifecycle
- Only one banner visible at a time. New errors replace the previous.
- Auto-dismiss after 8 seconds (set a timer in `MainModel`; clear if user dismisses manually first).
- Dismissing the banner only hides the visual — it doesn't retry the action. Retry requires user to resubmit.
- Banner is `aria-live="polite"`. Screen readers announce on appearance.

### What's not an error (and shouldn't surface)
- Empty submit attempts → already prevented by `CanSend` gate; no banner.
- Foundation invalid → Begin button just hidden; no banner.
- Composer hallucinates an artifact id that doesn't exist → silently filtered in the API parser; no banner. (The composer might emit `updates: { sketch: "..." }` even though there's no `sketch` artifact. Filter to known kinds and ignore the rest.)

---

## Disabled / unavailable states

Disabled has three visual treatments depending on context:

1. **Hidden** (no rendered control) — used when the control is irrelevant to the current state. Example: `Begin Composing` when foundation invalid.
2. **Ghost-disabled** (visible but inert, paper background, ink4 text, hairline border) — used when the control is contextually disabled but the user should know it exists. Example: `Send` button when input empty.
3. **Faded-disabled** (`Opacity=0.4`, `IsHitTestVisible=false`) — used during transient blocks like `IsThinking`. The user knows what they were about to click; we're just preventing it briefly.

| Control | Disabled when | Treatment |
|---|---|---|
| `Begin Composing` | `!FoundationReady \|\| Messages.Count > 0` | Hidden |
| `Send` | input empty OR `IsThinking` | Ghost-disabled |
| `Apply` (any layer) | `IsThinking` | Faded |
| `Show alternatives` | `IsThinking` | Faded |
| Alternative buttons | `IsThinking` | Faded |
| `Apply with these assets` | no asset typed OR `IsThinking` | Ghost-disabled (no assets) / Faded (thinking) |
| `AssetField` inputs | `IsThinking` | Faded |
| `InputBox` textarea | `IsThinking` | Faded |
| `ArtifactCard` header (clickable) | `Status == Planned` | Inert (cursor default, status text faded) |
| `Edit pill` (compact turn) | never | — |
| `Download bundle` | `DraftedCount < 8` | Hidden |
| `Reset` | never | — |
| Foundation inputs (name, platforms, runtime) | never | — |

The single rule: **if it's disabled because the user is in the middle of something, fade it; if it's disabled because it doesn't apply yet, hide it.**

---

## Pressed and focus states

### Pressed (active) feedback
- **Primary buttons** (Apply, Begin, Download bundle, Send): `ScaleTransform 0.98`, 100-140ms ease-out, releases on PointerReleased. No background change.
- **Ghost buttons** (Show alternatives, Edit pill, Reset): brief `Opacity 0.7`, 80ms.
- **Cards / rows** (ArtifactCard header, alternative button): brief `Opacity 0.85`, 80ms. No scale.
- **Chips** (PlatformChip, RuntimeChip): `ScaleTransform 0.97`, 100ms. The state morph itself takes over after release.

Pressed state is purely visual feedback — it does not gate the action. The action fires on `PointerReleased` (the WinUI default for `Click`).

### Focus states (keyboard)
The app must be fully keyboard-navigable. Focus is communicated via a 2px `InkBrush` outline at 2px offset, applied uniformly:

```xml
<!-- In Themes/FocusVisuals.xaml -->
<Style TargetType="Control" x:Key="EditorialFocusStyle">
    <Setter Property="UseSystemFocusVisuals" Value="False" />
    <Setter Property="FocusVisualPrimaryBrush" Value="{ThemeResource InkBrush}" />
    <Setter Property="FocusVisualPrimaryThickness" Value="2" />
    <Setter Property="FocusVisualSecondaryBrush" Value="Transparent" />
    <Setter Property="FocusVisualMargin" Value="-2" />
</Style>
```

Apply `EditorialFocusStyle` (or merge its setters) to every interactive control's default style. The focus visual appears **only** when focus arrived via keyboard (Uno honors `FocusState=Keyboard` vs `Pointer` — pointer focus does not show the outline).

### Where focus is *not* shown
- TextBoxes don't get the outline; the input border itself swaps to `InkBrush` when focused (already specified). The two would compete.
- The `ArtifactCard` body's read-only pre is not focusable; clicking it goes straight to edit mode.

---

## Tab order and keyboard navigation

Top-to-bottom, left-to-right reading order, with one deviation: the input box is reachable early to let keyboard users get there fast.

**Empty state (no conversation):**
1. Header `Reset` (always reachable but rarely the first stop).
2. App name TextBox.
3. Each `PlatformChip` in registry order: Web, Windows, Android, iOS, Desktop.
4. Each `RuntimeChip`: .NET 10, .NET 9.
5. `Begin Composing` button (when visible).
6. (Skip — input box is not yet relevant since there's no prompt to answer.)

**Mid-conversation state:**
1. Header `Reset`.
2. App name TextBox.
3. PlatformChips (5).
4. RuntimeChips (2).
5. Each `Edit pill` on every compact composer turn, in transcript order.
6. Latest composer turn's `Apply` button.
7. Latest composer turn's `Show alternatives` button.
8. (When alternatives panel open) each alternative button + (design layer) each AssetField + (when active) Apply with these assets.
9. Each suggestion chip in the InputBox (description layer only, when input is empty).
10. InputBox textarea.
11. `Send` button.
12. Each drafted `ArtifactCard` header in registry order.
13. `Download bundle` (when visible).

In practice, after Apply/Alternative click, the user's expected next action is reading the new composer turn — focus does not auto-advance to the input. We let the user Tab there.

**Important:** the Edit pills sit visually with their old composer turns in the transcript, but a power user might want to skip past them. Implement a **skip-to-latest** keyboard shortcut: pressing **End** in the transcript region jumps focus to the latest composer turn's primary action (Apply). Pressing **Home** jumps to the first compact turn's Edit pill. Both are nice-to-have and can land in Slice 13.

### Keyboard shortcuts (full)

| Key | Context | Action |
|---|---|---|
| `Enter` | InputBox | Submit if `CanSend`. |
| `Shift+Enter` | InputBox | Insert newline. |
| `Esc` | Artifact edit mode | Cancel edit. |
| `Esc` | Alternatives panel open | Close panel. (focus returns to Show alternatives button) |
| `Esc` | Anywhere else | No-op. |
| `Cmd/Ctrl+Enter` | Artifact edit mode | Commit edit. |
| `Tab` | Artifact edit mode | Commit edit (via Blur). |
| `Tab` / `Shift+Tab` | Anywhere | Move focus per tab order. |
| `Space` / `Enter` | Focused button | Activate (default WinUI). |
| `Space` / `Enter` | Focused chip | Toggle (default ToggleButton). |
| `Home` | Focused inside transcript | Jump to first compact turn's Edit pill (Slice 13+). |
| `End` | Focused inside transcript | Jump to latest composer turn's Apply (Slice 13+). |

There is no global Cmd+K, no command palette, no shortcut help overlay. The app is small enough that discoverability isn't an issue.

---

## After-action effects

Comprehensive table — for each user action, what changes.

| User action | Messages | Artifacts | ExpandedIds | EditingId | InputValue | AlternativesOpen | DesignAssets | Side effects |
|---|---|---|---|---|---|---|---|---|
| Type in `AppName` | — | README/CLAUDE re-draft | — | — | — | — | — | — |
| Toggle `PlatformChip` | — | README/scaffold re-draft | — | — | — | — | — | — |
| Toggle `RuntimeChip` | — | CLAUDE/scaffold re-draft | — | — | — | — | — | — |
| Click `Begin Composing` | append description prompt | — | set to `{Readme, Claude}` | — | — | — | — | `JustAddedId` set |
| Type description, Submit | append USER, mark description COMPOSER `appliedLabel`, after 50ms append wiring prompt | — | set to `{Mcp}` (after wiring push) | clear | clear | clear | — | `JustAddedId` set; reasoning API fired |
| Click `Apply` on suggestion | mark current COMPOSER `appliedLabel`, append next layer prompt | apply `Suggestion.Updates` | set to `LayerToArtifacts[NextLayer]` | clear | — | clear | — | `JustAddedId` set |
| Click an alternative | same as Apply but with `Alternative.Label/Updates/NextLayer` | apply `Alternative.Updates` | same | clear | — | clear | — | `JustAddedId` set |
| Click `Apply with these assets` | mark current COMPOSER appliedLabel (computed from assets), append next prompt | apply DESIGN.md with assets embedded | set to `{Interactions}` | clear | — | clear | — | `JustAddedId` set |
| Click `Show alternatives` | — | — | — | — | — | toggle | — | — |
| Type in InputBox | — | — | — | — | update | — | — | — |
| Submit free text (non-description) | append USER, set `IsThinking=true`, on response: append COMPOSER + after 200ms append next layer prompt | apply API `updates` | set to `LayerToArtifacts[NextLayer]` after 200ms | clear | clear | clear | — | API call; `JustAddedId` set; `IsThinking` toggled |
| Click `Edit pill` on compact turn | truncate to that index | reset per `ArtifactResetsFromLayer[layer]` | clear | clear | — | clear | — | After 30ms: push fresh prompt; `JustAddedId` set; if layer == Description, also clear `AppDescription` |
| Click ArtifactCard header (drafted) | — | — | toggle that id | if collapsing the editing one, clear | — | — | — | — |
| Click pre body (drafted, expanded) | — | — | — | set to that id | — | — | — | TextBox auto-focus, cursor at end |
| Edit body, Blur or Cmd/Ctrl+Enter | — | update Content for that id | — | clear | — | — | — | — |
| Edit body, Esc | — | — | — | clear | — | — | — | — |
| Type in `AssetField` | — | — | — | — | — | — | update field | — |
| Click `Reset` | clear | reset all to `(Planned, "")` | clear | clear | clear | clear | clear | Foundation: clear `AppName`, `AppDescription`, reset `Platforms` to `{Web, Android, iOS}`, `Runtime` to Net10. Cancel any in-flight API. Clear `ErrorMsg`, `Reasonings`. |
| Click `Download bundle` | — | — | — | — | — | — | — | Trigger `BundleExporter`; show "Saving…" briefly |
| Dismiss error banner | — | — | — | — | — | — | — | Clear `ErrorMsg` |

---

## Edge cases and race conditions

These are real situations that *will* happen. Handle them explicitly.

### Foundation changes mid-conversation
- User has advanced to (say) the architecture layer and then changes `AppName` in the foundation panel. Behavior:
  - README and CLAUDE re-draft with the new name (their content reflects the latest foundation state, regardless of conversation position).
  - The conversation transcript is not affected. Compact turns still show whatever applied label they had ("WIRING · Connect Uno docs and Figma" etc.).
  - The architecture suggestion in the latest turn does not automatically re-render with the new name (its content was generated when the prompt was pushed). To pick up the new name, the user must Edit-back to architecture.
- This is intentional and matches the prototype. The trade-off: foundation is always live; chat-driven artifacts are snapshots at decision time.
- If the user clears `AppName` entirely mid-conversation: README and CLAUDE go to `Planned` with empty content. The conversation continues normally. The artifact panel shows a regression in `DraftedCount`. The Download bundle button disables (since `DraftedCount < 8`). This is mildly weird but defensible.

### Rapid double-click on Apply
- The action fires on `PointerReleased`. Two very fast presses can both fire before state updates settle.
- Mitigation: the second click finds the latest COMPOSER message has already been mutated to have `appliedLabel != null`, and the suggestion panel for that turn is no longer rendering (it's been replaced by the compact view in the next render cycle). So the second click hits a different button.
- More robust: in `ApplySuggestion`, guard with a "currently applying" flag (`IState<bool> IsApplying`). If true, return early. Set true at command entry, false in finally.

### User edits an artifact, then Edits-back to a prior turn
- Custom edits to artifacts that get reset by `ArtifactResetsFromLayer` are lost — those artifacts go to `Planned` with empty content.
- For artifacts not reset (e.g. README/CLAUDE never reset on go-back), edits persist.
- If the user wants to preserve a custom-edited artifact across an Edit-back, they must download the bundle first. This is acceptable for v1; a "preserve my edits" flag is a possible future enhancement but not committed.

### Long applied label
- The `CompactComposerTurn` truncates `AppliedLabel` at 80 chars with `…`. The full label is available via `ToolTipService.ToolTip`.
- For descriptions specifically (which can be a paragraph), 80 chars is enough to show the gist.

### Composer hallucinates an unknown artifact id
- The API parser filters `updates` to known `ArtifactKind` values. Unknown keys are silently dropped.
- If the API returns *zero* known updates for a layer that should produce one, surface the parsed `pre` text as the composer body, push the next prompt anyway, and don't update artifacts. The user sees the conversation move on with no draft change — a visual signal that something was off without a hard error.

### Composer returns updates for an artifact NOT in the current layer
- E.g., on the wiring layer, the API returns `updates: { architecture: "..." }`. The parser accepts and applies it (the artifact registry, not the layer, defines validity).
- Implication: a sufficiently smart override could leapfrog through layers. This is feature-not-bug — the user's free text takes precedence.

### `IsThinking` true when component unmounts
- Reset, viewport navigation, or app close while an API call is in flight. Use a `CancellationTokenSource` per call, cancel on `Reset` or `OnDisposing`. Caught `OperationCanceledException` is silently swallowed (no banner).

### Animation interrupted by state change
- A platform chip is mid-morph (say, 200ms into a 360ms transition) and the user clicks again to deselect.
- VisualStateManager handles this: the new state's storyboards take over from the current property values. The morph reverses smoothly.
- If you implement morph manually with explicit `Storyboard.Begin()` calls, stop the running storyboard before starting the reverse one.

### `JustAddedId` while another action fires
- The 800ms `JustAddedId` window can be interrupted by another rapid action. Each new message sets a new `JustAddedId`; the timer for the previous one is overridden.
- Implementation: track the timer, cancel when `JustAddedId` changes:
```csharp
private CancellationTokenSource? _justAddedCts;
private async Task ClearJustAddedAfterDelayAsync(Guid id)
{
    _justAddedCts?.Cancel();
    _justAddedCts = new CancellationTokenSource();
    try { await Task.Delay(800, _justAddedCts.Token); }
    catch (OperationCanceledException) { return; }
    var current = await JustAddedId;
    if (current == id) await JustAddedId.SetAsync(null);
}
```

### Viewport resize during animation
- `MaxHeight`-based animations (alternatives panel, artifact body) measure on first open. If the viewport resizes mid-animation, the in-flight value may be wrong but the next open will re-measure.
- Acceptable behavior. Don't over-engineer.

### Browser tab close mid-conversation
- All state is in memory. Closing the tab loses everything. **No persistence** in v1. (See "Persistence" below.)
- Don't add a `beforeunload` warning — it's antagonistic to a developer tool. The user knows they're in an exploratory composer.

### Description submitted while reasoning generation is still running
- This shouldn't be possible — the description is submitted once, generates reasonings, then the user proceeds.
- If the user Edits-back to description and resubmits, the previous reasoning generation is canceled (track its CTS) and a new one starts.

### User opens Show alternatives, types in the InputBox, submits
- The free-text Submit takes priority. The alternatives panel state (`AlternativesOpen=true`) is irrelevant to the submit path — it just stays open visually until the next layer's prompt replaces the current turn.
- Asset field values are likewise irrelevant to a free-text submit. They persist in `DesignAssets` state (so if the user goes back, their typed values are still there) but don't affect this submit.

### Asset values persist across Edit-back
- If the user Edits-back to design after typing assets, then advances again without re-clicking Apply with these assets, the typed asset values are still in `DesignAssets` state. They'll be reflected in the asset fields' default values when the design layer is re-shown.
- Whether this is desired or surprising is debatable. The prototype keeps them; we'll keep them. Document so users aren't confused if they see their old Figma URL pre-filled after going back.

### Reset during error banner
- Clear `ErrorMsg` along with everything else. The banner unmounts.

### Multiple cards expanded simultaneously
- The chat-layer-driven expansion replaces `ExpandedIds` totally. So at any chat-layer transition, only the new layer's targets are expanded.
- Manual expansion (clicking a drafted card) adds to `ExpandedIds`. So you can have layer-driven expansion plus a manually-expanded older card. The next layer transition clobbers the manual one.
- This is a known trade-off. If preserving manual expansions across transitions is desired, change the merge strategy in `ApplyActionAsync` to `prev.Union(LayerToArtifacts[nextLayer])` — but then the previous layer's auto-expansion doesn't auto-collapse. Tracked but not implemented.

---

## Cancel and abort patterns

| Action | Cancel mechanism |
|---|---|
| In-flight API call | `Reset` cancels via `CancellationTokenSource`. No user-facing "Cancel" button — the `IsThinking` window is short (<5s typically). |
| Artifact edit | `Esc` cancels (restores prior content via TextBox `Text` reset). |
| Alternatives panel | `Esc` collapses (focus returns to Show alternatives button). |
| Asset typing | No explicit cancel — clearing the field is the cancel. Reset clears all. |
| Reasoning generation | Reset cancels via CTS. Otherwise lets it finish. |
| File save dialog | Browser dialog handles dismiss; `BundleExporter` returns null `file`, no state change. |

There is **no Undo/Redo stack** anywhere. Edit-back via the `Edit pill` is the only "undo" — and it's destructive (resets downstream artifacts).

There is **no confirmation dialog** for any action, including `Reset`. The composer is exploratory; reset is fast to recover from.

---

## Persistence and first-time vs returning state

**There is no persistence in v1.** Every browser tab load starts from a clean slate:
- `AppName=""`, `AppDescription=""`
- `Platforms={Web, Android, iOS}`, `Runtime=Net10` (defaults)
- `Messages=[]`, `Artifacts` all `Planned`
- All other state at initial values

There is no "returning user" state. There is no localStorage, no IndexedDB, no cookie. The `MainModel` is constructed fresh on each navigation to `MainPage`, which happens once per session.

**This is deliberate:** the composer is a one-shot tool. The output is the downloaded bundle. Once saved, the user owns it and doesn't need to "come back" to the composer for that project. They start a new conversation for a new project.

If persistence is added later (a possible follow-up), the minimum scope would be: foundation panel inputs (name, platforms, runtime) saved to localStorage so a user iterating on the same project name across sessions doesn't retype. The conversation itself should remain ephemeral — re-deriving an entire conversation from a saved foundation is the wrong abstraction.

---

## Touch and mobile behavior

The composer is a **desktop-first developer tool**. Touch is supported insofar as Skia/WinUI handles it natively, but the layout is not optimized for narrow viewports.

- Below 600px wide: layout is officially unsupported. Implement a "Best viewed on a wider screen" message at sub-600px breakpoints (text-only, centered, fills viewport). Don't try to make the chat work in 360px.
- 600-904px: single-column responsive collapse. Foundation panel becomes a collapsible header section; transcript fills the rest. See DESIGN.md "Responsive grid".
- All touch targets meet 44×44 (chips, buttons, pills clear this with their padding).
- Long-press: no special behavior. Tap-and-hold is treated as a regular click.
- Swipe: no swipe gestures. Right-click: no context menu.
- Pinch-zoom: browser-native, not interfered with.

Mobile-first redesign is a future possibility but not in scope. Don't add mobile affordances on the desktop layout.
