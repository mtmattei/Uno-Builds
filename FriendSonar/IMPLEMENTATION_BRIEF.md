# FriendSonar Feature Implementation Brief

## Codebase Analysis Summary

**Current State:**
- Single-page Uno Platform app with radar/sonar UI
- Canvas-based radar with rotating sweep animation (DispatcherTimer, 30ms/2deg)
- 5 sample friends with polar coordinate positioning (distance 0-1, angle 0-359)
- Status-based coloring (Active/Idle/Away)
- CRT overlay effect with scanlines
- Material theme with phosphor green palette
- UnoFeatures: Material, Toolkit, Hosting, Mvvm, ThemeService, SkiaRenderer

---

## Feature Mapping & Implementation Order

### Phase 1: Core Radar Enhancements (Foundation)

| # | Feature | Uno Components/Patterns | Priority |
|---|---------|------------------------|----------|
| 1.1 | **Bearing Numbers** | Canvas TextBlocks at 0/90/180/270 positions | Quick Win |
| 1.2 | **Sweep Line Enhancement** | Additional Path elements with opacity gradients, phosphor persistence | Visual |
| 1.3 | **Improved Blip Design** | Hollow ring + inner dot using nested Ellipses | Visual |
| 1.4 | **Animated Status Indicators** | Extended Storyboards per status type | Visual |

### Phase 2: Interaction & Feedback

| # | Feature | Uno Components/Patterns | Priority |
|---|---------|------------------------|----------|
| 2.1 | **Friend Detail Panel** | Toolkit DrawerControl or custom slide-up Border with TranslateTransform | Core |
| 2.2 | **Haptic Feedback** | Platform-specific Vibration APIs via conditional compilation | UX |
| 2.3 | **Long-press Blip Actions** | PointerPressed + DispatcherTimer for hold detection, MenuFlyout | UX |
| 2.4 | **Double-tap Radar Center** | DoubleTapped event, zoom-to-fit calculation | Quick Win |

### Phase 3: Animation Polish

| # | Feature | Uno Components/Patterns | Priority |
|---|---------|------------------------|----------|
| 3.1 | **Boot-up Animation** | Storyboard sequence: rings draw in, sweep starts, blips fade in | Visual |
| 3.2 | **Pull-to-Refresh Scan** | RefreshContainer or custom gesture, full-scan animation | UX |
| 3.3 | **Ping Animation Enhancement** | Multiple expanding rings, brightness flash, optional sound | Visual |
| 3.4 | **Ghost Blips / History Trail** | Collection of faded Ellipses tracking previous positions | Visual |

### Phase 4: Advanced Radar Features

| # | Feature | Uno Components/Patterns | Priority |
|---|---------|------------------------|----------|
| 4.1 | **Range Zoom Control** | Slider or ToggleButtons (1mi/5mi/10mi), ScaleTransform on radar | Core |
| 4.2 | **Compass Bearing Mode** | RotateTransform on entire radar, N/S/E/W labels | Core |
| 4.3 | **Live Location Simulation** | Enhanced movement interpolation, direction trails | Visual |
| 4.4 | **Proximity Alerts** | Configurable perimeter ring, distance threshold checking | Core |

### Phase 5: Contact List & Groups

| # | Feature | Uno Components/Patterns | Priority |
|---|---------|------------------------|----------|
| 5.1 | **Contact List Polish** | SwipeControl for actions, sort ComboBox, search AutoSuggestBox | UX |
| 5.2 | **Friend Groups / Squads** | Group property on Friend model, CollectionViewSource grouping | Core |
| 5.3 | **ETA Calculation** | Velocity tracking, distance/speed calculation, display formatting | Core |

### Phase 6: CRT & Visual Effects

| # | Feature | Uno Components/Patterns | Priority |
|---|---------|------------------------|----------|
| 6.1 | **Richer CRT Effects** | Barrel distortion (shader or clipping), chromatic aberration, noise | Visual |
| 6.2 | **Depth & Layering** | Perspective scaling for blips, grid texture background | Visual |
| 6.3 | **Header Improvements** | Animated radar icon, status indicator, timestamp | Visual |

### Phase 7: Communication Features

| # | Feature | Uno Components/Patterns | Priority |
|---|---------|------------------------|----------|
| 7.1 | **"Ping" a Friend** | Outgoing animation, friend selection, visual feedback | Core |
| 7.2 | **Sound Design** | MediaElement or platform audio APIs, toggle setting | UX |

### Phase 8: Settings & Modes

| # | Feature | Uno Components/Patterns | Priority |
|---|---------|------------------------|----------|
| 8.1 | **Dark/Green Theme Toggle** | ThemeService, amber color palette alternative | Quick Win |
| 8.2 | **Battery-Saver Mode** | Reduced timer intervals, simplified animations | UX |
| 8.3 | **Location Sharing Modes** | Enum property, UI for selection, visual differentiation | Core |

### Phase 9: Navigation & Pages

| # | Feature | Uno Components/Patterns | Priority |
|---|---------|------------------------|----------|
| 9.1 | **Bottom Nav Redesign** | Toolkit TabBar or NavigationView, icon+text buttons | UX |
| 9.2 | **Settings Page** | New XAML page, Frame navigation, preferences storage | Core |
| 9.3 | **Loading/Empty States** | VisualStateManager, skeleton loaders, scanning animation | UX |

### Phase 10: Advanced Features (Future)

| # | Feature | Uno Components/Patterns | Priority |
|---|---------|------------------------|----------|
| 10.1 | **Map Overlay Toggle** | Map control integration, hybrid rendering | Advanced |
| 10.2 | **AR Mode** | Platform camera APIs, coordinate projection | Advanced |
| 10.3 | **Historical Playback** | Time-series data storage, scrubber control | Advanced |
| 10.4 | **Multiplayer Ping** | Real-time sync, mutual awareness indicators | Advanced |

---

## Detailed Implementation Specifications

### 1.1 Bearing Numbers (Quick Win)

**Location:** `Controls/RadarDisplay.xaml`

**Implementation:**
- Add 4 TextBlock elements at cardinal positions (0, 90, 180, 270 degrees)
- Position using Canvas.Left/Top calculated from center (150,150) and radius
- Style: SonarDistanceLabelTextBlockStyle with slight modifications

**Uno Components:**
- TextBlock with Canvas positioning
- Existing style resources

**Code Pattern:**
```xml
<TextBlock Text="0" Canvas.Left="143" Canvas.Top="5" Style="{StaticResource SonarBearingTextBlockStyle}"/>
<TextBlock Text="90" Canvas.Left="280" Canvas.Top="143" Style="{StaticResource SonarBearingTextBlockStyle}"/>
<TextBlock Text="180" Canvas.Left="140" Canvas.Top="280" Style="{StaticResource SonarBearingTextBlockStyle}"/>
<TextBlock Text="270" Canvas.Left="5" Canvas.Top="143" Style="{StaticResource SonarBearingTextBlockStyle}"/>
```

---

### 1.2 Sweep Line Enhancement

**Location:** `Controls/RadarDisplay.xaml` and `.xaml.cs`

**Implementation:**
- Add 2-3 "echo" lines behind main sweep with decreasing opacity
- Implement phosphor persistence by tracking recently-swept areas
- Enhanced glow falloff using gradient brushes

**Uno Components:**
- Additional Line elements with RotateTransform
- Opacity animation synchronized with main sweep
- LinearGradientBrush for glow effect

**Pattern:**
- Echo lines rotate at same speed but with angle offset (-10, -20, -30 degrees)
- Opacity: 0.6, 0.3, 0.15 respectively

---

### 1.3 Improved Blip Design

**Location:** `Controls/RadarDisplay.xaml.cs` (CreateBlip method)

**Implementation:**
- Replace solid Ellipse with Grid containing:
  - Outer hollow ring (Ellipse with stroke, no fill)
  - Inner dot (smaller filled Ellipse)
  - Pulsing ring layer

**Uno Components:**
- Grid as blip container
- Multiple Ellipse elements
- ScaleTransform for pulse animation

**Visual Spec:**
- Outer ring: 14px diameter, 2px stroke, status color
- Inner dot: 4px diameter, filled, status color
- Pulse ring: starts at 14px, expands to 28px, fades out

---

### 2.1 Friend Detail Panel

**Location:** New control or MainPage integration

**Implementation:**
- Slide-up panel triggered by blip tap
- Content: avatar/emoji, full name, status, distance, bearing, last updated
- Quick actions: Navigate, Message, Call buttons
- Swipe down or tap outside to dismiss

**Uno Components:**
- Toolkit DrawerControl (if available) OR custom implementation:
  - Border with TranslateTransform for slide animation
  - Storyboard for open/close transitions
  - PointerPressed on blips to trigger
  - ManipulationDelta for swipe-to-dismiss

**Pattern:**
```xml
<Border x:Name="DetailPanel" VerticalAlignment="Bottom" RenderTransformOrigin="0.5,1">
    <Border.RenderTransform>
        <TranslateTransform x:Name="PanelTranslate" Y="300"/>
    </Border.RenderTransform>
    <!-- Panel content -->
</Border>
```

---

### 3.1 Boot-up Animation

**Location:** `Controls/RadarDisplay.xaml.cs`

**Implementation:**
- Sequence on control Loaded event:
  1. Rings draw in from center (scale 0 to 1, staggered)
  2. Crosshairs fade in
  3. Sweep line appears and starts rotating
  4. Blips fade in one by one
  5. CRT effect activates

**Uno Components:**
- Storyboard with multiple animations
- BeginTime for sequencing
- ScaleTransform, Opacity animations

**Timing:**
- Total duration: ~2 seconds
- Ring 1: 0-300ms, Ring 2: 100-400ms, Ring 3: 200-500ms, Ring 4: 300-600ms
- Crosshairs: 400-700ms
- Sweep: 600-800ms
- Blips: 800-1500ms (staggered 100ms each)

---

### 4.1 Range Zoom Control

**Location:** `MainPage.xaml` and `Controls/RadarDisplay.xaml.cs`

**Implementation:**
- Add range selector (ToggleButtons or Slider)
- Update distance label text based on selected range
- Recalculate blip positions based on new scale
- Animate transition between ranges

**Uno Components:**
- ToggleButton group or Slider control
- Property binding for current range
- ScaleTransform animation for zoom effect

**Data Model Update:**
```csharp
public double MaxRangeMiles { get; set; } = 3.0; // Default
// Options: 1, 3, 5, 10 miles
```

---

### 5.1 Contact List Polish

**Location:** `Controls/ContactList.xaml`

**Implementation:**
- Add SwipeControl wrapper for swipe actions (Message, Navigate, Hide)
- Add sort ComboBox (Distance, Name, Status)
- Add search AutoSuggestBox with filtering
- Stagger animation on load

**Uno Components:**
- SwipeControl with SwipeItems
- ComboBox for sorting
- AutoSuggestBox for search
- CollectionViewSource for sorting/filtering

---

## Implementation Order (Sequential)

Based on dependencies and impact:

1. **Bearing Numbers** - Foundation, no dependencies
2. **Improved Blip Design** - Visual foundation for other blip features
3. **Sweep Line Enhancement** - Builds on existing sweep
4. **Animated Status Indicators** - Extends blip animations
5. **Boot-up Animation** - Uses all radar elements
6. **Friend Detail Panel** - Core interaction feature
7. **Long-press Blip Actions** - Builds on detail panel
8. **Range Zoom Control** - Core radar functionality
9. **Pull-to-Refresh Scan** - UX enhancement
10. **Contact List Polish** - List improvements
11. **Haptic Feedback** - Platform integration
12. **Ghost Blips / History Trail** - Advanced visual
13. **Compass Bearing Mode** - Advanced radar
14. **Proximity Alerts** - Notification feature
15. **Friend Groups / Squads** - Data organization
16. **ETA Calculation** - Advanced data
17. **Richer CRT Effects** - Visual polish
18. **Sound Design** - Optional audio
19. **Theme Toggle** - Settings feature
20. **Settings Page** - Full settings implementation

---

## Unresolved Questions

1. Should the Friend Detail Panel use Toolkit's DrawerControl or a custom implementation for more control over animations?
2. For Range Zoom, should blips animate smoothly to new positions or snap instantly?
3. Should Ghost Blips persist across app sessions or only within current session?
4. What should trigger Proximity Alerts - entering zone, leaving zone, or both?
5. For Contact List search, should it filter in real-time or require explicit search action?
6. Should haptic feedback intensity be configurable in settings?
7. For Boot-up Animation, should it play every app launch or only on first launch?
8. Should sound effects be bundled assets or system sounds?
9. For Friend Groups, should groups be user-defined or preset categories?
10. Should the amber theme variant be a full theme or just accent color swap?
