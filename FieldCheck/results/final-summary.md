# FieldCheck — Final summary (Uno Platform run)

## Completion status

**COMPLETE** — every mandatory acceptance criterion passes on the final verification run for Android and Windows.
The Uno Desktop head also passes; it was measured separately after the core checkpoint.

| | Result |
|---|---|
| Acceptance (results/acceptance.json) | **96 PASS · 0 FAIL · 0 NOT TESTED** (96 criteria incl. A08 Desktop) |
| Final evidence run | `results/ci/run-27-full/` (source commit `e26bec1`) |
| Android end-to-end driver | 53 / 53 checks passed (emulator API 34, pixel_6, Release APK) |
| Windows end-to-end driver | 36 / 36 checks passed (windows-latest, Release, unpackaged WinAppSDK, 1440×900 client) |
| Desktop head end-to-end driver | 36 / 36 checks passed (Skia renderer, Win32 host, Release) + Linux X11 head verified locally |
| Unit tests | 47 / 47 passed (NUnit, net10.0) |
| Release build warnings | Android 0 · Windows 0 · Desktop 0 (0 errors) |

## Framework and tool versions

- .NET SDK 10.0.112 (local container); 10.0.401 on the GitHub-hosted runners (`setup-dotnet 10.0.x`).
- **Uno.Sdk 6.7.30** (latest stable at run start), Uno.WinUI 6.7.135, Uno.Toolkit 9.1.3, CommunityToolkit.Mvvm 8.4.2.
- Android: Microsoft.Android.Sdk 36.1.69 (net10.0-android), JDK 21, emulator API 34 google_apis x86_64.
- Windows: Windows App SDK 1.7.250909003, TFM net10.0-windows10.0.26100, unpackaged.
- Tests: NUnit 4.6.1, NUnit3TestAdapter 5.2.0, Microsoft.NET.Test.Sdk 18.10.1, FluentAssertions 6.12.0.
- Full inventory: `results/dependencies.txt`, `results/environment.json`.

## Timing (from results/timing.jsonl, UTC)

| Milestone | Timestamp | Elapsed from start |
|---|---|---|
| run_start | 2026-09-23 19:56:33 | 0:00 |
| capability_discovery_complete | 19:59:55 | 0:03 |
| first_source_edit | 20:00:10 | 0:04 |
| first_build_attempt | 20:00:13 | 0:04 |
| first_windows_build_success (CI) | 20:27:31 | 0:31 |
| first_android_build_success (CI) | 20:27:31 | 0:31 |
| first_android_launch_success (CI emulator) | 20:30:13 | 0:34 |
| first_windows_launch_success (CI runner) | 21:00:22 | 1:04 |
| verification_start | 22:38:48 | 2:42 |
| **core_complete** (Android + Windows) | **23:27:50** | **3:31** |
| **desktop_head_complete** | **2026-09-24 00:01:33** | **4:05** |
| run_finish | 2026-09-24 01:09:40 | 5:13 |

Notes:
- The Windows and Android build milestones were marked when the CI job result was observed. The actual
  step completion times were 20:26:37Z (Windows) and 20:27:02Z (Android).
- The Windows launch milestone was marked late (21:00). The first Windows launch actually happened in CI run 3
  at about 20:31–20:33Z, per the job's UI step.
- **Uno Desktop incremental (K06)**: 33 min 43 s from `core_complete` to `desktop_head_complete`. It needed no app
  code changes: a CI job for the Win32 Desktop head, one driver adjustment for the Win32 modal file dialog, and
  local verification of the Linux X11 head with an XDG portal.
- **Post-checkpoint work** (after `core_complete`, excluded from the Desktop metric where it served the core targets):
  - The test-only `slow` repository delay was raised from 4 s to 6 s.
  - Android/Windows driver robustness fixes: iterative window sizing, date-independent assertion, frame-based loading
    detection.
  - The final regression run was re-verified on all three targets.

## Build attempts

- **Local** (Linux container; Desktop head and net10.0 tests only):
  - 1 failed restore — `NETSDK1147`: the Android workload was missing from the Android TFM.
  - 2 failed XAML builds (`CS1061 _ContentSubject`: a template part named `Content`, and a VSM targeting a later part).
  - Many successful incremental Debug builds while iterating (not counted individually).
- **CI**: 27 workflow runs (4 cancelled by the concurrency group when superseded).
  - Failed build steps: 1 Android (run 1, invalid `-warnaserror-` switch) and 1 Windows (run 1).
  - Every later Android, Windows and Desktop Release build succeeded.

## Runtime defects found during verification (K10) — all fixed

| # | Category | Defect | Found by | Fix |
|---|---|---|---|---|
| 1 | Layout / platform | Adaptive `VisualStateGroups` attached to `Page` were ignored by WinUI, so Windows showed the phone layout | Windows CI screenshots | Moved to each page's root Grid |
| 2 | Layout | At 1000–1280 px the detail header squeezed the title to one letter per line | Windows CI screenshot (1028 px) | Compact header below 560 px pane width; 380 px master under 1280 px |
| 3 | Accessibility | Android Skia exposed only the first `ListViewItem` of a list to accessibility | Android uiautomator dumps | Rows rendered as named Buttons in an ItemsControl |
| 4 | Input / hit-testing | "Clear search" never received taps: the empty list's ScrollViewer overlaid the no-results panel (UIA Invoke on Windows hid it) | Android taps, reproduced on Desktop | Z-order fix |
| 5 | Focus / IME | Returning to Assets/History focused the search box and opened the Android keyboard unasked | Android screenshots (gesture-typed "by by by") | Pages take focus after navigation |
| 6 | Visual | Search/chip clipping at 412 px, form action spacing, attach width, detail padding, accelerator tooltip | Local Desktop preview | Styling fixes |

The first few Android and Windows CI failures were test-driver issues, not app defects:
- emulator ANR dialogs;
- the `pm clear` race;
- regexes anchored at the start of the name;
- a tap landing in the gesture-navigation strip;
- the Photo Picker's display names.

They are recorded in the commit history.

## Tests executed

- **Unit tests (47)**:
  - repository seeding, persistence across instances, ID generation and uniqueness, fixture immutability (SHA-256),
    attachment copy, write-failure atomicity, corrupt-data error, deterministic data modes;
  - view models: dashboard counts and states, asset search/filter/combination/no-results, master/detail selection,
    history order/search/filter, form validation (temperature bounds, checklist, conditional issue description),
    duplicate submission, save failure, cancel, picker select/cancel/failure, detail refresh, success navigation,
    formatting.
- **Android end-to-end (emulator, Release APK)**:
  - navigation, search/filters, detail, back stack, cancel, validation, keyboard, Photo Picker cancel/select,
    submit with a double tap;
  - success → View asset → History, persistence after `am force-stop`, background/resume, 1.3× font scale;
  - empty/error/retry/loading/save-error states.
- **Windows end-to-end (UIA, Release)**:
  - sidebar, master/detail at 1440×900, search/filters, Tab order, form validation, Win32 file dialog
    cancel/select/remove, submit with a double invoke, History;
  - responsive widths 1100/900/760, kill + relaunch persistence, minimize/restore, 130 % text scale;
  - empty/error/retry/loading/save-error states.
- **Desktop head**: the same UIA suite against the Skia Win32 host. The Linux X11 head was checked locally:
  portal file picker, submit, and `kill -9` + relaunch persistence.

## Screenshots

- `results/screenshots/android/` — 01-dashboard, 02-assets, 03-asset-detail, 04-new-inspection, 05-inspection-success,
  06-history, plus state, keyboard, picker and font-scale captures.
- `results/screenshots/windows/` — 01-dashboard, 02-assets-master-detail, 03-new-inspection, 04-inspection-success,
  05-history, plus responsive, focus, state and text-scale captures.
- `results/screenshots/desktop-win32/` and `results/screenshots/desktop-linux/` — Desktop head captures.
- Visual comparison: `results/visual-review.md`.

## Architecture summary

- Uno single project, MVVM with CommunityToolkit.Mvvm and a Microsoft.Extensions.DependencyInjection composition root.
- Frame-based `INavigator` owned by the shell:
  - bottom navigation below 720 px, persistent sidebar at 720 px and above;
  - master/detail from 1000 px.
- `IFieldCheckRepository` → `JsonFieldCheckRepository` (app-local JSON; seeded from the embedded read-only fixtures;
  atomic writes), wrapped by `DataModeRepository` for the deterministic slow/empty/error/error-once/save-error modes.
  Modes are chosen at launch only (environment variable, command-line argument, or Android intent extra).
- `IFilePickerService` → `FileOpenPicker`: Android system picker, WinAppSDK Win32 dialog with `InitializeWithWindow`,
  Win32 dialog on the Desktop head, XDG portal on X11.
- Views: x:Bind throughout, custom styles from the design tokens (`Themes/FieldCheck.xaml`),
  `StatusChip`/`StatePanel` controls.
- Details: `results/implementation-notes.md`.

## Tools, MCP servers and skills used

- **Uno Platform docs MCP** (`uno_platform_agent_rules_init`, `uno_platform_usage_rules_init`, docs search/fetch):
  default font behaviour, Skia accessibility architecture, TextBox/IME investigation.
- **Skill `uno-scaffolding`**: template choice and runtime gotchas (UseStudio, FileOpenPicker, pill radius, Android IME).
- **GitHub MCP**: workflow runs, jobs and job logs.
- **Uno Templates** (`dotnet new unoapp`); `ilspycmd` to read the Uno Android Skia text-input implementation.
- **Runtime verification**:
  - Xvfb, xdotool and ImageMagick (Desktop head);
  - adb/uiautomator on a KVM emulator (reactivecircus/android-emulator-runner);
  - pywinauto UIA and PrintWindow (Windows).
- **Not available**: the Uno App MCP (DevServer runtime tools). It is not configured in this cloud session and needs
  a signed-in Uno account.

## Environment constraints (declared asymmetry)

- The session ran in a Linux cloud container. Its network policy blocked `dl.google.com` (Android SDK),
  `dotnetcli.azureedge.net` and `*.blob.core.windows.net` (Actions artifacts), and there was no KVM and no Windows.
- Android and Windows builds, launches and UI verification therefore ran on GitHub-hosted runners, triggered by
  pushes from the session. CI outputs were committed back to the branch (`results/ci/`) because artifact downloads
  were blocked. This adds CI queue and wall time that a local Windows machine with an emulator would not have.

## Token usage / cost

Not available from inside the session. Claude Code does not expose session token or cost telemetry to the agent here,
so no values are estimated. **Attach the external session usage export as `results/external-session-usage.txt`**
(see METRICS_CAPTURE.md). No core-checkpoint usage snapshot could be taken, so the Desktop incremental tokens/cost
must be derived from external telemetry if it has timestamps, or recorded as unavailable.

## Known remaining defects

None blocking. Observations:
- Uno Skia Android omits *disabled* buttons from the accessibility tree, so a disabled Submit is not announced.
  The adjacent "To submit: …" summary explains what is missing.
- The Attention chip text on its soft fill is 4.32:1 contrast, a pair taken directly from the supplied palette.
- On Linux the Desktop picker needs an XDG desktop portal. Without one (headless, or a bare window manager) the
  picker returns nothing and the form stays usable.
