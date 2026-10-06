# Project Instructions

## Overview

FreewriteUno is a distraction-free freewriting editor: one entry per session, timer-driven, optional backspace lock, dark/light theme. Cross-platform via Uno (desktop primary; Windows + Android also targeted).

## Architecture

- Pattern: **MVVM** (`CommunityToolkit.Mvvm`) — single-page editor with mostly imperative chrome state and one async list. MVUX's reactive primitives would be overhead for this surface; revisit if the app grows multi-page or gains feed-driven views.
  - Decision: MVVM. Reason: one page, one VM, no paginated/streamed data. Tradeoff: gives up free async-state plumbing if scope expands.
- Target frameworks: `net10.0-desktop`, `net10.0-windows10.0.26100`, `net10.0-android`
- Navigation: none today (single page bootstrapped via `Frame.Navigate` in `App.OnLaunched`). When a second page is added, migrate to `Uno.Extensions.Navigation` regions.
- DI/Hosting: Uno.Extensions.Hosting + `IHostBuilder` (`App.xaml.cs`).

### MVUX vs MVVM

Default to **MVUX** for pages with async/reactive data (service feeds, paginated lists, network state). Use **MVVM** (`CommunityToolkit.Mvvm`) only for pages whose state is mostly imperative toggles and a few observable properties. Don't mix patterns inside a single page. This project picked MVVM — see Architecture above.

## Project Structure

- `FreewriteUno/` — Application (Single Project, no `src/` layer)
- `FreewriteUno/Models/` — Plain records (e.g. `Entry`)
- `FreewriteUno/ViewModels/` — `MainViewModel`, `Formatters`
- `FreewriteUno/Services/` — `IEntryStore` / `EntryStore`, `ISettingsStore`, `IPdfExporter`
- `FreewriteUno/Styles/` — Material color override, fonts, text styles
- `FreewriteUno/Platforms/` — Android (`BackspaceGuard`, `NoDeleteInputConnection`, `MainActivity`), Desktop (`Program.cs`)
- `FreewriteUno/Strings/<lang>/` — `Resources.resw`
- `FreewriteUno.Tests/` — xUnit; source-links `EntryStore.cs` (not WinRT partial) for testability

## Key References

Read before starting any feature or architectural decision:

- `docs/01-architecture-brief.md` — system architecture, layers, dependencies
- `docs/02-design-brief.md` — design tokens, spacing, color, component patterns
- `docs/03-interaction-brief.md` — state model, user flows, animations
- `docs/04-implementation-plan.md` — milestone plan and acceptance checks

## Skills enforcement (project-specific)

The global `CLAUDE.md` invoke-then-apply rule is **binding** for this repo. Project-specific reinforcement:

- **Any UI work** → invoke `uno-toolkit` and `winui-xaml` before writing XAML. Cite the pattern in your reply.
- **Any page with async data** → invoke `mvux` and cite which feed/state primitive you're using (`IFeed<T>`, `IListState<T>`, `IState<T>`, `FeedView`).
- **Any navigation work** → invoke `uno-navigation` before touching routes or `NavigationView`.
- **Any DI/hosting/config/auth/HTTP/logging** → invoke `uno-extensions-services` before adding a new service.
- **Any visual-design review or polish** → invoke `userinterface-wiki-uno` in addition to `winui-xaml`.

If I have to tell you "use the X skill" after the work is already started, the rule was missed. Re-invoke and **re-do the affected section** — don't patch over it.

## Component priority (must follow in order)

Before adding ANY new control, helper, or NuGet package, walk this list top-down and **stop at the first match**:

1. **Uno Toolkit** — `NavigationBar`, `TabBar`, `AutoLayout`, `SafeArea`, `Card`, `Chip`, `DrawerControl`, `ShadowContainer`, `CommandExtensions`, `InputExtensions`, `ResponsiveExtension`, `ItemsRepeaterExtensions`, `StatusBarExtensions`. Invoke the `uno-toolkit` skill and run `mcp__uno__uno_platform_search` for the control before assuming no equivalent exists.
2. **WinUI 3 built-in controls** — only if no Toolkit equivalent exists.
3. **Uno Community Toolkit** — only if no WinUI built-in fits.
4. **Custom `UserControl`** — requires a one-line written justification in the commit body (which Toolkit / WinUI control was inadequate and why).
5. **Third-party NuGet** — **STOP and ask** before adding. Never add a new package without explicit approval. State which tier above was insufficient and why when asking.

State the picked tier and reason in one line every time you reach for a control. Example: `Tier 1: AutoLayout (Toolkit) — replaces nested StackPanel and handles safe-area on mobile.`

## Conventions

- New pages get a corresponding partial record model in `Models/`.
- Use `INavigator` for navigation, never `Frame.Navigate`.
- Use `x:Bind` over `{Binding}`.
- Use theme brushes / resource keys, never hardcoded colors.
- User-facing strings go through `Strings/<lang>/`, never inline literals.
- Prefer Uno Toolkit controls over raw WinUI equivalents — see **Component priority** above.
- Keep XAML lean — Lightweight Styling over inline values.

## Anti-Patterns

Common mistakes to avoid in this codebase:

- `Frame.Navigate(...)` → use `INavigator` regions instead.
- `{Binding Path=...}` → use `x:Bind` (compile-time, faster, type-safe).
- Hardcoded `#RRGGBB` or `Color="..."` → use `{ThemeResource ...}` or Material brushes.
- Raw `NavigationView` without region setup — breaks navigation extensions.
- Platform-specific XAML without `mc:Ignorable` and the right namespace prefix.
- New singletons or `App.Current` lookups — register in DI instead.
- Custom `UserControl` when a Toolkit equivalent exists — see **Component priority**.
- Adding a third-party NuGet without asking first — see **Component priority**.
- Invoking a skill and then writing code that contradicts it — re-read the skill, rewrite the code.

## Comment Conventions

- `TODO:` — work intended for this branch/PR
- `REVIEW:` — something a human should sanity-check (e.g. possibly unused code)
- `HACK:` — known workaround; include a reason

Anything else should be code, not a comment.

## Definition of Done

A task is complete when:

- `dotnet build` succeeds for all configured TFMs.
- The change runs on at least one target (smoke-tested in a running app).
- Existing tests pass; new behavior has a test if a test project exists.
- No commented-out code, no orphaned files from the change.
- Every new control / helper / package added followed the **Component priority** rule, with the picked tier stated in the commit body.
- A conventional commit has been made.

## Verification

```bash
dotnet build
dotnet test                          # if a test project exists
dotnet run -f <active-tfm>           # smoke test on desktop/wasm/etc.
```
