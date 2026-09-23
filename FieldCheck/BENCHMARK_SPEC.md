# FieldCheck v1 — Frozen Benchmark Specification

Version: 1.0
Status: FROZEN once a measured run starts

## 1. Purpose

Build **FieldCheck**, a small but complete equipment-inspection application from the supplied reference design and mock data. The same product requirements apply to the Uno Platform and .NET MAUI runs.

The benchmark measures how effectively Claude Code can produce a complete, visually faithful, production-oriented application using each framework's real publicly available ecosystem.

## 2. Framework-neutral product definition

FieldCheck is used by a maintenance technician to review facility assets and record inspections.

The primary complete workflow is:

**Dashboard → Assets → Asset detail → Start inspection → Validate and submit inspection → Success → Return to asset → View persisted inspection in History → Relaunch app → Confirm inspection is still present.**

A screen merely rendering is not completion. Every user-facing feature present in this specification or reference UI must behave intentionally and the workflow must function end-to-end.

## 3. Required screens

1. **Dashboard**
   - App title and concise greeting/context.
   - Summary counts derived from mock asset data: total, operational, attention, critical.
   - "Needs attention" list containing Attention and Critical assets.
   - Selecting an asset opens Asset Detail.
   - Primary navigation to Assets and History.

2. **Assets**
   - Search by asset name, ID, type or location.
   - Status filter: All / Operational / Attention / Critical.
   - List all matching assets with status, ID/type and location.
   - Selecting an asset opens Asset Detail.
   - Search with zero matches shows a deliberate no-results state.
   - Data source supports deterministic loading, empty and error states for verification; these are testable without release/debug controls visible in the normal UI.

3. **Asset Detail**
   - Asset name, ID, type, location, status and last inspection date.
   - Full description; long content must scroll or reflow correctly.
   - Most recent inspection summary when available.
   - Primary action: **Start inspection**.
   - Back behavior returns to the previous context without losing state.

4. **New Inspection**
   - Shows selected asset identity.
   - Condition: Good / Attention / Critical.
   - "Operating normally" boolean input.
   - Temperature in °C: numeric, allowed range -50 through 250.
   - Checklist with exactly three required items:
     1. Guards and covers secure
     2. No visible leaks or damage
     3. Area clear and accessible
   - Notes: optional multiline text.
   - Attach photo/file: invoke a platform file/image picker; after selection show the chosen filename or thumbnail. Cancelling the picker must not break the form.
   - Conditional Issue Description:
     - hidden when Condition is Good AND Operating normally is Yes;
     - shown and required when Condition is Attention or Critical OR Operating normally is No.
   - Submit remains disabled until all required inputs are valid.
   - Cancel returns without saving.
   - Prevent accidental duplicate submission from repeated activation while submission is in progress.

5. **Inspection Success**
   - Clear success confirmation.
   - Generated inspection ID.
   - Asset name and resulting condition.
   - Actions: **View asset** and **View history**.
   - View asset reflects the new last-inspection date/status where applicable.

6. **History**
   - Newest inspections first.
   - Each row shows asset, date/time, resulting condition and inspector.
   - Search by asset name or ID.
   - Condition filter: All / Good / Attention / Critical.
   - Newly submitted inspection appears immediately and still appears after app process restart.

## 4. Data and persistence

- Use supplied `mock-data/assets.json` and `mock-data/inspections.json` as seed data.
- `mock-data/inspection-photo.png` is the deterministic file-picker test asset; use it during verification when the test environment allows selecting a local file.
- No network backend is required or allowed for core application behavior.
- Runtime changes must persist locally across process restart.
- Do not mutate the benchmark source fixture files. Copy/seed them into application storage on first run.
- Generated inspection IDs must be stable after save and unique within the local dataset.
- Inspector name for newly created inspections: **Alex Morgan**.

## 5. Relevant states

Implement these when contextually relevant:

- loading: initial asset/history repository load;
- populated: normal supplied dataset;
- empty: deterministic repository test mode returns no assets/inspections;
- error: deterministic repository test mode simulates a read failure and shows a user-safe retryable error state;
- no results: search/filter returns zero matches;
- validation: invalid/missing required inspection fields;
- disabled: submit unavailable until valid and while submission is executing;
- success: completed inspection;
- long content: CT-007 description validates scrolling/reflow.

Offline/authentication/notifications are not part of this small benchmark and must not be added.

## 6. Navigation

### Android

Use three primary destinations: Dashboard, Assets, History. A bottom navigation treatment is shown in the reference UI.

Asset Detail and New Inspection are pushed/detail destinations. System back must behave normally and must not exit the app when an in-app back destination is available.

### Windows

Use a persistent left navigation rail/sidebar for Dashboard, Assets and History.

At wide desktop width, Assets uses a **master/detail** layout: asset list left, selected Asset Detail right. At narrower width it may collapse to single-pane navigation as long as nothing clips or becomes unusable.

New Inspection uses the full working content area; modal treatment is not required.

### Uno additional Desktop head

After the Uno Android + Windows core benchmark has passed, validate the same application using Uno's Desktop head. This is an **additional measurement**, not part of the primary Uno-vs-MAUI Android + Windows comparison. Capture the incremental time/tokens required after core completion.

## 7. Target platforms

Primary comparable targets for both stacks:

- Android
- Windows

Uno additional target:

- Desktop head

## 8. Architecture

- .NET 10.
- MVVM is required for both frameworks.
- Use the latest stable public SDK/framework version compatible with .NET 10 as resolved immediately before the benchmark. Record the exact versions used.
- Preview/nightly framework builds are prohibited unless a stable compatible release does not exist, in which case stop and record BLOCKED rather than silently changing the rules.
- Views must not contain business/persistence logic that belongs in view models/services.

## 9. Visual fidelity

The PNGs in `reference-ui/` are the visual source of truth.

Aim to reproduce the reference composition, hierarchy, spacing, typography, colors, borders, component sizing and adaptive behavior as closely as practical with native framework controls/styles.

Do not use raw pixel-difference as the sole pass/fail rule because platform text rasterization and native rendering can differ. Screenshots must be captured at the specified reference viewport sizes and visual differences must be recorded.

Do not redesign the application.

## 10. Scope guardrails

Do not add:

- authentication;
- remote APIs;
- cloud sync;
- maps;
- notifications;
- camera capture beyond using a system picker for an existing file/image;
- analytics;
- settings/profile screens;
- speculative abstractions intended for a future larger benchmark.

Implement the minimum robust product that satisfies the frozen specification and acceptance criteria.
