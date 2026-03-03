# Smart Home Thermostat Dashboard - Project Scope

## Project Overview

Recreation of a modern smart home thermostat dashboard interface using Uno Platform, featuring a circular temperature control, real-time monitoring cards, energy usage visualization, and bottom navigation.

---

## Technology Stack

### Core Framework
- **Uno Platform 6.x** - Cross-platform UI framework
- **WinUI 3 / Windows App SDK** - Base UI framework
- **C# / XAML** - Development languages

### UI Components & Libraries
- **Uno Toolkit** (`Uno.Toolkit.WinUI`) - For ShadowContainer and NavigationBar
- **SkiaSharp** (`SkiaSharp.Views.Uno.WinUI`) - For custom circular arc and chart rendering
- **Alternative**: LiveCharts2 (`LiveChartsCore.SkiaSharpView.WinUI`) - Optional for chart visualization

### Target Platforms
- Windows (Desktop)
- Android

---

## UI Components Breakdown

### 1. Layout Structure

**Component**: Root Container
- **Implementation**: `Grid` with vertical `RowDefinitions`
- **Background**: `LinearGradientBrush` (dark teal → slate → purple)
- **Color Scheme**:
  - Top: `#0A2E3E` (dark teal)
  - Middle: `#1E293B` (slate)
  - Bottom: `#2D1B4E` (dark purple)

---

### 2. Header Section

**Component**: App Header
- **Layout**: `Grid` with 2 columns
- **Elements**:
  - Left: Greeting text ("Good Evening, User")
    - Font: 20px, SemiBold, White
  - Right: Settings button
    - Icon: `&#xE713;` (Settings glyph)
    - Style: Transparent background, no border

**Design Exclusions**:
- ❌ Time display (6:26)
- ❌ Status icons (signal, WiFi, battery)

---

### 3. Circular Temperature Display (Main Component)

**Component**: Interactive Thermostat Control

#### Container
- **Wrapper**: `ShadowContainer` (Uno Toolkit)
  - Corner radius: 200px (perfect circle)
  - Shadow: Large shadow (BlurRadius: 40, Opacity: 0.6)
- **Dimensions**: 350x350px
- **Background**: Dark semi-transparent layer
  - `SolidColorBrush` with `Color="#000000"` and `Opacity="0.4"`

#### Circular Arc Progress Indicator
- **Implementation**: `SKXamlCanvas` (SkiaSharp)
- **Arc Specifications**:
  - Start angle: 135° (bottom-left)
  - Total sweep: 270° (3/4 circle)
  - Stroke width: 20px
  - Cap style: Round
  
- **Color Segments** (gradient progression):
  1. Cyan (`#00D9FF`) - 25%
  2. Blue (`#0099FF`) - 25%
  3. Purple (`#6B4FBB`) - 25%
  4. Orange (`#FF8C42`) - 15%
  5. Yellow (`#FFD700`) - 10%

- **Background Arc**: 
  - Full 270° arc
  - Color: `#2A2A3E` (dark gray)
  - Always visible behind progress

- **Handle/Thumb**:
  - White circle at end of progress arc
  - Radius: 12px
  - Position: Dynamically calculated based on current value

#### Center Content
- **"INDOOR" Label**:
  - Home icon: `&#xE80F;`
  - Text: "INDOOR"
  - Font size: 12px
  - Color: `#A0A0A0` (gray)
  - Layout: Horizontal with 8px spacing

- **Temperature Display**:
  - Text: "21.8°"
  - Font size: 64px
  - Font weight: Light
  - Color: White

- **Status Text**:
  - Text: "Heating to 24°"
  - Font size: 14px
  - Color: `#FF8C42` (orange)

#### Data Binding Properties
```csharp
public double CurrentTemperature { get; set; } // 21.8
public double TargetTemperature { get; set; }  // 24.0
public double MaxTemperature { get; set; }     // 30.0
public string Mode { get; set; }               // "Heating", "Cooling", "Off"
```

#### Alternative Implementation
- **Option**: LiveCharts2 Gauge Chart
- **Pros**: Less custom code, built-in animations
- **Cons**: Less control over exact visual appearance

---

### 4. Information Cards Section

**Component**: Metric Display Cards (3 cards)

#### Container
- **Layout**: `Grid` with 3 equal columns
- **Spacing**: 5px between cards

#### Individual Card Structure
- **Wrapper**: `ShadowContainer`
  - Corner radius: 16px
  - Shadow: Medium shadow (BlurRadius: 20, Opacity: 0.5)
- **Background**: 
  - `SolidColorBrush` with `Color="#000000"` and `Opacity="0.3"`
- **Padding**: 16px
- **Layout**: Vertical `StackPanel` with 8px spacing

#### Card 1: Humidity
- **Icon**: `&#xE9CA;` (droplet)
- **Icon Color**: `#00D9FF` (cyan)
- **Value**: "45"
- **Unit**: "%" (smaller, gray)
- **Label**: "Humidity"

#### Card 2: Air Quality
- **Icon**: `&#xE81E;` (leaf/air)
- **Icon Color**: `#00FF7F` (spring green)
- **Value**: "98"
- **Unit**: AQI (implied)
- **Label**: "Air Quality"

#### Card 3: Power
- **Icon**: `&#xE945;` (lightning bolt)
- **Icon Color**: `#FFD700` (gold)
- **Value**: "1.2"
- **Unit**: "W" (smaller, gray)
- **Label**: "Power"

#### Typography
- **Value**: 28px, SemiBold, White
- **Unit**: 16-20px, Regular, Gray
- **Label**: 12px, Regular, Gray

---

### 5. Energy Usage Chart

**Component**: Line Chart with Gradient Fill

#### Container
- **Wrapper**: `ShadowContainer`
  - Corner radius: 16px
  - Shadow: Medium shadow
- **Background**: 
  - `SolidColorBrush` with `Color="#000000"` and `Opacity="0.3"`
- **Padding**: 20px

#### Header
- **Title**: "Energy Usage"
- **Font**: 16px, SemiBold, White

#### Chart Implementation (SkiaSharp)
- **Canvas**: `SKXamlCanvas` (Height: 150px)
- **Chart Type**: Smooth line chart with area fill
- **Data Points**: 12 values (representing hourly/daily data)
- **Line Color**: `#4ADE80` (green)
- **Line Width**: 3px
- **Fill Gradient**: 
  - Top: `#4ADE80` with alpha 100
  - Bottom: `#4ADE80` with alpha 0 (transparent)
- **Rendering**: Smooth curve interpolation

#### Sample Data Structure
```csharp
public ObservableCollection<double> EnergyData { get; set; } = new()
{
    30, 45, 35, 50, 40, 55, 45, 60, 50, 45, 55, 65
};
```

#### Alternative Implementation
- **Option**: LiveCharts2 CartesianChart
- **Series Type**: LineSeries with area fill
- **Pros**: Built-in animations, tooltips, better interactivity

---

### 6. Bottom Navigation Bar

**Component**: Navigation Control (Uno Toolkit)

#### Implementation
- **Control**: `NavigationBar` from Uno Toolkit
- **Position**: Fixed at bottom of screen
- **Items**: 4 navigation buttons

#### Navigation Items

1. **Home**
   - Icon: `&#xE80F;` (Home)
   - Label: "Home"

2. **Menu**
   - Icon: `&#xE700;` (Three horizontal lines)
   - Label: "Menu"

3. **Activity** (Default Selected)
   - Icon: `&#xE95E;` (Heart/Activity)
   - Label: "Activity"

4. **Profile**
   - Icon: `&#xE77B;` (Contact/Person)
   - Label: "Profile"

#### Selected State Styling
- **Background**: `LinearGradientBrush`
  - Start: `#A855F7` (purple)
  - End: `#3B82F6` (blue)
  - Direction: Horizontal (left to right)
- **Shape**: Pill-shaped (CornerRadius: 20px)
- **Padding**: 20px horizontal, 8px vertical
- **Foreground**: White

#### Unselected State
- **Background**: Transparent
- **Foreground**: Gray

---

## Technical Implementation Details

### Required NuGet Packages

```xml
<!-- Core Uno Platform -->
<PackageReference Include="Uno.WinUI" Version="5.2.*" />

<!-- Uno Toolkit for ShadowContainer and NavigationBar -->
<PackageReference Include="Uno.Toolkit.WinUI" Version="6.3.0" />

<!-- SkiaSharp for custom drawing -->
<PackageReference Include="SkiaSharp.Views.Uno.WinUI" Version="2.88.8" />
<PackageReference Include="SkiaSharp.Views.WinUI" Version="2.88.8" />

<!-- Optional: LiveCharts2 for charts -->
<PackageReference Include="LiveChartsCore.SkiaSharpView.WinUI" Version="2.0.0-rc3.3" />
```

### XAML Namespace Declarations

```xml
xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
xmlns:utu="using:Uno.Toolkit.UI"
xmlns:skia="using:SkiaSharp.Views.Windows"
xmlns:lvc="using:LiveChartsCore.SkiaSharpView.WinUI"
```

### Resource Definitions

```xml
<Application.Resources>
    <!-- Shadow Definitions -->
    <Shadow x:Key="MediumShadow" 
            BlurRadius="20" 
            Opacity="0.5" 
            OffsetY="4"
            Color="Black"/>
            
    <Shadow x:Key="LargeShadow" 
            BlurRadius="40" 
            Opacity="0.6" 
            OffsetY="8"
            Color="Black"/>
    
    <!-- Color Resources -->
    <Color x:Key="DarkTeal">#0A2E3E</Color>
    <Color x:Key="DarkSlate">#1E293B</Color>
    <Color x:Key="DarkPurple">#2D1B4E</Color>
    <Color x:Key="Cyan">#00D9FF</Color>
    <Color x:Key="Blue">#0099FF</Color>
    <Color x:Key="Purple">#6B4FBB</Color>
    <Color x:Key="Orange">#FF8C42</Color>
    <Color x:Key="Yellow">#FFD700</Color>
    <Color x:Key="Green">#4ADE80</Color>
</Application.Resources>
```

---

## Design Decisions

### ✅ Included Features

1. **ShadowContainer for Depth**
   - Modern glassmorphism alternative
   - Better performance than AcrylicBrush
   - Consistent across all platforms
   - No children limitation (unlike AcrylicBrush on mobile)

2. **SkiaSharp for Custom Graphics**
   - Full control over circular arc rendering
   - Smooth gradient transitions
   - Precise angle calculations
   - Custom chart rendering

3. **Uno Toolkit NavigationBar**
   - Native bottom navigation component
   - Built-in selection states
   - Easy customization
   - Platform-specific optimizations

4. **Semi-Transparent Backgrounds**
   - `SolidColorBrush` with low opacity
   - Predictable rendering
   - Better performance
   - Consistent visual result

### ❌ Excluded Features

1. **System Status Bar**
   - Time display
   - Signal/WiFi/Battery indicators
   - Reason: Platform-specific, not part of core app UI

2. **AcrylicBrush**
   - Reason: Mobile platform limitations (no children allowed)
   - Alternative: ShadowContainer with semi-transparent backgrounds

---

## Code Architecture

### ViewModel Structure

```csharp
public class MainViewModel : ObservableObject
{
    // Thermostat Properties
    private double _currentTemperature;
    private double _targetTemperature;
    private string _mode;
    
    // Metrics Properties
    private double _humidity;
    private int _airQuality;
    private double _powerConsumption;
    
    // Chart Properties
    private ObservableCollection<double> _energyData;
    
    // Commands
    public ICommand AdjustTemperatureCommand { get; }
    public ICommand ChangeModeCommand { get; }
    public ICommand NavigateCommand { get; }
}
```

### Key C# Files

1. **MainPage.xaml.cs** - Page code-behind
2. **MainViewModel.cs** - Data binding and business logic
3. **ThermostatRenderer.cs** - SkiaSharp circular arc rendering
4. **ChartRenderer.cs** - SkiaSharp chart rendering
5. **Models/ThermostatState.cs** - Data models

---

## Development Phases

### Phase 1: Project Setup ✓
- [ ] Create Uno Platform solution
- [ ] Install required NuGet packages
- [ ] Configure platform heads
- [ ] Set up resource dictionary

### Phase 2: Layout & Structure
- [ ] Implement root Grid with gradient background
- [ ] Create header section
- [ ] Set up row definitions for all sections
- [ ] Configure margins and spacing

### Phase 3: Thermostat Component
- [ ] Implement ShadowContainer wrapper
- [ ] Create center content (temperature, labels)
- [ ] Implement SKXamlCanvas setup
- [ ] Code circular arc rendering logic
- [ ] Add gradient color segments
- [ ] Implement white handle/thumb
- [ ] Add data binding
- [ ] Implement touch/interaction (optional)

### Phase 4: Information Cards
- [ ] Create card container Grid
- [ ] Implement Humidity card
- [ ] Implement Air Quality card
- [ ] Implement Power card
- [ ] Style typography and icons
- [ ] Add data binding

### Phase 5: Energy Chart
- [ ] Create chart container
- [ ] Implement SKXamlCanvas for chart
- [ ] Code line rendering logic
- [ ] Add gradient fill
- [ ] Implement sample data
- [ ] Add data binding

### Phase 6: Bottom Navigation
- [ ] Implement NavigationBar
- [ ] Add navigation items
- [ ] Style selected state with gradient
- [ ] Implement navigation logic
- [ ] Test platform-specific behavior

### Phase 7: Polish & Testing
- [ ] Add animations/transitions
- [ ] Test on all platforms
- [ ] Optimize performance
- [ ] Accessibility improvements
- [ ] Documentation

---

## Performance Considerations

### SkiaSharp Optimization
- Use `IsAntialias = true` for smooth rendering
- Cache paint objects where possible
- Minimize canvas redraws
- Use `InvalidateVisual()` only when needed

### Layout Optimization
- Use `Grid` over nested `StackPanel` where possible
- Minimize visual tree depth
- Avoid excessive transparency layering
- Use `x:Load` for deferred loading (if applicable)

### Memory Management
- Dispose SKPaint objects properly (`using` statements)
- Clear canvas before each paint operation
- Unsubscribe from events in cleanup

---

## Accessibility Requirements

### Screen Reader Support
- All interactive elements must have `AutomationProperties.Name`
- Temperature value should announce changes
- Navigation items must be clearly labeled

### Keyboard Navigation
- Bottom navigation must support keyboard focus
- Temperature adjustment should support keyboard input
- Logical tab order through all interactive elements

### High Contrast Support
- Ensure sufficient color contrast ratios
- Test with high contrast themes
- Provide alternative visual indicators

---

## Testing Strategy

### Unit Tests
- ViewModel logic
- Temperature calculation algorithms
- Data conversion methods

### UI Tests
- Navigation flow
- Temperature adjustment
- Card data display
- Chart rendering

### Platform-Specific Tests
- Windows Desktop
- WebAssembly (browser compatibility)
- iOS (touch interactions)
- Android (various screen sizes)
- macOS (native look and feel)

---

## Future Enhancements (Out of Scope for v1)

- [ ] Animated temperature changes
- [ ] Swipe gestures for temperature adjustment
- [ ] Real-time data updates from IoT devices
- [ ] Multiple room/zone support
- [ ] Schedule/automation features
- [ ] Historical data and analytics
- [ ] User preferences and settings
- [ ] Dark/Light theme toggle
- [ ] Localization support
- [ ] Voice control integration

---

## Success Criteria

### Visual Fidelity
- ✅ Matches reference design for all major components
- ✅ Smooth animations and transitions
- ✅ Consistent styling across platforms
- ✅ Proper depth and shadow effects

### Functionality
- ✅ Temperature display updates correctly
- ✅ Navigation works on all platforms
- ✅ Charts render data accurately
- ✅ All metrics display properly

### Performance
- ✅ 60 FPS rendering on modern devices
- ✅ Fast startup time (< 2 seconds)
- ✅ Responsive touch/click interactions
- ✅ Efficient memory usage

### Code Quality
- ✅ MVVM pattern implementation
- ✅ Proper separation of concerns
- ✅ Clean, maintainable code
- ✅ Comprehensive documentation

---

## Resources & References

### Documentation
- [Uno Platform Documentation](https://platform.uno/docs/)
- [Uno Toolkit](https://platform.uno/docs/articles/external/uno.toolkit.ui/doc/getting-started.html)
- [SkiaSharp Documentation](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/)
- [WinUI 3 Documentation](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)

### Code Samples
- [Uno Platform Samples Repository](https://github.com/unoplatform/Uno.Samples)
- [SkiaSharp Uno Sample](https://github.com/unoplatform/Uno.SkiaSharp)
- [Uno Toolkit Samples](https://github.com/unoplatform/uno.toolkit.ui)

### Design Resources
- Reference UI: Smart Home Thermostat Dashboard
- Color palette: Dark theme with cyan/blue/purple/orange gradient
- Typography: System default with specified sizes

---

## Project Timeline

**Estimated Duration**: 2-3 weeks

- Week 1: Phases 1-3 (Setup, Layout, Thermostat)
- Week 2: Phases 4-5 (Cards, Chart)
- Week 3: Phases 6-7 (Navigation, Polish, Testing)

---

## Contact & Support

- **Uno Platform Discord**: [https://platform.uno/discord](https://platform.uno/discord)
- **GitHub Issues**: For bug reports and feature requests
- **Documentation**: [https://platform.uno/docs/](https://platform.uno/docs/)

---

*Document Version: 1.0*  
*Last Updated: November 19, 2025*  
*Created By: Matt (Uno Platform Team)*
