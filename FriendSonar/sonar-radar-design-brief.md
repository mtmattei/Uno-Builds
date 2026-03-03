# SONAR Friend Radar — Design & Behavior Specification

## Overview

A mobile-first "Find My Friends" application with a retro submarine sonar aesthetic. The app displays friends' locations on an animated radar display, mimicking cold-war era CRT sonar screens with phosphor green coloring, scan lines, and glowing elements.

**Target Platform:** Mobile (portrait orientation, max-width 420px)  
**Design Language:** Retro-futuristic military terminal / submarine sonar  
**Primary Interaction Model:** Touch/tap with visual feedback

---

## Color Palette

### Primary Colors

| Name | Hex | RGB | Usage |
|------|-----|-----|-------|
| Phosphor Green | `#00FF41` | 0, 255, 65 | Primary accent, text, glows, active elements |
| Terminal Black | `#0A0F0A` | 10, 15, 10 | Primary background |
| Deep Green Black | `#0D1A0D` | 13, 26, 13 | Background gradient midpoint |
| Void Black | `#081208` | 8, 18, 8 | Background gradient end |

### Status Colors

| Status | Hex | RGB | Glow Color |
|--------|-----|-----|------------|
| Active | `#00FF41` | 0, 255, 65 | Same as fill |
| Idle | `#FFAA00` | 255, 170, 0 | Same as fill |
| Away | `#FF4141` | 255, 65, 65 | Same as fill |

### Opacity Variants (Phosphor Green)

| Opacity | Hex | Usage |
|---------|-----|-------|
| 100% | `#00FF41` | Primary text, active elements |
| 53% | `#00FF4188` | Secondary text, subtitles |
| 33% | `#00FF4155` | Tertiary text, distance labels |
| 27% | `#00FF4144` | Borders, dividers |
| 20% | `#00FF4133` | Faint borders, crosshairs |
| 12% | `#00FF411F` | Grid lines, rings |
| 7% | `#00FF4111` | Very faint dividers |

---

## Typography

### Font Stack

**Primary:** `'Courier New', 'Consolas', monospace`

A monospace font is essential for the military terminal aesthetic. Alternative options include:
- VT323 (Google Fonts) — authentic terminal feel
- Share Tech Mono — clean technical look
- IBM Plex Mono — modern but retro-compatible

### Type Scale

| Element | Size | Weight | Letter Spacing | Additional |
|---------|------|--------|----------------|------------|
| App Title | 28px | Normal | 4px | Animated glow effect |
| Subtitle | 11px | Normal | 2px | 53% opacity |
| Contact Count | 28px | Bold | Normal | — |
| Contact Count Label | 12px | Normal | Normal | 70% opacity |
| Status Bar | 11px | Normal | 1px | 50% opacity |
| Section Header | 11px | Normal | 2px | 53% opacity |
| Contact Name | 14px | Normal | Normal | — |
| Contact Info | 11px | Normal | Normal | 53% opacity |
| Button Text | 11px | Normal | 2px | — |
| Center Label | 9px | Normal | Normal | 70% opacity |
| Distance Labels | 9px | Normal | Normal | 33% opacity |
| Blip Label | 12px | Normal | Normal | With glow |

---

## Layout Structure

### Screen Regions (Top to Bottom)

```
┌─────────────────────────────┐
│         HEADER              │  ~70px
├─────────────────────────────┤
│                             │
│      RADAR DISPLAY          │  ~360px (including padding)
│                             │
├─────────────────────────────┤
│       STATUS BAR            │  ~20px
├─────────────────────────────┤
│                             │
│      CONTACT LIST           │  Flexible (scrollable)
│                             │
├─────────────────────────────┤
│     BOTTOM CONTROLS         │  ~70px (fixed)
└─────────────────────────────┘
```

### Container

- Maximum width: 420px
- Centered horizontally on larger screens
- Full height of viewport
- No horizontal scrolling

---

## Component Specifications

### 1. CRT Effect Overlays

Two overlay layers create the retro CRT monitor effect:

**Static Scanlines**
- Full-screen overlay
- Horizontal lines: 2px transparent, 2px at 10% black opacity
- Repeating pattern
- Non-interactive (pass-through pointer events)
- Z-index: 100

**Moving Scanline**
- Full-width horizontal bar, 4px height
- Vertical gradient: transparent → 15% green → transparent
- Animation: travels from top (-10%) to bottom (110%) over 6 seconds
- Continuous loop
- Z-index: 101

### 2. Header

**Layout:** Flexbox, space-between alignment

**Left Section:**
- Title: "SONAR" 
- Subtitle: "FRIEND DETECTION SYSTEM v2.4"

**Right Section:**
- Label: "CONTACTS"
- Count: Dynamic number of friends

**Border:** Bottom border, 1px, 20% phosphor green

**Animation:** Subtle flicker effect (opacity drops to 70% briefly every ~10 seconds)

### 3. Radar Display

**Container:** 300×300px circle, centered with 30px vertical / 20px horizontal padding

**Background:** Radial gradient from lighter green-black center to darker edges

**Border:** 3px solid, 27% phosphor green

**Animation:** Pulsing glow effect (outer shadow oscillates between 15% and 25% intensity over 4 seconds)

#### 3.1 Grid Elements

**Concentric Rings (4 total)**
- Positioned at 25%, 50%, 75%, and 100% of radar diameter
- Each is a circle with 1px border
- Opacity decreases toward the edge (12% → 8%)

**Crosshairs**
- Horizontal line: spans 90% of width, centered vertically
- Vertical line: spans 90% of height, centered horizontally  
- Both use gradient: transparent at edges, 20% green in center

**Distance Labels**
- Positioned to the right of center (52% from left)
- Three labels: "1mi", "2mi", "3mi"
- Placed at 20%, 33%, 46% from top
- 33% opacity

#### 3.2 Sweep Line

**Geometry:**
- Origin: center of radar
- Length: 50% of radar radius (extends to edge)
- Width: 2px
- Gradient: solid phosphor green at center, transparent at edge

**Glow:** Double box shadow, 10px and 20px spread, phosphor green

**Animation:**
- Rotates 360° continuously
- Speed: 2° per 30ms (~12 seconds per revolution)
- Transform origin: left center

#### 3.3 Sweep Trail

**Geometry:** Full circle overlay behind sweep line

**Visual:** Conic gradient starting from sweep angle
- 25% phosphor green at 0°
- Transparent by 50°
- Transparent remainder

**Animation:** Rotation synced with sweep line

#### 3.4 Center Point

**Geometry:** 12×12px circle, centered

**Visual:** Solid phosphor green with double glow (15px and 30px)

**Label:** "YOU" text, positioned 22px below center, 70% opacity

#### 3.5 Friend Blips

**Geometry:** 14×14px circles

**Positioning:** Polar coordinates converted to Cartesian
- Distance: 0.0 (center) to 1.0 (edge), multiplied by radius
- Angle: 0° at top, clockwise

**Polar to Cartesian Formula:**
```
x = centerX + (distance × radius × cos(angle - 90°))
y = centerY + (distance × radius × sin(angle - 90°))
```

**Visual by Status:**
- Active: Phosphor green fill + 12px glow
- Idle: Amber fill + 12px glow  
- Away: Red fill + 12px glow

**Animation:** Pulsing scale (1.0 → 1.2 → 1.0 over 2 seconds)
- Staggered: 0s, 0.5s, 1s delays based on status

**Hover State:**
- Scale: 1.4×
- Brightness: 150%
- Transition: 300ms ease

**Selected State:**
- Scale: 1.4×
- Label becomes visible

**Label:**
- Positioned 28px above blip, centered
- Shows emoji + name
- Hidden by default (opacity: 0)
- Visible on hover or selection (opacity: 1)
- Phosphor green with 10px glow
- Transition: 300ms opacity

### 4. Status Bar

**Layout:** Flexbox, evenly distributed

**Content (3 items):**
- "FREQ: 38.2 kHz" (static decorative)
- "RANGE: 3 MI" (static decorative)
- "PING: [X]°" (dynamic, shows current sweep angle)

**Style:** 50% opacity, 1px letter spacing

### 5. Contact List

**Container:**
- Margin: 20px all sides
- Padding: 15px
- Background: 3% phosphor green
- Border: 1px, 20% phosphor green
- Border radius: 4px

**Header:**
- Text: "▸ DETECTED CONTACTS"
- 53% opacity, 2px letter spacing
- Bottom border: 1px, 13% phosphor green
- Padding bottom: 10px, margin bottom: 10px

**Contact Item:**
- Flexbox, space-between
- Padding: 10px 8px
- Bottom border: 1px, 7% phosphor green

**Contact Item — Left Side:**
- Status dot (8×8px circle with matching glow)
- Emoji (16px)
- Name (14px, phosphor green)
- Gap: 12px between elements

**Contact Item — Right Side:**
- Distance in miles + bearing in degrees
- Format: "X.X MI · XXX°"
- 53% opacity

**Hover/Selected State:**
- Background: 10% phosphor green
- Transition: 200ms

### 6. Bottom Controls

**Position:** Fixed to bottom of screen

**Container:**
- Padding: 20px
- Background: Gradient from transparent to terminal black
- Max-width: 420px, centered
- Flexbox, centered with 10px gap

**Buttons (3 total):** "PING ALL", "FILTERS", "SETTINGS"

**Button Style:**
- Background: transparent
- Border: 1px, 27% phosphor green
- Text: phosphor green
- Padding: 12px 18px
- Monospace font, 11px, 2px letter spacing

**Button Hover:**
- Background: 10% phosphor green
- Border: solid phosphor green
- Transition: 200ms

**Bottom Spacer:** 80px empty space before fixed controls to prevent content overlap

---

## Animation Specifications

### Continuous Animations

| Animation | Duration | Easing | Properties |
|-----------|----------|--------|------------|
| Sweep Rotation | ~12s full rotation | Linear | transform: rotate |
| Scanline Travel | 6s | Linear | top position |
| Header Flicker | 10s | Step | opacity (brief dip to 70%) |
| Title Glow | 2s | Ease-in-out | text-shadow intensity |
| Radar Pulse | 4s | Ease-in-out | box-shadow spread |
| Blip Pulse | 2s | Ease-in-out | transform: scale |

### Transition Animations

| Element | Property | Duration | Easing |
|---------|----------|----------|--------|
| Blip hover/select | transform, filter | 300ms | Ease |
| Blip label | opacity | 300ms | Ease |
| Contact item | background | 200ms | Ease |
| Button hover | background, border | 200ms | Ease |

---

## Data Model

### Friend Object

```
Friend {
    id: Integer (unique identifier)
    name: String (display name)
    distance: Float (0.0 to 1.0, normalized radar distance)
    angle: Integer (0-359, degrees from top, clockwise)
    status: Enum ["active", "idle", "away"]
    emoji: String (single emoji character for avatar)
}
```

### Application State

```
AppState {
    friends: Array<Friend>
    selectedFriendId: Integer | null
    sweepAngle: Integer (0-359, current rotation)
}
```

---

## Interaction Behaviors

### Blip Tap/Click

1. If blip is not selected → select it
   - Set `selectedFriendId` to friend's ID
   - Blip scales to 1.4×
   - Label becomes visible
   - Corresponding contact list item highlights

2. If blip is already selected → deselect it
   - Set `selectedFriendId` to null
   - Blip returns to normal scale
   - Label hides
   - Contact list item unhighlights

### Contact List Item Tap/Click

Identical behavior to blip tap — selecting a contact highlights both the list item AND the corresponding blip on the radar.

### Selection Synchronization

Radar blips and contact list items are synchronized:
- Selecting a blip highlights the list item
- Selecting a list item highlights the blip
- Only one friend can be selected at a time
- Tapping the already-selected item deselects it

### Button Actions (Suggested Implementations)

**PING ALL**
- Trigger a visual "ping" animation from center outward
- Could briefly highlight all blips simultaneously
- Optional: play sonar ping sound effect

**FILTERS**
- Open a modal or bottom sheet
- Filter options: by status (active/idle/away), by distance range
- Apply filters to both radar display and contact list

**SETTINGS**
- Open settings screen or modal
- Options might include: range adjustment, update frequency, sound effects, theme variations

---

## Accessibility Considerations

- All interactive elements should be focusable via keyboard/switch control
- Blips should have accessible names (friend name + status)
- Contact list provides text alternative to visual radar
- Consider reduced motion mode that disables continuous animations
- Status colors should not be the only indicator (consider shapes or patterns)
- Minimum touch target: 44×44px (blips at 14px may need larger hit area)

---

## Responsive Behavior

**Width < 420px:** Full width, no horizontal margins

**Width ≥ 420px:** Centered container, max 420px width

**Radar scaling:** Consider making radar size responsive to viewport:
- Small phones: 260px
- Standard phones: 300px
- Large phones/small tablets: 340px

---

## Performance Considerations

- Sweep animation uses CSS transforms (GPU accelerated)
- Limit DOM updates to selection changes only
- Consider reducing animation frame rate on low-power devices
- Blip positions only recalculate on data change, not every frame
- CRT overlay effects are pure CSS, no JavaScript overhead

---

## Implementation Notes

### Coordinate System

The radar uses a polar coordinate system where:
- 0° is at the 12 o'clock position (top)
- Angles increase clockwise
- Distance 0.0 is center, 1.0 is edge

To convert to screen coordinates:
1. Subtract 90° from angle (to shift 0° from 3 o'clock to 12 o'clock)
2. Convert to radians
3. Calculate X = center + (distance × radius × cos(radians))
4. Calculate Y = center + (distance × radius × sin(radians))

### Timer Management

- Single timer for sweep animation (~30ms interval)
- Update sweep rotation each tick
- Update status bar ping angle display each tick
- Consider pausing animation when app is backgrounded

### State Updates

When selection changes:
1. Update selected friend ID in state
2. Re-render blips with updated selected class
3. Re-render contact list with updated selected class
4. Both updates should reference the same state to stay synchronized
