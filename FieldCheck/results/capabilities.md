# Capability Discovery — Uno Platform run

Recorded at `capability_discovery_complete` (see `timing.jsonl`).

## Agent Skills (installed in this Claude Code session)

| Skill | Relevance | Used |
|---|---|---|
| `uno-scaffolding` | SDK/template choice, binding style, validated runtime gotchas (UseStudio blocking window, FileOpenPicker threading, pill CornerRadius, Android TextBox IME) | Yes — read before scaffolding |
| `uno-verify` | Build + launch + runtime verification loop | Yes (verification pass) |
| `uno-app-ui-testing` / `uno-app-test-assertions` | Uno App MCP driven UI tests | Reviewed; App MCP not usable here (see below) |
| `winui-xaml` | x:Bind, layout, accessibility | Guidance applied (x:Bind for MVVM) |
| `uno-navigation`, `uno-toolkit`, `uno-material`, `mvux` | Region navigation / Toolkit / Material / MVUX | Not used: benchmark mandates MVVM; design is a custom palette, not Material |
| `uno-build-troubleshoot` | Build failures | On demand |

## MCP servers

| Server | Capability | Used |
|---|---|---|
| `uno` (Uno Platform docs MCP) | `uno_platform_agent_rules_init`, `uno_platform_usage_rules_init`, `uno_platform_docs_search`, `uno_platform_docs_fetch` | Yes |
| Uno App MCP (`uno-app`, DevServer runtime tools: screenshots, visual tree, pointer) | Runtime inspection | **Not available**: not configured in this cloud session, requires DevServer + signed-in Uno Platform account; no interactive Windows desktop |
| `Microsoft_Learn` | WinAppSDK / Windows docs | Available, on demand |
| `github` | GitHub Actions trigger/logs/artifacts | Yes — Android + Windows build/launch/test runs on hosted runners |

## Framework documentation / resources

- Uno Platform docs via the `uno` MCP (FileOpenPicker, SystemNavigationManager back handling, Skia Android, publishing).
- NuGet: Uno.Sdk 6.7.30 / Uno.Templates 6.7.30 = latest stable at start.

## CLI / templates / tooling

- `dotnet` 10.0.112, `dotnet new unoapp` (Uno.Templates 6.7.30): `-preset blank -presentation mvvm -markup xaml -theme fluent -platforms android windows desktop -tests unit -renderer skia`.
- CommunityToolkit.Mvvm (via `UnoFeatures` `Mvvm`).
- NUnit + FluentAssertions unit tests (template default).

## UI / runtime verification tools

- Local container: Xvfb + `net10.0-desktop` (X11) head, ImageMagick/`import` style screen capture; xdotool-style input where available.
- GitHub Actions `ubuntu-latest` + KVM Android emulator (reactivecircus/android-emulator-runner): `adb`, `uiautomator dump`, `screencap`, `am force-stop` relaunch, DocumentsUI picker driving.
- GitHub Actions `windows-latest`: Release build of `net10.0-windows10.0.26100`, launch, UI Automation (pywinauto `uia` backend), window screenshots.

## Environment constraints discovered

- No Android SDK / workload locally; network policy returns 403 for `dl.google.com` and `dotnetcli.azureedge.net`; no `/dev/kvm`.
- Linux container cannot build the WinAppSDK Windows head.
- Therefore Android and Windows build/launch/verification run on GitHub-hosted runners driven from this session. This is a declared setup asymmetry.
- The Desktop head is the only locally runnable target; it is used as the local preview during core work, and Desktop-specific verification/fixes are measured after `core_complete`.
