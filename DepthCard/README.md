# Depth Card Component

## Design & Architecture Specification


**Version:** 1.0  
**Status:** Ready for Implementation  
**Last Updated:** December 2025

---
 

## Executive Summary

Depth Card is a 3D interactive card component that creates a diorama-like parallax effect. As users move their cursor over the card, it tilts in response while internal elements separate along the Z-axis based on their assigned depth values. This creates a tactile, premium feel that elevates content presentation and encourages user interaction.


---

  

## Core Concept

### The Problem
Traditional card components are flat and static. While hover effects like shadows and scale transforms add some interactivity, they fail to create meaningful depth or hierarchy between card elements. Every element exists on the same plane, making it difficult to visually communicate importance or create memorable interactions.

  

### The Solution
A card system with true 3D depth where:

  

1. The card tilts toward the cursor position

2. Internal elements are assigned depth values (-1 to +1)

3. Elements physically separate in Z-space as the card tilts

4. A glare effect simulates light reflection for realism

5. Smooth animations create a polished, responsive feel

  

This creates a "living diorama" effect where badges float above content, backgrounds recede, and users can perceive actual spatial relationships.

  

---

## Visual Design

### Anatomy

```

┌─────────────────────────────────────────────┐

│  ┌─────────────────────────────────────┐    │

│  │         DEPTH LAYER: -1             │    │  ← Background (recedes)

│  │         (furthest back)             │    │

│  │    ┌───────────────────────────┐    │    │

│  │    │     DEPTH LAYER: 0        │    │    │  ← Content (neutral)

│  │    │     (neutral plane)       │    │    │

│  │    │  ┌─────────────────────┐  │    │    │

│  │    │  │   DEPTH LAYER: 0.5  │  │    │    │  ← Elevated content

│  │    │  └─────────────────────┘  │    │    │

│  │    └───────────────────────────┘    │    │

│  └─────────────────────────────────────┘    │

│                                ┌────────┐   │

│                                │DEPTH: 1│   │  ← Badge (floats highest)

│                                └────────┘   │

│ ············· GLARE OVERLAY ·············· │  ← Light reflection

└─────────────────────────────────────────────┘

       ↑

  PERSPECTIVE CONTAINER

```

  

### Depth Scale

  

| Depth Value | Visual Position | Typical Use Case |

|-------------|-----------------|------------------|

| -1.0 | Furthest back | Background images, patterns |

| -0.5 | Slightly back | Secondary backgrounds |

| 0 | Neutral plane | Main content, text |

| +0.5 | Slightly forward | Highlighted content, CTAs |

| +1.0 | Furthest forward | Badges, tags, floating elements |

  

### States

#### Idle State

- Card is flat (no rotation)

- All layers at base positions (no Z separation)

- Glare invisible or minimal

  

#### Hover State

- Card tilts toward cursor

- Layers separate based on depth values

- Glare visible, following cursor position

  

#### Transition State (Enter/Exit)

- Smooth interpolation between idle and hover

- Exit uses spring easing for natural "settle" effect

  

---

  

## Interaction Model

  

### Tilt Calculation

  

The card tilts based on cursor position relative to card center:

  

```

┌─────────────────────────────────────┐

│               ↑                     │

│               │ -Y rotation         │

│               │                     │

│  ←────────────┼────────────→        │

│  -X rotation  │  +X rotation        │

│               │                     │

│               │ +Y rotation         │

│               ↓                     │

└─────────────────────────────────────┘

```

  

**Formulas:**

  

```

centerX = cardLeft + (cardWidth / 2)

centerY = cardTop + (cardHeight / 2)

  

percentX = (cursorX - centerX) / (cardWidth / 2)   // Range: -1 to +1

percentY = (cursorY - centerY) / (cardHeight / 2)  // Range: -1 to +1

  

rotateY = percentX * intensity    // Positive = tilt right

rotateX = -percentY * intensity   // Negative because Y-axis is inverted

```

  

Where `intensity` is the maximum tilt angle in degrees (recommended: 10-25°).

  

### Layer Parallax Calculation

  

Each layer moves based on the card's rotation and its depth value:

  

```

translateX = rotateY * depth * parallaxFactor

translateY = -rotateX * depth * parallaxFactor

translateZ = depth * depthScale * hoverMultiplier

```

  

Where:

- `depth` is the layer's assigned depth (-1 to +1)

- `parallaxFactor` controls horizontal/vertical shift (recommended: 0.3-0.8)

- `depthScale` is the Z separation in pixels (recommended: 40-80)

- `hoverMultiplier` is 1 when hovered, 0 when idle

  

### Glare Position

  

The glare follows cursor position within the card:

  

```

glareX = ((cursorX - cardLeft) / cardWidth) * 100   // Percentage 0-100

glareY = ((cursorY - cardTop) / cardHeight) * 100   // Percentage 0-100

```

  

---

  

## Architecture

  

### Component Structure

  

```

DepthCard (Container)

├── Perspective Wrapper

│   └── Transform Container (receives tilt rotation)

│       ├── DepthLayer (depth: -1) ─── Background

│       ├── DepthLayer (depth: 0)  ─── Content

│       ├── DepthLayer (depth: 0.5) ── Elevated

│       ├── DepthLayer (depth: 1)  ─── Floating

│       └── Glare Overlay

```

  

### State Requirements

  

**DepthCard must track:**

- `isHovered: boolean` — Whether cursor is over card

- `rotateX: number` — Current X-axis rotation (degrees)

- `rotateY: number` — Current Y-axis rotation (degrees)

- `glarePosition: { x: number, y: number }` — Glare center (percentage)

  

**DepthCard must provide to children:**

- `rotateX` — For parallax calculation

- `rotateY` — For parallax calculation  

- `isHovered` — For transition timing

- `depthScale` — For Z translation calculation

  

### Data Flow

  

```

User moves cursor

       │

       ▼

DepthCard captures mouse position

       │

       ▼

Calculate rotateX, rotateY, glarePosition

       │

       ▼

Update state / trigger re-render

       │

       ▼

DepthCard applies tilt transform

       │

       ▼

Each DepthLayer reads context, calculates own transform

       │

       ▼

Glare overlay updates gradient position

```

  

### Communication Pattern

  

Use a context/provider pattern (or equivalent) to share tilt state with child layers:

  

```

// Pseudocode - Framework agnostic concept

  

DepthContext = {

  rotateX: number,

  rotateY: number,

  isHovered: boolean,

  depthScale: number

}

  

DepthCard:

  - Creates context with current state

  - Wraps children in context provider

  - Handles all mouse events

  

DepthLayer:

  - Consumes context

  - Calculates own transform based on depth prop

  - Applies transform to wrapper element

```

  

---

  

## Component API

  

### DepthCard

  

**Props/Inputs:**

  

| Property | Type | Default | Description |

|----------|------|---------|-------------|

| `intensity` | number | 15 | Maximum tilt angle in degrees |

| `depthScale` | number | 50 | Z-axis separation in pixels |

| `glareOpacity` | number | 0.2 | Maximum glare opacity (0-1) |

| `glareColor` | string | "white" | Glare gradient color |

| `transitionDuration` | number | 400 | Exit transition in ms |

| `easing` | string | "cubic-bezier(0.34, 1.56, 0.64, 1)" | Exit easing function |

| `disabled` | boolean | false | Disable all interactions |

  

**Events/Outputs:**

  

| Event | Payload | Description |

|-------|---------|-------------|

| `onTiltChange` | `{ rotateX, rotateY }` | Fired on every tilt update |

| `onHoverStart` | — | Fired when cursor enters |

| `onHoverEnd` | — | Fired when cursor leaves |

  

### DepthLayer

  

**Props/Inputs:**

  

| Property | Type | Default | Description |

|----------|------|---------|-------------|

| `depth` | number | 0 | Z-position from -1 (back) to +1 (front) |

| `parallaxFactor` | number | 0.5 | Multiplier for X/Y parallax movement |

  

---

  

## CSS/Styling Requirements

  

### Essential Styles for DepthCard

  

```css

.depth-card {

  /* Enable 3D space for children */

  perspective: 1000px;

  perspective-origin: center center;

}

  

.depth-card-inner {

  /* Preserve 3D transforms for children */

  transform-style: preserve-3d;

  /* Applied dynamically */

  transform: rotateX(var(--rotate-x)) rotateY(var(--rotate-y));

  /* Smooth transitions */

  transition: transform 0.4s cubic-bezier(0.34, 1.56, 0.64, 1);

}

  

.depth-card-inner.is-hovered {

  /* Faster response while hovering */

  transition: transform 0.1s ease-out;

}

```

  

### Essential Styles for DepthLayer

  

```css

.depth-layer {

  /* Applied dynamically based on calculations */

  transform: translate3d(

    var(--translate-x),

    var(--translate-y),

    var(--translate-z)

  );

  /* Match parent transition timing */

  transition: transform 0.4s cubic-bezier(0.34, 1.56, 0.64, 1);

}

  

.depth-layer.is-hovered {

  transition: transform 0.1s ease-out;

}

```

  

### Glare Overlay Styles

  

```css

.depth-card-glare {

  position: absolute;

  inset: 0;

  pointer-events: none;

  border-radius: inherit;

  z-index: 100;

  /* Applied dynamically */

  background: radial-gradient(

    circle at var(--glare-x) var(--glare-y),

    rgba(255, 255, 255, var(--glare-opacity)) 0%,

    transparent 60%

  );

}

```

  

---

  

## Animation Specifications

  

### Hover Enter

  

```

Property: transform (card tilt)

Duration: Immediate (0.1s ease-out for smoothing)

Behavior: Direct response to cursor position

```

  

### Hover Exit

  

```

Property: transform (card returns to flat)

Duration: 400-500ms

Easing: cubic-bezier(0.34, 1.56, 0.64, 1) — Spring overshoot

Behavior: Card "settles" back with slight bounce

```

  

### Layer Separation

  

```

Property: transform (translate3d)

Duration: Matches card transition

Easing: Matches card easing

Behavior: Layers smoothly separate/collapse with card tilt

```

  

### Glare Movement

  

```

Property: background (radial-gradient position)

Duration: 0.1s while hovered, 0.4s on exit

Easing: ease-out

Behavior: Follows cursor, fades on exit

```

  

---

  

## Reference Implementation: Depth Visualization

  

This minimal example demonstrates all core mechanics. Implement this first to validate your approach before building complex card layouts.

  

### Visual Target

  

```

┌────────────────────────────────────────────────────┐

│                                          ┌───────┐ │

│  ┌──────────────────────────────────┐    │depth:1│ │

│  │                                  │    │(front)│ │

│  │       depth: -1 (back)           │    └───────┘ │

│  │       [indigo tinted]            │              │

│  │                                  │              │

│  │    ┌────────────────────────┐    │              │

│  │    │                        │    │              │

│  │    │    depth: 0 (neutral)  │    │              │

│  │    │    [slate colored]     │    │              │

│  │    │                        │    │              │

│  │    │  ┌──────────────────┐  │    │              │

│  │    │  │                  │  │    │              │

│  │    │  │   depth: 0.5     │  │    │              │

│  │    │  │  [violet tinted] │  │    │              │

│  │    │  │                  │  │    │              │

│  │    │  └──────────────────┘  │    │              │

│  │    │                        │    │              │

│  │    └────────────────────────┘    │              │

│  │                                  │              │

│  └──────────────────────────────────┘              │

│                                                    │

└────────────────────────────────────────────────────┘

```

  

### Structure

  

```

DepthCard

├── Container (rounded, semi-transparent, padding)

│   ├── DepthLayer depth={-1}

│   │   └── Box (indigo tint, label "depth: -1 (back)")

│   │

│   ├── DepthLayer depth={0}

│   │   └── Box (slate, label "depth: 0 (neutral)")

│   │

│   ├── DepthLayer depth={0.5}

│   │   └── Box (violet tint, label "depth: 0.5")

│   │

│   └── DepthLayer depth={1}

│       └── Badge (amber, absolute top-right, label "depth: 1 (front)")

```

  

### Specifications

  

**Card Container:**

- Width: 320px

- Background: `rgba(30, 41, 59, 0.5)` with backdrop blur

- Border: 1px solid `rgba(255, 255, 255, 0.1)`

- Border radius: 16px

- Padding: 32px

  

**DepthCard Settings:**

- intensity: 25

- depthScale: 80

- glareOpacity: 0.1

  

**Layer: depth={-1} (Back)**

- Position: absolute, inset 16px from container edges

- Background: `rgba(99, 102, 241, 0.2)` (indigo)

- Border: 1px solid `rgba(99, 102, 241, 0.3)`

- Border radius: 12px

- Content: Centered text "depth: -1 (back)"

- Text color: `rgb(129, 140, 248)` (indigo-400)

- Font: monospace, 14px

  

**Layer: depth={0} (Neutral)**

- Position: relative (normal flow)

- Margin top: 32px (to show behind layer)

- Background: `rgba(51, 65, 85, 0.5)` (slate)

- Border: 1px solid `rgba(255, 255, 255, 0.1)`

- Border radius: 12px

- Padding: 24px

- Content: Text "depth: 0 (neutral)"

- Text color: `rgb(203, 213, 225)` (slate-300)

- Font: monospace, 14px

  

**Layer: depth={0.5}**

- Position: relative (normal flow)

- Margin top: 16px

- Background: `rgba(139, 92, 246, 0.2)` (violet)

- Border: 1px solid `rgba(139, 92, 246, 0.3)`

- Border radius: 12px

- Padding: 16px

- Content: Text "depth: 0.5"

- Text color: `rgb(167, 139, 250)` (violet-400)

- Font: monospace, 14px

  

**Layer: depth={1} (Front Badge)**

- Position: absolute, top 16px, right 16px

- Background: `rgb(245, 158, 11)` (amber-500)

- Text color: `rgb(69, 26, 3)` (amber-950)

- Padding: 4px 12px

- Border radius: 9999px (pill)

- Font: monospace, 12px, bold

- Box shadow: `0 4px 12px rgba(0, 0, 0, 0.3)`

- Content: Text "depth: 1 (front)"

  

### Expected Behavior

  

1. **Idle:** All layers appear stacked flat

2. **Hover top-left corner:** Card tilts up-left, back layer shifts right-down, front badge shifts left-up

3. **Hover bottom-right corner:** Card tilts down-right, back layer shifts left-up, front badge shifts right-down

4. **Exit:** Card springs back flat with overshoot, layers collapse smoothly

  

---

  

## Accessibility

  

### Reduced Motion

  

When user prefers reduced motion:

- Disable tilt effect (card stays flat)

- Disable layer separation

- Keep glare subtle or disable entirely

- Content remains fully accessible

  

```css

@media (prefers-reduced-motion: reduce) {

  .depth-card-inner,

  .depth-layer {

    transition: none;

    transform: none !important;

  }

}

```

  

### Keyboard Interaction

  

- Card effects are decorative; no keyboard activation needed

- Ensure any interactive elements inside layers remain focusable

- Focus states should be clearly visible despite 3D effects

  

### Screen Readers

  

- 3D effects are purely visual; no ARIA needed

- Ensure content hierarchy in DOM matches visual hierarchy

  

---

  

## Performance Considerations

  

### Optimization Strategies

  

1. **Use `transform` only** — Avoid animating layout properties

2. **Use `will-change: transform`** — Hint to browser for optimization

3. **Throttle mouse events** — Limit to 60fps (use requestAnimationFrame)

4. **Avoid shadows on layers** — Use on card container only

5. **Limit blur effects** — backdrop-filter is expensive

  

### GPU Acceleration

  

Ensure transforms trigger GPU acceleration:

  

```css

.depth-layer {

  /* Force GPU layer */

  transform: translate3d(0, 0, 0);

  will-change: transform;

}

```

  

### Event Handling

  

```javascript

// Throttle mouse move to animation frames

let frameRequested = false;

  

function handleMouseMove(event) {

  if (frameRequested) return;

  frameRequested = true;

  requestAnimationFrame(() => {

    updateTilt(event);

    frameRequested = false;

  });

}

```

  

---

  

## Testing Checklist

  

### Visual Tests

- [ ] Card tilts correctly toward cursor position

- [ ] Layers separate in correct direction based on depth

- [ ] Negative depth moves opposite to positive depth

- [ ] Glare follows cursor position

- [ ] Exit animation has spring overshoot

- [ ] Depth visualization matches specification

  

### Interaction Tests

- [ ] Tilt responds immediately to cursor movement

- [ ] Exit transition is smooth (no snapping)

- [ ] Rapid mouse movements don't cause jitter

- [ ] Works correctly at card edges

- [ ] Multiple cards on page work independently

  

### Edge Cases

- [ ] Very fast cursor entry/exit

- [ ] Cursor leaving viewport while over card

- [ ] Window resize while hovering

- [ ] Touch devices (graceful fallback)

- [ ] Reduced motion preference respected

  

### Performance Tests

- [ ] Smooth 60fps during interaction

- [ ] No layout thrashing (check DevTools)

- [ ] GPU layers created correctly

- [ ] Memory stable during extended interaction

  

---

  

## Browser Support

  

### Required Features

- CSS `perspective` and `transform-style: preserve-3d`

- CSS `transform: rotate3d()` and `translate3d()`

- CSS custom properties (variables)

- `requestAnimationFrame`

- `getBoundingClientRect()`

  

### Minimum Versions

- Chrome 36+

- Firefox 16+

- Safari 9+

- Edge 12+

  

### Fallback Behavior

For unsupported browsers, card displays flat without interaction effects. Content remains fully accessible.