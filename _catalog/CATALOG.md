# Uno-Builds-net10 — Project Catalog

A single-file inventory of every runnable app project in this workspace. Sorted by **Type** (Enterprise first), then alphabetical within each group.

- **Type** — *Enterprise* (B2B / business / data-heavy: dashboards, ERP, CRM, ops, sales, healthcare, finance) vs *Consumer* (creative tools, samples, demos, games, utilities, content/media).
- **Responsive** — *Yes* only when the source contains concrete adaptive markers: `AdaptiveTrigger`, `VisualState` with `MinWindowWidth`/`MinWindowHeight`, `<utu:AutoLayout`, `utu:ResponsiveExtension`, or `utu:ResponsiveLayout`.
- **Target Frameworks** — exact `<TargetFrameworks>` value from the csproj.

## Summary

- Total projects: **66**
- Enterprise: **16** · Consumer: **50**
- Responsive (code-evidenced): **18** · Not responsive: **48**
- .NET 10 projects: ~46 · .NET 9 projects: ~20

## Enterprise (16)

| # | Project | Goal | Target Frameworks | Responsive |
|---|---------|------|-------------------|------------|
| 1 | ClaudeDash | Real-time dashboard for monitoring Claude AI operations. | net10.0-browserwasm;net10.0-desktop | No |
| 2 | EnterpriseDashboard | Analytics dashboard with charts, tables, and real-time data. | net9.0-desktop | No |
| 3 | FieldOpsPro | Field service dispatch platform with work-order tracking and real-time crew visibility. | net10.0-android;net10.0-ios;net10.0-browserwasm;net10.0-desktop | Yes |
| 4 | Gridform | Industrial tooling distribution and warehouse spatial planning dashboard. | net10.0-windows10.0.26100;net10.0-browserwasm;net10.0-desktop | No |
| 5 | heatmap | Sales performance heatmap visualization with data analytics. | net9.0-desktop | Yes |
| 6 | Meridian | Desktop stock market dashboard with portfolio tracking and interactive watchlist. | net10.0-browserwasm;net10.0-desktop | No |
| 7 | Nexus | Industrial SCADA dashboard for real-time manufacturing operations monitoring. | net10.0-desktop | No |
| 8 | Orbital | Development environment dashboard for Uno developers with AI studio integration. | net10.0-desktop | Yes |
| 9 | QuoteCraft | Professional quoting and invoicing app for contractors and small businesses. | net10.0-android;net10.0-browserwasm;net10.0-desktop | Yes |
| 10 | ReservoomUno | Hotel reservation system migrated from WPF using MVVM and Entity Framework. | net10.0-android;net10.0-ios;net10.0-browserwasm;net10.0-desktop | No |
| 11 | SalesHeatmap | Sales metrics and performance analytics widget with responsive layout. | net10.0-android;net10.0-ios;net10.0-browserwasm;net10.0-desktop | No |
| 12 | Text-Grab | WPF OCR utility capturing text from screen, images, PDFs with editing. | net10.0-windows10.0.22621.0 | No |
| 13 | Unoblueprint | Blueprint management app with package installation and version control. | net9.0-desktop | No |
| 14 | UnoEnterpriseApp | Enterprise application with data-driven dashboard and navigation. | net9.0-desktop | No |
| 15 | vtrack | Video analysis and tracking platform with real-time playback. | net9.0-android;net9.0-ios;net9.0-browserwasm;net9.0-desktop | Yes |
| 16 | Zara | E-commerce product showcase with image gallery and filtering. | net10.0-android;net10.0-ios;net10.0-browserwasm;net10.0-desktop | No |

## Consumer (50)

| # | Project | Goal | Target Frameworks | Responsive |
|---|---------|------|-------------------|------------|
| 1 | AdaptiveInput | Demonstrates adaptive input techniques and responsive design. | net10.0-android;net10.0-ios;net10.0-browserwasm;net10.0-desktop | Yes |
| 2 | ADE | Lightweight token-counting IDE for AdTokens debugging. | net9.0-android;net9.0-ios;net9.0-browserwasm;net9.0-desktop | Yes |
| 3 | ADTest | Timeline-based dashboard with navigation rail and icons. | net10.0-desktop | No |
| 4 | AgentNotifier | Retro CRT-style desktop widget for AI agent status monitoring. | net10.0-desktop | No |
| 5 | AnimatedExtendedSplashScreen | Splash screen demo with animated transitions and navigation. | net9.0-android;net9.0-desktop | No |
| 6 | Caffe | Coffee brewing companion app with responsive multi-layout design. | net10.0-desktop | Yes |
| 7 | CCUI | Camera capture UI sample demonstrating device camera integration. | net10.0-android;net10.0-ios;net10.0-browserwasm;net10.0-desktop | No |
| 8 | Composer | Conversational bootstrapper for Uno Platform project scaffolding. | net10.0-browserwasm;net10.0-desktop | No |
| 9 | ConfPass | Cross-platform conference attendee badge with neumorphic styling. | net10.0-android;net10.0-ios;net10.0-maccatalyst;net10.0-windows10.0.26100;net10.0-browserwasm;net10.0-desktop | No |
| 10 | DepthCard | Interactive 3D card component demonstrating depth and parallax. | net9.0-desktop | No |
| 11 | ExtendedSplashHDdemo | HD splash screen demo with navigation and animations. | net9.0-android;net9.0-desktop | No |
| 12 | FibonacciSphere | Interactive 3D Fibonacci sphere visualization with animation. | net10.0-android;net10.0-ios;net10.0-browserwasm;net10.0-desktop | No |
| 14 | FluxTransit | Montreal transit dashboard with real-time route planning and crowd analytics. | net10.0-windows10.0.26100;net10.0-browserwasm;net10.0-desktop | No |
| 15 | FormaEspresso | Coffee brewing companion app with espresso recipe tracking and form management. | net10.0-android;net10.0-desktop | No |
| 16 | FreewriteUno | Distraction-free freewriting editor with timer and session-based entry history. | net10.0-android;net10.0-windows10.0.26100;net10.0-desktop | Yes |
| 17 | FriendSonar | Location-based friend radar showing nearby contacts with dynamic range adjustment. | net10.0-android;net10.0-desktop | Yes |
| 18 | HockeyBarn | Hockey spare-finder platform connecting players seeking last-minute game participants. | net9.0-android;net9.0-desktop | No |
| 19 | HorizontalCalendar | Horizontal date picker control with event calendar display and scheduling. | net9.0-android;net9.0-browserwasm;net9.0-desktop | No |
| 20 | HorizontalCalendarControl | Reusable horizontal calendar control with TabView demonstration app. | net9.0-android;net9.0-browserwasm;net9.0-desktop | No |
| 21 | InfiniteImage | 3D infinite image sphere viewer with interactive gesture controls. | net10.0-android;net10.0-desktop;net10.0-browserwasm | No |
| 22 | KineticSculptor | Interactive 3D sculpture creation canvas with SkiaSharp rendering engine. | net9.0-browserwasm;net9.0-desktop | No |
| 23 | liquidMorph | Animated liquid morphing transition effects and visual demos. | net10.0-desktop | No |
| 24 | listhold | Interactive list with press-and-hold reveal animations. | net10.0-desktop | No |
| 25 | Liveline.Demo | Demo application showcasing real-time line chart component with animation. | net10.0-desktop;net10.0-browserwasm | No |
| 26 | matrix | Matrix-themed navigation demo with red/blue pill choices. | net9.0-android;net9.0-browserwasm;net9.0-desktop | No |
| 27 | MCP-blog | Multi-page settings application with navigation and configuration management. | net10.0-desktop | No |
| 28 | MPE | Media player with course video streaming and responsive adaptive layouts. | net9.0-desktop | Yes |
| 29 | msn | MSN Messenger clone with animated gradient background and UI. | net9.0-desktop | No |
| 30 | MSYouTube | YouTube streaming app with featured content and personalized recommendations. | net10.0-browserwasm;net10.0-desktop | Yes |
| 31 | Olea | Olive oil tasting journal with flavor wheel and vineyard exploration. | net10.0-desktop | No |
| 32 | Pens | Ice hockey team management app for roster tracking and equipment logging. | net9.0-android;net9.0-desktop | Yes |
| 33 | PrecisionDial | Parametric rotary control with dial value input and radial menu variant. | net10.0-android;net10.0-ios;net10.0-browserwasm;net10.0-desktop | No |
| 34 | PuckUp | Ice hockey league management mobile app for player profiles and teams. | net9.0-android;net9.0-browserwasm;net9.0-desktop | No |
| 35 | radial-action-menu | Floating action button with radial menu deployment. | net9.0-android;net9.0-desktop | No |
| 36 | Riviera | Smart home control dashboard with CRT phosphor aesthetic and touch zones. | net10.0-desktop | No |
| 37 | RivTes | Retro CRT-themed dashboard with vehicle telemetry and climate display. | net10.0-android;net10.0-windows10.0.26100;net10.0-desktop | Yes |
| 38 | Sanctum | Attention curation tool for digital wellness with context-aware feed filtering. | net10.0-android;net10.0-desktop | No |
| 39 | SantaTracker | Holiday Santa-tracking app with map, reindeer status, naughty/nice tracking. | net10.0-windows10.0.26100;net10.0-desktop | Yes |
| 40 | SmartNotes | Note-taking app with local database storage and light/dark theming. | net9.0-desktop | No |
| 41 | Sweather | Weather app with temperature, conditions, daily forecast, and location search. | net10.0-android;net10.0-browserwasm;net10.0-desktop | Yes |
| 42 | TextGrab.Uno | Cross-platform OCR utility with text capture, editing, and regex tools. | net10.0-windows10.0.26100;net10.0-browserwasm;net10.0-desktop;net10.0 | No |
| 43 | Thermostat | Thermostat control interface for smart home temperature management. | net9.0-android;net9.0-desktop | Yes |
| 44 | ToolkitBench | Reference app demonstrating Uno Toolkit components with signature animations. | net10.0-browserwasm;net10.0-desktop | No |
| 45 | UnoVox | 3D voxel editor with webcam hand tracking and SkiaSharp rendering. | net10.0-desktop | No |
| 46 | UnoWallet | Financial wallet app tracking transactions, balance, and spending analytics. | net9.0-android;net9.0-desktop | No |
| 47 | VoxelWarehouse | Isometric voxel spatial editor for inventory and 3D visualization. | net10.0-browserwasm;net10.0-desktop | No |
| 48 | WinampClassic | Nostalgic Winamp-style music player with classic UI and equalizer. | net10.0-desktop | No |
| 49 | YUL | Airline boarding pass notification demo with QR code generation. | net9.0-android;net9.0-desktop | Yes |
| 50 | ZaraApp | E-commerce app prototype with tabs and product catalog interface. | net9.0-desktop | No |

## Notes

- *Enterprise* tag is domain-based, not complexity-based — small business tools like QuoteCraft still count if their job is B2B revenue.
- Several "dashboard" apps appear in the *Consumer* column when their purpose is hobby/utility (RivTes vehicle telemetry, Riviera smart home, FluxTransit transit info) — they're for individuals, not organizations.
- *Responsive = No* doesn't mean the app is desktop-only — many are multi-target but use fixed layouts. It only means the source has no adaptive breakpoint logic.
- Two related Text-Grab entries: *Text-Grab* is the original WPF/WinAppSDK port (Windows-only TFM); *TextGrab.Uno* is the cross-platform Uno migration.
