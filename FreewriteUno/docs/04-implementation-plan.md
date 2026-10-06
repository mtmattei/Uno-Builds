# FREEWRITE.UNO — Implementation Plan

Version 0.2 · 2026-05-20 · Author: M. Mattei
Spec: 4 of 4 (follows the three briefs)
References: 01-architecture-brief.md · 02-design-brief.md · 03-interaction-brief.md

---

## Context

This document is the execution plan for the v1 freewrite Uno Platform port. It assumes all three briefs (Architecture, Design, Interaction) have been read and agreed. Milestones below sequence the work so that each closes with a runnable, demonstrable, manually-verified increment.

The chassis is MVVM. The targets are Windows (WinAppSDK), Skia.Desktop, and Android. Video, transcription, and "Random font" are out of scope.

---

## Milestone Sequencing

```
M0 ─── M1 ─── M2 ─── M3 ─── v1 release
                                 │
                                 └── v1.1 (Android backspace + SAF)
```

Each milestone closes with the runtime verification checklist from §9 of the Interaction Brief.

---

## M0 — Skeleton

**Goal**: a project that builds and launches on all three targets, with no business logic yet.

### Work items

| # | Item | File(s) |
|---|---|---|
| 1 | `dotnet new unoapp -preset blank -o FreewriteUno` | (new project) |
| 2 | Set `<TargetFrameworks>net9.0-windows10.0.19041;net9.0-android;net9.0-desktop</TargetFrameworks>` | `FreewriteUno.csproj` |
| 3 | Add `<UnoFeatures>Toolkit;Material;Mvvm</UnoFeatures>` | `FreewriteUno.csproj` |
| 4 | Add `CommunityToolkit.Mvvm` and `QuestPDF` NuGet packages | `FreewriteUno.csproj` |
| 5 | Bundle Lato / Newsreader / JetBrains Mono `.ttf` files | `Assets/Fonts/` |
| 6 | Register fonts as `FontFamily` resources | `App.xaml` |
| 7 | Create `Platforms/Android/MainActivity.cs`, `Platforms/Desktop/Program.cs`, `Platforms/Windows/App.xaml.cs` per Uno templates | (new files) |
| 8 | Confirm `dotnet build` succeeds on each TFM | (build) |
| 9 | Confirm each TFM launches to an empty page | (manual run) |

### Exit criteria

The M0 checklist (Interaction Brief §9.1) passes on all three TFMs.

### Time budget

One focused evening. If any TFM build fights you for more than 2 hours, stop and audit `uno-check` output.

---

## M1 — Writing Loop

**Goal**: a writer can type, close the app, reopen it, and find their text.

### Work items

| # | Item | File(s) |
|---|---|---|
| 1 | Define `Entry` record | `Models/Entry.cs` |
| 2 | Define `IEntryStore` interface | `Services/IEntryStore.cs` |
| 3 | Implement `EntryStore` against `ApplicationData.Current.LocalFolder/Freewrite/` | `Services/EntryStore.cs` |
| 4 | Define `ISettingsStore` interface and `SettingsStore` implementation | `Services/ISettingsStore.cs`, `Services/SettingsStore.cs` |
| 5 | Wire `IHostBuilder` DI with singleton services and transient `MainViewModel` | `App.xaml.cs` |
| 6 | Implement `MainViewModel` with state, `OnTextChanged` enforcer, debounced save, entry collection management | `ViewModels/MainViewModel.cs` |
| 7 | Implement `MainPage` with `TextBox` two-way binding, `UpdateSourceTrigger=PropertyChanged` | `Views/MainPage.xaml(.cs)` |
| 8 | Apply `EditorTextBoxStyle` (basic version — no theming yet) | `Themes/TextBox.xaml` |
| 9 | Write `EntryStoreTests` per Architecture Brief §7.1 | `FreewriteUno.Tests/EntryStoreTests.cs` |
| 10 | Write `MainViewModelTests` for the debounce and enforcement logic | `FreewriteUno.Tests/MainViewModelTests.cs` |

### Exit criteria

The M1 checklist (Interaction Brief §9.2) passes on all three TFMs. All unit tests pass.

### Time budget

Two evenings. The debounce-via-CTS pattern is the trickiest piece; the tests will surface any issues fast.

---

## M2 — Chrome

**Goal**: the visual system is in place. Timer, theme toggle, font/size cycling, and sidebar all work.

### Work items

| # | Item | File(s) |
|---|---|---|
| 1 | Author `ColorPaletteOverride.xaml` with light + dark theme dictionaries per Design Brief §4 | `Themes/ColorPaletteOverride.xaml` |
| 2 | Author brush aliases in the same file per Design Brief §4.3 | `Themes/ColorPaletteOverride.xaml` |
| 3 | Wire `MaterialTheme` + `ColorPaletteOverride` in App.xaml's merged dictionaries | `App.xaml` |
| 4 | Author `TextBlock.xaml` with the six named styles from Design Brief §3.1 | `Themes/TextBlock.xaml` |
| 5 | Update `EditorTextBoxStyle` to use brush resources for `Foreground`, `CaretBrush`, `SelectionHighlightColor` | `Themes/TextBox.xaml` |
| 6 | Implement chrome pill layout with `StackPanel`, `Border`, separator hairlines | `Views/MainPage.xaml` |
| 7 | Wire chrome icon buttons to `[RelayCommand]` ViewModel commands | `Views/MainPage.xaml` |
| 8 | Implement timer countdown via `DispatcherTimer`, `ToggleTimerCommand`, `ResetTimerCommand` | `ViewModels/MainViewModel.cs` |
| 9 | Implement chrome fade-out Storyboard, triggered by `OnTimerIsRunningChanged` | `Views/MainPage.xaml.cs` |
| 10 | Implement bottom-edge pointer hover reveal | `Views/MainPage.xaml.cs` |
| 11 | Implement `OnTimerWheel` for ±5 min scroll adjustment (desktop) | `Views/MainPage.xaml.cs` |
| 12 | Implement font/size cycling commands | `ViewModels/MainViewModel.cs` |
| 13 | Implement theme toggle with persistence via `ISettingsStore` | `ViewModels/MainViewModel.cs` |
| 14 | Implement `SplitView`-based sidebar with `ListView` of entries, custom row template | `Views/MainPage.xaml` |
| 15 | Implement responsive markup via `ResponsiveExtension` for the < 600 px breakpoint | `Views/MainPage.xaml` |
| 16 | Implement long-press timer popover for Android | `Views/MainPage.xaml`, `Views/MainPage.xaml.cs` |

### Exit criteria

The M2 checklist (Interaction Brief §9.3) passes on all three TFMs.

### Time budget

Two to three evenings. Most of M2 is XAML and styling — Hot Reload should make the iteration fast.

---

## M3 — Polish

**Goal**: every v1 feature ships, with the Android backspace lock either working or cleanly deferred.

### Work items

| # | Item | File(s) |
|---|---|---|
| 1 | Implement backspace lock via `KeyDown` handler on desktop heads | `Views/MainPage.xaml.cs` |
| 2 | Implement 80 ms surface flash on backspace-while-locked | `Views/MainPage.xaml.cs` |
| 3 | Android `InputConnection` spike (1 day timebox) | `Platforms/Android/NoDeleteInputConnection.cs`, `BackspaceGuard.cs` |
| 4 | If spike succeeds: integrate `BackspaceGuard.Install` into `Editor_Loaded` | `Views/MainPage.xaml.cs` |
| 5 | If spike fails: disable Android backspace lock toggle with tooltip; ship feature in v1.1 | `Views/MainPage.xaml.cs` |
| 6 | Implement chat handoff via `Launcher.LaunchUriAsync` with clipboard fallback | `ViewModels/MainViewModel.cs` |
| 7 | Implement chat flyout UI on chat icon | `Views/MainPage.xaml` |
| 8 | Implement toast component for confirmations and errors | `Views/MainPage.xaml` |
| 9 | Implement `IPdfExporter` + `QuestPdfExporter` per Architecture Brief §4.2 | `Services/IPdfExporter.cs`, `Services/QuestPdfExporter.cs` |
| 10 | Wire `ExportPdfCommand` to `FileSavePicker` | `Views/MainPage.xaml.cs`, `ViewModels/MainViewModel.cs` |
| 11 | Implement error handling per Interaction Brief §5 (silent retry, persistent toast) | `ViewModels/MainViewModel.cs` |
| 12 | Implement `AutomationProperties` annotations on all interactive elements | `Views/MainPage.xaml` |
| 13 | Implement reduced-motion detection and animation collapse | `Views/MainPage.xaml.cs` |

### Exit criteria

The M3 checklist (Interaction Brief §9.4) passes on Windows and Skia.Desktop. The accessibility verification checklist (Interaction Brief §9.5) passes on Windows. Android backspace lock either passes M3 #2 or is cleanly disabled.

### Time budget

Two evenings + the 1-day Android `InputConnection` spike. Total ~4 days of focused work.

---

## v1 Release

The v1 build is what ships publicly. Pre-release work:

| # | Item |
|---|---|
| 1 | App icon (single SVG, generated at all required sizes per platform) |
| 2 | Manifest metadata (display name, version, capabilities) for each TFM |
| 3 | Signed builds for Windows (`.msix`) and Android (`.aab`) |
| 4 | Skia.Desktop distribution: `.deb` for Debian/Ubuntu, `.dmg` for macOS, `.exe` installer for Windows-without-WinAppSDK |
| 5 | Public README with screenshots and one-paragraph philosophy |
| 6 | LICENSE (MIT, matching the upstream macOS original) |
| 7 | A landing page mirroring freewrite.io's restraint |

The release process is documented in a separate `RELEASE.md` once the v1 build artifact format is settled.

---

## v1.1 — Android backspace + SAF

**Goal**: complete the macOS-original feature parity that v1 deferred.

| # | Item |
|---|---|
| 1 | Subclass `AppCompatEditText` as `FreewriteEditText` exposing an `InputConnection` factory callback |
| 2 | Register `FreewriteEditText` into Uno's Android `TextBox` template |
| 3 | Implement `BackspaceGuard.Install` to wire the callback to the active `MainViewModel.BackspaceDisabled` flag |
| 4 | Re-enable the Android backspace lock toggle in the chrome |
| 5 | Verify against Gboard, Samsung Keyboard, and SwiftKey |
| 6 | Implement an Android Storage Access Framework folder picker — user can move entries to a user-visible Documents folder |
| 7 | Implement equivalent Documents folder picker on Windows (`FolderPicker`) and Skia.Desktop |
| 8 | Implement an `IEntryStore` adapter that uses the picked folder when set, falls back to `LocalFolder` otherwise |
| 9 | Add a "Show in file manager" command to the chrome (opens the OS file manager at the entries folder) |

### Exit criteria

The macOS original's "your files live in `~/Documents/Freewrite/`" promise is honoured on all three TFMs (via SAF / folder picker where required).

### Time budget

One week of focused work. The `InputConnection` shim and the SAF flow are both fiddly but well-understood patterns.

---

## Consolidated Unresolved Questions

Rolled up from all three briefs. Each is annotated with which milestone resolves it.

### Architecture

- **Android `InputConnection` approach** (subclass vs runtime modification) — *resolved in M3 spike, fallback to v1.1 if spike runs over*.
- **Skia.Desktop font registration on Linux** — *verified in M0*.
- **`TextBox.LineHeight` fidelity across heads** — *verified in M1, may require `RichEditBox` fallback per head*.
- **DI host on Skia.Desktop activation path** — *verified in M0*.
- **QuestPDF font embedding** (Newsreader bundled in PDF vs system font reference) — *resolved in M3 when QuestPdfExporter lands*.

### Design

- **`TextBox.LineHeight` fidelity** *(duplicate of Architecture; same resolution)*.
- **Material vs Cupertino on Skia.Desktop macOS** — *deferred; v1 ships Material everywhere; revisit if Skia.Desktop macOS adoption becomes a goal*.
- **The mono-font-for-metadata convention** (sidebar dates, timer in JetBrains Mono vs all-Lato per the macOS original) — *ship as specified; user feedback after first session of real use will decide whether to revert*.
- **Mobile overflow menu visual** (ellipsis vs alternative) — *resolved in M3 when mobile responsive layout lands*.
- **Print typography for PDF export** (18 sp / 1.5× per editor vs 12 pt / 1.6× per print convention) — *resolved in M3 when QuestPdfExporter lands*.
- **Dark-mode selection highlight tone** (16% opacity may read too cool) — *tune in M2 against actual reading conditions*.

### Interaction

- **Skia.Desktop macOS keyboard shortcut mapping** (Cmd vs Ctrl) — *resolved in M2*.
- **`Esc`-to-close scope** (sidebar and chat flyout — extend to chrome reveal?) — *ship as specified; revisit after a session of use*.
- **Right-click vs Ctrl-click on Skia.Desktop macOS** — *resolved in M2*.
- **Toast persistence semantics after navigation** (does a save-failure toast stay through entry switch?) — *currently specified to stay; verify it doesn't read as broken*.
- **Soft keyboard adjustment on Android** (cursor visibility when IME appears) — *verified in M3 with each tested IME*.
- **Reduced-motion handling for the 80 ms backspace flash** — *current default keeps the flash; flag for testing*.

---

## Stop Conditions

Per session-workflow rules, the following events trigger a Stop Check before continuing:

- Any milestone running > 1.5× its budgeted time without a clear path to completion.
- The Android `InputConnection` spike running past the 1-day timebox.
- `TextBox.LineHeight` variance across heads turning out to be > 4 sp (would force a `RichEditBox` swap, which is a structural change worth re-discussing).
- A new dependency being considered (beyond CommunityToolkit.Mvvm and QuestPDF in the current plan).
- The unit test suite growing beyond ~30 tests (signals over-engineering for a product this size).

---

## Suggested Doc Updates Cadence

This plan and the three briefs are living documents through M3. Updates expected:

| Trigger | Doc to update |
|---|---|
| TFM build fight in M0 | Architecture Brief §6.1, §6.4 |
| `LineHeight` variance discovered in M1 | Design Brief §3.3, Architecture Brief §7.1 |
| Chrome motion feels wrong in M2 | Design Brief §3.4, Interaction Brief §6 |
| Android `InputConnection` spike result | Architecture Brief §6.5, Interaction Brief §1.6 |
| User testing feedback after v1 | All three briefs, plus this file's v1.1 section |

After v1 ships, the briefs become a reference for v1.1+ contributions rather than a working specification. The "Unresolved Questions" sections should be empty or near-empty before v1.0 tag is cut.
