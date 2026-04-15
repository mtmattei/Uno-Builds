# Project Instructions

## Overview

<!-- Brief description of what this app does -->

## Architecture

- Pattern: MVUX
- Navigation: Uno Navigation (regions-based)
- DI: Microsoft.Extensions.DependencyInjection via Uno.Extensions.Hosting

## Project Structure

<!-- Update this to match your actual layout -->
- `src/` — Application source
- `src/Models/` — MVUX feed/state records
- `src/Presentation/` — Pages, ViewModels, UserControls
- `src/Services/` — Service interfaces and implementations
- `src/Strings/en/` — Localization resources

## Conventions

- New pages get a corresponding partial record model in `Models/`.
- Use `INavigator` for navigation, never frame-based.
- Prefer Uno Toolkit controls (`NavigationBar`, `TabBar`) over raw WinUI equivalents.
- Keep XAML lean — use Lightweight Styling and theme resources over inline values.
- Search Uno Platform docs via MCP before assuming API usage or patterns.

## Key References

Before starting any new feature or architectural decision, read these first:

- `docs/ARCHITECTURE.md` — system architecture, layers, dependencies
- `docs/DESIGN-BRIEF.md` — design language, spacing, color tokens, component patterns

## Verification

```bash
dotnet build
dotnet test
dotnet run -f net10.0-desktop  # or net10.0-browserwasm, etc.
```

Always run `dotnet build` after changes to confirm the project still compiles. Run tests when they exist.
