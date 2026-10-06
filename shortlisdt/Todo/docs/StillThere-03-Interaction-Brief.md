# StillThere — Interaction Brief

Every flow, state, input behavior, and animation for the Uno Platform port, on `net9.0-desktop` (Skia) and `net9.0-android` (Skia). Behavior parity with the web prototype `stillthere.html`; the input model adapts to pointer (Desktop) vs. touch (Android).

Grounded in Uno animation, Skia custom-drawing, and Toolkit input guidance from the Uno docs MCP.

---

## Input model — pointer vs touch

`Framework best practice.` One View, input-adaptive via the `Responsive` extension and pointer/tap handling.

| Affordance | Desktop (pointer) | Android (touch) |
|---|---|---|
| Reveal row actions | hover shows `done · break · snooze` overlay | **tap row** expands it; actions appear inline |
| Enter edit | click row → inline edit | tap row → expand → fields editable in place |
| Commit edit | click outside / `Esc` / `Enter` (title) | tap outside / done key |
| Add task | type in inline add row, `Enter` | type in bottom-pinned add bar, send `+` / IME done |
| Snooze surface | centered dialog | bottom sheet (slide-up) |

`Project convention.` On Android a row has two tap meanings resolved by target: tapping a tag chip filters; tapping anywhere else on the row expands. Tag taps stop propagation so they never trigger expand. This mirrors the prototype's `stopPropagation` rule.

---

## Core flows

### Add a task
1. User types in the add input. Inline `#tags` are allowed in the title.
2. Commit (`Enter` / send): `Add(raw)` parses and strips `#tags` from the title, attaches them, sets `CreatedAt = now`, appends to `Tasks`.
3. The list re-projects; the new task appears at the **bottom** (it is the youngest — Fresh) and the input clears.
4. Persistence fires (debounced).

`Edge:` empty/whitespace title → no-op. Title that is *only* tags (`#work`) → keep the raw text as the title (prototype fallback) so nothing is lost.

### Edit (title / notes / tags)
1. Enter edit (hover-click on Desktop, tap-expand on Android). Title input autofocuses with caret at end.
2. Three borderless fields: title, notes (multiline), tags (space-separated, `#` optional).
3. Commit on focus-out / `Esc`: non-empty title saved; notes saved as-is; tags parsed, normalized (lowercase, deduped, `#` stripped), order preserved.
4. Only one row is expanded at a time; expanding another or tapping outside commits the current one.

### Complete (the signature interaction)
Sequence — see "Completion animation" for the mechanics:
1. `done` invoked → row enters `Completing`.
2. Hand-drawn scratch draws across the **text extent only** (~500 ms).
3. Sage `✓` springs in where the timer was (starts ~280 ms, lands as the scratch finishes); the timer fades out.
4. Row collapses its own height to 0 and lifts 8 px; rows below glide up to close the gap (~380 ms).
5. `Complete(id)` archives the task to `Completed` with `CompletedAt = now`; persistence fires.

### Break it down
1. `break` invoked → prompt for 2–4 steps (Android: a small input dialog; Desktop: dialog or inline multi-field — see open question).
2. `BreakDown(id, steps)` removes the parent, creates one child per step with a **fresh** `CreatedAt` and the parent's **inherited tags**.
3. List re-projects; children appear Fresh at the bottom.

### Snooze with reason
1. `snooze` opens the snooze surface (sheet on Android, dialog on Desktop).
2. Four reasons: Waiting on someone · Not important anymore · Too big — needs breakdown · Need more info. Plus "cancel · clock keeps running".
3. Selecting a reason calls `Snooze(id, reason)` — logs `SnoozeReason`, **the clock keeps running** (no age change), closes the surface.

`Project convention.` Snooze is deliberately low-power: it records *why* something sits, it does not relieve the pressure. That is the product's point.

### Completed: view, restore, clear
1. `› {n} completed` toggle reveals the completed list (most-recent first). Each row: struck title, "`{relative} ago · took {lifespan}`", `restore` on reveal.
2. `restore` → `Restore(id)` moves it back to `Tasks` with the **original `CreatedAt` preserved**, so the clock resumes where it left off (a restored old task is immediately Stale/Rotten again).
3. `clear all completed` → confirm → `ClearCompleted()` empties the archive.

### Delete
`delete` (in expanded edit only) → confirm → `Delete(id)` hard-removes. Not archived. Confirmation prevents accidental loss; on Android use a `ContentDialog`, on Desktop the same.

### Filtering & search
- **State toggle:** `all ↔ only stale+`. When `stale+`, the list shows only Stale/Rotten; header count becomes `{n} of {total}`.
- **Tag filter:** tap a tag chip (in the tag row or on a task) → filter to that tag; tap again clears. Active tag glows `Stale`.
- **Search:** live substring match across title + notes + tags as the user types. Focus is preserved across re-projection (the search field is not inside the re-rendered list region).
- **Combine:** state + tag + search are ANDed.
- **Clear filters:** appears in the header summary when any filter is active; resets all three at once.

---

## State inventory

### Age states (timer color only)
Fresh (<2 h) · Active (<24 h) · Aging (1–3 d) · Stale (3–7 d) · Rotten (7 d+). Transitions are driven by the `Now` tick and surface through the row's `VisualStateManager` group.

### Empty states
| Condition | Message |
|---|---|
| No tasks at all | `nothing yet` |
| Search/tag active, no match | `no matches` |
| `stale+` active, none qualify | `nothing stale or rotten — good week` |
| No completed | toggle reads `no completed yet` (disabled) |

`Common convention.` Empty messages use the body family in italic `InkMute`, centered, generous top padding. They are quiet, not illustrated.

### Loading state
On activation, hydrate from `ITaskStore`. The store read is fast (one small JSON doc); show the list directly. If first paint could precede hydrate on Android cold start, show the empty-state placeholder briefly rather than a spinner — no skeleton chrome.

### Row states
`Default` · `PointerOver` (Desktop, actions visible) · `Expanded` (edit) · `Completing` (scratch + check) · `Collapsing` (height → 0). These are `VisualState`s; `Completing`/`Collapsing` are entered programmatically by the completion sequence.

### Filter states
`All` · `StalePlus` · `TagActive(tag)` · `Searching(query)` · any combination · `FiltersCleared`.

### Reduced motion
`Framework best practice / accessibility.` Honor the OS reduced-motion preference. When set, completion skips the scratch, check spring, and height-collapse; it does a 150 ms opacity fade then archives. All other transitions drop to instant.

---

## Animations

`Framework best practice.` Uno animations use XAML `Storyboard` (`DoubleAnimation`, `ColorAnimation`, keyframes). WinUI/Uno has no custom cubic-bezier easing — approximate the prototype's curves with the closest built-in easing (`CubicEase`/`QuinticEase`/`BackEase`, `EaseOut`) or keyframes.

### 1) Completion scratch — `ScratchView : SKCanvasElement`

`Decision.` Render the scratch with SkiaSharp custom drawing, not a XAML `Path`.
`Reason.` SkiaSharp reproduces the prototype exactly: the procedurally generated zig-zag path, the draw-on reveal via `SKPathMeasure`, and the pencil jitter via Perlin-noise turbulence + displacement — the direct equivalents of the web's `feTurbulence`/`feDisplacementMap`. A XAML `Path` with `StrokeDashOffset` can draw a line on but cannot reproduce the hand-drawn edge.
`Tradeoff.` A small custom control instead of pure XAML. Justified — this is the product's signature moment and must match the prototype.

Mechanics:
- `ScratchView` exposes dependency properties: `Progress` (0→1), `Seed` (int), `TextWidth`, `TextLeft`. It overlays the title in the row template, sized to the title.
- On `Completing`, the row measures the rendered title width (analogous to the web Range measurement), sets `TextLeft`/`TextWidth`, randomizes `Seed`, and animates `Progress` 0→1.
- Path generation (in C#, mirroring the web): a dense **up-and-down zig-zag** confined to the text, ~1 stroke per 16 px (5–28 strokes), alternating top/bottom of a band ≈ 0.6× line height, with per-vertex jitter and a slight overall lean. Smoothed into one continuous `SKPath` (quadratic through midpoints).
- `RenderOverride`: use `SKPathMeasure.GetSegment(0, totalLength * progressSnapshot, dst, true)` to draw only the revealed portion. Stroke = `Done`, ~2.1 px, round cap/join.
- Pencil edge: bake jitter into the geometry (works on every target including the Android compositor thread). Optionally layer a turbulence-displacement `SKImageFilter` on Desktop; keep geometry-jitter as the cross-platform baseline.

`Framework best practice (Android compositor thread).` In `RenderOverride`, do not read UI-thread dependency properties directly. Capture `Progress`/`Seed` into fields on the UI thread (in the property-changed callback) and read those snapshots while drawing; call `Invalidate()` from the callback to schedule the frame.

Drive `Progress` with a `Storyboard` `DoubleAnimation` (≈500 ms, `QuinticEase`/`CubicEase` `EaseOut`) targeting the DP; its changed-callback invalidates the canvas.

### 2) Check spring
A `✓` `TextBlock` in `Done`, `ScaleTransform` 0.3→1 with `BackEase` `EaseOut` (overshoot), ~400 ms, begin ~280 ms. Opacity 0→1 over the first ~120 ms of that.

### 3) Timer release
The age `TextBlock` fades opacity 1→0 (~220 ms) as the scratch begins — the climbing number lets go.

### 4) Row collapse
`DoubleAnimation` on the row container `Height` from measured → 0, plus padding → 0, opacity → 0, `TranslateY` 0→−8, ~380 ms `CubicEase` `EaseOut`.
`Framework best practice.` Height is a layout (dependent) property — set `EnableDependentAnimation="True"` on the animation. Capture the measured height before collapsing. The `ItemsRepeater` reflows as the item shrinks, producing the gap-closing glide. Archive the item on the height animation's `Completed`.

### 5) Snooze surface
Android bottom sheet: `TranslateY` 100%→0 with a settling ease (~280 ms), backdrop fade. Desktop dialog: scale 0.95→1 + opacity, ~200 ms.

### 6) Completed reveal & chevron
Section expands its height; the `›` chevron rotates 90° (~200 ms).

`Project convention.` Completion is the only place with layered, sequenced motion. Snooze and break-down have no exit flourish by design — completion is the single rewarded action.

---

## Feedback states

- **Add:** input clears immediately; new row appears at bottom — the clear is the confirmation.
- **Edit commit:** silent; the row simply reflects the new text. No toast.
- **Complete:** the scratch + check + collapse *is* the feedback.
- **Restore:** the row leaves Completed and reappears in the active list (often already warm-colored — itself meaningful feedback that time kept passing).
- **Destructive (delete / clear):** the only modal confirmations in the app. Everything else is undoable-by-nature (complete → restore) so it needs no confirm.
- **Filter active:** header count switches to `{n} of {total}` and `clear filters` appears — persistent, ambient feedback that a filter is on.

---

## Accessibility

`Framework best practice.`
- `AutomationProperties.Name` on every interactive element: add input, each row action, tag chips ("filter by work"), filter toggle, completed toggle, restore, sheet reasons.
- Each task row exposes a composed name: "`{title}, {ageState}, open {age}`" so a screen reader conveys neglect without sight of color.
- Color is never the sole signal: the `AgeState` is in the row's automation name and the timer text itself carries the duration. The cool/warm palette is an enhancement, not the only channel.
- Focus order follows visual order: header controls → search → state toggle → tag chips → tasks (oldest→newest) → add → completed. `TabIndex` set where DOM-order differs.
- Touch targets ≥ 44×44 on Android via the `Responsive` extension on action buttons.
- Reduced-motion respected (see above).
- Contrast verified against the Design Brief tokens (body ≥ 4.5:1, large timer ≥ 3:1).
- `x:Uid` on visible/interactive elements for localization (EN/FR), e.g. `ListPage.Add.Placeholder`, `SnoozeSheet.Reason.WaitingOnSomeone`.

---

## Runtime verification steps

`Framework best practice.` Verify on Desktop first (fast Hot Reload), then Android. Build before run; drive via Hot Reload.

1. **Aging cadence:** seed a task, confirm the timer increments each second and the `oldest` summary tracks it. Cross a threshold (use a seed near 2 h / 24 h / 3 d / 7 d) and confirm the timer color flips with no manual refresh.
2. **Sort order:** add several tasks; confirm strict oldest-first ordering and that a brand-new task lands at the bottom.
3. **Add with inline tags:** `Buy sand #home #errands` → title is "Buy sand", two tags attached, tags appear in the tag row with counts.
4. **Edit round-trip:** expand, change title/notes/tags, commit by tapping/clicking outside; relaunch the app and confirm persistence (LocalFolder JSON).
5. **Completion sequence:** invoke done; confirm scratch draws over the text extent only (not the empty space), check springs in sage, timer fades, row collapses, neighbors glide up, item appears in Completed with a plausible "took" lifespan.
6. **Reduced motion:** enable OS reduced motion; confirm completion is a quick fade with no scratch/collapse.
7. **Break-down:** split a task into 3 steps; confirm parent gone, 3 Fresh children inheriting the parent's tags.
8. **Snooze:** open the surface (sheet on Android, dialog on Desktop), pick a reason; confirm the clock keeps running (age unchanged) and the surface closes.
9. **Filters:** toggle stale+ (count → `{n} of {total}`), tap a tag (filter + glow), type in search (live, focus retained), then clear filters (all reset).
10. **Restore:** complete a 5-day-old task, restore it; confirm it returns already Stale (CreatedAt preserved).
11. **Empty states:** delete/complete everything → `nothing yet`; search a nonsense string → `no matches`; stale+ with only fresh tasks → `nothing stale or rotten — good week`.
12. **Android specifics:** SafeArea respects the notch/status bar; the add bar sits above the gesture inset; tap-to-expand and tag-tap-to-filter resolve correctly; IME does not cover the add input.
13. **Parity:** the scratch on Android and Desktop look the same (Skia single visual tree) — spot-check side by side.

---

## Unresolved Questions

- Break-down input: a quick prompt-style `ContentDialog` (closest to the prototype) or an inline "add steps" mode within the expanded row (nicer, more work)? Affects the break-down flow above.
- Completion timing total ≈ 1 s (scratch 500 ms + collapse 380 ms, overlapped). On a long list with rapid completions, do we queue/allow concurrent collapses, or lock the row out and process sequentially? (Prototype processes per-row; concurrent is fine but worth confirming.)
- Snooze currently has no visible aftereffect on the row (clock keeps running). Should a snoozed task show a small "snoozed: {reason}" line in the meta, or stay invisible until opened? The prototype logs silently.
- Desktop hover vs Android tap is the only interaction fork. Confirm we keep zero `#if` and express it purely through pointer handling + `Responsive`, rather than any platform-specific code path.
- Reduced-motion source: rely on the OS setting, or also expose an in-app toggle (adds a settings surface we otherwise do not have in v1)?
