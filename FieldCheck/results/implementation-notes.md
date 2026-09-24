# FieldCheck — Uno Platform implementation notes

## Layout

```
FieldCheck/                     benchmark run folder (copied from 01-uno/)
  app/FieldCheck/               Uno solution (dotnet new unoapp, Uno.Templates 6.7.30)
    FieldCheck/                 single-project app: net10.0-android; net10.0-windows10.0.26100; net10.0-desktop; net10.0
      Models/                   Asset, Inspection, InspectionDraft, PickedFile (records)
      Services/                 IFieldCheckRepository, JsonFieldCheckRepository, DataModeRepository,
                                INavigator + FrameNavigator, IFilePickerService + FilePickerService, seed source
      ViewModels/               Dashboard, Assets, AssetDetail, NewInspection, InspectionSuccess, History, Shell
      Views/                    ShellPage + 6 pages + AssetDetailView (shared by phone page and wide detail pane)
      Controls/                 StatusChip, StatePanel (loading/empty/error), StatusVisuals (x:Bind helpers)
      Themes/FieldCheck.xaml    design tokens, type styles, button/chip/segment/list-row templates, lightweight styling
    FieldCheck.Tests/           NUnit tests against the net10.0 head (repository + view models)
  ci/                           Android (adb/uiautomator) and Windows (UIA) end-to-end drivers
  results/                      benchmark outputs; results/ci/run-N-*/ are CI-committed verification outputs
.github/workflows/fieldcheck.yml  unit tests, Android build + emulator e2e, Windows build + UIA e2e, publish
```

## Architecture decisions

- **MVVM with CommunityToolkit.Mvvm** (benchmark mandate). View models hold all state, validation and commands;
  code-behind is limited to resolving the VM, `OnNavigatedTo` parameter hand-off, `ItemClick` forwarding and
  master/detail width detection.
- **Frame-based navigation behind `INavigator`** instead of Uno.Extensions region navigation.
  Reason: the app has six screens and one adaptive rule (master/detail ≥ 1000 px); a small navigator gives exact
  control over back-stack edits (success page removes the form from the stack; section switches clear it).
  Tradeoff: no route registry/deep links, which the spec does not require.
- **Composition root with Microsoft.Extensions.DependencyInjection** (already referenced by the Uno template).
  Section VMs are singletons so search/filter state survives back navigation; detail/form/success VMs are transient.
- **Persistence**: JSON files in `LocalApplicationData/FieldCheck` (Android app-private files dir, `%LOCALAPPDATA%` on
  Windows, `~/.local/share` on Linux desktop). First run seeds from the benchmark fixtures, which are embedded
  read-only (`EmbeddedResource` links to `mock-data/*.json`, never written). Writes go to a temp file then an atomic
  replace; in-memory state commits only after both files are written. A save updates the asset's status and last
  inspection date.
- **Inspection IDs**: `INS-{max existing number + 1}` → `INS-24092` for the first new record; unique and stable.
- **Deterministic state modes** (`DataModeRepository` decorator): `slow`, `empty`, `error`, `error-once`, `save-error`.
  Selected only at launch via `FIELDCHECK_DATA_MODE`, `--data-mode=` or the Android intent extra `data_mode`.
  Never visible in the UI.
- **Duplicate submission**: `AsyncRelayCommand` rejects re-entry, `CanExecute` is false while `IsSubmitting`, and the
  method guards again. Verified by unit test and by a double-tap/double-invoke on both platforms.
- **File picker**: `Windows.Storage.Pickers.FileOpenPicker` (SAF on Android; Win32 dialog on Windows with
  `InitializeWithWindow`). The chosen file is copied into app storage on save. Cancel returns null and keeps the
  previous choice; picker exceptions show an inline message and leave the form usable.
- **Responsive**: `AdaptiveTrigger`s — < 720 px phone layout with bottom navigation; ≥ 720 px sidebar; ≥ 1000 px Assets
  master/detail; ≥ 1100 px two-column inspection form and History table. Windows sets a 760 × 560 minimum window
  size so the sidebar is always present there.
- **Typography**: `UnoDefaultFont=None` so Android renders Roboto and Windows Segoe UI (spec: system sans, no bundled
  font).

## Uno-specific findings during the run

- The XAML generator (Uno.Sdk 6.7.30) failed with `CS1061 ... _ContentSubject` when a `VisualStateManager` in a
  ControlTemplate targeted a part declared *after* it (and a part named `Content`). Fixed by renaming the part and
  declaring the VSM after the targets.
- Passing `-p:TargetFrameworks=...` globally made the NUnit test project multi-targeted and dropped the adapter's
  props (tests silently not discovered). Replaced with an app-only `OverrideTargetFrameworks` property.
- `UserControl.Padding` is not applied (no template); used `Margin`.
- Grid `ColumnSpacing` still applies around zero-width columns; spacing is set only in the wide visual state.
- Template `NUnit3TestAdapter 4.5.0` is not discovered by VSTest 18; bumped test packages.
- **WinUI vs Uno difference**: `VisualStateManager.VisualStateGroups` attached to the `Page` itself worked on Uno
  (Android/Desktop) but is ignored by WinUI, so Windows stayed in the phone layout. Groups now live on each page's
  root Grid. Found by the Windows CI screenshots.
- **Android Skia accessibility**: with `ListView`, only the first realized `ListViewItem` appeared in the
  uiautomator/TalkBack tree. Rows are now `Button`s in an `ItemsControl` (12 items; no virtualization needed) so
  every row is a named, focusable element on all heads. Master/detail selection moved to `AssetRow.IsSelected`.
- **Hit-testing defect**: the empty list's `ScrollViewer` was layered above the "No matching assets" panel and
  swallowed taps on "Clear search". UIA `Invoke` on Windows bypasses hit-testing, so only real pointer input
  (Android taps, Desktop clicks) exposed it. Fixed by z-ordering; reproduced and verified on the Desktop head.
- A 1000–1280 px Windows width left the detail pane ~330 px wide and squeezed the title to one letter per line;
  the detail header now stacks the Start button under the title below 560 px of pane width and the master column
  is 380 px until 1280 px.

## Environment constraints (declared asymmetry)

- The measured session ran in a Linux cloud container: no Android SDK (dl.google.com blocked by network policy),
  no KVM, no Windows. Android and Windows builds, launches and UI verification therefore ran on GitHub-hosted
  runners (`ubuntu-latest` + KVM emulator, `windows-latest`) triggered by pushes from this session.
- Artifact downloads (`*.blob.core.windows.net`) are blocked, so the workflow commits its outputs to
  `results/ci/run-N-<scenario>/` on the branch.
- The Desktop head (X11 under Xvfb) was the only locally runnable target and served as the quick visual preview
  during core work; Desktop-specific verification is reported separately after `core_complete`.
