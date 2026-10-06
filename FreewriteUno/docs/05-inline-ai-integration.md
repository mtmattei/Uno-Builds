# Spec — Inline AI Integration (post-session editing)

Bring the **InlineAiChat** demo (selection-scoped, streaming AI with Apply-back) into Freewrite as a
review-phase editing companion. Adapted from the demo's three briefs to two Freewrite decisions:

- **MVUX island.** The chat surface stays MVUX (`InlineAiModel` unchanged); the rest of the app stays MVVM.
  The model is self-contained, so the patterns don't mix inside one class — they coexist in one app.
- **Post-session edit mode.** The AI pill is suppressed during the writing phase (timer running, distraction-free).
  It activates only in **review mode** — after the countdown completes, or when no timed session is active.

Decision: MVUX island. Reason: reuse the demo's tested streaming/can-execute plumbing without rewriting it; the
chat is a feed-driven async surface, the exact case `CLAUDE.md` flags for MVUX. Tradeoff: two patterns in one app,
and selection anchoring stays imperative outside the reactive model.

---

## Architecture Brief

### Module structure
Self-contained feature under `FreewriteUno/InlineAi/` so it can be lifted out again:
- `InlineAi/Models/` — `AiIntent`, `ToneOption`, `SelectionContext`, `AiRequest`, `ChatMessage` (+ `RewriteResult`). Plain records/enums, pattern-agnostic.
- `InlineAi/Services/` — `IAiService`/`FakeAiService` (streaming boundary), `IClipboardService`/`ClipboardService`, `IEditorBridge`/`EditorBridge` (model↔document seam).
- `InlineAi/Presentation/` — `InlineAiModel` (MVUX), `InlineAiView` UserControl + action pill, restyled to Freewrite's palette. (P2–P3.)

### State model
MVUX. `InlineAiModel` owns `IState<SelectionContext> Selection`, `IListState<ChatMessage> Messages`,
`IState<string> Draft`, `IState<bool> IsResponding`, `IState<bool> IsChatOpen`, `IState<bool> IsToneExpanded`,
`IState<string?> Toast`. Commands auto-generate from public async methods. Streaming accumulates tokens from
`IAiService.StreamAsync(...)` into the in-flight `ChatMessage`.

### Navigation
None. Overlay-only: an action-pill `Popup` over the selection, a chat `Popup` on pill tap, light-dismiss + `Esc`.
No frame/region navigation; Freewrite stays single-page (MVVM `MainPage` hosts the overlay).

### Services / DI
Registered in `App.xaml.cs` host builder as singletons: `IAiService→FakeAiService`, `IClipboardService→ClipboardService`,
`IEditorBridge→EditorBridge`. `MainPage` (the host) registers the bridge's `Applier` + `SelectionReleaser` on load,
mirroring the demo's `EditorPage`.

### Platform constraints
- Skia desktop is the primary target; Windows + Android follow.
- **Selection rect (highest risk):** `TextBox` exposes no selection geometry on Skia. The demo mirror-measures a hidden
  `TextBlock`; Freewrite's editor is **centered, `MaxWidth=720`, inside a `ScrollViewer`** — the rect math must be
  redone against that layout and the scroll offset (P4).
- **Apply invariant:** Freewrite's editor enforces a leading `"\n\n"` prefix and debounced autosave. `RewriteResult`
  offsets must be prefix-relative; Apply writes via `TextBox.Select`/`SelectedText`, which fires the existing
  `TextChanged → debounced save` path (reused, not bypassed).

### Testing
- Model unit tests (no UI) against `FakeAiService`: idle→streaming→result, `IsResponding` gates Send, tone expand,
  Apply payload offsets (including the prefix shift). MVUX makes this testable without the view.
- Reuse the existing xUnit project (`FreewriteUno.Tests`).

---

## Design Brief

- **Palette:** restyle the demo's Material/DSP surfaces to Freewrite's `ColorPaletteOverride` semantic brushes
  (`EditorBackgroundBrush`, `ChromeBackgroundBrush`, `ChromeOutlineBrush`, `ChromeIconBrush*`). No hardcoded hex.
- **Typography:** Freewrite's `TextBlock.xaml` styles (Lato body, JetBrains Mono meta). The chat prose uses the body style.
- **Pill:** small floating affordance ("Ask AI"), anchored above the selection. Tier-1 Toolkit `Chip`/`AutoLayout` where it fits.
- **Card:** anchored chat popup — context chip, message stream, quick-action chips, composer. Width ~390, clamps on narrow.
- **Quiet by default:** matches Freewrite's muted, paper-like chrome; no accent-heavy Material look.

---

## Interaction Brief

### Review-mode gate (Freewrite-specific)
- **Writing phase** (timer running, or fresh session): **no pill**, no AI affordance. Backspace lock + timer own the surface.
- **Review mode** activates when the countdown reaches `0` (already snaps chrome visible today) or when there is no active
  timed session and the entry has content. In review mode, selecting ≥2 chars shows the "Ask AI" pill.
- Starting the timer again leaves review mode and re-suppresses the pill.

### Primary flow (unchanged from demo, gated by review mode)
Select text → pill above selection → tap *Ask AI* → chat opens, selection held as context → quick action or typed prompt →
streamed response → if a rewrite, **Apply / Copy / Discard**. Apply writes over the original range and closes.

### Input
- Composer: multiline, `Enter` sends, `Shift+Enter` newline, Send disabled while empty or responding.
- During a response, input + quick actions disabled (no overlap).

### States
- Empty: context chip + quick-action chips + composer placeholder.
- Loading: user turn immediate → typing dots → streamed tokens with caret → result card on completion.
- Error: inline error row + Retry; never dump stack traces.
- Apply failure: keep card, "Couldn't apply" toast, Copy stays as fallback.

### Accessibility / motion
- `AutomationProperties.Name` on pill, chips, send, close, result actions; `x:Uid` for EN/FR localization.
- Honor reduced motion (Freewrite already detects `UISettings.AnimationsEnabled`): opacity-only fallbacks.

### Existing "Send to AI" button
- The current chrome flyout (Claude/ChatGPT browser handoff for the whole entry) **stays** for now as the
  "send the whole thing out" path; inline AI is the "work on a selection" path. Revisit consolidation after the prototype.

---

## Implementation Plan (phased, each shippable)

- **P1 — seams (this commit):** models + `IAiService`/`FakeAiService` + `IClipboardService` + `IEditorBridge`, DI-registered. No MVUX feature, no UI. Builds clean.
- **P2 — model:** add `Mvux` UnoFeature; bring `InlineAiModel` as an island; unit-test transitions.
- **P3 — view:** `InlineAiView` + pill, restyled to Freewrite palette.
- **P4 — anchoring:** redo selection-rect math for the centered ScrollViewer editor; verify on desktop.
- **P5 — apply + gate:** wire `EditorBridge.Applier` to the prefix/autosave/lock-aware path; gate the pill to review mode.
- **P6 — real backend:** swap `FakeAiService` for a streaming AI client (`HttpKiota` + key config). Out of scope until P1–P5 land.

## Unresolved Questions

- Review-mode trigger: only on timer-complete, or also a manual "review" toggle in the chrome for sessions written without a timer?
- Apply offsets: confirm `SelectionContext.Start/Length` are stored prefix-relative end-to-end so Apply lands on the right characters.
- Does the inline chat reset per selection (demo behavior) or persist across selections within one review session?
- Keep both AI paths (browser handoff + inline) long-term, or consolidate once inline proves out?
- Backend choice for P6 (Anthropic vs OpenAI) and where the API key lives (user setting vs config).
