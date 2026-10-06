# Uno-Builds

A working lab of Uno Platform apps, controls and experiments, built with C#, XAML and .NET 9/10. Each top-level
folder is a self-contained project with its own solution, `global.json` and `Directory.*.props`. There is no
shared build at the root.

- Full inventory with target frameworks and responsive status: [`_catalog/CATALOG.md`](_catalog/CATALOG.md)
- Curated showcase shortlist with recaps and screenshots: [`shortlisdt/`](shortlisdt/)
- Portfolio audit and cleanup plan: [`docs/github-audit-2026-10.md`](docs/github-audit-2026-10.md)

## Run a project

```powershell
cd <Folder>
dotnet run --project <Folder>/<Project>.csproj -f net10.0-desktop   # net9.0-desktop for older projects
```

Run `uno-check` first on a fresh machine. Check each project's `global.json` for its pinned Uno.Sdk version.

## Business and data apps

| Folder | What it is |
|---|---|
| `QuoteCraft` | Quoting and invoicing for contractors and small businesses (flagship; has architecture, design and go-to-market docs) |
| `CrmDashboard` | Three-pane enterprise CRM sample (MVVM) |
| `ClaudeDash` | Real-time dashboard for monitoring Claude AI operations |
| `EnterpriseDashboard` | Analytics dashboard with charts, tables and maps (LiveCharts2, Mapsui, SkiaSharp) |
| `FieldOpsPro` | Field service dispatch with work orders and crew visibility |
| `Gridform` | Industrial tooling distribution and warehouse spatial planning |
| `heatmap` | Sales performance heatmap |
| `Meridian` / `Meridian-Dark` | Desktop stock market terminal with portfolio and watchlist; light and dark variants |
| `Liveline` | Real-time SkiaSharp line chart control plus demo (used by Meridian) |
| `Nexus` | Industrial SCADA dashboard for manufacturing operations |
| `nakatomi` | Smart City platform desktop app (energy data layer, SkiaSharp 4 preview) |
| `Orbital` | Developer environment dashboard with AI studio integration |
| `ReservoomUno` | Hotel reservations, migrated from WPF (MVVM, Entity Framework) |
| `SalesDashboard` | SalesHeatmap: sales metrics widget (MVUX) |
| `Text-Grab` | Text-Grab OCR utility migrated from WPF |
| `Unoblueprint` | Blueprint and package management |
| `UnoEnterpriseApp` | Data-driven enterprise app shell |
| `vtrack` | Video analysis and tracking |
| `Zara` | E-commerce product showcase |

## Consumer apps

| Folder | What it is |
|---|---|
| `ADE` | Token-counting IDE for AdTokens debugging |
| `ADTest` | Timeline of AI conversation history |
| `AgentNotifier` | Retro CRT desktop widget for AI agent status |
| `Caffe` / `FormaEspresso` | Coffee brewing companions |
| `CCUI` | Camera capture reference sample |
| `Composer` | Conversational bootstrapper for Uno project scaffolding |
| `ConfPass` | Conference badge with neumorphic styling |
| `FluxTransit` | Montreal transit dashboard |
| `FreewriteUno` | Distraction-free writing editor with inline AI |
| `FriendSonar` | Friend radar with retro sonar UI |
| `HockeyBarn` / `Pens` | Hockey spare-finder and team management |
| `InfiniteImage` | 3D infinite image sphere with gestures |
| `KineticSculptor` | 3D sculpture canvas (SkiaSharp) |
| `MCP-blog` | Multi-page settings app |
| `MPE` | Course video player with adaptive layouts |
| `msn` | MSN Messenger reimagined |
| `MSYouTube` | YouTube-style streaming app with API and auth |
| `Olea` | Olive oil tasting journal |
| `Riviera` / `RivTes` | CRT-styled smart home and vehicle dashboards |
| `Sanctum` | Attention curation and feed filtering |
| `SantaTracker` | Holiday Santa tracker with map |
| `SmartNotes` | Notes with local database |
| `SpaceXhistory` | SpaceX launch history |
| `Sweather` | Weather app |
| `Thermostat` | Smart home thermostat with drag dial |
| `UnoVox` | Voxel editor with webcam hand tracking |
| `UnoWallet` | Banking and wallet UI |
| `VoxelWarehouse` | Isometric voxel editor for inventory |
| `WinampClassic` | Winamp-style music player |
| `YUL` | Boarding pass notifications with QR codes |

## Controls, effects and splash screens

| Folder | What it is |
|---|---|
| `AdaptiveInput` | Text input that morphs to the detected input type |
| `AnimatedExtendedSplashScreen` / `ExtendedSplashHDdemo` | Extended splash screen demos |
| `DepthCard` | 3D parallax card |
| `FibonacciSphere` | Interactive Fibonacci sphere |
| `HorizontalCalendar` / `HorizontalCalendarControl` | Horizontal date picker control and demo |
| `liquidMorph` | Liquid morph transitions |
| `listhold` | Press-and-hold progressive reveal list |
| `matrix` | Matrix-rain page transition with pointer physics |
| `PrecisionDial` | Parametric rotary control |
| `radial-action-menu` | FAB that expands into a radial menu |
| `ReticleLab` | HUD loading ring as a `ProgressRing` retemplate |
| `ToolkitBench` | Uno Toolkit reference app |

## Tooling and research

| Folder | What it is |
|---|---|
| `design-graph-kit` | Design Graph IR: schema, scorer, evals and experiments |
| `_catalog` | Project inventory |
| `shortlisdt` | Showcase shortlist, audit rubric and recaps |
| `docs` | Portfolio audit and cleanup script |
