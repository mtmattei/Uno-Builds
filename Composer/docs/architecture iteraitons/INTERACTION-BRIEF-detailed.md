# Composer Context Engine — Interaction Brief

**Version:** v11 (canonical)
**Audience:** an implementation agent reproducing every user-facing behavior. Self-contained.
**Companion briefs:** `ARCHITECTURE-BRIEF-detailed.md` (structural), `DESIGN-BRIEF-detailed.md` (visual). This brief is the behavioral truth — every input, every response, every transition.

The Composer Context Engine is a **conversational state machine over an eight-layer composition**. Every interaction either edits a layer, moves between layers, or produces output. This document specifies every one of those interactions exhaustively.

If this brief and the reference prototype (`composer-context-engine.jsx`) disagree, the prototype wins. If the brief and the Architecture brief disagree on a state name or method signature, the Architecture brief wins.

---

## 1. Per-layer state machine

### 1.1 The four states

Every layer is in exactly one of these four states at any moment. State is scoped per-layer — Intent can be Locked while UX is Dirty.

| State        | Meaning                                                                           |
|--------------|-----------------------------------------------------------------------------------|
| `Clean`      | No edits since locked or initial. Lock-and-continue is the natural action.        |
| `Dirty`      | User edited canvas or typed in prompt. Generate-preview is the natural action.    |
| `Previewing` | AI rendered proposed redraw. Accept-or-discard is the natural action.             |
| `Locked`     | Terminal. Layer settled, file drafted, advanced past.                             |

### 1.2 Transition diagram

```
                          ┌──── lock-and-continue ─────────────┐
                          │                                     ▼
       (default)          │                                  [LOCKED]
     ┌─────────────►   [CLEAN] ──── edit canvas/prompt ──►  [DIRTY]
     │                    ▲ ▲                                 │
     │  ┌─ revisit ───────┘ │                                 │ generate-preview
     │  │                   │                                 │ (only if aiConfigured)
     │  │                   └── discard-edits ────────────────┤
     │  │                   ┌── discard-preview ──────────────┤
     │  │                   │                                 ▼
     │  │              [PREVIEWING] ◄── edit canvas/prompt ─[stays DIRTY]
     │  │                   │
     │  │                   └── accept-and-lock ─────► [LOCKED]
     │  │
     └──┴─ reset (any state → CLEAN, all layers)
```

### 1.3 Triggers per transition

| From         | To           | Trigger                                                                          |
|--------------|--------------|----------------------------------------------------------------------------------|
| Clean        | Dirty        | Any value change in the layer's canvas (intent field edit, design token edit, override edit) |
| Clean        | Dirty        | Composer prompt textarea transitions from empty → non-empty                      |
| Clean        | Locked       | User clicks `Lock and continue →` (or `Continue →` on first layer)               |
| Clean        | Locked       | User presses Cmd/Ctrl+Enter while composer prompt is empty                       |
| Dirty        | Previewing   | User clicks `Generate preview →` (only if `aiConfigured == true`)                |
| Dirty        | Previewing   | User presses Cmd/Ctrl+Enter while composer prompt is non-empty (only if aiConfigured) |
| Dirty        | Clean        | User clicks `Discard edits`                                                      |
| Previewing   | Locked       | User clicks `Accept and lock →`                                                  |
| Previewing   | Clean        | User clicks `← Discard preview` or presses Esc                                   |
| Previewing   | Dirty        | User makes further canvas edits (keeps preview state until explicit accept/discard) — exception: editing a different layer doesn't change the current layer's state |
| Locked       | Clean        | User clicks `Revisit ↗` on the locked context card                               |
| Locked       | Clean        | User clicks the locked layer's row in CompositionStack                           |
| Any          | Clean        | User clicks `Reset` button in top-bar (all layers, all values reset)             |

### 1.4 Snapshot capture and restore

On the **first** Clean → Dirty transition for a given layer, ShellModel captures a snapshot of all mutable layer state:

```csharp
public record LayerSnapshot(
    IntentValues? Intent,
    DesignTokens? Design,
    ArchitectureBlueprint? Architecture,
    UXFlow? UX,
    InteractionsMatrix? Interactions,
    DataContracts? Data,
    BuildPlan? Implementation,
    ImmutableDictionary<LayerKind, string>? OverrideMarkdown);
```

Only the layer's own data is captured into its snapshot. Other layers' state is untouched.

Subsequent edits within the same Dirty session do **not** re-snapshot. The snapshot represents the last "known good" state.

On `Discard edits` or `Discard preview`:
1. Restore from snapshot
2. Clear the composer prompt for this layer
3. Clear `previewAck` for this layer
4. Transition to Clean
5. Remove snapshot from the snapshots dictionary

### 1.5 Lock advancement

`Lock and continue →` and `Accept and lock →` both follow the same advancement rule:

```
1. Add current layer kind to LockedIds
2. Set current layer state to Locked
3. Clear composer prompt for current layer
4. Clear previewValues for current layer
5. Clear previewAck for current layer
6. If lockedIds.Count == 0 before this lock, set RevisitHintShown = true (one-shot)
7. If activeIndex < 7, increment activeIndex and navigate to next layer's route
```

The `Accept and lock →` path also adopts the proposed values into the layer's state before locking. `Lock and continue →` (from Clean) has no proposed values — current values are already canonical.

### 1.6 Revisit

```
1. Set activeIndex to target layer's index
2. If target layer's state is Locked, transition to Clean (values preserved)
3. Navigate to target layer's route via Region navigation

Values are not modified. Locked downstream layers remain locked. When the
user advances again (lock-and-continue), this layer re-locks but downstream
layers don't need to. Their context cards stay in place.
```

---

## 2. Workspace startup behavior

### 2.1 First launch

On first launch (no persisted state — v11 has no persistence):

1. Shell renders, navigates to `Intent` route
2. ActiveIndex = 0, LockedIds = empty
3. `RailsVisible` evaluates false → both rails collapse to width 0, opacity 0
4. Center column tightens to 720 max-width, padding-top 64, centered
5. IntentCanvas renders with `INTENT_EXAMPLE` values pre-populated (Field-service scheduling, etc.)
6. Example-values banner shows above the field grid (any field still matches example)
7. ComposerFooter shows lead question "*If I summarize the intent right now, the agent has enough to scaffold a meaningful skeleton. Anything else worth adding before locking?*"
8. Primary action button label is `Continue →` (softer, since this is the first layer and no other layers are locked)
9. Suggestion chips visible below textarea: `Mobile-first`, `Offline-first`, `No backend yet`

### 2.2 First layer lock

When the user clicks `Continue →` (or types in prompt and clicks `Generate preview →` then `Accept and lock →`):

1. Intent layer transitions to Locked
2. `lockedIds.Count` was 0 — fires the one-shot revisit-hint state
3. `activeIndex` advances 0 → 1, navigation to `UX` route
4. `RailsVisible` flips false → true
5. **Rail reveal animation fires** (see §5.1)

### 2.3 Subsequent layer transitions

Pages 2-8 follow the standard lock advancement. RailsVisible stays true. No animation other than the active page swap (which is instant — no fade or slide between pages in v11).

### 2.4 Reset

Top-bar contains a `Reset` button (small, ghost-style, ink4). Click triggers:

1. Confirm dialog: "Reset will discard all locked layers and start over. Continue?"
2. On confirm:
   - `activeIndex` → 0
   - `lockedIds` → empty
   - `layerStates` → all Clean
   - `intent` → `INTENT_EXAMPLE`
   - `design` → `DesignModel.Defaults`
   - All snapshots cleared
   - All previewValues cleared
   - All previewAcks cleared
   - All prompts cleared
   - All overrides cleared
   - Navigation to `Intent` route
   - RailsVisible flips true → false (reverse rail animation)

---

## 3. Composer footer behaviors

The ComposerFooter is the user's primary input surface. It renders below every active canvas and changes shape based on layer state.

### 3.1 Three-state UI

| State           | Header status word | Lead text                                          | Action buttons                                                      |
|-----------------|--------------------|----------------------------------------------------|---------------------------------------------------------------------|
| Clean           | `Refining`         | Layer-specific lead question                       | `Lock and continue →` (or `Continue →` on first layer) + italic note "accepting the recommendation" |
| Dirty           | `Listening`        | Same lead question                                 | `Generate preview →` (amber tone, disabled if !aiConfigured) + `Discard edits` ghost + italic note (varies by aiConfigured) |
| Previewing      | `Proposing`        | "Here's how I'd redraw this layer with your edits applied. Accept to lock the proposed version, or discard to revert." + acknowledgment line below | `Accept and lock →` ink + `← Discard preview` ghost |

The header status word maps to `COMPOSER_STATUS[layerState]`:

```csharp
public static class ComposerStatus
{
    public const string Clean = "Refining";
    public const string Dirty = "Listening";
    public const string Previewing = "Proposing";
}
```

### 3.2 Composer textarea

Per-layer textarea, content bound to `ComposerModel.Prompts[currentLayer.Kind]`.

Behaviors:
- Placeholder: "Refine, or accept what's drawn…" in ink4
- Focus ring: 1px border transitions `HairlineBrush` → `Ink2Brush` over 140ms ease
- Multi-line support: `AcceptsReturn=true`, `TextWrapping=Wrap`
- Min-height: 22 (single line), grows up to ~80 on multi-line content
- Per-layer content persistence: switching layers preserves the prompt for the previously active layer; switching back restores it
- Typing transitions Clean → Dirty (only on the first non-empty character; subsequent typing doesn't re-trigger)
- Clearing the textarea (returning to empty) does **not** transition Dirty → Clean automatically — only explicit `Discard edits` does

### 3.3 Suggestion chips

Below the textarea, always visible while suggestions are available for the current layer. Pre-v11 only showed when textarea was empty; v11 keeps them always visible (they're inspiration, not just empty-state filler).

Layout:
```
TRY  [chip 1] [chip 2] [chip 3]                                  ⌘↵ to submit
```

- `Try` eyebrow (9px ink4 weight 500) left-aligned
- Chips (12px ink3, hairline border) with hover transition to ink + ink3 border
- `⌘↵ to submit` keyboard hint right-aligned (10px ink4 mono)

Click on chip:
1. Sets the textarea value to the chip's text
2. Transitions Clean → Dirty (if not already Dirty/Previewing)
3. Focuses the textarea
4. Places caret at end of inserted text

Per-layer suggestion sets:

| Layer          | Suggestions                                                                |
|----------------|----------------------------------------------------------------------------|
| Intent         | `Mobile-first`, `Offline-first`, `No backend yet`                          |
| UX             | `Drag-to-reorder`, `Return to dashboard`, `Modal confirmation`             |
| Architecture   | `Region-based navigation`, `Single {userNoun} role`, `Offline-first`       |
| Design         | `Stay with amber`, `Use a brand color`, `Show alternatives`                |
| Interactions   | `Queue silently`, `Always show banner`, `Banner only when queue has items` |
| Data           | `Latest status only`, `Audit trail`, `GeoPoint as record`                  |
| Implementation | `Strictly linear`, `Parallelize`, `Add a tooling phase`                    |
| Scaffold       | *(no chips — Scaffold has no composer prompt)*                             |

For Architecture, `{userNoun}` is derived from Intent. Field-service default produces `Single technician role`; a habit-tracker app produces `Single user role`.

### 3.4 Keyboard shortcuts in the textarea

| Key combination                          | Action                                                                       |
|------------------------------------------|------------------------------------------------------------------------------|
| `Cmd+Enter` (macOS) / `Ctrl+Enter` (Win/Linux) | Submit:<br>- If layer is Dirty and aiConfigured: fire `GeneratePreview`<br>- If layer is Clean: fire `LockAndContinue`<br>- If layer is Previewing: no-op |
| `Enter` (plain)                          | Insert newline (standard textarea behavior)                                  |
| `Esc`                                    | If layer is Previewing: fire `DiscardPreview` (also blurs textarea)<br>If layer is Dirty: blurs textarea (no state change)<br>Otherwise: blurs textarea |
| `Tab`                                    | Standard focus advancement (per WinUI default tab order)                     |

### 3.5 Generate Preview button state

```
isDisabled = (layerState != 'dirty') OR (!aiConfigured)

if (!aiConfigured && layerState == 'dirty'):
    italic note reads: "Add an API key in settings to enable previews"
else if (aiConfigured && layerState == 'dirty'):
    italic note reads: "I'll redraw with your edits"
```

Hover on disabled button shows tooltip `Generate preview requires an AI provider`.

### 3.6 Acknowledgment line (preview state only)

When the user clicks `Generate preview →` (or fires it via Cmd/Ctrl+Enter), the composer captures the user's prompt and templates an acknowledgment.

Template:
```
You asked: "{first sentence, max 80 chars}" — here's what changes if I apply that.
```

Extraction rules:
1. Trim whitespace from prompt
2. Find first sentence via regex `^[^.!?]+[.!?]?`
3. If first-sentence length > 80, truncate to 80 chars and append `…`
4. Wrap in `"..."` in the template

Rendering:
- Below the preview lead question
- Italic Inter 13px ink2
- 12px left padding behind a 1px `AmberBrush` left rule
- 10px margin-top

If `userPrompt` was empty at preview-generation time (user only edited canvas, no typing), `previewAck` is null and the line is not rendered.

The acknowledgment persists for the duration of the preview state. On `Accept` or `Discard`, the ack is cleared.

---

## 4. Layer-specific canvas interactions

### 4.1 IntentCanvas

**Field grid behaviors:**

Each of the 4 visible fields (`appType`, `primaryUser`, `workflow`, `platforms`) plus the hidden `notes` field is bound 2-way.

| User action                       | System response                                                              |
|-----------------------------------|------------------------------------------------------------------------------|
| Click field                       | Focus enters TextBox, cursor at click position, text-select cursor           |
| Type any character                | Field value updates, ShellModel.MarkDirty(Intent) called                     |
| First Dirty trigger               | Snapshot captured of all IntentValues                                        |
| Tab from field                    | Focus moves to next field (Intent has Tab-order: appType → primaryUser → workflow → platforms → composer textarea) |
| Esc while focused                 | Blurs field (no value reverts)                                               |

**Example-values banner:**

The banner shows above the field grid whenever any single field still matches `INTENT_EXAMPLE`. Computed:

```csharp
ShowingExample = AppType == Example.AppType
              || PrimaryUser == Example.PrimaryUser
              || Workflow == Example.Workflow
              || Platforms == Example.Platforms;
```

Click on `Clear all`:
1. Wipes all 4 fields to empty strings (Notes also cleared)
2. Transitions Clean → Dirty
3. Captures snapshot of pre-clear values
4. Banner disappears (since all fields are now empty, not matching examples)
5. Field text colors turn ink4 (placeholder-style) since all are empty

**Discard edits with cleared fields:**
The snapshot restores the example values. Banner reappears.

**Preview state:**

When `LayerState == Previewing`:
- Banner is hidden
- All fields disabled (read-only display)
- Background of changed fields turns `AmberSoftBrush` with 4px padding inflation + radius 4
- `Proposed` eyebrow appears in column 3 of changed-field rows
- Transitions over 220ms ease

### 4.2 UXFlowStripCanvas

v11 has no direct manipulation. Tiles are read-only. Only interaction:

| User action                       | System response                                                |
|-----------------------------------|----------------------------------------------------------------|
| Hover over tile                   | No visual change (no hover state in v11)                       |
| Hover over canvas                 | No effect                                                       |

All canvas evolution happens via composer prompt. The UI shows derived content based on Intent context.

### 4.3 ArchitectureBlueprintCanvas

The most interactive canvas. Hover-to-explore drives most behavior.

**Module hover:**

| User action                  | System response                                                              |
|------------------------------|------------------------------------------------------------------------------|
| Mouse enter module rect      | Set `HoveredModuleId` to that module's ID                                    |
| Mouse leave module rect      | Set `HoveredModuleId` to null                                                |
| Click module                 | (No effect in v11 — click does not select)                                   |

**Visual response to hover:**

Computed connected set: hovered module + all modules directly connected via any edge + the edges themselves.

| Element                          | No hover     | Hover + connected     | Hover + not connected   |
|----------------------------------|--------------|------------------------|--------------------------|
| Module rect opacity              | 1.0          | 1.0                    | 0.25                     |
| Module rect border weight        | 1px          | 1.8px (bold variant)   | 1px                      |
| Module rect filter               | rough-arch (seed 3) | rough-arch-bold (seed 5) | rough-arch (seed 3) |
| Module label color               | module color | module color           | module color, opacity 0.25 |
| Edge stroke color                | Ink3Brush    | InkBrush               | Ink4Brush                |
| Edge stroke weight               | 1px          | 1.8px                  | 1px                      |
| Edge opacity                     | 0.65         | 1.0                    | 0.12                     |
| Edge label color                 | Ink3Brush    | InkBrush, weight 600   | Ink4Brush                |
| File-count badge (top-right)     | hidden       | visible                | hidden                   |

Transition all visual properties over 240ms ease.

**Detail panel below SVG:**

Updates in real-time as `HoveredModuleId` changes.

When hovering:
```
{Module label}              {N} files · {M} connections
{Module description}
```

When not hovering (resting state):
```
                                                          (italic ink4)
Hover any module to trace its connections
```

**`↻ Regenerate` action button:**

Located in SectionHeader.Action slot.

| User action            | System response                                                              |
|------------------------|------------------------------------------------------------------------------|
| Click `↻ Regenerate`   | Calls `ShellModel.GeneratePreview()` directly — bypasses dirty-state check (explicit re-roll). If !aiConfigured, button is disabled with tooltip. |

### 4.4 DesignTokenGridCanvas

The most-edited canvas.

**Color swatch click:**

Opens `ColorPicker` flyout positioned below the clicked swatch. Standard WinUI `ColorPicker` configured with:
- `ColorSpectrumShape="Box"`
- `IsAlphaEnabled="False"` (v11 doesn't support alpha)
- `IsAlphaSliderVisible="False"`
- `IsColorPreviewVisible="True"`

On `Color` property change:
1. `DesignModel.UpdateToken(tokenName, color)` called
2. Layer transitions to Dirty (or stays Dirty)
3. Token swatch updates immediately
4. **The XAML mirror at the bottom of the canvas updates immediately** (live synthesis — no `Generate preview` required for the mirror to reflect changes)
5. The mini control gallery's "Assign tech" button background updates immediately if the changed token is `Action`

**Body font dropdown (`ComboBox`):**

Three options: `Inter`, `Newsreader`, `Fraunces`.

On selection change:
1. `DesignModel.UpdateToken("BodyFont", value)` called
2. Layer marked Dirty
3. The type-scale samples in DesignCanvas re-render with the new family immediately
4. The Annotation primitives in DesignCanvas re-render (they use BodyFont indirectly)

**Preview state:**

Same diff-visualization pattern as IntentCanvas:
- Color swatch row backgrounds turn `AmberSoftBrush`
- "was {oldHex}" appears in the proposed-was column
- Body font shows "Was {oldFont}" eyebrow in the type-scale section
- All inputs disabled (color pickers can't open, ComboBox grayed out)

### 4.5 StateTransitionDiagramCanvas

Two interaction layers — hover and click — and they have distinct semantics.

**Hover model (`HoveredStateId`):**

| User action               | System response                                              |
|---------------------------|---------------------------------------------------------------|
| Mouse enter state pill    | Set `HoveredStateId` to that state's ID                      |
| Mouse leave state pill    | Set `HoveredStateId` to null                                 |

**Click model (`ActiveStateKind`):**

| User action               | System response                                              |
|---------------------------|---------------------------------------------------------------|
| Click state pill          | Set `ActiveStateKind` to that state's kind, retain selection |
| Click empty area          | No effect — selection persists                                |

**Visual response (the hovered-vs-active model):**

The hover-to-explore three-state ternary applies to connected highlighting (same pattern as Architecture).

Two additional visual elements specific to Interactions:

1. **Pulsing dot on active state:** A 7×7 amber ellipse positioned at top-right of the active state pill, pulsing opacity 1↔0.35 over 1.6s ease-in-out infinite. Renders only when `isActive && !isHovered` — when the user hovers the active state itself, the dot hides (hover preview takes visual priority).

2. **Detail panel left rule:** When hovering a state that is **not** the active state:
   - Left rule turns `AmberBrush`
   - Hint text reads "hover preview · click to select"
   - Description shown is the hovered state's

   When not hovering, or hovering = active:
   - Left rule is `HairlineBrush`
   - Hint text reads "hover any state to trace its transitions"
   - Description shown is the active state's

**Flow tabs (in SectionHeader.Action):**

Three tabs: `Create job` (or context-derived `Create habit` etc.), `Sign in`, `Sync data`. The flow ID is stable across layers — only the label varies.

| User action            | System response                                                                |
|------------------------|--------------------------------------------------------------------------------|
| Click flow tab         | Set `ActiveFlowId`. **Reset `ActiveStateKind` to `Default`.** Diagram re-renders with the new flow's transition descriptions. |

The reset on flow change is intentional — when you switch flows, you're starting a new exploration; the active state from the previous flow shouldn't carry over.

### 4.6 DataContractGridCanvas

v11 read-only. No direct interactions. Composer-driven only.

### 4.7 ImplementationPhaseGridCanvas

v11 read-only. No direct interactions. Composer-driven only.

### 4.8 ScaffoldTerminalCanvas

Three action buttons.

**Command block `Copy` (top-right of dark block):**

| User action           | System response                                                              |
|-----------------------|------------------------------------------------------------------------------|
| Click `Copy`          | Calls `ScaffoldModel.CopyScaffoldCommandToClipboard()`. Button text replaces with `✓ Copied` for 1400ms, then reverts. |

**Action row buttons:**

| Button                       | Click action                                                                 |
|------------------------------|------------------------------------------------------------------------------|
| `Download bundle ↓`          | Calls `ScaffoldModel.DownloadBundle()`. Builds full bundle as Markdown, triggers browser/OS save dialog with `{AppName}-bundle.md` filename. Button text replaces with `✓ Bundle downloaded` for 1800ms. Then locks the Scaffold layer. |
| `Copy prompt-context.md`     | Calls `ScaffoldModel.CopyPromptContextToClipboard()`. Copies the concatenated all-layer markdown to clipboard. Button text replaces with `✓ Copied` for 1400ms. |

**Scaffold has no ComposerFooter** — no prompt, no suggestion chips, no preview state. Lock-and-continue is implicit when the user downloads the bundle.

---

## 5. Workspace shell interactions

### 5.1 Rail reveal animation

Triggered by changes to `RailsVisible`:

```
RailsVisible = LockedIds.Count > 0 OR ActiveIndex > 0
```

When this flips false → true (first lock):

| Property                              | From → To       | Duration | Delay | Easing               |
|---------------------------------------|-----------------|----------|-------|----------------------|
| `CompositionStack.Width`              | 0 → 260         | 480ms    | 0     | EaseOutQuintic       |
| `FilesRail.Width`                     | 0 → 340         | 480ms    | 0     | EaseOutQuintic       |
| `ActivePage.MaxWidth`                 | 720 → 880       | 480ms    | 0     | EaseOutQuintic       |
| `ActivePage.Padding.Top`              | 64 → 32         | 480ms    | 0     | EaseOutQuintic       |
| `CompositionStack.Opacity`            | 0 → 1           | 320ms    | 160ms | EaseInOut            |
| `FilesRail.Opacity`                   | 0 → 1           | 320ms    | 160ms | EaseInOut            |

When this flips true → false (reset):
Reverse storyboard. All values transition back over the same durations.

### 5.2 ProgressIndicator updates

`ActiveIndex` changes → segment width animates over 480ms with EaseOutQuintic from old fraction to new fraction.

### 5.3 CompositionStack row clicks

| User action                          | System response                                                              |
|--------------------------------------|------------------------------------------------------------------------------|
| Click active layer row               | No-op (cursor: default, no hover state)                                      |
| Click locked layer row               | `ShellModel.Revisit(kind)` — sets activeIndex, transitions that layer Locked → Clean, navigates |
| Click future layer row               | No-op (cursor: default, not hit-testable)                                    |

Mouse hover behaviors:
- Active row: no hover styling
- Locked row: background fades to `Paper2Brush` over 140ms
- Future row: no hover styling, cursor stays default

### 5.4 LockedContextCard interactions

**Default expansion state:**

```
Most recent 2 locked layers (by activeIndex distance): expanded by default
All older locked layers: collapsed by default
```

Computed in ShellModel:

```csharp
public IFeed<ImmutableList<LockedCardData>> LockedCards => …
public IFeed<ImmutableHashSet<LayerKind>> DefaultExpandedKinds =>
    Feed.Combine(ActiveIndex, LockedIds).Select(t =>
    {
        var (idx, locked) = t;
        var lockedBeforeActive = Layers.All
            .Where((l, i) => i < idx && locked.Contains(l.Kind))
            .TakeLast(2)
            .Select(l => l.Kind)
            .ToImmutableHashSet();
        return lockedBeforeActive;
    });
```

**Per-card collapse/expand:**

Each card has local state `IsExpanded`. Initial value = `DefaultExpandedKinds.Contains(this.Kind)`.

When `DefaultExpandedKinds` changes (e.g. user locks another layer):
- Cards whose membership in the set changed sync their local `IsExpanded` to match
- User-manually-toggled cards may briefly desync, then re-sync on next change (acceptable)

| User action                  | System response                                                              |
|------------------------------|------------------------------------------------------------------------------|
| Click `+` (collapsed)        | Set local `IsExpanded = true`. Render expanded form. Layout shift, no animation in v11 (could add 240ms height transition in future). |
| Click `−` (expanded)         | Set local `IsExpanded = false`. Render collapsed form. |
| Click `Revisit ↗`            | `ShellModel.Revisit(this.Kind)` (same as clicking the locked stack item)    |
| Mouse hover on card          | Show BlockHandle (`⋮⋮`) in left gutter at -22px offset                       |
| Mouse leave card             | Hide BlockHandle                                                              |

### 5.5 FuturePreviewCard

| User action            | System response                                          |
|------------------------|----------------------------------------------------------|
| Mouse hover            | No effect (cards are `IsHitTestVisible="False"`)         |
| Click                  | No effect                                                |

Cards re-compute opacity whenever ActiveIndex changes — they animate over 480ms EaseOutQuintic as layers are locked and the stack shifts.

---

## 6. FilesRail interactions

### 6.1 Layout (top to bottom)

1. **Live file panel** (or Full bundle view if on Scaffold + ViewAllMode)
2. Hairline divider
3. **File list** (8 layer files + prompt-context.md row, 9 total)
4. Hairline divider
5. **Locked count status panel** (`N of 8 locked` + italic context line)

### 6.2 Live file panel header

When NOT in View All mode:

```
Live file                                          [Per-layer view active]
Updates as the canvas changes. Edit to override.

ux-flows.md                      [Copy] [Preview][Edit]
GENERATED FROM CANVAS
```

When on Scaffold + ViewAllMode is true:

```
Full bundle                                              [← Per-layer]
All locked layers, concatenated. Copy to ship the full brief.

prompt-context.md                                            [Copy]
ALL 8 LAYERS · CONCATENATED
```

### 6.3 Toggle behaviors

**View all toggle button:**

Visible only when `(activeLayer == Scaffold) && (lockedIds.Count >= 7)`. The condition `>= 7` means at least 7 of 8 layers must be locked for the toggle to appear (the user typically reaches Scaffold with all 7 prior layers locked).

| User action                        | System response                                                              |
|------------------------------------|------------------------------------------------------------------------------|
| Click `View all →` (per-layer mode) | Toggle `ViewAllMode` to true. Header updates. Preview/Edit buttons hide. Content switches to full concatenated bundle. |
| Click `← Per-layer` (view all mode) | Toggle `ViewAllMode` to false. Returns to active layer's individual file.   |

The toggle is local to FilesRailModel state — leaves no persistent effect when navigating away from Scaffold.

**Preview / Edit toggle (when not in View All mode):**

Two segmented buttons. Default `EditingMode = false` (Preview).

| User action                | System response                                                              |
|----------------------------|------------------------------------------------------------------------------|
| Click `Preview` (Edit active) | Set `EditingMode = false`. Render `MarkdownPreview` of content.            |
| Click `Edit` (Preview active) | Set `EditingMode = true`. Render textarea with editable content.           |

**Edit textarea:**

| User action                | System response                                                              |
|----------------------------|------------------------------------------------------------------------------|
| Type any character         | Calls `ComposerModel.SetOverride(activeLayer.Kind, newValue)`. Marks layer Dirty. |
| Clear all text             | Override is set to "" (not null). Override remains "active." |
| Click Copy button          | Calls `FilesRailModel.CopyActiveContentToClipboard()`. Button text replaces with `✓ Copied` for 1400ms. |

### 6.4 Override active indicator

When `overrides[activeLayer.Kind]` is set (not null):

```
Override active                                                    Reset
```

`Override active` is an `EyebrowSmallTextStyle` amber weight 600.
`Reset` is an underlined ink3 link-button.

| User action            | System response                                                              |
|------------------------|------------------------------------------------------------------------------|
| Click `Reset`          | `ComposerModel.SetOverride(activeLayer.Kind, null)`. Content falls back to generator output. Layer stays Dirty (the reset is itself an edit). |

### 6.5 FileRow interactions

| User action                  | System response                                                              |
|------------------------------|------------------------------------------------------------------------------|
| Click on file row            | (No-op in v11 — rows are display-only)                                       |
| Hover on file row            | No visual change                                                              |

The amber `Drafted` dot has a 4px box-shadow at 13% alpha — a subtle glow visible in good light. The indigo `Writing` dot pulses opacity 1↔0.5 over 1.6s ease-in-out infinite.

---

## 7. Cross-cutting flows

### 7.1 The standard advancement flow (happy path)

A user advancing through all 8 layers without backtracking:

```
1. Land on Intent. See example values + Continue → button.
2. Click "Clear all" → fields wipe, banner disappears.
3. Type values into 4 fields. Layer goes Dirty as soon as first character typed.
4. Optional: type prompt + Cmd/Ctrl+Enter → preview state with ack line.
5. Click Continue → / Accept and lock → → Intent locks, advance to UX.
6. Rails animate in.
7. UX canvas renders with derived screen names. Read-only.
8. Optional: type prompt + Cmd/Ctrl+Enter for preview. Or just Lock and continue.
9. Repeat for Architecture, Design, Interactions, Data, Implementation.
10. Land on Scaffold. See dotnet new command.
11. Click Download bundle ↓ or Copy prompt-context.md.
12. The composition is complete.
```

### 7.2 The revisit flow

A user who locks 4 layers then realizes Intent needs editing:

```
1. User on Interactions layer (activeIndex = 4). Intent through Design are locked.
2. User clicks "Revisit ↗" on the Intent locked context card. (Or clicks the Intent
   row in CompositionStack.)
3. ShellModel.Revisit(Intent) fires:
   - activeIndex = 0
   - layerStates[Intent] transitions Locked → Clean
   - Navigation to Intent route
4. IntentCanvas renders with current values (NOT example values — they're whatever
   the user had when they originally locked).
5. The 3 downstream locked layers (UX, Architecture, Design) STAY LOCKED.
   Their context cards remain in place. Their files stay drafted.
6. User edits Intent. Layer goes Dirty.
7. User locks Intent again. activeIndex advances to 1 (UX).
8. UX is still locked. Its file content has now updated due to derived feed
   re-computation (UXModel.Flow recomputes from new Intent + ctx).
9. User can continue advancing — UX through Design re-lock with new content,
   or can revisit them too.
```

The system does **not** automatically invalidate downstream layers when an upstream one is edited. The user is in control of how far the change cascades.

### 7.3 The discard flow

A user who tries an edit then changes their mind:

```
1. Layer is Clean. User edits a value.
2. Snapshot captured. Layer transitions to Dirty.
3. User clicks "Discard edits" in the composer footer.
4. Snapshot restored. Composer prompt cleared. Layer returns to Clean.

Or for preview:

1. Layer is Dirty. User clicks "Generate preview →".
2. Preview values computed. Acknowledgment line generated. Layer transitions to Previewing.
3. User clicks "← Discard preview" (or presses Esc).
4. Snapshot restored (back to pre-Dirty state, not pre-preview state — the
   discard goes all the way back to the last Locked or initial state).
5. Composer prompt cleared. previewAck cleared. previewValues cleared.
6. Layer returns to Clean.
```

### 7.4 The reset flow

The "nuclear option" — wipes the entire composition.

```
1. User clicks Reset (top-bar).
2. Confirm dialog: "Reset will discard all locked layers and start over. Continue?"
3. On confirm:
   - All ShellModel state reset to initial values
   - All layer model state reset (intent = INTENT_EXAMPLE, design = DEFAULTS, etc.)
   - All composer prompts cleared
   - All overrides cleared
   - All snapshots cleared
   - All preview-related state cleared
   - Navigation to Intent route
4. RailsVisible flips true → false. Rail reverse-animation plays.
5. User is back at the focused first screen.
```

---

## 8. Error and edge cases

### 8.1 AI service failures

`ILayerPreviewService.GeneratePreviewAsync` may throw on network errors, API errors, etc.

Expected handling:
1. ShellModel catches any exception from the call
2. Falls through to `IdentityLayerPreviewService.GeneratePreviewAsync` behavior (returns current values as proposed values, summary = "Showing your edits as proposed.")
3. Transitions to Previewing with identity-copy values
4. **Does NOT show an error toast** in v11 — failures are silent. The "preview = identity copy" is the visible signal.

Future versions: surface an error chip in the composer footer ("Couldn't reach AI. Showing your edits unchanged.").

### 8.2 Clipboard unavailable

`navigator.clipboard.writeText` (WASM) or `Clipboard.SetContent` (WinUI) may fail (permission denied, secure context required, etc.).

Expected handling:
1. Catch the exception
2. Toast: "Couldn't copy to clipboard. Check browser permissions."

In v11, the button still shows `✓ Copied` for 1400ms even on failure (graceful degradation). This is a known compromise.

### 8.3 Download bundle blocked

On WASM, `URL.createObjectURL` + anchor click is the download mechanism. Browser may block downloads in some configurations.

Expected handling:
1. Catch any exception during Blob creation or click
2. Fall back to copying the content to clipboard
3. Toast: "Couldn't trigger download. Bundle copied to clipboard instead."

### 8.4 Composer prompt with only whitespace

If the user types only whitespace into the composer:
- `MarkDirty` does NOT fire (whitespace counts as empty)
- Layer stays Clean
- `Generate preview →` button stays disabled (not in Dirty state)
- `Lock and continue →` remains the active button

The trigger condition is `prompt.Trim().Length > 0`.

### 8.5 Editing a locked layer's overrides via FilesRail

Possible edge case: user is on the Interactions layer (active), but the FilesRail has been somehow showing UX overrides. Actually no — FilesRail always shows the active layer's content. Locked layers cannot be edited via the FilesRail unless the user navigates back to them (which transitions them to Clean).

### 8.6 RailsVisible flickering

If the user resets while on layer 0 with no locks, RailsVisible stays false → no animation. If they reset while on a later layer, RailsVisible flips true → false → animation plays.

No special handling required — the binding naturally produces the right behavior.

### 8.7 Rapid layer-switching during preview

User is in Previewing state on Architecture. They click `Revisit ↗` on the Intent locked card.

Expected behavior:
1. Architecture's previewing state is preserved (its previewValues, ack stay in memory)
2. Navigation to Intent. Intent transitions Locked → Clean.
3. User edits Intent. Intent goes Dirty.
4. User locks Intent. Advance to UX (since UX is the next not-locked-or-current layer, but UX was locked too — so actually advance to Architecture, which is still in Previewing).
5. **Wait — UX was locked before Architecture, so on lock advance from Intent the activeIndex goes to 1. But UX is still locked, so the user sees UX's Locked context card and an active UX page with state Locked.**

This is a known v11 quirk — locking a revisited layer always advances by 1, regardless of whether downstream layers are locked. The user has to manually advance through them again.

Future: a "Re-lock all" option that smartly walks forward and re-locks downstream layers in place.

---

## 9. The VSM agent contract

The Interactions layer's StateKind enum is **load-bearing** — it maps directly to VisualStateManager state names in generated XAML. This is the contract the agent must preserve when generating downstream code.

### 9.1 Naming convention

For each flow, the generated VSM group is named `{FlowIdPascalCased}StateGroup`:

| Flow ID         | VSM group name           |
|-----------------|--------------------------|
| `create-job`    | `CreateJobStateGroup`    |
| `sign-in`       | `SignInStateGroup`       |
| `sync`          | `SyncStateGroup`         |

For each flow, the 6 VSM states match `StateKind` enum names verbatim:

```xml
<VisualStateGroup x:Name="CreateJobStateGroup">
    <VisualState x:Name="Default" />
    <VisualState x:Name="Loading" />
    <VisualState x:Name="Empty" />
    <VisualState x:Name="Error" />
    <VisualState x:Name="Success" />
    <VisualState x:Name="Offline" />
</VisualStateGroup>
```

The agent generating XAML downstream **MUST** use these exact names (case-sensitive). The view model exposes `CurrentState` as `IState<string>` and binds via:

```xml
<Page xmlns:utu="using:Uno.Toolkit.UI"
      utu:VisualStateManager.States="{Binding CurrentState}">
```

This binding-driven approach replaces procedural `VisualStateManager.GoToState(...)` calls. No code-behind state transitions.

### 9.2 The 6 mandatory states

Every screen in a generated app must implement all 6 states. The Interactions canvas exposes this as a fixed 6-state contract for that reason.

| StateKind | Purpose                                          | Typical visual                                  |
|-----------|--------------------------------------------------|-------------------------------------------------|
| Default   | Baseline / first paint                           | Render data if cached, else show skeleton       |
| Loading   | Async work in progress                           | Skeleton rows, spinner, "Loading…" indicator    |
| Empty     | No data yet (legitimate, not an error)           | Empty-state illustration, primary CTA visible   |
| Error     | User or system error, recoverable                | Coral message, retry button                     |
| Success   | Confirmation, terminal positive                  | Sage banner, "Done" indication                  |
| Offline   | Connection unavailable, queued or read-only      | Amber banner, "Will sync when reconnected"      |

### 9.3 Agent guidance generated into `interaction-spec.md`

The locked Interactions layer's output file includes for each flow's each state:

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

This is the contract downstream agents (Claude Code, Cursor, etc.) consume when generating screen XAML and view-model state machines.

---

## 10. Acceptance criteria

A v11-conformant behavioral implementation:

### State machine
- [ ] Each layer's state is one of {Clean, Dirty, Previewing, Locked} — never two simultaneously
- [ ] Editing any canvas value transitions Clean → Dirty
- [ ] Typing the first non-empty character in the composer prompt transitions Clean → Dirty
- [ ] Clean → Dirty captures a snapshot of all layer state on the first transition
- [ ] Dirty → Clean (via Discard edits) restores snapshot, clears composer prompt
- [ ] Dirty → Previewing fires `ILayerPreviewService.GeneratePreviewAsync` (or identity fallback)
- [ ] Previewing → Clean (via Discard preview or Esc) restores snapshot
- [ ] Previewing → Locked adopts proposed values, clears preview state, advances activeIndex
- [ ] Clean → Locked (via Lock and continue) advances activeIndex without preview
- [ ] Locked → Clean (via Revisit) preserves layer values, sets activeIndex, navigates
- [ ] Reset wipes all layers, clears all transient state, returns to Intent

### Composer footer
- [ ] Header status word maps from layerState: Clean→"Refining", Dirty→"Listening", Previewing→"Proposing"
- [ ] First-layer primary button reads "Continue →" instead of "Lock and continue →"
- [ ] Generate preview button is disabled when `aiConfigured == false`, with explanatory italic note
- [ ] Suggestion chips are always visible when suggestions exist (not just empty state)
- [ ] Cmd/Ctrl+Enter fires generate-preview (dirty) or lock-and-continue (clean)
- [ ] Esc fires discard-preview when in Previewing
- [ ] Per-layer prompt content persists across layer navigation
- [ ] Acknowledgment line appears in Previewing state when promptValue was non-empty at preview-generation time
- [ ] Acknowledgment is templated: `You asked: "{first sentence, max 80 chars}" — here's what changes if I apply that.`

### Canvases
- [ ] IntentCanvas shows example-values banner when any field matches INTENT_EXAMPLE
- [ ] Clear all wipes intent to empty strings and marks Dirty
- [ ] DesignCanvas color picker updates token + XAML mirror live (no Generate preview needed for mirror)
- [ ] Architecture hover sets `HoveredModuleId`, drives three-state ternary visuals
- [ ] Architecture detail panel updates with hovered module + file/connection counts
- [ ] Architecture `↻ Regenerate` button bypasses dirty-state check
- [ ] Interactions has separate `HoveredStateId` and `ActiveStateKind`
- [ ] Pulsing dot renders only when `isActive && !isHovered`
- [ ] Flow tab change resets ActiveStateKind to Default
- [ ] Scaffold has no ComposerFooter (no prompt, no chips, no preview state)
- [ ] Scaffold Download bundle button triggers a real file download
- [ ] Scaffold Copy prompt-context.md button writes to clipboard with confirmation flash

### Shell
- [ ] RailsVisible flips false → true on first layer lock
- [ ] Rail reveal animation runs as specified (480ms width + 320ms opacity with 160ms delay)
- [ ] Reset triggers reverse rail animation
- [ ] ProgressIndicator segment width animates on activeIndex change
- [ ] LockedContextCard auto-collapses all but the most recent 2 by default
- [ ] LockedContextCard `+`/`−` toggle works manually
- [ ] LockedContextCard `Revisit ↗` button triggers Revisit
- [ ] CompositionStack locked layer click triggers Revisit
- [ ] CompositionStack future layer click is a no-op

### FilesRail
- [ ] Preview is the default mode (Preview button starts active)
- [ ] Live file panel is at top of rail, file list below
- [ ] Markdown preview renders H1/H2/H3, lists, blockquotes, code blocks, inline bold/italic/code
- [ ] Editing in the textarea calls `SetOverride` and marks layer Dirty
- [ ] "Override active" indicator appears when overrides[currentLayer] is set
- [ ] Reset link clears override (sets to null), content falls back to generator
- [ ] View all toggle visible only on Scaffold layer with `>= 7` locks
- [ ] View all mode shows concatenated bundle, hides Preview/Edit toggle
- [ ] Copy button works in any mode
- [ ] FileRow status dots: amber (drafted) with glow, indigo (writing) with pulse, hollow ring (planned)

### Keyboard
- [ ] Cmd+Enter on macOS triggers submit
- [ ] Ctrl+Enter on Windows/Linux triggers submit
- [ ] Plain Enter inserts newline (no submit)
- [ ] Esc in Previewing discards preview
- [ ] Tab advances focus per WinUI default order

### Error handling
- [ ] ILayerPreviewService failures fall through to identity preview silently
- [ ] Clipboard failures still show "Copied" confirmation (graceful degradation, v11)
- [ ] Download failures fall back to clipboard (future)

---

## 11. Out of scope (v11)

These behavioral capabilities are not in v11:

- **Drag-edit on canvases** — modules can't be moved, screens can't be reordered
- **Direct rename on canvas elements** — labels are derived, not edited inline
- **Persistent state across sessions** — refresh loses the composition
- **Multi-tab sync** — tabs are isolated
- **Undo/redo across layers** — only per-layer discard exists
- **Pre-flight validation before lock** — locking always succeeds; no required-fields enforcement
- **Animated page transitions** — page swaps are instant
- **Reduced-motion accessibility mode** — animations always play
- **Touch gestures** — pointer-only in v11; no swipe, no long-press
- **Localization of UI strings** — English-only
- **Real-time multi-user collaboration** — single-user workspace only

The companion `ARCHITECTURE-BRIEF-detailed.md` covers structural truth. The companion `DESIGN-BRIEF-detailed.md` covers visual truth. This brief covers behavioral truth — refer to it for any question about "what does the user do, and what happens."
