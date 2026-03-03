# Agent Status Notifier Widget

## Design & Architecture Brief + Product Requirements Document

**Version:** 1.0  
**Last Updated:** February 2026  
**Status:** Ready for Implementation

---

## 1. Executive Summary

### 1.1 Product Vision

A compact, always-visible desktop widget that monitors AI agent processes (Claude, GPT, local LLMs, automation scripts) and provides clear visual/audio notification when an agent completes a task or requires human input.

### 1.2 Problem Statement

When running AI agents or automated workflows, users frequently context-switch to other tasks. They need a persistent, glanceable indicator that alerts them precisely when attention is required—without constant polling or missing critical decision points.

### 1.3 Target Users

- Developers running AI coding assistants (Claude Code, Cursor, Copilot)
- Power users orchestrating multi-step agent workflows
- Automation engineers monitoring batch processes
- Anyone using AI tools that require periodic human-in-the-loop input

### 1.4 Success Metrics

| Metric | Target |
|--------|--------|
| Time to notice state change | < 3 seconds |
| Glanceability (identify state without focus) | 95% accuracy at 1m distance |
| Memory footprint | < 50MB RAM |
| CPU usage (idle) | < 1% |
| Setup time | < 2 minutes |

---

## 2. Design Specifications

### 2.1 Visual Identity

**Aesthetic Direction:** Retro-futuristic CRT terminal with skeuomorphic hardware frame and pixel art iconography. Inspired by JARVIS (Iron Man), classic sci-fi interfaces, and 1980s computing hardware.

**Core Principles:**
1. **High contrast** — Status must be identifiable in peripheral vision
2. **Nostalgic warmth** — Physical hardware textures create emotional connection
3. **Functional animation** — Motion serves purpose, not decoration
4. **Information density** — Maximum data in minimum space

### 2.2 Dimensions

```
┌─────────────────────────────────────────────────────┐
│  OUTER FRAME (with hardware chrome)                 │
│  Width:  432px (408px screen + 24px bezel)          │
│  Height: 198px (168px screen + 30px bezel/controls) │
│                                                     │
│  ┌─────────────────────────────────────────────┐   │
│  │  SCREEN AREA                                │   │
│  │  Width:  384px                              │   │
│  │  Height: 152px                              │   │
│  │  Padding: 16px                              │   │
│  └─────────────────────────────────────────────┘   │
│                                                     │
│  [ BTN ] [ BTN ] [ BTN ]              ● PWR        │
└─────────────────────────────────────────────────────┘
```

**Responsive Breakpoints:**
| Mode | Dimensions | Use Case |
|------|------------|----------|
| Compact | 320 × 160px | Minimal desktop footprint |
| Standard | 432 × 198px | Default desktop widget |
| Expanded | 540 × 240px | High-DPI / accessibility |

### 2.3 Color System

#### 2.3.1 Base Palette (Dark Theme)

| Token | Hex | RGB | Usage |
|-------|-----|-----|-------|
| `--bg-frame` | `#2a2a3a` | 42, 42, 58 | Hardware frame base |
| `--bg-frame-highlight` | `#3a3a4a` | 58, 58, 74 | Frame bevel highlight |
| `--bg-frame-shadow` | `#1a1a28` | 26, 26, 40 | Frame bevel shadow |
| `--bg-screen` | `#0a0a12` | 10, 10, 18 | CRT screen background |
| `--bg-panel` | `#00000066` | 0, 0, 0, 40% | Content panel overlay |
| `--border-subtle` | `#ffffff14` | 255, 255, 255, 8% | Subtle separators |
| `--text-primary` | `#ffffffcc` | 255, 255, 255, 80% | Primary text |
| `--text-muted` | `#ffffff40` | 255, 255, 255, 25% | Labels, secondary |
| `--text-dim` | `#ffffff1a` | 255, 255, 255, 10% | Disabled, decorative |

#### 2.3.2 Status Accent Colors

| Status | Primary | Dark | Glow (50% opacity) |
|--------|---------|------|---------------------|
| **Working** | `#00ffff` | `#008b8b` | `rgba(0, 255, 255, 0.5)` |
| **Waiting** | `#ffaa00` | `#8b5a00` | `rgba(255, 170, 0, 0.5)` |
| **Finished** | `#00ff66` | `#008833` | `rgba(0, 255, 102, 0.5)` |
| **Error** | `#ff4466` | `#8b2233` | `rgba(255, 68, 102, 0.5)` |

#### 2.3.3 Color Application Rules

```
STATUS INDICATOR:
  Icon        → status.primary (with glow)
  Label       → status.primary (with glow)
  Border      → status.dark
  Background  → status.glow at 15% opacity
  Box-shadow  → status.glow, 8-15px blur

HARDWARE FRAME:
  Use linear gradients mixing frame colors
  145° angle for consistent light source (top-left)
  Inset shadows for depth

DATA READOUT:
  Labels      → text-muted
  Values      → text-primary
  Borders     → border-subtle
```

### 2.4 Typography

#### 2.4.1 Font Stack

| Context | Font | Fallback Stack |
|---------|------|----------------|
| **Pixel text** (status, labels) | Press Start 2P | `"Courier New", monospace` |
| **Data values** | Press Start 2P | `"Courier New", monospace` |
| **System UI** (if needed) | SF Pro / Segoe UI | `-apple-system, sans-serif` |

**Font Loading:**
```
Google Fonts: https://fonts.googleapis.com/css2?family=Press+Start+2P&display=swap
```

#### 2.4.2 Type Scale

| Token | Size | Line Height | Letter Spacing | Usage |
|-------|------|-------------|----------------|-------|
| `--text-lg` | 12px | 1.2 | 1px | Status label |
| `--text-md` | 9px | 1.3 | 1px | Sublabel, messages |
| `--text-sm` | 7px | 1.4 | 0.5px | Data labels, timestamps |
| `--text-xs` | 5-6px | 1.4 | 0.5px | Button labels, PWR |

### 2.5 Iconography

#### 2.5.1 Pixel Icon Grid

All status icons rendered on an **8×8 pixel grid**. Each pixel renders as a 3×3px or 4×4px square (configurable).

```
WORKING (processing/loading)        WAITING (input required)
  . . # # # # . .                     . . . # # . . .
  . # . . . . # .                     . . . # # . . .
  # . . # # . . #                     . . . # # . . .
  # . # . . # . #                     . . . # # . . .
  # . # . . # . #                     . . . # # . . .
  # . . # # . . #                     . . . . . . . .
  . # . . . . # .                     . . . # # . . .
  . . # # # # . .                     . . . # # . . .

FINISHED (complete/success)         ERROR (failed/alert)
  . . . . . . . .                     . # # # # # # .
  . . . . . . . #                     # . . . . . . #
  . . . . . . # .                     # . # # # # . #
  # . . . . # . .                     # . # . . # . #
  . # . . # . . .                     # . # # # # . #
  . . # # . . . .                     # . . . . . . #
  . . . # . . . .                     # . . # # . . #
  . . . . . . . .                     . # # # # # # .
```

#### 2.5.2 Icon Rendering

```
Pixel size:     3-4px per grid cell
Gap:            1px between cells
Glow effect:    box-shadow: 0 0 {pixel-size}px {status.primary}
Blink behavior: Opacity toggles 1.0 ↔ 0.3 (waiting state only)
Blink interval: 500ms
```

### 2.6 Layout Structure

```
┌─────────────────────────────────────────────────────────────┐
│ ● AGENT.SYS                                  v2.1  00:00:00 │  ← Header
├─────────────────────────────────────────────────────────────┤
│ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ │  ← Pixel border
├─────────────────────────────────────────────┬───────────────┤
│                                             │               │
│  ┌───────┐                                  │  SES   A7F3   │
│  │ ICON  │  PROCESSING                      │  TSK   REVW   │
│  │  8×8  │  TASK IN PROGRESS                │  MEM   64M    │
│  └───────┘  ████████████░░░░                │               │
│                                             │               │
├─────────────────────────────────────────────┴───────────────┤
│ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ │  ← Pixel border
└─────────────────────────────────────────────────────────────┘

HEADER:
  Left:   System name (color-coded to status)
  Right:  Version, Clock (HH:MM:SS)

MAIN PANEL (left, ~70% width):
  Pixel icon in bordered container
  Status label (large, glowing)
  Status message (small, muted)
  Progress indicator (contextual)

DATA PANEL (right, ~30% width):
  Key-value pairs, vertically stacked
  Monospace alignment

PIXEL BORDERS:
  Alternating filled/empty segments
  Color matches status.dark
```

### 2.7 Effects & Animation

#### 2.7.1 CRT Screen Effects

| Effect | Implementation | Intensity |
|--------|----------------|-----------|
| **Scanlines** | Horizontal 1px lines, 2px apart | 6% opacity |
| **Scanline sweep** | Animated horizontal bar | 30% opacity, 3s cycle |
| **Vignette** | Radial gradient darkening edges | 25% at corners |
| **Screen glare** | Diagonal gradient highlight | 2% opacity, top-left |
| **Curvature** | Subtle radial shadow | Optional, 15% at edges |

#### 2.7.2 Animation Specifications

| Animation | Duration | Easing | Trigger |
|-----------|----------|--------|---------|
| **Blink (icon + cursor)** | 500ms | step(1) | Waiting state only |
| **Progress bar** | Continuous | linear | Working state, synced to scan |
| **Scanline sweep** | 3000ms | linear | Always running |
| **State transition** | 150ms | ease-out | Status change |
| **Button press** | 50ms | ease-out | On click |
| **Glow pulse** | 2000ms | ease-in-out | Waiting state (optional) |

#### 2.7.3 Skeuomorphic Hardware Details

**Frame bevels:**
```css
/* Top-left highlight */
inset 2px 2px 4px rgba(255, 255, 255, 0.1)

/* Bottom-right shadow */
inset -2px -2px 4px rgba(0, 0, 0, 0.5)

/* Drop shadow */
0 8px 32px rgba(0, 0, 0, 0.6)
```

**Screw details:**
```
Position:     4 corners, 5px inset
Size:         10px diameter
Gradient:     145° from highlight to shadow
Inner circle: 6px diameter, slightly darker
```

**Button states:**
```
NORMAL:
  Background: linear-gradient(180deg, frame-highlight, frame-shadow)
  Shadow:     inset highlight top-left, shadow bottom-right, drop shadow

PRESSED / ACTIVE:
  Background: linear-gradient(180deg, frame-shadow, frame-base)
  Shadow:     inset shadow all around, glow in status color
  Transform:  translateY(1px)
```

**LED indicator:**
```
Size:       6px diameter
Background: status.primary
Shadow:     0 0 4px {status.primary}
            inset 0 -1px 2px rgba(0, 0, 0, 0.5)  /* bottom shadow for dome effect */
```

---

## 3. Component Architecture

### 3.1 Component Hierarchy

```
AgentNotifierWidget
├── FrameContainer
│   ├── Screw × 4
│   ├── ScreenBezel
│   │   ├── ScanlineOverlay
│   │   ├── ScanlineSweep
│   │   ├── VignetteOverlay
│   │   ├── GlareOverlay
│   │   └── ScreenContent
│   │       ├── Header
│   │       │   ├── SystemName
│   │       │   ├── VersionLabel
│   │       │   └── Clock
│   │       ├── PixelBorder (top)
│   │       ├── MainContent
│   │       │   ├── StatusPanel
│   │       │   │   ├── PixelIcon
│   │       │   │   ├── StatusLabel
│   │       │   │   ├── StatusMessage
│   │       │   │   └── ProgressIndicator (contextual)
│   │       │   └── DataPanel
│   │       │       └── DataRow × N
│   │       └── PixelBorder (bottom)
│   └── ControlBar
│       ├── ButtonGroup
│       │   └── HardwareButton × N
│       └── LEDIndicator
└── NotificationService (non-visual)
```

### 3.2 Component Specifications

#### 3.2.1 AgentNotifierWidget (Root)

**Responsibilities:**
- Window management (always-on-top, positioning)
- Global state coordination
- External API exposure
- Notification dispatch

**Props/Config:**
```typescript
interface WidgetConfig {
  position: 'top-left' | 'top-right' | 'bottom-left' | 'bottom-right' | { x: number, y: number };
  size: 'compact' | 'standard' | 'expanded';
  alwaysOnTop: boolean;
  opacity: number; // 0.0 - 1.0
  enableAudio: boolean;
  enableSystemNotifications: boolean;
  theme: 'dark' | 'light'; // light theme TBD
}
```

#### 3.2.2 StatusPanel

**Responsibilities:**
- Display current agent status
- Render appropriate icon and colors
- Show contextual progress/prompt

**State:**
```typescript
interface StatusState {
  status: 'idle' | 'working' | 'waiting' | 'finished' | 'error';
  label: string;
  message: string;
  progress?: number; // 0-100, optional
  timestamp: Date;
}
```

#### 3.2.3 DataPanel

**Responsibilities:**
- Display session metadata
- Real-time value updates

**Data Contract:**
```typescript
interface SessionData {
  sessionId: string;
  taskName: string;
  elapsedTime: Duration;
  customFields?: Record<string, string | number>;
}
```

#### 3.2.4 PixelIcon

**Responsibilities:**
- Render 8×8 pixel grid
- Apply glow effects
- Handle blink animation

**Props:**
```typescript
interface PixelIconProps {
  pattern: string[]; // 8 strings of 8 chars each ('.' or '#')
  color: string;
  pixelSize: number;
  glow: boolean;
  blink: boolean;
  blinkInterval: number;
}
```

#### 3.2.5 ProgressIndicator

**Variants:**
1. **Segmented bar** (working) — 12-16 discrete blocks
2. **Blinking cursor** (waiting) — Single rectangle + text
3. **Checkmark** (finished) — Static success indicator
4. **None** (idle/error)

### 3.3 State Machine

```
                    ┌─────────┐
                    │  IDLE   │
                    └────┬────┘
                         │ start_task
                         ▼
          ┌──────────────────────────────┐
          │           WORKING            │◄──────────┐
          └──────────────┬───────────────┘           │
                         │                           │
           ┌─────────────┼─────────────┐             │
           │             │             │             │
           ▼             ▼             ▼             │
    ┌──────────┐  ┌──────────┐  ┌──────────┐        │
    │ WAITING  │  │ FINISHED │  │  ERROR   │        │
    └────┬─────┘  └──────────┘  └────┬─────┘        │
         │                           │              │
         │ user_responded            │ retry        │
         └───────────────────────────┴──────────────┘

TRANSITIONS:
  IDLE → WORKING:      start_task
  WORKING → WAITING:   needs_input
  WORKING → FINISHED:  task_complete
  WORKING → ERROR:     task_failed
  WAITING → WORKING:   user_responded
  ERROR → WORKING:     retry
  ANY → IDLE:          reset
```

---

## 4. Data Contracts & Integration

### 4.1 Status Update Protocol

#### 4.1.1 JSON Message Format

```json
{
  "type": "status_update",
  "timestamp": "2026-02-01T14:32:00.000Z",
  "payload": {
    "status": "waiting",
    "label": "WAITING",
    "message": "Approve code changes?",
    "session": {
      "id": "a7f3-2d91",
      "task": "code_review",
      "elapsed_ms": 167000
    },
    "metadata": {
      "agent": "claude-code",
      "files_modified": 3,
      "lines_changed": 47
    }
  }
}
```

#### 4.1.2 Status Enum

```typescript
enum AgentStatus {
  IDLE = 'idle',
  WORKING = 'working',
  WAITING = 'waiting',
  FINISHED = 'finished',
  ERROR = 'error'
}
```

#### 4.1.3 Full Message Schema

```typescript
interface StatusMessage {
  type: 'status_update' | 'ping' | 'config';
  timestamp: string; // ISO 8601
  payload: StatusPayload | PingPayload | ConfigPayload;
}

interface StatusPayload {
  status: AgentStatus;
  label?: string;        // Override default label
  message?: string;      // Contextual description
  progress?: number;     // 0-100 for working state
  session: {
    id: string;
    task: string;
    elapsed_ms: number;
    started_at?: string;
  };
  metadata?: Record<string, unknown>;
}

interface PingPayload {
  // Empty, used for keepalive
}

interface ConfigPayload {
  position?: WidgetPosition;
  opacity?: number;
  enableAudio?: boolean;
}
```

### 4.2 Integration Methods

#### 4.2.1 File Watcher

Simplest integration. Agent writes status to a JSON file; widget watches for changes.

```
Path:     ~/.agent-notifier/status.json
          %APPDATA%\AgentNotifier\status.json (Windows)

Format:   StatusPayload (see above)

Polling:  500ms (fallback if no filesystem events)
```

**Pros:** Zero dependencies, works with any language  
**Cons:** Latency, filesystem overhead

#### 4.2.2 Named Pipe / Unix Socket

Low-latency local IPC.

```
Unix:     /tmp/agent-notifier.sock
Windows:  \\.\pipe\AgentNotifier

Protocol: Newline-delimited JSON (NDJSON)
```

**Pros:** Fast, bidirectional  
**Cons:** Platform-specific handling

#### 4.2.3 Local HTTP/WebSocket Server

Widget runs a local server; agents POST updates or connect via WebSocket.

```
HTTP Endpoint:
  POST http://localhost:9847/status
  Content-Type: application/json
  Body: StatusMessage

WebSocket:
  ws://localhost:9847/ws
  Messages: StatusMessage (JSON)
```

**Pros:** Language-agnostic, supports remote agents  
**Cons:** Port conflicts, security considerations

#### 4.2.4 CLI Tool

Companion CLI for one-shot updates from shell scripts.

```bash
# Update status
agent-notify --status working --task "build" --message "Compiling..."

# Mark waiting
agent-notify --status waiting --message "Deploy to production?"

# Mark complete
agent-notify --status finished

# With session
agent-notify --status working --session abc123 --progress 45
```

### 4.3 Notification Dispatch

#### 4.3.1 Triggers

| Event | Audio | System Toast | Flash Window |
|-------|-------|--------------|--------------|
| → WAITING | ✓ Chime | ✓ "Input required" | ✓ |
| → FINISHED | ✓ Success | Optional | ✓ (once) |
| → ERROR | ✓ Alert | ✓ "Task failed" | ✓ |
| → WORKING | — | — | — |

#### 4.3.2 Audio Assets

```
/assets/audio/
├── chime-waiting.wav    # 2-tone ascending, 0.5s
├── chime-finished.wav   # 3-tone major chord, 0.8s
├── chime-error.wav      # 2-tone descending, 0.6s
└── tick.wav             # Subtle tick for progress (optional)

Format:   WAV or OGG, 44.1kHz, mono
Volume:   User-configurable, default 50%
```

---

## 5. Platform Implementation Notes

### 5.1 Windows (WinUI 3 / WPF)

```
Window Configuration:
  - ExtendsContentIntoTitleBar = true
  - OverlappedPresenter: IsAlwaysOnTop, no resize/min/max
  - Transparent background with custom chrome

Rendering:
  - Use CompositionAPI for blur/acrylic (optional)
  - DispatcherTimer for animations
  - Custom pixel rendering via Canvas or Grid

IPC:
  - NamedPipeServerStream for pipe
  - FileSystemWatcher for file-based
  - HttpListener or Kestrel for HTTP

System Tray:
  - NotifyIcon (WPF) or custom implementation
  - Context menu for settings/quit
```

### 5.2 macOS (SwiftUI / AppKit)

```
Window Configuration:
  - NSPanel with .nonactivatingPanel
  - window.level = .floating
  - window.styleMask = [.borderless]
  - window.isOpaque = false

Rendering:
  - SwiftUI Canvas for pixel icons
  - NSVisualEffectView for blur (optional)
  - CADisplayLink for animations

IPC:
  - Unix domain socket via NWConnection
  - FSEvents for file watching
  - Local NWListener for HTTP

Menu Bar:
  - NSStatusItem for menu bar presence
  - NSPopover for quick access
```

### 5.3 Linux (GTK / Qt)

```
Window Configuration:
  - GTK: gtk_window_set_keep_above, gtk_window_set_decorated(false)
  - Qt: setWindowFlags(Qt::FramelessWindowHint | Qt::WindowStaysOnTopHint)

Rendering:
  - Cairo (GTK) or QPainter (Qt) for custom drawing
  - CSS-like styling in GTK4

IPC:
  - Unix socket via GSocket (GTK) or QLocalSocket (Qt)
  - inotify for file watching
  - libsoup (GTK) or QHttpServer (Qt) for HTTP

System Tray:
  - libappindicator or StatusNotifierItem (modern)
  - Legacy systray protocol (XEmbed) fallback
```

### 5.4 Electron / Web

```
Window Configuration:
  - BrowserWindow: frame: false, alwaysOnTop: true, transparent: true
  - resizable: false, skipTaskbar: true (optional)

Rendering:
  - Pure CSS/JS as per React prototype
  - backdrop-filter for blur
  - CSS animations preferred over JS

IPC:
  - IPC between main/renderer for system integration
  - WebSocket server in main process
  - fs.watch for file-based integration

System Tray:
  - Electron Tray API
  - nativeImage for dynamic icons
```

### 5.5 Cross-Platform Frameworks

| Framework | Suitability | Notes |
|-----------|-------------|-------|
| **Tauri** | ★★★★★ | Rust backend, web frontend, tiny footprint |
| **Flutter** | ★★★★☆ | Good for complex UI, larger binary |
| **.NET MAUI** | ★★★☆☆ | Windows/macOS only for desktop |
| **Avalonia** | ★★★★☆ | WPF-like, true cross-platform |
| **Uno Platform** | ★★★★☆ | WinUI everywhere, good for existing XAML skills |

---

## 6. Accessibility

### 6.1 Requirements

| Criterion | Implementation |
|-----------|----------------|
| **Color contrast** | All text ≥ 4.5:1 against background |
| **Motion sensitivity** | Respect `prefers-reduced-motion`; disable scanline sweep |
| **Screen reader** | Announce status changes via live region / accessibility API |
| **Keyboard navigation** | Tab through buttons, Enter/Space to activate |
| **High contrast mode** | Detect and switch to simplified high-contrast theme |

### 6.2 Reduced Motion Mode

When `prefers-reduced-motion: reduce`:
- Disable scanline sweep animation
- Disable blink (use static indicators)
- Disable glow pulse
- Keep state transitions instant

### 6.3 Screen Reader Announcements

```
On status change:
  "Agent status: Waiting. Input required."
  "Agent status: Complete. Task finished successfully."
  "Agent status: Error. Task failed."

Live region (web):
  <div role="status" aria-live="assertive" aria-atomic="true">
    {statusMessage}
  </div>

Windows:
  AutomationPeer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged)

macOS:
  NSAccessibility.postNotification(.announcementRequested, ...)
```

---

## 7. Configuration & Persistence

### 7.1 Config File Location

```
Windows:  %APPDATA%\AgentNotifier\config.json
macOS:    ~/Library/Application Support/AgentNotifier/config.json
Linux:    ~/.config/agent-notifier/config.json
```

### 7.2 Config Schema

```json
{
  "$schema": "https://agent-notifier.dev/config-schema.json",
  "version": 1,
  "window": {
    "position": "bottom-right",
    "offset": { "x": 20, "y": 20 },
    "size": "standard",
    "opacity": 1.0,
    "alwaysOnTop": true
  },
  "notifications": {
    "audio": {
      "enabled": true,
      "volume": 0.5,
      "sounds": {
        "waiting": "default",
        "finished": "default",
        "error": "default"
      }
    },
    "system": {
      "enabled": true,
      "onlyWhenMinimized": false
    },
    "flash": {
      "enabled": true
    }
  },
  "integration": {
    "method": "socket",
    "socket": {
      "path": "/tmp/agent-notifier.sock"
    },
    "http": {
      "port": 9847,
      "host": "127.0.0.1"
    },
    "file": {
      "path": "~/.agent-notifier/status.json"
    }
  },
  "appearance": {
    "theme": "dark",
    "reducedMotion": "system",
    "customColors": null
  },
  "advanced": {
    "startOnLogin": true,
    "checkForUpdates": true,
    "debugMode": false
  }
}
```

---

## 8. Future Considerations

### 8.1 Potential Enhancements

- **Multi-agent support** — Tabs or stacked indicators for multiple concurrent agents
- **History log** — Expandable panel showing recent status changes
- **Quick actions** — Configurable buttons that send commands back to agent
- **Themes** — Additional visual themes (green phosphor, amber CRT, light mode)
- **Plugins** — Extensible integration system for specific tools
- **Mobile companion** — Push notifications to phone when away from desk
- **Analytics** — Track time spent in each state, waiting time metrics

### 8.2 Out of Scope (v1)

- Two-way agent control (beyond notifications)
- Multi-monitor spanning
- Cloud sync of configuration
- Team/shared dashboard view
- Embedded terminal or log viewer

---

## 9. Appendix

### 9.1 Asset Checklist

```
□ Font: Press Start 2P (Google Fonts or bundled)
□ Audio: chime-waiting.wav, chime-finished.wav, chime-error.wav
□ Icons: App icon (multiple sizes), tray icons (16×16, 32×32)
□ Screenshots: For documentation and store listings
```

### 9.2 Reference Implementation

The React/JSX prototype serves as the reference implementation for visual styling:
- `jarvis-pixel-widget.jsx` — Complete styling reference
- Run in any React environment to preview

### 9.3 Glossary

| Term | Definition |
|------|------------|
| **Agent** | Any automated process (AI assistant, script, workflow) that runs tasks |
| **Waiting state** | Agent is blocked, requiring human input to proceed |
| **Session** | A single task execution from start to finish |
| **Widget** | The always-visible desktop notification panel |

---

## 10. Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02 | — | Initial specification |

---

*End of Document*
