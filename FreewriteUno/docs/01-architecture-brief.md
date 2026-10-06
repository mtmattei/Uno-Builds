# FREEWRITE.UNO — Architecture Brief

Version 0.2 · 2026-05-20 · Author: M. Mattei
Spec: 1 of 3 (Architecture · Design · Interaction)
Target stack: Uno Platform, single project, three TFMs — `net9.0-windows10.0.19041`, `net9.0-android`, `net9.0-desktop`

---

## Context

Freewrite is a distraction-free writing surface ported from the macOS SwiftUI original to Uno Platform for Windows, Skia.Desktop, and Android. Writing-only in v1: video journaling, speech transcription, and the "Random font" feature are dropped. See Design Brief for the visual system and Interaction Brief for behaviour. This document defines the code shape.

The chassis is MVVM. Plain MVVM, with `CommunityToolkit.Mvvm` source generators (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`). No MVUX, no MVUX-style feeds, no reactive composition operators. The state surface is small enough that two-way bindings on a single ViewModel express the entire app cleanly.

---

## 1. App / Module Structure

Single Uno project, no class libraries, no abstraction-for-abstraction's-sake. The directory tree is the architecture:

```
FreewriteUno/
├── Assets/
│   └── Fonts/
│       ├── Lato-Regular.ttf
│       ├── Newsreader-Regular.ttf
│       └── JetBrainsMono-Regular.ttf
├── Models/
│   └── Entry.cs
├── Services/
│   ├── IEntryStore.cs
│   ├── EntryStore.cs
│   ├── IPdfExporter.cs
│   ├── QuestPdfExporter.cs
│   └── ISettingsStore.cs        (wraps ApplicationData.LocalSettings)
├── ViewModels/
│   └── MainViewModel.cs
├── Views/
│   ├── MainPage.xaml(.cs)
│   └── Converters/
├── Themes/
│   ├── ColorPaletteOverride.xaml
│   ├── TextBlock.xaml
│   └── TextBox.xaml
├── Platforms/
│   ├── Android/
│   │   ├── MainActivity.cs
│   │   ├── NoDeleteInputConnection.cs
│   │   └── BackspaceGuard.cs
│   ├── Desktop/
│   │   └── Program.cs
│   └── Windows/
│       └── App.xaml.cs
├── App.xaml(.cs)
├── GlobalUsings.cs
└── FreewriteUno.csproj
```

Modules and their responsibilities, written down so we can refuse to grow beyond them:

| Module | Responsibility | Notable non-responsibilities |
|---|---|---|
| `Models` | Entry record; nothing else | No business logic, no persistence concerns |
| `Services` | Disk I/O, PDF export, settings persistence | No UI, no view-aware types |
| `ViewModels` | All UI state, all commands, no `using Microsoft.UI.Xaml` | No file access, no platform code |
| `Views` | XAML + minimal code-behind for chrome animation and platform hooks | No business state, no I/O |
| `Themes` | Resource dictionaries — colours, brushes, text styles, control styles | No application logic |
| `Platforms` | Per-target hosts, native interop (Android `InputConnection`) | No business logic, no view markup |

If a new file doesn't have an obvious home in this list, that's a signal to refuse the feature, not to grow the architecture.

---

## 2. State Model

### 2.1 Single ViewModel

`MainViewModel` owns the entire UI state. There is no `MainPageViewModel`, no `SidebarViewModel`, no per-entry ViewModel. The page has one DataContext and one source of truth.

```csharp
public sealed partial class MainViewModel : ObservableObject
{
    // Observable state
    public ObservableCollection<Entry> Entries { get; } = new();

    [ObservableProperty] private Entry? _selectedEntry;
    [ObservableProperty] private string _text = "\n\n";
    [ObservableProperty] private int _timeRemaining = 900;
    [ObservableProperty] private bool _timerIsRunning;
    [ObservableProperty] private bool _backspaceDisabled;
    [ObservableProperty] private double _fontSize = 18;
    [ObservableProperty] private string _selectedFont = "Lato";
    [ObservableProperty] private ElementTheme _theme = ElementTheme.Light;
    [ObservableProperty] private bool _isSidebarOpen;
    [ObservableProperty] private double _chromeOpacity = 1.0;

    // Commands (RelayCommand source-generated)
    [RelayCommand] private Task NewEntryAsync() { ... }
    [RelayCommand] private void CycleFont() { ... }
    [RelayCommand] private void CycleSize() { ... }
    [RelayCommand] private void ToggleTimer() { ... }
    [RelayCommand] private void ResetTimer() { ... }
    [RelayCommand] private void ToggleBackspace() { ... }
    [RelayCommand] private void ToggleTheme() { ... }
    [RelayCommand] private void ToggleSidebar() { ... }
    [RelayCommand] private async Task CopyChatPromptAsync(string target) { ... }
    [RelayCommand] private async Task ExportPdfAsync() { ... }
}
```

### 2.2 What is and is not state

| State (in ViewModel) | Not state (computed or local) |
|---|---|
| Current entry text | Word count, character count, line count |
| Selected entry id | Whether the editor is focused |
| Timer remaining seconds | Wall-clock time |
| Timer running flag | Time elapsed since session start |
| Backspace lock toggle | Last keystroke timestamp |
| Theme (light / dark) | OS theme preference |
| Sidebar open flag | Sidebar scroll position |
| Chrome opacity (0 or 1) | Cursor position in the editor |

Things on the right are intentionally absent. Adding any of them is a feature decision that crosses the spec boundary.

### 2.3 Persistence

| State | Persisted to | Lifetime |
|---|---|---|
| Entries | Disk (`.md` files via `IEntryStore`) | Forever |
| Theme | `ISettingsStore` (LocalSettings) | Across sessions |
| Selected font, font size, timer duration, backspace lock | Not persisted | Session only |

Font/size/timer state is intentionally non-persistent. The macOS original behaves the same way and the philosophy is sound: each session starts at the defaults, and the writer adjusts only if they want to.

---

## 3. Navigation Model

There is no navigation. Specifically:

- No `Frame.Navigate` calls.
- No Uno Extensions `Region.Attached` / `Region.Name` markup.
- No `NavigationView`, `TabView`, or `Pivot`.
- No deep-linking, no URL-style routes.

`MainPage` is the only page. The history sidebar is a transient overlay on `SplitView` (`DisplayMode="Overlay"`, `PanePlacement="Right"`); selecting a different entry updates `SelectedEntry` on the ViewModel and closes the sidebar. There is nowhere else to go.

If this product ever requires navigation, that's a signal to question whether the new surface belongs in the product at all.

---

## 4. Services & Dependencies

### 4.1 Dependency injection

Standard `IHostBuilder` setup in `App.xaml.cs`:

```csharp
public sealed partial class App : Application
{
    public static IHost Host { get; private set; } = null!;

    public App()
    {
        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<IEntryStore, EntryStore>();
                services.AddSingleton<IPdfExporter, QuestPdfExporter>();
                services.AddSingleton<ISettingsStore, SettingsStore>();
                services.AddTransient<MainViewModel>();
            })
            .Build();
        InitializeComponent();
    }
}
```

`MainPage`'s code-behind resolves `MainViewModel` from `App.Host.Services`. No view-models are referenced from XAML via `x:Type`.

### 4.2 Service contracts

**`IEntryStore`** — the only service that touches the file system:

```csharp
public interface IEntryStore
{
    Task<IReadOnlyList<Entry>> LoadAllAsync(CancellationToken ct = default);
    Task<Entry>                CreateAsync(CancellationToken ct = default);
    Task<string>               ReadAsync(Entry entry, CancellationToken ct = default);
    Task                       WriteAsync(Entry entry, string content, CancellationToken ct = default);
    Task                       DeleteAsync(Entry entry, CancellationToken ct = default);
}
```

`EntryStore` writes plain `.md` files to `ApplicationData.Current.LocalFolder/Freewrite/` on all targets. UUID + timestamp filename, regex-extracted on read. Identical to the macOS original's convention for byte-level forward compatibility.

**`IPdfExporter`** — wraps QuestPDF, single implementation:

```csharp
public interface IPdfExporter
{
    Task<byte[]> RenderAsync(string content, string title, CancellationToken ct = default);
}
```

ViewModel hands the byte array to a platform-specific `FileSavePicker` invocation. The picker is in code-behind, not the service, because pickers are inherently view-coupled.

**`ISettingsStore`** — typed wrapper around `ApplicationData.Current.LocalSettings`:

```csharp
public interface ISettingsStore
{
    ElementTheme Theme { get; set; }
}
```

One property in v1. The wrapper exists so the ViewModel doesn't `using Windows.Storage` directly — that import on the ViewModel layer would leak across platforms in subtle ways.

### 4.3 No platform services on the interface boundary

`IEntryStore` returns `Entry` records and `string` content. It does not return `StorageFile`, `IStorageItem`, or any WinRT-coloured type. This keeps the ViewModel portable and the test surface easy.

---

## 5. Data Flow

The data flow is small enough to describe in one sequence diagram in prose:

1. User types in `TextBox`.
2. `TextBox.Text` two-way binding (`UpdateSourceTrigger=PropertyChanged`) updates `MainViewModel.Text`.
3. `OnTextChanged` partial method runs:
   - If the new value does not start with `\n\n`, rewrite to enforce the two-newline prefix and exit (the re-entrant `Text` set triggers `OnTextChanged` again with the corrected value).
   - Otherwise, schedule a debounced save.
4. `DebouncedSave(content)`:
   - Cancel any pending `CancellationTokenSource`.
   - Create a new one.
   - On a background task: `await Task.Delay(150, ct); await _entryStore.WriteAsync(entry, content, ct);`.
5. Subsequent keystrokes within the 150 ms window cancel the previous task and schedule a new one. Quiet periods of ≥ 150 ms flush to disk.

Switching entries forces a synchronous flush of the pending write before loading the new entry's content. No data loss is possible across rapid entry switches.

The same partial-method pattern enforces every other invariant:

- `OnSelectedEntryChanged` triggers `LoadAsync(value)`.
- `OnTimerIsRunningChanged` resets the 1-second fade timer.
- `OnThemeChanged` writes the new theme to `ISettingsStore`.

There is no `Task.Run` in the ViewModel except inside `DebouncedSave`. There is no `DispatcherQueue` marshalling because every entry point is already on the UI thread (binding setters, command invocations, partial methods).

---

## 6. Platform Constraints

### 6.1 Target framework matrix

| TFM | Head | UI framework | Notes |
|---|---|---|---|
| `net9.0-windows10.0.19041` | WinAppSDK | WinUI 3 | Primary developer-experience target |
| `net9.0-desktop` | Skia.Desktop | Uno Skia renderer | Single TFM covers Windows-without-WinAppSDK, Linux X11/Wayland, macOS |
| `net9.0-android` | Android | Native AndroidX views via Uno | API 26+ floor |

iOS, WebAssembly, and Catalyst are out of scope for v1. Adding them later is a TFM addition, not an architecture change.

### 6.2 Per-platform code

All platform-specific code lives in `Platforms/<TFM>/`. Shared code uses no `#if` preprocessor directives except where the conditional sits next to its cross-platform counterpart in a single file (e.g., a no-op `BackspaceGuard.Install` on non-Android, the real one on Android).

The four pieces of platform-specific code in v1:

| Platform | File | Concern |
|---|---|---|
| Android | `NoDeleteInputConnection.cs` | Subclass of `InputConnectionWrapper` that suppresses delete operations |
| Android | `BackspaceGuard.cs` | Hooks the wrapped `InputConnection` onto the native `AppCompatEditText` |
| Desktop | `Program.cs` | Skia.Desktop entry point with `appBuilder` config |
| Windows | `App.xaml.cs` | WinAppSDK launch + windowing |

### 6.3 File system

Storage is `ApplicationData.Current.LocalFolder/Freewrite/` on all three TFMs. On Android this is app-private; on Windows and Desktop it's a per-user app-data path. No platform permissions required. The Storage Access Framework (Android) and Documents picker (Windows/Desktop) are deferred to v1.1 behind the same `IEntryStore` interface.

### 6.4 Fonts

Bundled `.ttf` files in `Assets/Fonts/`, registered via WinUI's `FontFamily` URI scheme: `ms-appx:///Assets/Fonts/Lato-Regular.ttf#Lato`. Verified to load on:

- WinAppSDK: native support.
- Android: native support via the Uno font asset pipeline.
- Skia.Desktop: requires verification — the Skia text renderer may need explicit `SKTypeface` registration on first run. If this turns out to be a real constraint, the implementation lives in `Platforms/Desktop/FontRegistration.cs` as a static `Initialize()` call from `Program.Main`.

### 6.5 Android `InputConnection`

The backspace lock cannot be implemented purely from the WinUI `KeyDown` event on Android because modern IMEs (Gboard, Samsung Keyboard) route deletes through `InputConnection.deleteSurroundingText` and zero-length `commitText` rather than dispatching `KEYCODE_DEL`. The Android-specific `NoDeleteInputConnection` wraps the native `InputConnection`'s relevant methods and short-circuits them when the lock is engaged.

Hooking this into the Uno `TextBox`'s native `AppCompatEditText` requires either:

(a) A subclass of `AppCompatEditText` registered into the Uno control template on Android, overriding `OnCreateInputConnection`.
(b) A runtime modification of the `TextBox`'s native view tree from code-behind after `Loaded`.

Approach (a) is cleaner. Approach (b) is faster to prototype. M3 spike will determine which is feasible. If the spike runs past two days, the Android backspace lock toggle ships disabled in v1 with a tooltip, and the feature lands in v1.1.

---

## 7. Testing & Validation Approach

### 7.1 Unit tests

Single test project: `FreewriteUno.Tests` (xUnit, runs on `net9.0`).

**`EntryStoreTests`** — the highest-value tests, because `IEntryStore` is the only place we own non-trivial logic:

| Test | Asserts |
|---|---|
| `LoadAllAsync_EmptyDirectory_ReturnsEmpty` | Returns `[]` against a fresh temp folder |
| `CreateAsync_WritesFileWithExpectedNameFormat` | Regex match on filename + two-newline content |
| `LoadAllAsync_ReturnsEntriesNewestFirst` | Sort order from filename timestamps |
| `LoadAllAsync_IgnoresFilesThatDontMatchPattern` | A `.md` file with a non-UUID name is skipped, not crashed |
| `LoadAllAsync_HandlesUnreadableFile` | A file the regex matches but the OS can't read produces an empty preview, not an exception |
| `WriteAsync_OverwritesExistingFile` | Re-writing the same `Entry` replaces, not appends |
| `DeleteAsync_RemovesFile` | After delete, `LoadAllAsync` doesn't return it |
| `DeleteAsync_NonexistentFile_Succeeds` | Idempotent delete |
| `PreviewText_Is30CharsOrEllipsis` | Off-by-one defence on the preview slice |

Tests use `Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())` as the temp root. `EntryStore` accepts an injectable root path in a test-only constructor to avoid touching `ApplicationData`.

**`MainViewModelTests`** — selective tests for the state-machine bits:

| Test | Asserts |
|---|---|
| `OnTextChanged_LeadingNewlinesEnforced` | Setting `Text="hello"` produces `"\n\nhello"` |
| `OnTextChanged_RapidUpdates_DebouncedToSingleWrite` | 10 updates within 150 ms produce 1 `WriteAsync` call (use a fake `IEntryStore`) |
| `OnSelectedEntryChanged_FlushesPendingSave` | Switching entries while a save is debounced flushes synchronously before load |
| `InitializeAsync_NoEntries_CreatesWelcome` | Fresh start creates exactly one entry |
| `InitializeAsync_HasTodayEmpty_SelectsIt` | Existing empty entry from today is reselected, not duplicated |

These use an in-memory `FakeEntryStore` to avoid disk.

### 7.2 Manual verification

Unit tests do not cover what matters most for this product, which is "does it *feel* like freewrite?" The manual verification checklist runs against each TFM at the close of each milestone:

| Check | Pass criterion |
|---|---|
| Cold launch → cursor in editor | < 250 ms from window visible to cursor focused |
| Type for 30 seconds, force-quit app, relaunch | All typed text present in entry |
| Open file in OS file manager | File is readable plain Markdown |
| Start timer, wait 1 second | Chrome fades to invisible over ~240 ms |
| Hover bottom 96 px while timer running | Chrome returns over ~160 ms |
| Move mouse away from bottom edge | Chrome fades again after ~600 ms |
| Engage backspace lock, hit backspace | Keystroke is rejected (desktop + Android with shim) |
| Double-click timer | Resets to 15:00 |
| Scroll wheel over timer (desktop) | Adjusts ±5 min |
| Toggle theme | Surface and text colours transition over ~200 ms |
| Cycle font Lato → Newsreader → JetBrains Mono | Editor face changes, chrome face stays Lato |
| Open sidebar, select older entry | Editor loads that entry, sidebar closes |
| Send to Claude / ChatGPT | Browser tab opens or clipboard receives prompt for text > 6000 chars |
| Export PDF | File saved at user-chosen location; opens in OS PDF viewer with correct typography |

This checklist becomes a Markdown file in `/docs/verification.md` and is run by hand at the end of each milestone. We do not invest in UI automation for this product — the test surface is small and the human eye catches motion regressions that test runners cannot.

### 7.3 Hot Reload

XAML Hot Reload is the development feedback loop for every theme and layout change. Per Uno guidance, the app is started via the configured Hot Reload path:

```powershell
$env:DOTNET_MODIFIABLE_ASSEMBLIES = "debug"
Start-Job { dotnet run -f net9.0-windows10.0.19041 --project FreewriteUno.csproj }
```

Theme dictionary edits, text styles, and layout tweaks reload in < 1 second. ViewModel changes require a full build.

---

## Unresolved Questions

- **Android `InputConnection` subclassing approach.** Subclass-via-template (cleaner, undocumented) vs. runtime native-view modification (faster, fragile). Resolved in M3 spike. Fallback: Android lock ships disabled in v1, feature lands in v1.1.
- **Skia.Desktop font registration on Linux.** Whether `ms-appx:///Assets/Fonts/...` resolves to the right `SKTypeface` on X11 and Wayland without explicit registration. Verified in M0.
- **`TextBox.LineHeight` fidelity across heads.** WinUI `TextBox` partially honours `LineHeight`; older Android `EditText` does not. The 1.5× target may render differently across TFMs. Verified in M1. Fallback: swap `TextBox` for `RichEditBox` on heads where the variance is visible.
- **DI host on Skia.Desktop.** `Microsoft.Extensions.Hosting` works on `net9.0-desktop` but the WinAppSDK app activation path differs slightly. Verify the singleton resolution behaves the same way. M0.
- **QuestPDF font embedding.** Bundling Newsreader inside the rendered PDF (vs. referencing the system font) affects portability of the exported file. Lean toward embedding; verify file-size impact is < 200 KB.
