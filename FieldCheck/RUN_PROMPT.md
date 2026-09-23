# Claude Code Measured Run — Uno Platform

You are the implementation agent for a controlled framework benchmark. Your assigned stack is **Uno Platform**.

## Non-negotiable benchmark rules

1. Work only inside the current run directory and its `app/`, `results/` and supplied benchmark files. **Do not inspect parent/sibling directories or any other framework run.**
2. Read these files before changing code:
   - `BENCHMARK_SPEC.md`
   - `DESIGN_SPEC.md`
   - `ACCEPTANCE_CRITERIA.md`
   - `reference-ui/REFERENCE_INDEX.md`
   - supplied JSON fixtures in `mock-data/`
3. The spec is frozen. Do not simplify, reinterpret or remove requirements to make implementation easier. Do not add speculative product scope.
4. Use .NET 10, MVVM, and the latest stable public **Uno Platform** SDK/framework compatible with .NET 10. Record exact versions.
5. This is a **real-world ecosystem benchmark**. Before implementation, perform capability discovery and record it in `results/capabilities.md`:
   - relevant installed/public Agent Skills;
   - relevant MCP servers;
   - official/public framework documentation/resources;
   - framework CLI/templates/tooling;
   - UI/runtime verification tools available to you.
   Relevant framework-specific capabilities are not merely permitted: **use them when they can materially help implementation or verification.** Do not use private/internal resources unavailable to an ordinary public developer.
6. Capability discovery, documentation use, debugging and verification are part of the measured run. Do not exclude them from time/cost.
7. Use reference PNGs as the visual source of truth. Do not redesign the app.
8. Every feature you implement must actually work. Do not satisfy acceptance criteria with static facades, hard-coded test outcomes or screenshot-only tricks.
9. Continue until the application reaches verified COMPLETE. There is no fixed time/turn limit. Do not continue discretionary polish after all mandatory criteria pass.
10. Do not estimate tokens or dollars. Session/token/cost numbers must come from Claude Code/provider/harness telemetry. If unavailable to you, write `null`/`unavailable`; never invent values.

## Timing

Immediately before any capability discovery or implementation work, run:

`python benchmark_ctl.py start`

Use the controller at these milestones when they occur for the first time:

- `python benchmark_ctl.py mark capability_discovery_complete`
- `python benchmark_ctl.py mark first_source_edit`
- `python benchmark_ctl.py mark first_build_attempt`
- `python benchmark_ctl.py mark first_android_build_success`
- `python benchmark_ctl.py mark first_windows_build_success`
- `python benchmark_ctl.py mark first_android_launch_success`
- `python benchmark_ctl.py mark first_windows_launch_success`
- `python benchmark_ctl.py mark verification_start`
- `python benchmark_ctl.py mark core_complete`

For Uno only, after `core_complete`, continue with Desktop-head verification and mark:

- `python benchmark_ctl.py mark desktop_head_complete`

Finally run:

`python benchmark_ctl.py finish`

Do not reset/rewrite timing history.

## Environment record

Before implementation, create `results/environment.json` containing measured/discovered values for:

- UTC start timestamp
- OS and version
- CPU
- RAM if discoverable
- .NET SDK version(s)
- assigned framework and exact stable version
- Java/Android tooling versions relevant to build
- Windows SDK/tooling relevant to build
- Claude Code version if discoverable
- active Claude model if discoverable
- git state if applicable
- cache policy: `warm`

Unknown values must be `null`, not guessed.

## Implementation target

Create the application under `app/`.

For the primary core checkpoint, Android + Windows are the comparable targets. Once both targets satisfy the acceptance suite, mark `core_complete` BEFORE doing any Desktop-head-specific remedial work. Snapshot any directly available session usage/cost at that point if the host exposes it. Then validate Uno's Desktop head and fix only issues necessary for that target. Mark `desktop_head_complete` and report the incremental effort separately. If Desktop already passes without changes, record that fact; verification time still belongs to the additional Desktop measurement.

Build a high-quality general implementation, not one narrowly hard-coded to tests. Use framework-standard patterns and the minimum architecture needed for the current specification.

## Required verification behavior

You may not declare COMPLETE merely because the project compiles.

Before completion you must:

1. build Release for Android and Windows;
2. launch both targets;
3. exercise the required primary workflow end-to-end;
4. verify persistence after process termination/relaunch;
5. exercise relevant loading/empty/error/no-results/validation/success/disabled/long-content states;
6. exercise file picker select and cancel behavior where the environment permits;
7. run automated tests;
8. capture implementation screenshots matching the reference views and save them under `results/screenshots/android/` and `results/screenshots/windows/`;
9. compare screenshots to the supplied reference and record meaningful visual differences in `results/visual-review.md`;
10. perform every item in `ACCEPTANCE_CRITERIA.md` and write `results/acceptance.json` plus a readable `results/acceptance.md`.

If the environment prevents a test, mark it NOT TESTED and explain exactly why. Do not silently claim success.

When verification finds a defect, continue implementation, then re-verify. Do not stop the benchmark clock during debugging.

## Completion definition

`COMPLETE` means all mandatory acceptance criteria that the environment can execute pass, there are no known blocking defects, and the app has completed the publication-readiness pass defined in the acceptance document.

If a requirement is impossible because of an external/environment constraint, document the constraint and continue with everything else. Only use `BLOCKED` if the constraint actually prevents meaningful completion.

## Results

Produce at minimum:

- `results/environment.json`
- `results/capabilities.md`
- `results/timing.jsonl` (via benchmark controller)
- `results/acceptance.json`
- `results/acceptance.md`
- `results/visual-review.md`
- `results/implementation-notes.md`
- `results/dependencies.txt`
- `results/screenshots/...`
- `results/final-summary.md`

`final-summary.md` must include:

- completion status;
- exact framework/.NET versions;
- acceptance pass/fail/not-tested counts;
- defects remaining, if any;
- first-build/first-launch/completion timestamps derived from `timing.jsonl`;
- build attempts and failed-build count if known;
- tests executed/results;
- screenshot paths;
- architecture summary;
- relevant tool/MCP/skill usage;
- token usage/cost only if directly available from telemetry, otherwise explicitly state that external session telemetry must be attached after the run.

Do not create a subjective framework verdict. This run produces measurements and artifacts only.
