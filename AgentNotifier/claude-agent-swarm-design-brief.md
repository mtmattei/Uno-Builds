# Claude Agent Swarm — UI Design Brief

**Version:** 1.0  
**Date:** February 2026  
**Design System:** Utilitarian Pixel Terminal  

---

## Design Philosophy

### Aesthetic Direction
**Utilitarian Terminal × Pixelated Retro**

This design merges industrial control-room utility with 8-bit pixel aesthetics. The result is a dashboard that feels like mission control software from an alternate timeline where CRT monitors never died—functional, information-dense, and visually distinctive.

### Core Principles
1. **Function over decoration** — Every element serves a purpose
2. **Monochromatic depth** — Grayscale hierarchy with singular accent
3. **Pixel precision** — Hard edges, grid alignment, no rounded corners
4. **Terminal authenticity** — Scanlines, monospace type, blinking cursors

---

## Color System

### Base Palette (Monochromatic Grays)

| Token | Hex | Usage |
|-------|-----|-------|
| `--bg-void` | `#0A0A0B` | Page background, deepest layer |
| `--bg-base` | `#0D0D0F` | Inset containers, input fields |
| `--bg-card` | `#131316` | Card backgrounds, panels |
| `--bg-elevated` | `#1A1A1F` | Headers, elevated surfaces |
| `--border-subtle` | `#1F1F24` | Subtle dividers |
| `--border-default` | `#252529` | Card borders, grid lines |
| `--border-strong` | `#333338` | Emphasized borders |
| `--text-muted` | `#444449` | Labels, tertiary text |
| `--text-subtle` | `#555560` | Secondary labels |
| `--text-secondary` | `#666670` | Inactive states |
| `--text-tertiary` | `#888890` | Body text, values |
| `--text-secondary-bright` | `#9999A3` | Task descriptions |
| `--text-primary` | `#E8E8ED` | Primary text, headings |

### Accent Color (Sugar Pink)

| Token | Hex | Usage |
|-------|-----|-------|
| `--accent` | `#FF6B9D` | Primary accent, active states |
| `--accent-muted` | `#CC5580` | Progress bar stripes |
| `--accent-glow` | `rgba(255,107,157,0.3)` | Shadows, glows |
| `--accent-bg` | `rgba(255,107,157,0.08)` | Badge backgrounds |
| `--accent-border` | `rgba(255,107,157,0.25)` | Badge borders |

### Status Colors

| Status | Color | Hex |
|--------|-------|-----|
| Processing | Sugar Pink | `#FF6B9D` |
| Completed | Mint Green | `#4ADE80` |
| Queued | Gray | `#666670` |
| Error | Red | `#EF4444` |

---

## Typography

### Font Stack

| Role | Font Family | Fallback |
|------|-------------|----------|
| Display / Labels | Press Start 2P | monospace |
| Data / Body | JetBrains Mono | Consolas, monospace |

### Type Scale

| Element | Font | Size | Weight | Color | Letter Spacing |
|---------|------|------|--------|-------|----------------|
| Page Title | Press Start 2P | 10px | 400 | `--accent` | 2px |
| Section Label | Press Start 2P | 7px | 400 | `--text-muted` | 1-2px |
| Micro Label | Press Start 2P | 5-6px | 400 | `--text-muted` | 1px |
| Stat Value (Large) | JetBrains Mono | 24px | 700 | varies | -1px |
| Stat Value (Medium) | JetBrains Mono | 14px | 700 | varies | 0 |
| Body Text | JetBrains Mono | 11-12px | 400 | `--text-tertiary` | 0 |
| Micro Text | JetBrains Mono | 10px | 400 | `--text-muted` | 0 |
| Status Badge | Press Start 2P | 7px | 400 | status color | 0 |

---

## Layout Structure

### Page Hierarchy

```
┌─────────────────────────────────────────────────────────────┐
│  HEADER                                                     │
│  [Title + Subtitle]                        [LIVE indicator] │
├─────────────────────────────────────────────────────────────┤
│  SUMMARY STATS BAR (5-column grid)                          │
│  ┌─────────┬─────────┬─────────┬─────────┬─────────┐       │
│  │  TOTAL  │  ACTIVE │  TOKENS │ ELAPSED │   COST  │       │
│  └─────────┴─────────┴─────────┴─────────┴─────────┘       │
├─────────────────────────────────────────────────────────────┤
│  AGENT LIST                                                 │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  [●] Agent Name          TOKENS    TIME    [STATUS] │   │
│  │      AGT-XXX              X,XXX    XX:XX            │   │
│  ├─────────────────────────────────────────────────────┤   │
│  │  EXPANDED CONTENT (when selected)                   │   │
│  │  - Task description                                 │   │
│  │  - Progress bar                                     │   │
│  │  - Detail metrics grid (4 columns)                  │   │
│  └─────────────────────────────────────────────────────┘   │
│  [Repeat for each agent...]                                 │
├─────────────────────────────────────────────────────────────┤
│  FOOTER TICKER                                              │
│  [Scrolling system metrics]                                 │
└─────────────────────────────────────────────────────────────┘
```

### Spacing System

| Token | Value | Usage |
|-------|-------|-------|
| `--space-xs` | 4px | Inline gaps |
| `--space-sm` | 6-8px | Tight padding |
| `--space-md` | 10-14px | Card padding |
| `--space-lg` | 16px | Section padding |
| `--space-xl` | 20-24px | Container padding |

### Grid Specifications

- **Summary Bar:** 5 equal columns, 1px gap (border color fills gap)
- **Detail Metrics:** 4 equal columns, 12px gap
- **Agent List:** Single column, 8px gap between cards
- **Max Container Width:** 900px, centered

---

## Component Specifications

### 1. Summary Stat Cell

```
┌─────────────────┐
│                 │
│      24px       │  ← Large numeric value
│     "3,247"     │     (JetBrains Mono 700)
│                 │
│     6px gap     │
│                 │
│    "TOKENS"     │  ← Micro label
│                 │     (Press Start 2P, 6px)
└─────────────────┘
Background: --bg-card
Padding: 16px
Text-align: center
```

### 2. Agent Card (Collapsed)

```
┌──────────────────────────────────────────────────────────────┐
│ [●] Agent Name                    X,XXX   XX:XX   [STATUS]  │
│     AGT-XXX                       TOKENS  TIME              │
└──────────────────────────────────────────────────────────────┘

Structure:
- Left: Status dot (8×8px) + Name stack (name + ID)
- Right: Token stat + Time stat + Status badge
- Background: --bg-elevated (header area)
- Border: 1px solid --border-default
```

### 3. Agent Card (Expanded)

```
┌──────────────────────────────────────────────────────────────┐
│ [●] Agent Name                    X,XXX   XX:XX   [STATUS]  │
│     AGT-XXX                       TOKENS  TIME              │
├──────────────────────────────────────────────────────────────┤
│  CURRENT TASK                                                │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ ▸ Task description text here...                        │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  ████████████████████░░░░░░░░░░  (progress bar)             │
│                                                              │
│  MODEL       RATE        COST        QUEUE                   │
│  opus-4.5   25 t/s     $0.0487      #001                    │
└──────────────────────────────────────────────────────────────┘

Border: 1px solid --accent (highlighted when expanded)
```

### 4. Status Indicator Dot

| State | Size | Color | Effect |
|-------|------|-------|--------|
| Processing | 8×8px | `--accent` | Pulsing glow animation |
| Completed | 8×8px | `#4ADE80` | Static, no glow |
| Queued | 8×8px | `#666670` | Static, no glow |
| Error | 8×8px | `#EF4444` | Pulsing glow animation |

### 5. Status Badge

```
┌─────────┐
│  PROC   │
└─────────┘

Font: Press Start 2P, 7px
Padding: 4px 8px
Background: status color @ 8% opacity
Border: 1px solid status color @ 25% opacity
```

Status label mappings:
- Processing → "PROC"
- Completed → "DONE"
- Queued → "WAIT"
- Error → "ERR!"

### 6. Progress Bar

```
████████████████░░░░░░░░░░░░░░

Height: 4px
Background: --bg-elevated
Fill: Diagonal stripe pattern
  - Stripe 1: --accent (3px)
  - Stripe 2: --accent-muted (3px)
  - Pattern: repeating-linear-gradient at 90deg
```

### 7. Task Description Box

```
┌────────────────────────────────────────────┐
│ ▸ Task description text...                 │
└────────────────────────────────────────────┘

Background: --bg-base
Border: 1px solid --border-subtle
Padding: 10px 12px
Font: JetBrains Mono, 11px
Arrow (▸): --accent color
```

---

## Visual Effects

### 1. Scanline Overlay

Apply to cards and containers for CRT effect:
```css
background: repeating-linear-gradient(
  0deg,
  transparent,
  transparent 2px,
  rgba(0, 0, 0, 0.1) 2px,
  rgba(0, 0, 0, 0.1) 4px
);
```

### 2. Glow Effects

Active/accent elements get a soft glow:
```css
box-shadow: 0 0 8px var(--accent);        /* Status dots */
text-shadow: 0 0 20px var(--accent-glow); /* Accent values */
```

### 3. Pulse Animation

For active status indicators:
```css
@keyframes pulse {
  0%, 100% { opacity: 0.6; }
  50% { opacity: 1; }
}
animation: pulse 1.5s ease-in-out infinite;
```

### 4. Blink Animation

For cursor/processing indicators:
```css
@keyframes blink {
  0%, 50% { opacity: 1; }
  51%, 100% { opacity: 0; }
}
animation: blink 1s step-end infinite;
```

---

## Data Display Formats

| Data Type | Format | Example |
|-----------|--------|---------|
| Token Count | Locale string with commas | `3,247` |
| Time Elapsed | MM:SS with zero padding | `02:07` |
| Cost | USD with 3-4 decimals | `$0.121` or `$0.0487` |
| Rate | Integer + unit | `25 t/s` |
| Agent ID | AGT-XXX format | `AGT-001` |

---

## Interaction States

### Card Selection
- **Default:** Border `--border-default`
- **Hover:** Border `--border-strong` (optional)
- **Selected/Expanded:** Border `--accent`

### Value Updates
When token counts or time updates:
- Brief scale animation (1.0 → 1.05 → 1.0)
- Duration: 300ms ease

---

## Responsive Considerations

### Breakpoints

| Breakpoint | Behavior |
|------------|----------|
| > 900px | Full layout, 5-column stats |
| 600-900px | Stats wrap to 3+2 or stack |
| < 600px | Single column, stacked stats |

### Mobile Adaptations
- Summary stats: 2×3 grid or vertical stack
- Agent cards: Full width, maintain information hierarchy
- Font sizes: Maintain pixel fonts but allow container scaling

---

## Accessibility Notes

- Maintain minimum 4.5:1 contrast for body text
- Status indicators use both color AND text labels
- Animations respect `prefers-reduced-motion`
- Interactive elements have visible focus states

---

## Asset Checklist

- [ ] Press Start 2P font (Google Fonts)
- [ ] JetBrains Mono font (Google Fonts / JetBrains)
- [ ] Status icons (optional, dots are CSS-only)
- [ ] No external images required

---

## Reference Files

- `dashboard-preview.html` — Static HTML/CSS reference implementation
- `agent-dashboard.jsx` — React implementation with live state

Open `dashboard-preview.html` in any browser to see the design rendered.

---

## Design Preview

```
╔═══════════════════════════════════════════════════════════════════╗
║  CLAUDE AGENT SWARM                                    [●] LIVE   ║
║  Multi-agent orchestration dashboard                              ║
╠═══════════════════════════════════════════════════════════════════╣
║  ┌─────────┬─────────┬─────────┬─────────┬─────────┐             ║
║  │    5    │    3    │  8,098  │  02:07  │  $0.121 │             ║
║  │  TOTAL  │ ACTIVE  │ TOKENS  │ ELAPSED │   COST  │             ║
║  └─────────┴─────────┴─────────┴─────────┴─────────┘             ║
╠═══════════════════════════════════════════════════════════════════╣
║  AGENT INSTANCES                                                  ║
║  ┌───────────────────────────────────────────────────────────┐   ║
║  │ [●] Code Analyzer              3,247    02:07    [PROC]   │   ║
║  │     AGT-001                    TOKENS   TIME              │   ║
║  ├───────────────────────────────────────────────────────────┤   ║
║  │  CURRENT TASK                                             │   ║
║  │  ▸ Analyzing codebase structure and generating docs...    │   ║
║  │  ██████████████████████░░░░░░░░░░░                        │   ║
║  │  MODEL      RATE       COST       QUEUE                   │   ║
║  │  opus-4.5   25 t/s     $0.0487    #001                    │   ║
║  └───────────────────────────────────────────────────────────┘   ║
║  ┌───────────────────────────────────────────────────────────┐   ║
║  │ [●] Test Writer                1,856    01:04    [PROC]   │   ║
║  └───────────────────────────────────────────────────────────┘   ║
║  ┌───────────────────────────────────────────────────────────┐   ║
║  │ [●] PR Reviewer                2,103    01:29    [DONE]   │   ║
║  └───────────────────────────────────────────────────────────┘   ║
╠═══════════════════════════════════════════════════════════════════╣
║  ▪ ORCHESTRATOR v2.1.0  ▪ API LATENCY: 142ms  ▪ UPTIME: 99.97%   ║
╚═══════════════════════════════════════════════════════════════════╝
```

---

*End of Design Brief*
