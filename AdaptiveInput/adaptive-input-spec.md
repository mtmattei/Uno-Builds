# Adaptive Input Component
## Design & Technical Specification

**Version:** 1.0
**Status:** Ready for Implementation
**Last Updated:** December 2024

---

## Summary

The Adaptive Input is a text input component that detects the semantic type of user input in real-time and morphs its UI to provide contextually appropriate input tools. Rather than requiring users to select input types upfront, the component infers intent from natural typing patterns and surfaces the right tool automatically.

---

## Core Concept

### The Problem

Traditional form inputs force users to context-switch between multiple input types:
- Click a calendar icon → navigate months → select date
- Click a color swatch → open picker → choose color
- Remember that "@" triggers mentions in this app but not that one

This creates friction and cognitive load, especially in unified input contexts like chat, command palettes, or quick-capture interfaces.

### The Solution

A single input field that:
1. Accepts freeform text
2. Analyzes input patterns in real-time
3. Detects semantic intent (date, color, mention, tag, number range)
4. Morphs to reveal the appropriate specialized tool
5. Falls back gracefully to plain text when no pattern matches

---

## Detection Patterns

### Priority Order (Critical)

Detection must follow this exact priority to resolve ambiguous patterns:

| Priority | Type | Pattern | Examples |
|----------|------|---------|----------|
| 1 | Mention | `@` followed by characters | `@alice`, `@team` |
| 2 | Hex Color | `#` + only hex digits (1-6) | `#FF5500`, `#fff`, `#a1b2c3` |
| 3 | Color Keyword | Known color names | `red`, `blue`, `rgb(...)`, `hsl(...)` |
| 4 | Tag | `#` + word containing non-hex letter | `#urgent`, `#bug`, `#v2-release` |
| 5 | Number Range | Number patterns with range indicators | `50-100`, `10 to 50`, `75` |
| 6 | Date | Natural language or date formats | `tomorrow`, `jan 15`, `3/14/2025` |
| 7 | URL | Standard URL patterns | `https://...`, `www.` |
| 8 | Email | Standard email format | `name@domain.com` |

### Pattern Definitions

#### Mention (`@`)
```
Trigger: ^@.+
Examples: @alice, @engineering-team, @123
Behavior: Opens contact/user picker with search
```

#### Color (Hex)
```
Trigger: ^#[0-9a-fA-F]{1,6}$
Examples: #F00, #FF5500, #abc
Excludes: #urgent (contains non-hex 'u')
Behavior: Opens color palette with custom hex input
```

#### Color (Keyword)
```
Trigger: ^(red|blue|green|yellow|purple|orange|pink|cyan|
          black|white|gray|grey|navy|teal|coral|salmon|
          gold|silver|maroon|olive|lime|aqua|fuchsia|
          indigo|violet|rgb\(|hsl\()
Examples: red, rgb(255,0,0)
Behavior: Opens color palette
```

#### Tag
```
Trigger: ^#[a-z_][a-z0-9_-]*$ OR ^#[0-9a-f]*[g-z_][a-z0-9_-]*$
Examples: #urgent, #bug, #release-v2, #abc123test
Key: Must contain at least one non-hex letter (g-z) or underscore
Behavior: Opens tag picker with create option
```

#### Number Range
```
Trigger: ^\d+\s*(-|–|to|through)\s*\d+$ OR ^\d+$ (standalone)
Examples: 50-100, 10 to 50, 25 through 75, 50
Behavior: Opens dual-handle range slider
```

#### Date
```
Trigger (natural): ^(today|tomorrow|yesterday|next\s+|last\s+|this\s+)
Trigger (day): ^(mon|tue|wed|thu|fri|sat|sun)(day)?$
Trigger (month): ^(jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)
Trigger (format): ^\d{1,2}[\/\-]\d{1,2}([\/\-]\d{2,4})?$
Examples: tomorrow, next week, jan 15, 3/14/2025
Behavior: Opens mini calendar with quick picks
```

---

## Visual Design

### Component States

#### 1. Idle State
```
┌─────────────────────────────────────────────────────────┐
│  ┌──────────┐                                      ┌──┐ │
│  │ ✦  Auto  │  Type anything...                    │ → │ │
│  └──────────┘                                      └──┘ │
└─────────────────────────────────────────────────────────┘
     Type Badge        Placeholder              Submit
```

#### 2. Active State (Type Detected)
```
┌─────────────────────────────────────────────────────────┐
│  ┌──────────┐                                      ┌──┐ │
│  │ 📅 Date  │  tomorrow                            │ → │ │
│  └──────────┘                                      └──┘ │
├─────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────┐    │
│  │               EXPANDED PANEL                    │    │
│  │           (Calendar in this case)               │    │
│  └─────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

### Type Badge

The badge provides immediate visual feedback about detected type:

| Type | Icon | Label | Color (HSL suggested) |
|------|------|-------|----------------------|
| Auto (default) | Sparkle/asterisk | "Auto" | Neutral gray |
| Date | Calendar | "Date" | Teal (170°, 70%, 45%) |
| Color | Palette/circle | "Color" | Violet (260°, 70%, 60%) |
| Mention | Person | "Mention" | Blue (215°, 80%, 55%) |
| Tag | Tag/label | "Tag" | Amber (40°, 90%, 50%) |
| Range | Sliders | "Range" | Indigo (235°, 70%, 60%) |
| URL | Link | "Link" | Cyan (190°, 85%, 45%) |
| Email | Envelope | "Email" | Orange (25°, 90%, 55%) |

### Expanded Panel

Each detected type reveals a specialized picker panel below the input:

- **Anchored** to input field (flows downward)
- **Dismissible** via: outside click, Escape key, close button
- **Animated** entrance: fade + slide up (200-300ms ease-out)
- **Shadow** to create depth separation from page
- **Header** shows type icon + "Select {Type}" + close button

---

## Interaction Patterns

### Input Flow

```
User types → Detect pattern → Update badge → Expand panel (if applicable)
     ↓
User selects from panel → Value updates → Panel closes → Focus returns
     ↓
User presses Enter (or clicks submit) → onSubmit({ type, value })
```

### Keyboard Navigation

| Key | Action |
|-----|--------|
| `Enter` | Submit value (if panel closed) or confirm selection |
| `Escape` | Close expanded panel, keep value |
| `Tab` | Move focus into panel (if open) |
| `Arrow keys` | Navigate within panel (calendar days, color grid, etc.) |

### Touch Considerations

- Panels should be sized for touch targets (minimum 44x44px interactive areas)
- Color swatches: at least 40x40px with 8px gap
- Calendar days: full aspect-square cells
- Consider bottom-sheet pattern on mobile viewports

---

## Picker Specifications

### Date Picker

**Components:**
- Month/year header with prev/next navigation
- 7-column day grid (Su-Sa)
- Current day highlighted distinctly
- Quick picks row: "Today", "Tomorrow", "Next Week"

**Behavior:**
- Selecting a date formats to locale string and closes panel
- Quick picks calculate relative dates from current date
- Month navigation does not close panel

### Color Picker

**Components:**
- Preset grid (20 colors recommended, 10x2)
- Custom hex input with live preview swatch
- "Apply" button for custom colors

**Preset Palette (suggested):**
```
#EF4444  #F97316  #F59E0B  #EAB308  #84CC16
#22C55E  #10B981  #14B8A6  #06B6D4  #0EA5E9
#3B82F6  #6366F1  #8B5CF6  #A855F7  #D946EF
#EC4899  #F43F5E  #FFFFFF  #94A3B8  #1E293B
```

**Behavior:**
- Clicking preset immediately selects and closes
- Custom hex requires explicit "Apply" action
- Live preview updates as user types

### Mention Picker

**Components:**
- Scrollable contact list (max-height constrained)
- Avatar, name, handle, status indicator per row
- Optional: role/department secondary text
- Empty state for no matches

**Behavior:**
- Filters list as user types after `@`
- Selecting contact replaces input with `@handle`
- Status colors: green=online, amber=away, gray=offline

### Tag Picker

**Components:**
- Flex-wrap grid of existing tags
- Each tag shows: color dot, name, usage count
- "Create new tag" button (appears when typing unknown tag)

**Behavior:**
- Clicking existing tag selects it
- Create option only shows when no exact match exists
- Tags should have distinct colors for scannability

### Range Picker

**Components:**
- Two number displays (min/max) with direct edit
- Visual range bar with filled segment
- Dual-handle slider (or two overlapping range inputs)
- Quick preset buttons (e.g., "0-50", "0-100", "50-150")

**Behavior:**
- Dragging handles updates number displays in real-time
- Number inputs clamp to valid ranges (min < max)
- "Apply" button confirms selection

---

## Component API

### Props/Inputs

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `placeholder` | string | "Type anything..." | Input placeholder text |
| `value` | string | "" | Controlled input value (optional) |
| `disabled` | boolean | false | Disable input |
| `autoFocus` | boolean | false | Focus on mount |

### Events/Outputs

| Event | Payload | Description |
|-------|---------|-------------|
| `onSubmit` | `{ type: string, value: string }` | Fired when user submits (Enter or button) |
| `onChange` | `{ type: string \| null, value: string }` | Fired on every input change |
| `onTypeDetected` | `{ type: string \| null }` | Fired when detected type changes |

### Type Values

```
type DetectedType =
  | 'date'
  | 'color'
  | 'mention'
  | 'tag'
  | 'number'
  | 'url'
  | 'email'
  | null  // No pattern detected, plain text
```

---

## Accessibility

### ARIA Requirements

- Input: `role="combobox"`, `aria-expanded`, `aria-haspopup="dialog"`
- Panel: `role="dialog"`, `aria-label="Select {type}"`
- Type badge: `aria-live="polite"` to announce type changes
- Close button: `aria-label="Close picker"`

### Focus Management

1. Panel open → focus moves to first interactive element in panel
2. Panel close → focus returns to input
3. Tab from input (panel open) → enters panel
4. Shift+Tab from first panel element → returns to input

### Screen Reader Announcements

- On type detection: "Detected {type}. {Type} picker available."
- On panel open: "{Type} picker opened"
- On selection: "Selected {value}"

---

## Animation Specifications

### Badge Transition
```
Property: background-color, color
Duration: 200ms
Easing: ease-out
```

### Panel Enter
```
Properties: opacity, transform
From: opacity: 0, translateY(-8px)
To: opacity: 1, translateY(0)
Duration: 250ms
Easing: cubic-bezier(0.4, 0, 0.2, 1)
```

### Panel Exit
```
Properties: opacity, transform
To: opacity: 0, translateY(-4px)
Duration: 150ms
Easing: ease-in
```

### List Item Stagger (mentions, tags)
```
Delay per item: 30ms
Max total delay: 300ms (cap at 10 items)
```

---

## Edge Cases

| Scenario | Behavior |
|----------|----------|
| Empty input | Badge shows "Auto", no panel |
| Ambiguous `#abc` | Detected as Color (valid 3-digit hex) |
| `#abc123test` | Detected as Tag (contains non-hex letters) |
| `@` alone | Detected as Mention, shows full contact list |
| `#` alone | No detection (needs at least one character after) |
| Very long input | Detection still runs, but may not match patterns |
| Paste multi-line | Take first line only, or reject |
| Invalid hex `#GGGGGG` | Falls through to Tag detection |

---

## Implementation Notes

### Performance

- Detection runs on every keystroke - keep regex simple
- Debounce is NOT recommended (feels laggy)
- Panel content can lazy-load on first open per type

### Extensibility

Consider allowing custom type definitions:

```javascript
{
  type: 'phone',
  pattern: /^\+?[\d\s\-\(\)]{7,}$/,
  icon: PhoneIcon,
  label: 'Phone',
  color: { bg: '...', text: '...' },
  picker: PhonePickerComponent  // Optional
}
```

### Localization

- Date formats should respect locale
- Day/month names in detection should support localized variants
- Quick pick labels ("Today", "Tomorrow") need translation

---

## Dependencies

The component requires implementations for:

1. **Pattern detection engine** - can be pure functions, no library needed
2. **Date picker** - any calendar component or native date input
3. **Color picker** - custom or library-based
4. **Contact/user data source** - app-specific integration
5. **Tag data source** - app-specific integration

No external runtime dependencies required for core functionality.

---

## Testing Checklist

### Detection Tests
- [ ] `@alice` → mention
- [ ] `#FF5500` → color
- [ ] `#fff` → color
- [ ] `#urgent` → tag
- [ ] `#abc123` → color (valid hex)
- [ ] `#abc123test` → tag (contains 't')
- [ ] `tomorrow` → date
- [ ] `jan 15` → date
- [ ] `3/14` → date
- [ ] `50-100` → number
- [ ] `50 to 100` → number
- [ ] `hello world` → null (plain text)

### Interaction Tests
- [ ] Panel opens on pattern match
- [ ] Panel closes on outside click
- [ ] Panel closes on Escape
- [ ] Selection updates input value
- [ ] Focus returns to input after selection
- [ ] Enter submits when panel closed
- [ ] Keyboard navigation within panels

### Accessibility Tests
- [ ] Screen reader announces type changes
- [ ] Focus trap works in panels
- [ ] All interactive elements reachable via keyboard
