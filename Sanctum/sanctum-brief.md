# Sanctum: Product & Design Specification

## 1. Product Philosophy

**Core Proposition:** A calm-first attention curation tool designed to restore attention spans through minimalist filtering and intentional configuration.

**Tone:** Calm, Premium, Confident, Editorial, "Digital Wellness"

**Differentiation:** Unlike blockers (Freedom) or task lists (Todoist), Sanctum emphasizes *Synthesis* and *Mode-based Context*—moving from passive filtering to active agency through features like the Context Engine, Anti-To-Do, and Finite Feeds.

**Platform:** Web Application (Responsive)

---

## 2. Visual Design System

### A. Color Palette ("Stone")

A monochromatic, warm-gray scale anchored by high-contrast black and white to create a serious, paper-like aesthetic.

| Token | Hex | Tailwind | Usage |
|-------|-----|----------|-------|
| Base | `#FAFAF9` | `bg-stone-50` | Main app background |
| Surface | `#FFFFFF` | `bg-white` | Cards, elevated surfaces |
| Surface Alt | `#F5F5F4` | `bg-stone-100` | Secondary backgrounds, hover states |
| Dark Surface | `#1C1917` | `bg-stone-900` | Stats cards, primary actions |
| Ink | `#1C1917` | `text-stone-900` | Primary text, active states |
| Muted | `#78716C` | `text-stone-500` | Secondary text |
| Subtle | `#A8A29E` | `text-stone-400` | Disabled states, placeholders |
| Highlight | `#E7E5E4` | `border-stone-200` | Borders, dividers |
| Accent | `#8B5E3C` | `text-amber-700` | Brand moments, synthesis icons |

**Status Colors:**

| Status | Background | Text |
|--------|------------|------|
| Allowed/Success | `#DCFCE7` | `#15803D` |
| Batched/Warning | `#FEF3C7` | `#B45309` |
| Muted/Neutral | `#F5F5F4` | `#78716C` |
| Alert/Urgent | `#FEF2F2` | `#DC2626` |

### B. Typography

A classic pairing of a sharp serif for emotion and a clean sans-serif for utility.

**Headings:** Playfair Display (Serif)
- Usage: Page titles, manifestos, section headers, "Big Number" displays, synthesis quotes
- Weights: Regular (400), Bold (700)
- Vibe: Editorial, human, authoritative but gentle

**UI & Body:** Inter (Sans-serif)
- Usage: Buttons, labels, body copy, navigation, settings
- Weights: Regular (400), Medium (500), Semibold (600)
- Vibe: Utilitarian, highly legible, invisible

**Scale:**

| Role | Size | Font |
|------|------|------|
| Display | 3rem–4.5rem (48–72px) | Playfair Display |
| Section Header | 1.5rem–2.25rem (24–36px) | Playfair Display |
| Body | 1rem (16px) | Inter |
| Micro-label | 0.625rem–0.75rem (10–12px) | Inter, uppercase, wide tracking |

### C. Shape & Depth

**Radii:**
- Cards: `rounded-2xl` (16px)
- Primary containers: `rounded-3xl` (24px)
- Buttons: `rounded-full` (pill/stadium)

**Shadows:**
- Soft lift: `0 4px 20px -2px rgba(26, 26, 26, 0.04)`
- Hover elevation: Increase shadow spread on interaction

**Borders:** Thin, subtle borders (`1px solid #E7E5E4`) to define structure without harsh lines.

### D. Animation & Motion

**Entrance:**
- Elements fade in and slide up (`opacity: 0 → 1`, `translateY(10px) → 0`)
- Duration: 300–500ms, ease-out
- Staggering: 100ms, 300ms, 500ms delays for sequential elements

**Interactions:**
- Hover: `scale(1.02)`, shadow increases, borders darken
- Active/Press: `scale(0.98)`
- Transitions: 300ms duration

**Loading States:**
- Text updates: "Analyzing...", "Curating..."
- Container opacity: 50% while fetching
- Spinner (`RefreshCw` icon) only for feed generation

---

## 3. Layout & Information Architecture

### A. Responsive Grid

**Desktop (Split View):**
- Left Rail (Fixed, 288px): Navigation, Profile, Brand
- Main Stage (Fluid): Content area, `max-w-3xl` for readability

**Tablet:**
- Fluid containers with `max-w-2xl` for onboarding, `max-w-5xl` for dashboard

**Mobile (Stacked View):**
- Header (Sticky): Brand + Notifications
- Main Stage (Fluid): Scrollable content
- Bottom Nav (Fixed): Primary navigation tabs

### B. Spacing

- Internal card padding: 2rem–3rem (32–48px)
- Section spacing: 1.5rem–2rem between major elements

### C. Navigation Structure

| Section | Purpose |
|---------|---------|
| Command Center | Home dashboard, mode switching, status overview |
| Signal Feed | Finite scrolling curated content |
| Daily Digest | Batched notifications (Email, Slack, Calendar) |
| Rules Studio | Source control and logic configuration |
| Insights | Data visualization of attention reclaimed |

---

## 4. Component Library

### A. Primary Button

- Shape: Pill/Stadium (`rounded-full`)
- Padding: `px-8 py-4`
- Default: Dark Stone (`#1C1917`), white text
- Hover: `scale(1.02)`, elevated shadow
- Active: `scale(0.98)`
- Loading: Spinner overlay, text `opacity: 0`
- Disabled: `opacity: 0.8`

### B. Stepper (Onboarding Navigation)

- Position: Fixed top, `z-50`
- Visual: 3 horizontal bars
- Inactive: Small (`w-2`), color `#E7E5E4`
- Active/Past: Color `#1C1917`
- Current: Elongated (`w-8`), color `#1C1917`

### C. Mode Switcher

- Location: Top of Main Stage
- Active state: White background, shadow, ink text
- Inactive state: Transparent background, muted text
- Modes:
  - **Explore:** Standard browsing state
  - **Focus:** Triggers Anti-To-Do UI
  - **Recover:** Softer UI, suggests offline activities

### D. Selection Cards

**Goal Cards (Multi-select):**
- Unselected: White background, light border
- Selected: Dark border (`#1C1917`), checkmark icon

**Source Cards (Multi-select):**
- Unselected: White background, gray text
- Selected: Stone-100 background, dark text

### E. Status Pills

- Shape: Pill
- Interaction: Click cycles through states (Allowed → Batched → Muted → Allowed)
- Each state uses corresponding status colors

### F. Toggle (Slide to Enable)

- Initial: "Slide to Enable" with track UI
- Active: Dark background, toggle shifts right, "Focus Mode Active" text

---

## 5. User Flows

### Flow A: Onboarding (3 Steps)

#### Step 1: Welcome

**Goal:** Establish value proposition

**Content:**
- Centered icon: Wind/Air symbol
- Headline: "Silence the *digital noise*." (italicize "digital noise")
- Subtext: Explanation of curation and filtering
- Feature grid (3 columns): Focus First, Privacy Core, Calm UI

**Action:** "Set Preferences" button

**Transition:** Fade out → Slide in Step 2

#### Step 2: Personalize

**Goal:** Collect user inputs for AI generation

**Section 1 — Primary Intentions:**
- Options: Deep Focus, Restorative Sleep, Digital Calm
- Interaction: Multi-select goal cards

**Section 2 — Sources to Regulate:**
- Options: Social Feeds, Email, News, Instant Chat
- Interaction: Multi-select source cards

**Action:** "Generate Plan" button (fixed at bottom with gradient fade mask)

**Validation:** Must select at least 1 goal

**Logic:** Triggers AI generation (1.5s simulated delay)

#### Step 3: Ready (The Plan)

**Goal:** Present the AI-generated "Sanity Plan"

**Header:** "Ready to synchronize" badge (green status)

**Content:**
- AI-generated manifesto headline
- "Active Protocols" numbered list (3 rules)

**Interaction:**
1. "Slide to Enable" toggle → transforms to "Focus Mode Active"
2. Reveals "Enter Sanctum" primary button

**Action:** "Enter Sanctum" → Navigate to Dashboard

---

### Flow B: Dashboard (Command Center)

#### Layout

- Header: "Command Center" title, user avatar (initial), logout
- Grid: 2 columns (left: feed, right: tools)
- Mobile: Stacks vertically

#### Widget: Smart Synthesis (AI Summary)

- Concept: AI summary of missed notifications
- Visual: Gradient background, sparkle icon
- Items display: Icon, title, timestamp, summary, priority badge
- Hover interaction: Reveals "Expand" link
- AI Input: Current mode + context (calendar, email counts)
- AI Output: Single sentence, max 20 words, calming tone

#### Widget: Source Control

- Concept: Toggle permissions for app categories
- Items: Icon + Label + Status Pill
- Interaction: Click status pill to cycle states

#### Widget: Overview Stats

- Visual: Dark card (`bg-stone-900`), white text
- Data: "Attention reclaimed" with time value
- Decoration: Abstract blurred circle (top-right)
- Progress bar: Stone-700 track, green-400 fill

---

### Flow C: Focus Session (Anti-To-Do)

#### Trigger
Mode Switcher set to "Focus"

#### Input State
- Full-screen overlay (`z-50`)
- User enters single objective
- Visual: Large serif input, centered

#### Active State
- Displays objective in large type
- Countdown timer
- Controls: Pause/Play, +10 minutes
- Badge: "Auto-Responder Active" (green pulse animation)

---

### Flow D: Signal Feed (Calm Feed)

#### Philosophy
Anti-infinite scroll—feed is intentionally finite.

#### Item Card Structure
- Source label (uppercase, accent color)
- Tag (pill shape)
- Title (serif, large)
- Summary (sans, muted)

#### End State
- Visual: Checkmark icon in circle
- Copy: "You are all caught up."

---

### Flow E: Daily Digest

#### Categories
- **Must See:** Critical items (red/urgent styling)
- **Nice to Know:** Batched items (collapsed, 90% opacity)

#### AI Summary
- Input: List of batched notification headers
- Output: 2-sentence executive summary

---

### Flow F: Context Engine

#### Component
Toast notification (glassmorphism: `bg-white/90 backdrop-blur`)

#### Trigger
Time-based simulation (5 seconds after load)

#### Position
Floating at bottom center

#### Actions
- "Switch": Changes mode to Recover
- "Dismiss": Closes toast

---

## 6. Data Architecture

### Types

```typescript
interface UserPreferences {
  goals: string[];
  sources: string[];
}

interface SanityPlan {
  manifesto: string;
  rules: string[];
}

type AppMode = 'EXPLORE' | 'FOCUS' | 'RECOVER';

type ViewMode = 'onboarding' | 'dashboard';

interface AppState {
  viewMode: ViewMode;
  currentStep: number; // 1-3 for onboarding
  currentMode: AppMode;
  activeTab: string;
  isFocusSessionActive: boolean;
  showContextSuggestion: boolean;
  isLoading: boolean;
}
```

---

## 7. AI Integration (Gemini)

### Model
`gemini-2.5-flash`

### Endpoints

| Function | Input | Output |
|----------|-------|--------|
| `generateSanityPlan` | Goals + Sources | JSON: manifesto + 3 rules |
| `generateSmartSynthesis` | Mode + Context | Single sentence (≤20 words) |
| `generateDigestOverview` | Notification list | 2-sentence summary |
| `generateSignalFeed` | Prompt | JSON array: title, source, summary, tag |

### Prompting Context
- Role: "Architect of a high-end digital wellness app"
- Tone: Poetic, calming, actionable

### Fallback
If API unavailable, return hardcoded mock data to prevent UI breakage.

---

## 8. Technical Notes

### Dependencies
- Icons: `lucide-react` (stroke-width: 1.5–2px)
- Fonts: Google Fonts (Playfair Display, Inter)
- CSS: Tailwind CSS v3.x
- AI: `@google/genai`

### Key Utilities
- `animation-fill-mode: forwards` for entrance animations
- Custom webkit scrollbar: 6px width, pill-shaped

### Accessibility
- Contrast: Ink on Base meets AAA standards
- Motion: Respects `prefers-reduced-motion`
- Focus management: Inputs in Focus Mode use `autoFocus`
- Semantic HTML: `<article>`, `<header>`, `<nav>`, `<main>`, `<button>`

---

## 9. Build Order

1. **Foundation:** Tailwind config (colors, fonts), TypeScript types
2. **Shell:** Desktop sidebar, mobile bottom nav, responsive breakpoints
3. **Onboarding:** 3-step flow with stepper, selection cards, transitions
4. **Dashboard Core:** Mode switcher, Smart Synthesis with Gemini
5. **Focus Feature:** Timer logic, Anti-To-Do overlay
6. **Feed Feature:** JSON-mode Gemini prompt, finite list rendering
7. **Polish:** Transitions, shadows, Context Engine toast, status cycling
