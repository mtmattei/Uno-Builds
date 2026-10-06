# ⏳ StillThere — *Time-aware task list* &nbsp;`SPEC / NOT YET BUILT`

> A design-led, time-aware to-do app whose signature interaction is a procedurally generated hand-drawn scratch-out on complete. **Currently specced, not yet implemented** — the `Todo/` folder holds briefs only.

<!-- 📸 Add a mockup/prototype shot: drop it in screenshots/StillThere/ and point the src below at it -->
<!-- <img src="../screenshots/StillThere/<file>.png" alt="StillThere prototype" width="360" /> -->
> 📸 *Placeholder — to be added.*

## What's planned
A single time-aware task list with add, inline edit, complete (with the scratch-out animation), break-down, snooze-with-reason, delete, search, tag/state filters, and a collapsible completed section. Local persistence, live 1 Hz aging. Focus Mode / Insights / Dashboard are architected for but not built.

## Highlights (from the briefs)
- **Skia renderer on both heads** — one visual tree means the custom scratch + collapse animations render pixel-identically on Desktop and Android, no per-platform fork.
- **`SKCanvasElement` scratch-out** — the signature complete animation is procedurally drawn, the same draw code on every target.
- **MVUX data layer** with `Storage` persistence; shell structured so future regions slot in without reworking state.

## Status & docs
**Spec only** — see `Todo/docs/StillThere-01-Architecture-Brief.md`, `-02-Design-Brief.md`, `-03-Interaction-Brief.md`.

Planned `UnoFeatures`: `MVUX`, `Toolkit`, `Storage`, `Serialization`, `Hosting`, `Extensions`, `Skia`, `SkiaRenderer` · target heads `net9.0-desktop` + `net9.0-android`.
