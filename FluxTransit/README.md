# Flux Transit

## Problem Statement

Daily Montreal public transit commuters need a single, elegant dashboard to check network health, crowd levels, and get AI-optimized routes before leaving home. Current solutions require checking multiple apps and lack real-time intelligence.

**Target user:** STM (Société de transport de Montréal) commuters who value aesthetics and data density.

**Core job-to-be-done:** Minimize commute friction by aggregating real-time tracking, crowd analytics, and AI-driven route optimization into a single "at-a-glance" dashboard.

---

## Design Brief

See [implementation-brief.md](implementation-brief.md) for the complete Uno Platform implementation specification.

### Visual Direction
- **Theme:** Cyber-Noir / Deep Aurora
- **Style:** Glassmorphism with high contrast dark mode (only theme)
- **Principle:** "Data floats" - layers of opacity rather than solid borders

### Key Screens
1. **Dashboard** - Main view with AI planner, live routes, crowd chart, network status
2. **Profile** - Modal for OPUS card management and settings

---

## Architecture

See [implementation-brief.md](implementation-brief.md) Section 6 for detailed architecture.

### Summary
- **Pattern:** MVUX (reactive state with IState/IFeed)
- **Navigation:** Region-based with TabBar (responsive Bottom/Vertical)
- **Theme:** Material (customized for glassmorphism)
- **Data:** Mock services, ready for STM GTFS-Realtime integration

### Solution Structure
```
FluxTransit/
├── Presentation/     # Pages, Controls, Shell
├── Models/           # MVUX partial records
├── Services/         # Transit, AI service interfaces
├── DataContracts/    # DTOs and entities
├── Styles/           # Colors, Typography, GlassPanel
└── Strings/          # EN/FR localization
```

---

## Validation Notes

### Decisions Made
- **MVUX over MVVM:** Real-time data flows and async AI responses benefit from reactive feeds
- **TabBar over NavigationView:** Simpler responsive shell for 2-screen app
- **Dark mode only:** Per brief requirement, no theme switching needed
- **LiveCharts2:** Open source charting for crowd visualization
- **Real GTFS from start:** Build with STM GTFS-Realtime data immediately
- **Abstract dashboard:** No map layer, keep focused on data
- **Browser localStorage:** OPUS balance via Uno.Extensions.Storage
- **Solid semi-transparent colors:** No AcrylicBrush for cross-platform consistency

### Remaining Unresolved
1. **Gemini API security:** Backend proxy needed for production key management

---

## Prompts

**Single kickoff prompt**: *To be written after implementation is complete and validated.*



**Full prompt log**: See [prompts.md](prompts.md)

**Total prompts**:

**Time spent**:

---

## Technical Recap

**Platform targets**:
- [x] WebAssembly (Primary - Desktop + Mobile responsive)
- [x] Windows
- [ ] iOS
- [ ] Android
- [x] macOS
- [x] Linux

**Architecture pattern**:
- [x] MVUX
- [ ] MVVM
- [ ] Other:

**Key Uno Platform features**:
- MVUX reactive state (IState, IFeed, IListFeed)
- Region-based navigation with dialog support
- FeedView for async data display
- Responsive markup extension for breakpoints
- Localization (EN/FR hybrid)

**Uno Toolkit components**:
- TabBar (Bottom + Vertical styles)
- ChipGroup (Favorites)
- LoadingView (AI processing state)
- AutoLayout (Spacing management)
- ControlExtensions.Icon (Button icons)

**Third-party integrations**:
- Google Gemini API (gemini-2.5-flash) - AI route planning
- STM GTFS-Realtime - Real transit data (from start)
- LiveCharts2 - Crowd visualization

---

## Twitter Caption

*Ready-to-post caption for video recording:*

```
🚇 Built a real-time Montreal transit dashboard with Uno Platform

✨ Glassmorphism dark UI
🤖 AI-powered route planning
📊 Live crowd analytics
🔄 60fps progress animations

Single C# codebase → Web, Windows, macOS, Linux

#UnoPlatform #dotnet #csharp #XAML #WebAssembly
```

---

## Assets

*Links to screenshots, recordings, design files:*

- [ ] Design mockup
- [ ] Desktop screenshot
- [ ] Mobile screenshot
- [ ] Demo video recording

---

## Lessons Learned

*What worked well? What would you do differently?*

- *To be filled after implementation*
