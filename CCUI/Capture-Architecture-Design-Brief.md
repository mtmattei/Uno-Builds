# Capture — Architecture & Design Brief

*A self-documenting camera reference sample in the spirit of Naoto Fukasawa*

| | |
|---|---|
| **Platform** | Uno Platform (WinUI 3 / .NET 9) |
| **Targets** | iOS, Android, Windows, macOS, WASM, Linux |
| **Pattern** | MVUX (Model-View-Update eXtended) |
| **Design System** | Material 3 — custom warm palette |
| **Sample Type** | Annotated Reference Sample with Learn Mode |
| **Version** | 1.1 — March 2026 |

---

## 1. Vision & Design Philosophy

### 1.1 Design Intent

Capture is a camera app designed in the spirit of Naoto Fukasawa's "Without Thought" philosophy — the idea that the best design dissolves into the user's natural behavior. The interface should feel like an extension of the hand, not a screen to be read.

Every interaction is reduced to its essential gesture. There are no menus, no settings panels, no feature toggles. One button opens the camera. One button captures. One link resets. The app does exactly one thing and does it with quiet confidence.

### 1.2 The Learning Surface

Capture is not just a sample app — it is a **self-documenting reference sample**. When Learn Mode is active, the UI itself becomes a teaching surface. Animated badges highlight which Uno Platform APIs and patterns are in play at each moment, and tapping a badge opens an annotation panel that explains the implementation, shows the C# code, and reveals what native API runs under the hood on each target platform.

The annotations are contextual: they change based on app state so developers only see what's relevant right now. The goal is to make Uno Platform's cross-platform abstraction visible and tangible — to answer the question "what's actually happening here?" at every step.

### 1.3 Core Principles

- **Essentialism** — Remove everything that isn't the act of capturing a moment
- **Material warmth** — Warm neutral tones (kraft, stone, linen) that feel organic, not digital
- **Tactile feedback** — Shutter button mimics the physical depression of a real camera button
- **Silent typography** — Ultra-light weight text that recedes behind the content
- **Temporal patina** — Captured photos carry a subtle date/time stamp like the back of a print
- **Learn by seeing** — The app teaches its own implementation through contextual annotations

### 1.4 Anti-Patterns

The following are explicitly out of scope for the app itself: gallery/history views, filters or editing tools, sharing integrations, settings screens, onboarding flows, camera mode switching. The app ships with one mode: capture a JPEG photograph. The annotation system is the only feature layer beyond core capture.

---

## 2. Technical Architecture

### 2.1 Project Structure

The app uses the Uno Platform recommended template with MVUX, Material theming, and Toolkit extensions:

```bash
dotnet new unoapp -preset=recommended -o Capture
```

Required UnoFeatures in the `.csproj`:

```xml
<UnoFeatures>
  Material;
  Toolkit;
  MVUX;
  Extensions;
  Hosting;
</UnoFeatures>
```

### 2.2 Solution Layout

| Path | Purpose |
|------|---------|
| `Models/CaptureModel.cs` | MVUX Model — `IState<ImageSource?>`, capture command, learn mode state |
| `Models/AnnotationDefinition.cs` | Record defining a single annotation (label, color, platforms, code) |
| `Models/AnnotationRegistry.cs` | Static registry of all annotations with per-state visibility rules |
| `Services/ICameraService.cs` | Camera abstraction interface |
| `Services/CameraService.cs` | CameraCaptureUI wrapper with platform guards |
| `Controls/AnnotationBadge.xaml` | Reusable badge UserControl — pulsing dot + monospace label |
| `Controls/AnnotationPanel.xaml` | Expanded detail panel — summary, platform selector, code snippet |
| `Controls/AnnotationOverlay.xaml` | Container UserControl that manages badge positioning + panel display |
| `Controls/LearnModeToggle.xaml` | Toggle pill in the top-right corner |
| `Controls/ShutterButton.xaml` | Custom-templated circular shutter button |
| `Presentation/MainPage.xaml` | Single-page view with three visual states + annotation overlay |
| `Presentation/MainPage.xaml.cs` | Code-behind: DataContext wiring only |
| `Styles/ColorPaletteOverride.xaml` | Warm neutral Material 3 color overrides |
| `Styles/AnnotationStyles.xaml` | Badge colors, panel theming, code block styles |
| `Styles/AppResources.xaml` | Custom brushes, typography, and control styles |
| `App.xaml` | Resource dictionary references and theme configuration |

### 2.3 MVUX Model

The model tracks both the captured image and the learn mode / annotation selection state. MVUX auto-generates the ViewModel and `IAsyncCommand` bindings.

```csharp
public partial record CaptureModel(ICameraService Camera)
{
    public IState<ImageSource?> CapturedImage =>
        State<ImageSource?>.Value(this, () => null);

    public IState<bool> IsLearnModeActive =>
        State<bool>.Value(this, () => true);

    public IState<string?> ActiveAnnotationKey =>
        State<string?>.Value(this, () => null);

    public IState<string> SelectedPlatform =>
        State<string>.Value(this, () => "windows");

    public async ValueTask CapturePhoto(CancellationToken ct)
    {
        var file = await Camera.CapturePhotoAsync(ct);
        if (file is not null)
        {
            var bitmapImage = new BitmapImage();
            using var stream = await file.OpenReadAsync();
            await bitmapImage.SetSourceAsync(stream);
            await CapturedImage.UpdateAsync(_ => bitmapImage, ct);
        }
        await ActiveAnnotationKey.UpdateAsync(_ => null, ct);
    }

    public async ValueTask Reset(CancellationToken ct)
    {
        await CapturedImage.UpdateAsync(_ => null, ct);
        await ActiveAnnotationKey.UpdateAsync(_ => null, ct);
    }

    public async ValueTask ToggleLearnMode(CancellationToken ct)
    {
        await IsLearnModeActive.UpdateAsync(v => !v, ct);
        await ActiveAnnotationKey.UpdateAsync(_ => null, ct);
    }

    public async ValueTask SelectAnnotation(string key, CancellationToken ct)
    {
        var current = await ActiveAnnotationKey;
        await ActiveAnnotationKey.UpdateAsync(
            _ => current == key ? null : key, ct);
    }

    public async ValueTask SelectPlatform(string platform, CancellationToken ct)
    {
        await SelectedPlatform.UpdateAsync(_ => platform, ct);
    }
}
```

### 2.4 Camera Service

`CameraCaptureUI` is supported on Android, iOS, and WinUI. On other platforms (WASM, Linux, macOS), `CaptureFileAsync` returns `null`. The service wraps this:

```csharp
public interface ICameraService
{
    Task<StorageFile?> CapturePhotoAsync(CancellationToken ct);
}

public class CameraService : ICameraService
{
    public async Task<StorageFile?> CapturePhotoAsync(CancellationToken ct)
    {
#if __ANDROID__ || __IOS__ || __WINDOWS__
        var captureUI = new CameraCaptureUI();
        captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
        return await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
#else
        return null;
#endif
    }
}
```

### 2.5 Platform Permissions

| Platform | Required Configuration |
|----------|----------------------|
| **Android** | `android.permission.CAMERA` and `android.permission.WRITE_EXTERNAL_STORAGE` in assembly attributes |
| **iOS** | `NSCameraUsageDescription` in `Info.plist` — iOS simulator falls back to Photo Library |
| **Windows** | No additional config — `CameraCaptureUI` is native WinUI |
| **WASM / Linux** | `CaptureFileAsync` returns `null` — show graceful fallback UI |

---

## 3. Annotation System Architecture

This is the core educational layer that transforms the sample from a demo into a learning surface.

### 3.1 Annotation Data Model

Each annotation is a self-contained record describing one API concept. The registry holds all annotations and knows which ones are visible in each app state.

```csharp
public record PlatformDetail(
    string ApiName,
    string Note
);

public record AnnotationDefinition(
    string Key,              // e.g. "cameraCaptureUI"
    string Label,            // e.g. "CameraCaptureUI"
    string IconGlyph,        // FontIcon glyph
    Color BadgeColor,        // Badge background
    string Summary,          // Plain-English description
    string CodeSnippet,      // C# implementation
    ImmutableDictionary<string, PlatformDetail> Platforms
);
```

```csharp
public static class AnnotationRegistry
{
    public static ImmutableList<AnnotationDefinition> All { get; } = [
        new("cameraCaptureUI",
            Label: "CameraCaptureUI",
            IconGlyph: "\uE722",  // Camera glyph
            BadgeColor: Color.FromArgb(255, 196, 85, 61),
            Summary: "Triggers the native camera capture experience",
            CodeSnippet: """
                var captureUI = new CameraCaptureUI();
                captureUI.PhotoSettings.Format =
                    CameraCaptureUIPhotoFormat.Jpeg;
                var file = await captureUI
                    .CaptureFileAsync(CameraCaptureUIMode.Photo);
                """,
            Platforms: new Dictionary<string, PlatformDetail>
            {
                ["windows"] = new("CameraCaptureUI",
                    "Native WinUI camera dialog"),
                ["android"] = new("CameraCaptureUI → Intent",
                    "Wraps native Camera intent. Requires android.permission.CAMERA"),
                ["ios"] = new("UIImagePickerController",
                    "Native picker. Requires NSCameraUsageDescription in Info.plist"),
                ["wasm"] = new("Not supported",
                    "CaptureFileAsync returns null — show graceful fallback"),
            }.ToImmutableDictionary()),
        new("bitmapImage", ...),
        new("istate", ...),
        new("autoLayout", ...),
    ];

    /// <summary>
    /// Returns which annotation keys are visible for each app state.
    /// Developers only see what's relevant right now.
    /// </summary>
    public static ImmutableList<string> VisibleKeys(string appState) =>
        appState switch
        {
            "idle"     => ["autoLayout", "istate"],
            "live"     => ["cameraCaptureUI", "autoLayout"],
            "captured" => ["bitmapImage", "istate", "cameraCaptureUI"],
            _          => [],
        };
}
```

### 3.2 The Four Annotations

| Key | Badge Label | Color | Visible In | What It Teaches |
|-----|------------|-------|------------|-----------------|
| `cameraCaptureUI` | `CameraCaptureUI` | Terracotta `#C4553D` | Live, Captured | How `CameraCaptureUI` maps to native camera APIs per platform |
| `bitmapImage` | `BitmapImage` | Sage `#4A7C6F` | Captured | How `BitmapImage.SetSourceAsync` displays the JPEG from a `StorageFile` stream |
| `istate` | `IState<T>` | Gold `#8B6914` | Idle, Captured | How MVUX `IState<ImageSource?>` drives visual state transitions reactively |
| `autoLayout` | `AutoLayout` | Plum `#6B5B95` | Idle, Live | How Uno Toolkit's `AutoLayout` + `Responsive` markup extension handle layout |

### 3.3 AnnotationBadge Control

A reusable `UserControl` that renders as a colored pill with a pulsing ring animation and a monospace label. Positioned absolutely via `Canvas.Left` / `Canvas.Top` on the overlay.

**Key implementation details:**

- The badge uses a `Storyboard` with a looping `DoubleAnimation` on a `Border`'s `Opacity` (0 → 0.3 → 0, 2.5s period) to create the pulsing ring effect
- An entrance animation (`DoubleAnimation` on `Opacity` 0→1 + `TranslateTransform.Y` 8→0, 500ms, `CubicEase`) fires each time the badge becomes visible via `VisualStateManager`
- `Command="{Binding SelectAnnotation}"` with `CommandParameter` set to the annotation key
- The badge is `Collapsed` by default and becomes `Visible` when its key is in `AnnotationRegistry.VisibleKeys(currentState)`

```xml
<UserControl x:Class="Capture.Controls.AnnotationBadge"
             xmlns:utu="using:Uno.Toolkit.UI">

  <Grid x:Name="Root" Opacity="0">
    <!-- Pulse ring -->
    <Border x:Name="PulseRing"
            CornerRadius="14"
            BorderThickness="2"
            BorderBrush="{x:Bind BadgeColor}"
            Opacity="0"
            Margin="-3" />

    <!-- Badge pill -->
    <Border CornerRadius="12"
            Background="{x:Bind BadgeColor}"
            Padding="8,5,12,5">
      <StackPanel Orientation="Horizontal" Spacing="6">
        <Border CornerRadius="9"
                Width="18" Height="18"
                Background="#33FFFFFF">
          <FontIcon Glyph="{x:Bind IconGlyph}"
                    FontSize="10"
                    Foreground="White" />
        </Border>
        <TextBlock Text="{x:Bind Label}"
                   Style="{StaticResource LabelSmall}"
                   Foreground="White"
                   FontFamily="DM Mono" />
      </StackPanel>
    </Border>

    <Grid.Resources>
      <Storyboard x:Name="PulseAnimation" RepeatBehavior="Forever">
        <DoubleAnimation Storyboard.TargetName="PulseRing"
                         Storyboard.TargetProperty="Opacity"
                         From="0" To="0.3"
                         Duration="0:0:1.25"
                         AutoReverse="True" />
      </Storyboard>
      <Storyboard x:Name="EntranceAnimation">
        <DoubleAnimation Storyboard.TargetName="Root"
                         Storyboard.TargetProperty="Opacity"
                         From="0" To="1"
                         Duration="0:0:0.5">
          <DoubleAnimation.EasingFunction>
            <CubicEase EasingMode="EaseOut" />
          </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
      </Storyboard>
    </Grid.Resources>
  </Grid>
</UserControl>
```

### 3.4 AnnotationPanel Control

The expanded detail panel that opens when a badge is tapped. It is a `UserControl` rendered inside a `Popup` or as an overlay `Border` positioned adjacent to the tapped badge.

**Structure:**

```
┌─────────────────────────────────────┐
│ ● CameraCaptureUI               ✕  │  ← Header: colored dot + label + close
├─────────────────────────────────────┤
│ Triggers the native camera capture  │  ← Summary text (BodyMedium)
│ experience                          │
│                                     │
│ ┌─Win─┬─And─┬─iOS─┬─WASM──────┐   │  ← Platform selector (segmented)
│                                     │
│ ┌───────────────────────────────┐   │
│ │ CameraCaptureUI → Intent     │   │  ← Platform-specific API name
│ │ Wraps native Camera intent.  │   │  ← Platform-specific note
│ │ Requires android.permission  │   │
│ └───────────────────────────────┘   │
│                                     │
│ IMPLEMENTATION                      │  ← Code label (LabelSmall)
│ ┌───────────────────────────────┐   │
│ │ var captureUI = new ...      │   │  ← Code block (dark bg, mono)
│ │ captureUI.PhotoSettings ...  │   │
│ └───────────────────────────────┘   │
└─────────────────────────────────────┘
```

**Key implementation details:**

- Background uses `AcrylicBrush` or fallback `SolidColorBrush` with the warm overlay color `#EBF5F2ED`
- Platform selector is a horizontal `ItemsRepeater` of styled `ToggleButton` controls bound to `SelectedPlatform`
- Code block is a `Border` with `Background="#FF2C2926"` containing a `TextBlock` with `FontFamily="Consolas"` / `DM Mono`
- Entrance animation: `DoubleAnimation` on `Opacity` 0→1 + `TranslateTransform.Y` 8→0, 350ms
- Panel dismisses on outside tap via `Popup.IsLightDismissEnabled="True"` or by binding `Visibility` to `ActiveAnnotationKey`

### 3.5 AnnotationOverlay Control

The container `UserControl` placed as the final child of the main `Grid` on `MainPage`. It manages all badge positioning and panel display.

```xml
<UserControl x:Class="Capture.Controls.AnnotationOverlay">
  <Canvas x:Name="OverlayCanvas">

    <!-- Badges are positioned absolutely relative to the viewfinder -->
    <controls:AnnotationBadge
        x:Name="BadgeCameraCaptureUI"
        Canvas.Left="{x:Bind CameraBadgeLeft}"
        Canvas.Top="{x:Bind CameraBadgeTop}"
        Label="CameraCaptureUI"
        IconGlyph="&#xE722;"
        BadgeColor="#C4553D"
        Command="{Binding SelectAnnotation}"
        CommandParameter="cameraCaptureUI"
        Visibility="{x:Bind IsCameraVisible}" />

    <controls:AnnotationBadge
        x:Name="BadgeBitmapImage"
        Canvas.Left="{x:Bind ImageBadgeLeft}"
        Canvas.Top="{x:Bind ImageBadgeTop}"
        Label="BitmapImage"
        IconGlyph="&#xEB9F;"
        BadgeColor="#4A7C6F"
        Command="{Binding SelectAnnotation}"
        CommandParameter="bitmapImage"
        Visibility="{x:Bind IsImageVisible}" />

    <!-- ... additional badges ... -->

    <!-- Annotation detail panel -->
    <controls:AnnotationPanel
        x:Name="DetailPanel"
        Canvas.Left="{x:Bind PanelLeft}"
        Canvas.Top="{x:Bind PanelTop}"
        Annotation="{x:Bind ActiveAnnotation}"
        SelectedPlatform="{Binding SelectedPlatform, Mode=TwoWay}"
        Visibility="{x:Bind IsPanelVisible}" />

  </Canvas>
</UserControl>
```

**Badge positioning strategy:** Badges are placed at fixed offsets relative to the viewfinder's corners — shutter-related badges to the right, state/layout badges to the left. On narrow screens (< 600dp), badges collapse below the viewfinder in a horizontal row rather than floating to the sides.

### 3.6 Learn Mode Toggle

A `ToggleButton` styled as a minimal pill in the top-right corner of the page. Uses a dot indicator (terracotta when active, muted when off) and a text label.

```xml
<ToggleButton x:Name="LearnToggle"
              IsChecked="{Binding IsLearnModeActive, Mode=TwoWay}"
              Style="{StaticResource LearnModeToggleStyle}"
              HorizontalAlignment="Right"
              VerticalAlignment="Top"
              Margin="0,16,16,0">
  <StackPanel Orientation="Horizontal" Spacing="10">
    <Ellipse Width="8" Height="8"
             Fill="{x:Bind LearnDotColor}" />
    <TextBlock Text="{x:Bind LearnLabel}"
               Style="{StaticResource LabelSmall}" />
  </StackPanel>
</ToggleButton>
```

When Learn Mode is off, the `AnnotationOverlay` collapses entirely — the app becomes the clean Fukasawa experience with no educational chrome.

---

## 4. Visual Design System

### 4.1 Color Palette

The palette is derived from Fukasawa's material vocabulary — unbleached paper, warm stone, and aged linen. All colors are defined as Material 3 overrides in `ColorPaletteOverride.xaml`.

| Token | Value | Usage |
|-------|-------|-------|
| Background | `#F5F2ED` | Page background — warm off-white |
| Surface | `#EFECE6` | Viewfinder resting state, card surfaces |
| Border | `#E2DED6` | Dividers, empty-state rings, subtle outlines |
| OnSurface | `#2C2926` | Primary text, shutter button fill, icons |
| OnSurfaceVariant | `#8A8580` | Secondary text, hints, timestamps |
| Primary | `#4A7C6F` | Accent — success states, focus indicators |
| PrimaryContainer | `#E8F0EC` | Table headers, highlighted zones |

**Annotation-specific colors** (defined in `AnnotationStyles.xaml`):

| Token | Value | Usage |
|-------|-------|-------|
| BadgeCameraCaptureUI | `#C4553D` | Terracotta — camera/capture APIs |
| BadgeBitmapImage | `#4A7C6F` | Sage — image display APIs |
| BadgeIState | `#8B6914` | Gold — MVUX state management |
| BadgeAutoLayout | `#6B5B95` | Plum — layout and toolkit controls |
| AnnotationPanelBg | `#EBF5F2ED` | Semi-transparent warm overlay for panels |
| CodeBlockBg | `#FF2C2926` | Dark surface for code snippets |
| CodeBlockFg | `#FFE2DED6` | Muted warm text on dark code blocks |

### 4.2 Typography

All typography uses existing Material 3 TextBlock styles. No explicit font sizes or weights are set inline.

- **DisplaySmall** — App wordmark ("CAPTURE"), de-emphasized at 45% opacity
- **BodyMedium** — Hint text, secondary actions, annotation summaries
- **LabelSmall** — Timestamps, badge labels, code block headers
- **Monospace** — `Consolas` (Windows) / `DM Mono` (cross-platform fallback) for badge labels and code blocks

### 4.3 The Shutter Button

A custom-templated `Button`: 64dp outer ring (`Border` with `CornerRadius=32`) containing a 50dp filled circle. On press, both scale to 0.95 via `Storyboard`. No text — recognized by shape alone. Bound to the MVUX-generated `CapturePhoto` command.

### 4.4 Shutter Flash & Photo Reveal

When the capture command fires, a full-viewfinder white overlay flashes at 80% opacity for 140ms. The captured image fades in with a 550ms ease-out opacity animation + subtle 1.008× scale-down — the sensation of an instant print settling into place.

### 4.5 Badge Animations

Badges use two layered animations:

- **Pulse ring** — A `DoubleAnimation` on a surrounding `Border`'s `Opacity` (0 → 0.3 → 0), repeating forever at 2.5s period. Creates a subtle "breathing" ring in the badge's own color.
- **Entrance** — A one-shot `DoubleAnimation` on `Opacity` (0→1) + `TranslateTransform.Y` (8→0), 500ms with `CubicEase`. Fires when the badge transitions from `Collapsed` to `Visible` on state change. Each badge has a staggered `BeginTime` (0ms, 100ms, 200ms) for a cascading reveal.

---

## 5. View & Layout Specification

### 5.1 Visual States

The single `MainPage` has exactly three visual states. Each controls both the app UI and which annotation badges are visible.

| State | App UI | Annotation Badges |
|-------|--------|-------------------|
| **Idle** | Lens icon, hint text, "open camera" pill | `AutoLayout`, `IState<T>` |
| **Live** | Camera preview, shutter button | `CameraCaptureUI`, `AutoLayout` |
| **Captured** | Photo, timestamp, "take another" | `BitmapImage`, `IState<T>`, `CameraCaptureUI` |

### 5.2 Layout Structure

The page uses a single vertical `AutoLayout` centered on both axes, with the `AnnotationOverlay` layered on top via a wrapping `Grid`.

```
Grid (full page)
├── AutoLayout (centered content)
│   ├── TextBlock "CAPTURE" (wordmark)
│   ├── Border (viewfinder, 4:3)
│   │   ├── [Idle] Empty state (lens icon + hint)
│   │   ├── [Live] Camera preview
│   │   └── [Captured] Image + timestamp
│   └── StackPanel (controls)
│       ├── [Idle] "open camera" pill Button
│       ├── [Live] Shutter Button
│       └── [Captured] "take another" link Button
├── LearnModeToggle (top-right, fixed)
└── AnnotationOverlay (Canvas, full page)
    ├── AnnotationBadge × n (positioned around viewfinder)
    └── AnnotationPanel (detail popup)
```

### 5.3 Responsive Behavior

The viewfinder width adapts using the `Responsive` markup extension:
- **Narrow (0–599dp):** 90% of available width; badges reflow below the viewfinder in a horizontal row
- **Medium (600–904dp):** Cap at 480dp; badges float to left/right of viewfinder
- **Wide (905dp+):** Cap at 540dp; badges float with more generous spacing; annotation panel opens inline beside the viewfinder

### 5.4 Accessibility

- All interactive elements have `AutomationProperties.Name` via `x:Uid` localization keys
- Touch targets meet 44×44dp minimum — shutter button is 64dp, badges are ≥44dp tap targets
- Color contrast exceeds 4.5:1 for all text on the warm background
- Annotation badges include `AutomationProperties.Name` combining the label and summary (e.g., "CameraCaptureUI — Triggers the native camera capture experience")
- Learn mode toggle announces state change via `AutomationProperties.LiveSetting="Polite"`

---

## 6. XAML View Skeleton

```xml
<Page x:Class="Capture.Presentation.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:utu="using:Uno.Toolkit.UI"
      xmlns:controls="using:Capture.Controls"
      Background="{ThemeResource BackgroundBrush}">

  <Grid>
    <!-- ── Core app ── -->
    <utu:AutoLayout PrimaryAxisAlignment="Center"
                    CounterAxisAlignment="Center"
                    Spacing="0" Padding="24">

      <TextBlock Text="CAPTURE"
                 Style="{StaticResource DisplaySmall}"
                 Opacity="0.45" Margin="0,0,0,32"
                 x:Uid="MainPage_Wordmark" />

      <Border x:Name="Viewfinder"
              CornerRadius="5"
              Background="{ThemeResource SurfaceBrush}"
              Width="{utu:Responsive Narrow=340, Wide=540}"
              Translation="0,0,16">
        <Border.Shadow><ThemeShadow /></Border.Shadow>
        <!-- VisualStateManager swaps content here -->
      </Border>

      <StackPanel Spacing="14" Margin="0,36,0,0"
                  HorizontalAlignment="Center">
        <Button x:Name="OpenButton"
                Content="open camera"
                Command="{Binding CapturePhoto}"
                Style="{StaticResource CaptureOpenButtonStyle}" />
        <Button x:Name="ResetButton"
                Content="take another"
                Command="{Binding Reset}"
                Style="{StaticResource CaptureTextButtonStyle}" />
      </StackPanel>
    </utu:AutoLayout>

    <!-- ── Learn mode toggle ── -->
    <controls:LearnModeToggle
        HorizontalAlignment="Right"
        VerticalAlignment="Top"
        Margin="0,16,16,0"
        IsActive="{Binding IsLearnModeActive, Mode=TwoWay}" />

    <!-- ── Annotation overlay (Canvas) ── -->
    <controls:AnnotationOverlay
        Visibility="{Binding IsLearnModeActive}"
        ActiveKey="{Binding ActiveAnnotationKey, Mode=TwoWay}"
        SelectedPlatform="{Binding SelectedPlatform, Mode=TwoWay}"
        AppState="{Binding CurrentAppState}" />
  </Grid>
</Page>
```

---

## 7. Dependency Injection & Hosting

```csharp
private IHost Host { get; set; }

protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    var builder = this.CreateBuilder(args);

    builder.Services
        .AddSingleton<ICameraService, CameraService>()
        .AddTransient<CaptureModel>();

    Host = builder.Build();
}
```

---

## 8. Edge Cases & Platform Graceful Degradation

- **WASM / Linux / macOS:** `CaptureFileAsync` returns `null`. UI stays in Idle state with an alternate hint: "camera not available on this platform." The annotation system still works — developers can explore the badges and see what *would* happen on supported platforms.
- **iOS Simulator:** Falls back to Photo Library picker automatically. No code change needed.
- **Permission denied:** `CaptureFileAsync` returns `null`. UI stays in Idle — no retry loop, no permission rationale dialog.
- **CaptureFileAsync throws:** Wrapped in `try/catch` in the service. Returns `null`. The model treats `null` as "capture cancelled."
- **Learn mode on unsupported platform:** The annotation for `CameraCaptureUI` auto-selects the current platform via `DeviceInfo` and highlights the "Not supported" note, making the gap visible and educational rather than mysterious.

---

## 9. Reusability: The Annotation Pattern

The annotation system is designed to be extractable and reusable across any Uno Platform reference sample. The pattern consists of:

1. **`AnnotationDefinition`** — A portable data record (label, platforms, code, summary)
2. **`AnnotationRegistry`** — A static class mapping app states to visible annotations
3. **`AnnotationBadge`** — A reusable `UserControl` (pill + pulse animation)
4. **`AnnotationPanel`** — A reusable `UserControl` (detail popup with platform selector)
5. **`AnnotationOverlay`** — A `Canvas`-based container managing positioning and visibility

For a new reference sample, a developer only needs to define the `AnnotationDefinition` records and the state→visibility mapping. The controls, animations, and panel layout are fully reusable.

---

## 10. Implementation Checklist

1. Scaffold project with `dotnet new unoapp -preset=recommended -o Capture`
2. Add UnoFeatures: Material, Toolkit, MVUX, Extensions, Hosting
3. Create `ColorPaletteOverride.xaml` with warm neutral tokens
4. Create `AnnotationStyles.xaml` with badge colors and code block styles
5. Implement `ICameraService` and `CameraService` with platform guards
6. Build `AnnotationDefinition`, `PlatformDetail`, and `AnnotationRegistry`
7. Build `CaptureModel` with `IState<ImageSource?>`, learn mode states, and annotation selection
8. Create `AnnotationBadge.xaml` UserControl with pulse + entrance animations
9. Create `AnnotationPanel.xaml` UserControl with platform selector and code block
10. Create `AnnotationOverlay.xaml` with `Canvas` positioning and per-state badge visibility
11. Create `LearnModeToggle.xaml` styled as a minimal pill toggle
12. Create `ShutterButton.xaml` with tactile `ControlTemplate`
13. Assemble `MainPage.xaml` with `AutoLayout`, viewfinder, three visual states, and overlay
14. Add Android permissions and iOS `Info.plist` keys
15. Configure DI in `App.xaml.cs`
16. Test Learn Mode on each target — verify badges animate, panel opens, platform selector highlights the current target, and code snippets are accurate
17. Test App Mode on each target — verify all annotation chrome is fully hidden
18. Test on WASM specifically — verify graceful degradation messaging and that the annotation system still teaches even without a working camera

---

*— end of brief —*
