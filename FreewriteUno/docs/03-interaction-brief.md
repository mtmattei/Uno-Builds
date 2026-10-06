# FREEWRITE.UNO — Interaction Brief

Version 0.2 · 2026-05-20 · Author: M. Mattei
Spec: 3 of 3 (Architecture · Design · Interaction)

---

## Context

Freewrite is a writing surface ported from macOS SwiftUI to Uno Platform. This document specifies *how the product behaves* — every user gesture, every state transition, every animation, and the verification steps that confirm each behaves correctly. Visual specification lives in 02-design-brief.md. Code architecture lives in 01-architecture-brief.md.

The single behavioural principle: the product should not require the writer's attention. Every interaction in this brief is graded against that bar.

---

## 1. User Flows

### 1.1 First-ever launch

```
Launch app
  └─ MainViewModel.InitializeAsync()
      ├─ EntryStore.LoadAllAsync() → []
      ├─ No entries exist
      ├─ EntryStore.CreateAsync() → new entry (today, empty)
      ├─ Entries := [new entry]
      ├─ SelectedEntry := new entry
      └─ Text := "\n\n"
  └─ MainPage rendered
      └─ Editor focused, cursor positioned at end of "\n\n"
```

Time budget: cursor focused within 250 ms of the window being visible. No splash screen, no welcome dialog, no "tell us about yourself."

### 1.2 Subsequent launch (entries exist)

```
Launch app
  └─ MainViewModel.InitializeAsync()
      ├─ EntryStore.LoadAllAsync() → existing entries (sorted newest-first)
      ├─ Entries := existing
      ├─ Look for an empty entry created today
      │   ├─ Found     → SelectedEntry := that entry
      │   └─ Not found → EntryStore.CreateAsync() → new entry; SelectedEntry := new
      └─ Text := SelectedEntry.content
  └─ MainPage rendered
      └─ Editor focused, cursor at end of text
```

The rule mirrors the macOS original exactly: the writer either continues today's session (if they started one and left it empty) or starts a fresh entry.

### 1.3 The writing loop

```
User types a character
  └─ TextBox.Text changes (UpdateSourceTrigger=PropertyChanged)
  └─ MainViewModel.Text changes
  └─ OnTextChanged()
      ├─ If !value.StartsWith("\n\n") → rewrite to "\n\n" + cleaned value; exit
      └─ DebouncedSave(value)
          ├─ Cancel any pending save CTS
          ├─ Schedule new save 150 ms in the future
          └─ Background: await EntryStore.WriteAsync(entry, content)
```

The 150 ms debounce is imperceptible to the writer and prevents disk thrashing during fast typing. No save indicator. No spinner. No "Saved" badge.

### 1.4 Switching entries

```
User opens sidebar (clicks 🕐 or presses Ctrl+H)
  └─ IsSidebarOpen := true (220 ms slide-in)
User clicks an entry row
  └─ SelectedEntry := entry
  └─ OnSelectedEntryChanged()
      ├─ Flush any pending DebouncedSave synchronously
      ├─ Text := await EntryStore.ReadAsync(entry)
      └─ Editor focus + cursor at end
  └─ IsSidebarOpen := false (220 ms slide-out)
```

The synchronous flush before load is the data-loss-prevention guarantee. There is no time window during which a keystroke from before the switch can be lost.

### 1.5 Running a session

```
User clicks timer
  └─ TimerIsRunning := true
  └─ Schedule chrome fade-out 1000 ms in the future
User does not touch the mouse for 1 second
  └─ ChromeOpacity := 0 (240 ms fade)
User types continuously
  (no UI change — the writer is in the session)
Timer reaches 0
  └─ TimerIsRunning := false
  └─ ChromeOpacity := 1 (immediate)
  └─ No sound, no toast, no celebration
```

The writer decides what the end of the session meant. The product does not interpret it.

### 1.6 Engaging the backspace lock mid-session

```
User taps the ⌫ icon
  └─ BackspaceDisabled := true
  └─ Icon colour shifts from OnSurfaceMuted to OnSurface (160 ms)
User presses Backspace
  └─ KeyDown handler intercepts (Windows / Desktop)
  └─ Android: InputConnection wrapper intercepts
  └─ e.Handled = true (no character deleted)
  └─ Editor background flashes BackgroundAccentColor for 80 ms
```

The flash is the only feedback. No banner, no error, no instructional tooltip.

### 1.7 Sending to AI

```
User clicks the chat icon
  └─ Flyout opens (Claude / ChatGPT)
User selects target
  └─ Build prompt:
      [coaching prefix] + "\n\n" + trimmed entry content
  └─ If urlEncoded.length < 6000:
      └─ Launcher.LaunchUriAsync(chatService + "?prompt=" + encoded)
      └─ Browser opens with the prompt pre-populated
  └─ Else:
      └─ DataPackage clipboard write
      └─ Toast: "Prompt copied for Claude" / "...ChatGPT" (1.8 s)
```

The flyout is dismissed in both branches. No confirmation dialog.

### 1.8 PDF export

```
User clicks the PDF icon
  └─ FileSavePicker opens with suggested filename
      (first 4 words of entry, stripped of punctuation, or "Entry [date]")
User picks location
  └─ IPdfExporter.RenderAsync(content, title) → byte[]
  └─ File.WriteAllBytesAsync(path, bytes)
  └─ Toast: "Saved to [filename]" (1.8 s)
User cancels picker
  └─ No toast, no error, no state change
```

---

## 2. Input Behaviour

### 2.1 Keyboard

| Key / chord | Context | Behaviour |
|---|---|---|
| Any character key | Editor focused | Insert character at cursor (standard) |
| `Backspace` / `Delete` | Editor focused, `BackspaceDisabled=false` | Standard deletion |
| `Backspace` / `Delete` | Editor focused, `BackspaceDisabled=true` | Swallowed; 80 ms surface flash |
| `Enter` | Editor focused | New line (standard, since `AcceptsReturn=true`) |
| `Ctrl+N` / `Cmd+N` | Anywhere | Invoke `NewEntryCommand` |
| `Ctrl+H` / `Cmd+H` | Anywhere | Invoke `ToggleSidebarCommand` |
| `Ctrl+T` / `Cmd+T` | Anywhere | Invoke `ToggleTimerCommand` |
| `Esc` | Sidebar open | Close sidebar |
| `Esc` | Chat flyout open | Close flyout |
| `Tab` | Anywhere | Standard focus traversal (chrome icons in document order) |

Keyboard shortcuts are not advertised in the UI. Writers who use them know they exist. Writers who don't, don't need them.

### 2.2 Pointer

| Gesture | Target | Behaviour |
|---|---|---|
| Click | Chrome icon | Invoke associated command |
| Click | Sidebar row | Select entry, close sidebar |
| Click | Editor surface | Focus editor (standard) |
| Right-click | Sidebar row | Open context menu — only "Delete" in v1 |
| Double-click | Timer | Reset to 15:00 (`ResetTimerCommand`) |
| Scroll wheel | Timer (when hovered) | Adjust ±5 min, clamped 5–120 min |
| Hover | Bottom 96 px of viewport | Reveal chrome (only when timer running) |
| Hover | Chrome icon | Colour shift from `OnSurfaceMutedBrush` to `OnSurfaceBrush` (160 ms) |

### 2.3 Touch (Android)

| Gesture | Target | Behaviour |
|---|---|---|
| Tap | Chrome icon | Invoke associated command |
| Tap | Sidebar row | Select entry, close sidebar |
| Tap | Editor surface | Focus editor, soft keyboard appears |
| Long-press | Sidebar row | Open action sheet — "Delete entry" |
| Long-press | Timer | Open ±5 min adjustment popover with explicit +/− buttons |
| Tap outside | Sidebar open | Close sidebar |
| Tap outside | Chat flyout open | Close flyout |
| Swipe right-to-left from edge | Anywhere | Open sidebar (mirror of left-to-right close) |

The scroll-wheel-on-timer pattern has no touch analog. The long-press popover is the substitute.

---

## 3. Empty States

### 3.1 No entries exist

Handled by the launch flow (§1.1) — a new entry is created. The user never sees an "empty state" for the entries collection. There is always at least one entry.

### 3.2 Selected entry is empty

The editor shows the two-newline prefix and a blinking caret. No placeholder text. No "Start writing…" hint. The cursor itself is the affordance.

In the sidebar, an empty entry's preview row reads "Empty entry" in `OnSurfaceMutedBrush` italic. The sidebar metadata (date) shows "Today" / "Yesterday" / "MMM d".

### 3.3 Sidebar with one entry

No special treatment. The sidebar shows one row. There is no "Create your first entry" call to action because the only entry that exists is the one the user is already in.

---

## 4. Loading States

### 4.1 Initial load

`MainViewModel.InitializeAsync()` runs synchronously on the UI thread before the first frame is rendered. With < 1000 entries (the typical realistic ceiling) `LoadAllAsync` completes in < 100 ms. No spinner is shown.

If load exceeds 500 ms (e.g. on a cold disk, on Android first-launch with hundreds of cached entries), the editor renders empty and the sidebar populates progressively. No spinner; the editor's emptiness is itself the "still loading" signal.

### 4.2 Entry switching

`ReadAsync` on a single `.md` file is sub-millisecond on every target. No loading state.

### 4.3 PDF export

`QuestPdfExporter.RenderAsync` for a typical 1–5 KB entry completes in < 200 ms. No spinner shown. If render exceeds 1 second (an entry of unusual size), the chrome's PDF icon dims to `OnSurfaceFaintBrush` for the duration of the render. The icon does not animate.

### 4.4 No long-running operations exist

That sentence is the design. The product has no network calls, no cloud sync, no AI inference, no heavy computation. Every operation completes faster than a loading indicator can communicate. The absence of spinners is a feature.

---

## 5. Error States

### 5.1 Save failure

`EntryStore.WriteAsync` fails (disk full, permission revoked, etc.). The debounced save task observes the exception:

1. First failure: silent retry once after 500 ms.
2. Second failure: persistent toast at the bottom of the screen, `OnSurfaceColor` background, `SurfaceColor` text, no dismiss button. Text: "Couldn't save — check disk space."
3. The toast remains until a subsequent save succeeds.

The writer keeps writing. The product does not block input. The worst-case outcome is the current session lives only in memory — an acceptable failure mode given the failure is rare.

### 5.2 Load failure

`EntryStore.LoadAllAsync` fails (folder permissions, etc.). The catch in `InitializeAsync` falls back to:

1. Empty `Entries` collection.
2. Create a new entry in memory (not yet persisted).
3. Toast at the bottom of the screen: "Couldn't access your entries folder."

The writer can still write. If they keep writing, the next `WriteAsync` will reveal whether the underlying issue is permanent (likely re-fail with the §5.1 path) or transient.

### 5.3 PDF export failure

`QuestPdfExporter.RenderAsync` throws or `File.WriteAllBytesAsync` fails. Toast at the bottom: "Couldn't export PDF." No retry; the writer can try again from the same button.

### 5.4 Font load failure

A bundled `.ttf` cannot be loaded at startup. The font name still appears in the cycle, but selecting it leaves the editor using the previous font. No error message. The cycle skips broken fonts silently on the next press.

### 5.5 Clipboard write failure

`DataPackage` write fails (rare). The toast text reads "Couldn't copy to clipboard." No retry.

### 5.6 What we explicitly do not do

- No modal error dialogs anywhere.
- No "Send error report" prompt.
- No multi-line error explanations.
- No iconography on error toasts (no ⚠, no ❌). Text alone.

---

## 6. Animations & Transitions

Every duration is in the 80–240 ms band. No springs, no overshoots, no parallax.

| Transition | Trigger | Duration | Easing |
|---|---|---|---|
| Chrome fade out | Timer running ≥ 1 s with no hover | 240 ms | EaseInOut, 1000 ms delay before start |
| Chrome fade in (timer end) | `TimerRemaining` reaches 0 | 0 ms | Snap |
| Chrome fade in (hover) | Pointer enters bottom 96 px | 160 ms | EaseOut |
| Chrome fade out (hover end) | Pointer leaves bottom 96 px | 240 ms | EaseInOut, 600 ms delay |
| Sidebar open | `IsSidebarOpen` → true | 220 ms | EaseInOut, translateX |
| Sidebar close | `IsSidebarOpen` → false | 220 ms | EaseInOut, translateX |
| Theme switch | `Theme` changes | 200 ms | EaseInOut, cross-fade Surface + OnSurface |
| Backspace flash | `Backspace` pressed while locked | 80 ms in, then transparent | EaseOut |
| Toast appear | Toast set | 160 ms | EaseOut, opacity 0 → 0.92 |
| Toast dismiss | 1.8 s after appear | 240 ms | EaseInOut, opacity 0.92 → 0 |
| Chrome icon hover | Pointer enter / leave icon | 160 ms | EaseOut, colour transition |
| Sidebar row hover | Pointer enter / leave row | 120 ms | EaseOut, background transition |
| Entry switch | New entry selected | 0 ms | Snap (no transition) |

Entry switching is intentionally instant. A fade or slide between entries would call attention to the act of switching — the opposite of what we want.

---

## 7. Feedback States

The full vocabulary of "the product noticed your input":

| Signal | Visual | Used for |
|---|---|---|
| Hover | Colour shift to `OnSurfaceBrush` (160 ms) | All chrome icons, all chrome text buttons |
| Active / toggled-on | Colour stays at `OnSurfaceBrush` | Backspace lock when engaged, sidebar toggle when open |
| Selected | `BackgroundAccentBrush` on the row | Sidebar selected entry |
| Confirmation | Toast at bottom, 1.8 s | Chat prompt copied, PDF saved |
| Rejection | 80 ms surface flash | Backspace pressed while locked |
| Persistent error | Non-dismissing toast | Save / load failure (§5) |

That is the complete list. There are no other feedback patterns. Adding one is a feature decision that crosses the spec boundary.

---

## 8. Accessibility Considerations

### 8.1 Contrast

| Pair | Light mode ratio | Dark mode ratio | WCAG |
|---|---|---|---|
| `OnSurface` on `Surface` | 12.6:1 | 13.1:1 | AAA |
| `OnSurfaceMuted` on `Surface` | 4.2:1 | 4.1:1 | AA |
| `OnSurfaceFaint` on `Surface` | 1.6:1 | 1.7:1 | Below AA (used only for disabled state, no informational content) |

`OnSurfaceFaint` is intentionally below the AA threshold because its content is *absence*, not communication. A disabled control should not compete for attention.

### 8.2 Touch targets

All chrome icon buttons are minimum 32 × 32 px (28 px content + 4 px padding) on desktop and minimum 44 × 44 px on Android. Sidebar rows are 56 px tall (44 px content + 12 px padding) on all platforms. These meet Material's minimum and Apple's HIG minimum.

### 8.3 AutomationProperties

Per Uno guidance, all interactive elements get `AutomationProperties` annotations:

| Element | `AutomationProperties.Name` | `AutomationProperties.HelpText` |
|---|---|---|
| Editor TextBox | "Editor" | "Type your entry here" |
| Font size button | "Font size" | "{value} points. Click to cycle." |
| Font name button | "Font" | "{value}. Click to cycle." |
| Timer button | "Timer" | "{MM:SS}. Click to start or pause. Double-click to reset." |
| Backspace lock | "Backspace lock" | "{Engaged \| Disengaged}. Click to toggle." |
| Chat | "Send to AI" | "Open AI chat options." |
| PDF export | "Export PDF" | "Save the current entry as PDF." |
| New entry | "New entry" | "Create a new writing entry." |
| Theme toggle | "Theme" | "{Light \| Dark}. Click to toggle." |
| Sidebar toggle | "History" | "Show or hide your entry history." |

### 8.4 Keyboard focus

Tab order traverses chrome icons left-to-right when the chrome is visible. When the chrome is hidden by the fade, tabbing into a chrome icon also reveals the chrome (sets `ChromeOpacity := 1` on focus). The editor is the default focused element on every launch and after every entry switch.

### 8.5 Screen reader behaviour

Sidebar entries are announced as `"{preview text}, {relative date}"`. Empty entries are announced as `"Empty entry, {relative date}"`. Toast messages are announced with `LiveSetting=Polite`. The chrome icon labels read from the `AutomationProperties.Name` values above.

### 8.6 Reduced motion

If `Windows.UI.ViewManagement.UISettings.AnimationsEnabled` is `false` (Windows reduced-motion setting) or the Android `Settings.Global.ANIMATOR_DURATION_SCALE` is 0, all transitions in §6 collapse to 0 ms. The product remains fully functional with no animation; the chrome simply appears and disappears without fade.

---

## 9. Runtime Verification Steps

This checklist is the per-milestone exit criteria. Each row is executed by hand on each of the three TFMs at the end of the relevant milestone.

### 9.1 M0 checklist (skeleton)

| # | Action | Pass criterion |
|---|---|---|
| 1 | `dotnet build` on all three TFMs | Returns 0, no warnings |
| 2 | Launch the app on each TFM | Window appears with empty editor |
| 3 | Type a character | Character appears in editor |

### 9.2 M1 checklist (writing loop)

| # | Action | Pass criterion |
|---|---|---|
| 1 | Cold launch → cursor focused | < 250 ms |
| 2 | Type a sentence, wait 200 ms | File appears in `ApplicationData.Current.LocalFolder/Freewrite/` with matching content |
| 3 | Force-quit app, relaunch | The same entry is re-selected with the same content |
| 4 | Type rapidly for 5 seconds | Only one file write per ~150 ms quiet period (verify via file system events) |
| 5 | Open the saved `.md` file in an external editor | Plain UTF-8 markdown, leading `\n\n`, content matches |
| 6 | Click "New entry" | New file appears, new entry is selected, editor is empty |
| 7 | Quit, edit the `.md` file externally, relaunch | Edited content appears in editor |

### 9.3 M2 checklist (chrome)

| # | Action | Pass criterion |
|---|---|---|
| 1 | Click timer | Chrome fades to invisible over ~240 ms, starting 1 s after click |
| 2 | Move pointer to bottom of viewport | Chrome returns over ~160 ms |
| 3 | Move pointer away from bottom | Chrome fades again after ~600 ms |
| 4 | Wait for timer to reach 0 | Chrome snaps to visible immediately |
| 5 | Double-click timer | Timer resets to 15:00 |
| 6 | Scroll mouse wheel over timer (desktop) | Adjusts in 5-minute increments, clamped 5–120 |
| 7 | Toggle theme | Background and text cross-fade over ~200 ms |
| 8 | Quit and relaunch | Theme persists |
| 9 | Cycle font, cycle size | Editor visual changes; chrome stays in Lato |
| 10 | Open sidebar | Slides in from right over ~220 ms |
| 11 | Click an older entry | Loads in editor, sidebar dismisses |
| 12 | Type in old entry, close, relaunch, return to it | Edits persist |

### 9.4 M3 checklist (polish)

| # | Action | Pass criterion |
|---|---|---|
| 1 | Toggle backspace lock, hit Backspace (desktop) | Keystroke rejected, surface flashes for 80 ms |
| 2 | Toggle backspace lock, hit Backspace (Android, IME varies: Gboard / Samsung / SwiftKey) | Keystroke rejected on each IME (or feature is disabled with tooltip if shim is not ready) |
| 3 | Open chat flyout, select Claude | Browser tab opens with prompt populated (or clipboard receives prompt for > 6000 chars) |
| 4 | Same for ChatGPT | Same behaviour |
| 5 | Click export PDF | File save picker opens with sensible default name |
| 6 | Confirm export | PDF saved at chosen location, opens in OS viewer with correct typography |
| 7 | Run the full M2 checklist again | All M2 checks still pass |

### 9.5 Accessibility verification

| # | Action | Pass criterion |
|---|---|---|
| 1 | Tab through all chrome icons | Focus visits each in document order; focus ring visible |
| 2 | Tab into a chrome icon while chrome is hidden | Chrome reveals |
| 3 | Engage screen reader (NVDA / TalkBack / VoiceOver via Skia) | Editor announces as "Editor". Each icon announces its name + help text |
| 4 | Verify with high-contrast OS setting | All text remains readable; no information conveyed only by colour |
| 5 | Enable OS reduced-motion | All animations collapse to instant transitions; app remains functional |
| 6 | Resize window to 320 × 600 (Android phone) | Chrome compresses per §6.3 of the Design Brief |

---

## Unresolved Questions

- **Keyboard shortcuts on Skia.Desktop macOS.** Should `Ctrl+N` become `Cmd+N` automatically, or should we register both? Uno's `KeyboardAccelerator` handles this for WinUI but Skia.Desktop's behaviour needs verification. M2.
- **The `Esc`-to-close convention.** Currently specified for sidebar and chat flyout. Should it also collapse the chrome when timer is paused? Likely overreach; flag and revisit after a session of use.
- **Right-click vs long-press parity.** Desktop has right-click-on-sidebar-row → delete. Android has long-press → action sheet with delete. The macOS Skia.Desktop build needs to decide between right-click and Ctrl-click for the same gesture. M2.
- **Toast persistence after navigation.** If a save failure toast is showing and the user switches entries, should the toast stay visible? Currently specified to stay until the next successful save. Verify this doesn't read as "broken" to a writer.
- **Soft keyboard padding adjustment on Android.** When the soft IME appears, does the editor scroll its content correctly so the cursor remains visible? Uno's `SafeArea` should handle this; verify with each IME during M3.
- **Reduced motion vs the backspace flash.** The 80 ms surface flash is technically motion. Should reduced-motion users still get it? Argument for: it's the only "no" signal in the product. Argument against: 80 ms is below the motion-disturbance threshold. Lean toward keeping it; flag for testing.
