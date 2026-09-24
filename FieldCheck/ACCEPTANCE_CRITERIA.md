# FieldCheck v1 — Acceptance Criteria

Every criterion is recorded as **PASS / FAIL / NOT TESTED**. "Not applicable" is allowed only where this document explicitly says contextual.

## Completion model

Completion has three layers:

1. **Existence** — every intended screen/control/state exists.
2. **Workflow completeness** — the feature actually works end-to-end, including relevant validation, state and failure behavior.
3. **Publication readiness** — no knowingly unfinished user-facing behavior, dead controls, debug UI, placeholder content or primary-flow crashes remain.

A rendering is not a completed feature. A button existing is not a completed interaction. A form accepting text is not a completed workflow.

## A. Build and launch

- A01 — Project uses .NET 10 and the frozen framework assignment.
- A02 — Exact framework/SDK versions are recorded in `results/environment.json`.
- A03 — Release configuration restores and builds for Android with zero build errors.
- A04 — Release configuration restores and builds for Windows with zero build errors.
- A05 — Android app launches to a usable Dashboard.
- A06 — Windows app launches to a usable Dashboard.
- A07 — Compiler warnings are recorded; avoidable warnings introduced by the app are resolved where reasonable.

Uno only after core checkpoint:
- A08 — Desktop head builds and launches; incremental effort is recorded separately.

## B. Screens and navigation

- B01 — Dashboard exists and matches reference hierarchy.
- B02 — Assets exists.
- B03 — Asset Detail exists.
- B04 — New Inspection exists.
- B05 — Inspection Success exists.
- B06 — History exists.
- B07 — Android primary navigation reaches Dashboard / Assets / History.
- B08 — Android back returns from Inspection → Asset Detail and Asset Detail → originating list/context appropriately.
- B09 — Windows sidebar reaches Dashboard / Assets / History.
- B10 — Windows wide Assets view is master/detail.
- B11 — Cancel from New Inspection returns without creating an inspection.
- B12 — Success actions navigate to the correct Asset and History destinations.

## C. Data

- C01 — Seed contains all 12 supplied assets.
- C02 — Dashboard counts are derived from data, not hard-coded display values.
- C03 — Assets search matches name, ID, type and location case-insensitively.
- C04 — Asset status filter correctly filters Operational / Attention / Critical.
- C05 — Search + status filter combine correctly.
- C06 — History is newest-first.
- C07 — History search matches asset name or ID.
- C08 — History condition filter works.
- C09 — A submitted inspection is persisted locally.
- C10 — Newly submitted inspection remains after full process termination and relaunch.
- C11 — Saving does not mutate benchmark fixture files.
- C12 — Generated inspection IDs are unique and persisted.

## D. Inspection behavior

- D01 — Condition supports Good / Attention / Critical.
- D02 — Operating normally supports Yes/No.
- D03 — Temperature accepts numeric values from -50 through 250 inclusive.
- D04 — Temperature outside range shows a clear validation error and blocks submit.
- D05 — All three checklist items are individually required.
- D06 — Notes accepts multiline optional content.
- D07 — File/image picker opens from Attach photo/file.
- D08 — Selecting a file/image shows filename or thumbnail.
- D09 — Cancelling the picker leaves the form usable.
- D10 — Issue Description is hidden for Good + Operating normally Yes.
- D11 — Issue Description is shown for Attention or Critical.
- D12 — Issue Description is shown when Operating normally is No.
- D13 — When shown, Issue Description is required.
- D14 — Submit is disabled until all currently-required fields are valid.
- D15 — During save, duplicate activation cannot create duplicate inspections.
- D16 — Successful submit creates exactly one inspection.
- D17 — Success view shows generated inspection ID, asset and result.
- D18 — Asset Detail reflects the newest inspection after save.

## E. States and failure handling

- E01 — Initial data load has an intentional loading state.
- E02 — Deterministic empty repository mode shows a deliberate empty state.
- E03 — Deterministic repository read failure shows a user-safe error state.
- E04 — Error state exposes Retry and can recover when the data source succeeds.
- E05 — Search with zero matches shows a no-results state distinct from repository empty.
- E06 — Validation messages identify what must be corrected.
- E07 — CT-007 long description remains readable and reachable without clipping.
- E08 — Unexpected local persistence failure does not silently report success.

The deterministic empty/error mechanism must be test-only infrastructure or injectable repository behavior; do not expose a debug-state switch in normal release UI.

## F. Responsive/platform behavior

- F01 — Android reference viewport 412×915 has no clipped primary content.
- F02 — Android form scrolls so every field/action can be reached with keyboard visible or after keyboard dismissal.
- F03 — Windows 1440×900 matches the master/detail/reference composition.
- F04 — Windows window contraction reflows/collapses before content becomes unusable.
- F05 — Text scaling/reasonable OS font scaling does not make primary workflows unusable.
- F06 — Android system back follows navigation expectation.
- F07 — Windows keyboard tab/focus can reach interactive controls in a logical order.
- F08 — File picker behavior is appropriate to each platform.

## G. Accessibility

- G01 — Interactive elements have semantic/accessibility names where visual text does not already provide one.
- G02 — Status is communicated by text as well as color.
- G03 — Focus indicator is visible for keyboard navigation on Windows.
- G04 — Touch/click targets for primary controls are approximately 44 logical px minimum.
- G05 — Form errors are associated clearly with their fields and not communicated by color alone.
- G06 — Contrast is reasonably consistent with the supplied high-contrast palette.

## H. Visual fidelity

Capture implementation screenshots at the reference sizes for:

- H01 — Dashboard
- H02 — Assets
- H03 — Asset Detail (Android) / master-detail Assets (Windows)
- H04 — New Inspection
- H05 — Inspection Success
- H06 — History

For each screenshot record visual differences in:

- structure/composition;
- spacing/alignment;
- typography hierarchy;
- color;
- borders/radii;
- component sizing;
- content density;
- responsive behavior.

A different font rasterization or small native-control metric difference is not by itself a failure. Material structural divergence or redesign is.

## I. Architecture/code quality

- I01 — MVVM is used.
- I02 — Persistence/data access is separated from Views.
- I03 — Business/validation logic is not primarily implemented in code-behind unless platform glue makes a narrowly scoped exception reasonable.
- I04 — No intentionally dead code or abandoned duplicate implementation remains.
- I05 — No hard-coded test-only shortcut is used to pass acceptance checks.
- I06 — Dependencies are limited to justified framework/application needs and recorded.
- I07 — Critical domain/view-model behavior has automated tests where practical.
- I08 — Tests are not weakened/removed merely to make the build green.

## J. Publication-readiness pass

- J01 — No placeholder/lorem ipsum/debug UI remains.
- J02 — No dead buttons or links remain.
- J03 — No reproducible crash in the required workflow.
- J04 — Clean install/first run succeeds.
- J05 — Process kill + relaunch succeeds and preserves submitted inspection.
- J06 — Background/resume is exercised where the test environment permits and does not corrupt state.
- J07 — File picker cancel and unavailable/failed selection path does not break the app.
- J08 — No secrets/credentials are embedded in source or output.
- J09 — Primary workflow is exercised end-to-end on Android and Windows.
- J10 — Release builds used for final verification.

## K. Metrics to report (not quality pass/fail by themselves)

- K01 — Wall time to first source edit.
- K02 — Wall time to first build attempt.
- K03 — Wall time to first successful build by target.
- K04 — Wall time to first successful launch by target.
- K05 — Wall time to verified core completion.
- K06 — Uno incremental wall time from core completion through Desktop-head verification.
- K07 — Total model input/output/cache tokens from external/session telemetry when available.
- K08 — Actual reported USD/session cost when available; otherwise token-derived estimate clearly labeled.
- K09 — Number of failed build attempts.
- K10 — Number and category of runtime defects discovered during verification.
- K11 — Number of acceptance criteria passed/failed/not tested.
- K12 — Final source LOC/file count/package dependencies as descriptive complexity measures.
- K13 — Startup/build timing if readily measurable on the benchmark machine.

Do not collapse these into a single subjective "quality score." Compare completion, correctness, visual fidelity, defects and resource consumption side-by-side.
