# Progressive Hold-to-Reveal List Component

## Concept

A list where items start collapsed showing minimal information. Users press and hold to progressively reveal more content in stages. The longer the hold duration, the more detail appears. Once fully held, the item locks open until explicitly collapsed.

---

## Core Interaction

### Hold Mechanic

| Parameter | Value |
|-----------|-------|
| Total hold duration | 800ms |
| Update interval | 16ms (~60fps) |
| Lock threshold | 95% of duration (760ms) |

### Input Events

- **Start hold**: `PointerPressed` / `MouseDown` / `TouchStart`
- **End hold**: `PointerReleased` / `MouseUp` / `TouchEnd` / `PointerExited`

### Behavior Rules

1. On hold start: Begin incrementing progress from 0 → 1 over 800ms
2. On hold end before lock: Reset progress to 0 (animate back)
3. On reaching 95%: Lock the item in expanded state, stop incrementing
4. Locked items ignore hold input until collapsed via button

---

## State Machine

```
┌─────────────┐
│   IDLE      │ ← Initial state, progress = 0
└──────┬──────┘
       │ pointer down
       ▼
┌─────────────┐
│  HOLDING    │ ← Progress incrementing 0 → 1
└──────┬──────┘
       │
       ├── pointer up (progress < 0.95) → IDLE (reset progress)
       │
       ▼ progress >= 0.95
┌─────────────┐
│   LOCKED    │ ← Fully expanded, progress = 1
└──────┬──────┘
       │ collapse button clicked
       ▼
┌─────────────┐
│   IDLE      │
└─────────────┘
```

---

## Progressive Reveal Stages

Content reveals in 4 stages based on hold progress:

| Stage | Progress Threshold | Content Revealed |
|-------|-------------------|------------------|
| 0 | 0% - 29% | Title + Preview text only |
| 1 | 30% - 59% | + Description paragraph |
| 2 | 60% - 89% | + Meta tags row |
| 3 | 90% - 100% | + Action buttons |

### Stage Calculation

```
if progress < 0.30 → stage 0
else if progress < 0.60 → stage 1
else if progress < 0.90 → stage 2
else → stage 3
```

---

## Visual Feedback

### Progress Indicator

- Circular ring positioned at left edge of item
- Stroke dasharray animates from 0 to full circumference
- Center icon: "+" when collapsed, "−" when locked
- Icon rotates 180° on lock

### Item Container

- Subtle scale down while holding: `scale(0.995)`
- Background gradient sweep from left showing progress
- Elevated shadow when locked

### Reveal Animations

Each stage section animates in with:
- `max-height`: 0 → natural height
- `opacity`: 0 → 1
- `translateY`: -8px → 0
- Duration: 300-400ms
- Easing: ease-out or cubic-bezier(0.4, 0, 0.2, 1)

Action buttons additionally stagger with 50ms delay between each.

---

## Data Structure

```
ListItem {
    id: unique identifier
    title: string (primary text, always visible)
    preview: string (secondary text, always visible)
    details: string (stage 1 content)
    meta: dictionary/map of key-value pairs (stage 2 content)
    actions: array of action labels (stage 3 content)
}
```

---

## Layout Structure

```
┌────────────────────────────────────────────────────┐
│ ┌──────┐                                           │
│ │ ○    │  Title Text                    Preview    │
│ │ ring │                                           │
│ └──────┘  ─────────────────────────────────────    │
│           Description text appears here when       │
│           stage 1 is reached...                    │
│           ─────────────────────────────────────    │
│           ┌─────┐ ┌─────┐ ┌─────┐                  │
│           │meta1│ │meta2│ │meta3│  ← stage 2      │
│           └─────┘ └─────┘ └─────┘                  │
│           ─────────────────────────────────────    │
│           [Action 1] [Action 2] [Action 3] ← stg 3 │
│                                                    │
│                                    [Collapse] ← if locked
└────────────────────────────────────────────────────┘
```

---

## Implementation Checklist

### State Variables (per item)
- [ ] `holdProgress`: float 0-1
- [ ] `isHolding`: boolean
- [ ] `isLocked`: boolean
- [ ] `holdStartTime`: timestamp (for calculating elapsed)

### Timer/Animation Loop
- [ ] Start timer on pointer down
- [ ] Calculate progress = elapsed / 800ms
- [ ] Clamp progress to max 1.0
- [ ] Check lock threshold each tick
- [ ] Stop timer on pointer up or lock

### UI Elements
- [ ] Progress ring (SVG circle or arc drawing)
- [ ] Collapsible sections for each stage
- [ ] Collapse button (visible only when locked)
- [ ] Optional: "Hold to reveal" hint text

### Animations
- [ ] Ring stroke-dasharray binding to progress
- [ ] Section height/opacity transitions
- [ ] Scale transform on container while holding
- [ ] Button stagger animation on stage 3

### Edge Cases
- [ ] Pointer leaves item while holding → treat as release
- [ ] Rapid tap (< 100ms) → no visible change
- [ ] Multiple items can be locked simultaneously
- [ ] Action buttons must stop event propagation to prevent re-triggering hold

---

## Timing Reference

| Event | Duration/Timing |
|-------|----------------|
| Full hold to lock | 800ms |
| Progress update rate | 16ms |
| Section expand animation | 300-400ms |
| Button stagger delay | 50ms each |
| Scale down while holding | 0.995 (immediate) |
| Progress reset on release | immediate (or 200ms ease-out) |

---

## Accessibility Considerations

- Provide alternative expand trigger (tap/click toggle, or keyboard)
- Announce state changes to screen readers
- Ensure sufficient color contrast on progress indicator
- Consider reduced-motion preference (instant expand instead of animated)
