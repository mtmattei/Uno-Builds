# Sales Heatmap Report - Implementation Brief

## Overview

A dark-themed sales dashboard widget displaying an animated greyscale heatmap, live-updating financial totals with smooth number transitions, and a 3-city geographic breakdown.

---

## Visual Design Specifications

### Container
- Max width: 420px
- Padding: 32px
- Corner radius: 24px
- Background: Near-black with slight transparency (#0F0F19 at 95% opacity)
- Border: 1px solid white at 8% opacity
- Shadow: Large diffuse shadow (80px blur, black at 60% opacity)

### Color Palette
```
Background (page):     #0A0A0F
Card background:       #0F0F19 (95% opacity)
Primary accent:        #00FF88 (green, for title)
Secondary accent:      #FF00FF (magenta, monthly indicator)
Tertiary accent:       #00FFFF (cyan, yearly indicator)
Positive change:       #00FF88
Negative change:       #FF4466
Live indicator:        #FF4466
Text primary:          #FFFFFF
Text secondary:        #FFFFFF at 75% opacity
Text tertiary:         #FFFFFF at 50% opacity
Text muted:            #FFFFFF at 35% opacity
Divider:               #FFFFFF at 10% opacity
```

### Typography
- Font family: JetBrains Mono (fallback: Consolas, SF Mono, monospace)
- Title: 28px, bold, letter-spacing -0.5px
- Large numbers: 32px, bold, letter-spacing -1px
- Body text: 15px, medium weight
- Small labels: 13px, uppercase, letter-spacing 0.5px
- Micro text: 10-12px

---

## Component Structure

```
┌─────────────────────────────────────┐
│ [LIVE indicator]          top-right│
│                                     │
│ Sales Report              (title)   │
│                                     │
│ ┌─────────────────────────────────┐ │
│ │                                 │ │
│ │     HEATMAP GRID (12x8)         │ │
│ │                                 │ │
│ └─────────────────────────────────┘ │
│                                     │
│ ● Monthly          ● Yearly         │
│ $XXX,XXX           $XXX,XXX         │
│ +X.X% MoM          +X.X% YoY        │
│                                     │
│ ─────────────────────────────────── │
│                                     │
│ ● Los Angeles              XXX,XXX  │
│ ● New York                 XXX,XXX  │
│ ● Canada                   XXX,XXX  │
│                                     │
└─────────────────────────────────────┘
```

---

## Data Models

### HeatmapCell
```
{
  id: string,           // Unique identifier (e.g., "row-col")
  row: int,             // 0-7
  column: int,          // 0-11
  intensity: float      // 0.0 to 1.0
}
```

### TotalsData
```
{
  monthlyTotal: float,      // Actual value (target)
  yearlyTotal: float,       // Actual value (target)
  displayMonthly: float,    // Animated display value
  displayYearly: float,     // Animated display value
  monthlyChange: float,     // Percentage (-100 to +100)
  yearlyChange: float       // Percentage (-100 to +100)
}
```

### CityData
```
{
  name: string,             // "Los Angeles", "New York", "Canada"
  value: int,               // Actual value (target)
  displayValue: float,      // Animated display value
  dotColor: color           // #FF6B9D, #00D4FF, #A855F7
}
```

---

## Heatmap Specifications

### Grid Layout
- Columns: 12
- Rows: 8
- Total cells: 96
- Cell gap: 3px
- Cell corner radius: 3px
- Container padding: 8px
- Container corner radius: 12px
- Container background: Black at 40% opacity
- Aspect ratio: 1.5 (width:height)

### Greyscale Color Mapping
```
intensity (0.0 - 1.0) → lightness (12% - 85%)

Formula:
  lightness = 12 + (intensity * 73)
  
  intensity 0.0 → hsl(0, 0%, 12%)  // Dark grey
  intensity 0.5 → hsl(0, 0%, 48%)  // Mid grey  
  intensity 1.0 → hsl(0, 0%, 85%)  // Light grey
```

### Cell Opacity
```
opacity = 0.7 + (intensity * 0.3)

  intensity 0.0 → 70% opacity
  intensity 1.0 → 100% opacity
```

### Cell Glow Effect (optional)
```
blur_radius = intensity * 10px
glow_opacity = intensity * 0.35
color = white

box-shadow: 0 0 {blur_radius}px rgba(255, 255, 255, {glow_opacity})
```

---

## Animation Specifications

### Timer 1: Heatmap Animation
```
Interval: 150ms
Action: For each cell, adjust intensity randomly

  delta = (random(0,1) - 0.5) * 0.12
  new_intensity = clamp(intensity + delta, 0.05, 1.0)
```

### Timer 2: Data Updates
```
Interval: 2000ms
Actions:
  - Update monthlyTotal: += (random - 0.3) * 500
  - Update yearlyTotal: += (random - 0.3) * 200
  - Update monthlyChange: += (random - 0.5) * 0.5, round to 1 decimal
  - Update yearlyChange: += (random - 0.5) * 0.3, round to 1 decimal
  - Update each city value: += (random - 0.4) * 300
  
All values: clamp to minimum 0
```

### Timer 3: Number Interpolation
```
Interval: 16ms (60fps)
Action: Smooth lerp from display value toward actual value

  diff = actualValue - displayValue
  
  if (abs(diff) > threshold):
    step = sign(diff) * max(1, abs(diff) * 0.1)
    displayValue += step
    
Thresholds:
  - Totals: 10
  - Cities: 5
  
Lerp factor:
  - Totals: 0.1 (10% per frame)
  - Cities: 0.15 (15% per frame)
```

### Live Indicator Pulse
```
Animation: Opacity pulse
Duration: 1.5s per cycle
Easing: ease-in-out
Keyframes:
  0%   → opacity: 1.0, scale: 1.0
  50%  → opacity: 0.5, scale: 1.2
  100% → opacity: 1.0, scale: 1.0
```

---

## Number Formatting

### Currency (Totals)
```
Format: $XXX,XXX
Example: $312,134

- Currency symbol: $
- Thousands separator: comma
- No decimal places
```

### Percentage (Changes)
```
Format: +X.X% or -X.X%
Example: +10.0%

- Always show sign (+ or -)
- One decimal place
- Suffix: %
```

### Plain Number (Cities)
```
Format: XXX,XXX
Example: 201,173

- Thousands separator: comma
- No decimal places
```

---

## Initial Data Values

```
monthlyTotal: 312134
yearlyTotal: 312134
monthlyChange: 10.0
yearlyChange: 2.0

cities:
  - Los Angeles: 201173, color #FF6B9D
  - New York: 107854, color #00D4FF
  - Canada: 165271, color #A855F7

heatmap cells:
  - All 96 cells initialized with random intensity (0.0 - 1.0)
```

---

## Layout Spacing

```
Title margin bottom: 24px
Heatmap margin bottom: 28px
Totals section margin bottom: 24px
Divider margin: 8px top, 20px bottom
City rows gap: 16px
City row padding: 8px vertical

Totals grid:
  - 2 columns, equal width
  - Column gap: 20px

City row:
  - Dot size: 6px
  - Dot margin right: 12px
  - Justify: space-between
  - Border bottom: 1px solid white at 5% opacity
```

---

## Indicator Dots

### Totals Section
```
Monthly dot:
  - Size: 8px
  - Color: #FF00FF (magenta)
  - Glow: 0 0 10px #FF00FF

Yearly dot:
  - Size: 8px  
  - Color: #00FFFF (cyan)
  - Glow: 0 0 10px #00FFFF
```

### Live Indicator
```
  - Size: 6px
  - Color: #FF4466
  - Glow: 0 0 8px #FF4466
  - Animated pulse (see animation specs)
```

### City Dots
```
  - Size: 6px
  - No glow
  - Colors: #FF6B9D, #00D4FF, #A855F7
```

---

## Gradient Overlay (Heatmap)

Apply a subtle fade at the bottom of the heatmap:

```
Type: Linear gradient, vertical
Direction: Top to bottom
Stops:
  - 0%: transparent
  - 60%: transparent  
  - 100%: card background at 50% opacity
  
Position: Absolute, covers entire heatmap
Pointer events: none
```

---

## Divider

```
Type: Horizontal line
Height: 1px
Background: Linear gradient, horizontal
Stops:
  - 0%: transparent
  - 50%: white at 10% opacity
  - 100%: transparent
```

---

## Accessibility Considerations

- Ensure sufficient contrast for text elements
- Provide aria-live regions for updating values (web)
- Consider reduced motion preferences - disable animations if requested
- Heatmap should have alt text describing general data distribution

---

## Performance Guidelines

- Batch UI updates where possible
- Use efficient data structures for 96-cell updates
- Consider virtualization only if grid exceeds 1000+ cells
- Debounce rapid property changes if binding system is slow
- For GPU rendering (Canvas/Skia), batch draw calls

---

## Extension Points

To connect real data, replace Timer 2 logic with:
```
async function fetchData():
  data = await salesAPI.getLatest()
  monthlyTotal = data.monthly
  yearlyTotal = data.yearly
  monthlyChange = data.monthlyChange
  yearlyChange = data.yearlyChange
  cities = data.cityBreakdown
```

Heatmap can represent real data by mapping values to intensity:
```
intensity = (cellValue - minValue) / (maxValue - minValue)
```
