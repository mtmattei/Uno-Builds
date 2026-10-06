# GitHub audit — mtmattei (2026-10-06)

Scope: every repo owned by `mtmattei` (103 source repos + 21 forks) plus the 64 app folders in `Uno-Builds`.
`unoplatform/*` repos are out of scope.

Method: blob-less clone of every repo (full history, file tree, file hashes). Duplicates were found by
comparing git blob hashes across repos and against each `Uno-Builds/` folder, so "93% shared" means 93% of
the files are byte-identical. Ideas were ranked from READMEs/specs, commit depth, recency, and
`weekly-ops/projects.md` (your own board). Nothing was built or run.

Companion script: [`github-cleanup.ps1`](github-cleanup.ps1) (dry run by default).

---

## 1. Headline numbers

| Bucket | Count | Action |
|---|---|---|
| Completely empty | 4 | Delete |
| Trivial / starter leftovers | 5 | Delete (save one zip first) |
| Byte-level duplicates of `Uno-Builds` folders | 14 | Archive |
| Snapshot / aggregate repos overlapping `Uno-Builds` | 3 | Rescue unique folders, then archive |
| Old demos, finished labs, superseded repos | 15 | Archive |
| Repos merging into a flagship | 11 | Archive after the code moves |
| Stale forks | 15 | Delete |
| Forks to keep (open PRs) | 3 | Keep |
| Flagships worth building out | 5 apps + 2 libraries | Invest |

67 of 124 repos can be archived or deleted, which leaves 57 active or reference repos.

---

## 2. Where to invest: the best ideas

Selection rule: an original concept, real depth already built, ongoing activity, and a fit with the Uno
DevRel work (SOTW, Gallery, Studio videos). Each flagship also absorbs smaller repos that are the same idea.

### Flagship apps

| # | Flagship | Why it wins | Absorbs | Next features |
|---|---|---|---|---|
| 1 | **`dorval-youngtimers`** — sports league app | 283 commits, 7 branches, active (`feat/nav-host`), real users, already your SOTW candidate | `Puck` (PuckSub spare-player call-ups), `PuckUp`, `Uno-Builds/HockeyBarn`, `Dorval`, empty `DYT` | Deep linking (in flight) · write path for RSVPs/call-ups to Supabase · push notifications for "spare needed" · live game scoring · standings/stats page · WASM PWA install |
| 2 | **`Patina`** — field conservation app | v1.0.0, CI green, 4 targets, EN/FR, local-first, MVUX + Toolkit done properly. Your most production-grade app. | `trace` (equipment inspection), `fieldcheck-maui`, `Uno-Builds/FieldOpsPro`. Also your backlog items "municipal operations map" and "environmental collection/custody" | Map view of artworks (the municipal map sample) · chain-of-custody log (the custody sample) · PDF condition reports · photo annotation · sync/export · Android keystore + Linux/macOS zips (already on your board) |
| 3 | **`Cargo`** + **`Meridian`**, sharing **Liveline** | Cargo is your most active build (95 commits, Oct 1). Meridian (53 commits) and Cargo both depend on the Liveline chart control, which is copied into 4 places. | `driftline` (= Liveline, 81% shared), `Uno-Builds/Liveline`, `Uno-Builds/Meridian`, `Build-Samples/Meridian`, `meridian-dark` (in net10) | Make Liveline one repo + NuGet (this also clears your "publish liveline-window so a fresh clone builds" blocker) · Meridian dark theme from `Meridian-Dark` · Cargo motion items on your board |
| 4 | **`loadpath`** — truss workbench | Original, useful, educational, active (Oct 1), real solver in `Loadpath.Core` | — | Save/open/share designs · preset library (Pratt, Warren, Howe) · member sizing + failure highlight · export PNG/PDF · WASM build for students |
| 5 | **`QuoteCraft`** — quotes/invoices for contractors | The most "real product" business app; clear users; broad feature set already | `Uno-Builds/QuoteCraft` (newer, 2026-05-19, 217 files) is the canonical copy. Retire the standalone. | Invoice conversion · payment status · client portal link · recurring jobs · Dataverse backend as a second store (reuses UnoBusiness work) |

`UnoBusiness` stays active (it is a work deliverable on your board). It does not need to absorb anything.

### Flagship libraries (both already on your backlog)

| Library | Absorbs | Next |
|---|---|---|
| **`uno.particles`** (Uno.Effects: 64 effects, render tests) | `Tactile` (4 SkSL surface shaders), `Blur-shader`, `LiquidGlassLab` (glass panel), `LiquidMorph`, `Hyperspeed`, `DigitalFidget`, `Uno-particle-effects` (empty) | One NuGet, one gallery: particles + shaders + glass. Publish to nuget.org, add to Uno Gallery. |
| **`neu-uno`** (Neu.Uno neumorphism theme) | `ConfPass` becomes its showcase sample | Finish validation, publish NuGet (your backlog item) |

### Agent tooling: consolidate, don't expand

| Keep | Retire into it |
|---|---|
| `uno-scaffold` (canonical skills + rules + lint) | — |
| `claude-uno-plugins` (distribution of those skills) | `UnoPlatformSkills` (75% of its files already in the plugin) |
| `uno-design-graph-plugin` + `Uno-Builds/design-graph-kit` | `Workflow/designgraphkitv0.5` |
| `Designmd2uno` + `Motiontokens` | Merge into one "Uno design tokens" repo: colors/type/spacing from DESIGN.md, plus motion tokens |
| `AppMap` (Atlas) | — (novel; keep as is) |

### Everything else in `Uno-Builds`

`Uno-Builds` stays the lab: 64 one-commit app folders that make good gallery and demo material. Don't
extend them further. Promote one to its own repo only when it becomes a flagship.

---

## 3. Delete (no unique content)

| Repo | Vis | Evidence |
|---|---|---|
| `ChefsOmakase-test` | private | Empty (no commits) |
| `DYT` | public | Empty. The sports app lives in `dorval-youngtimers` (has `feat/nav-host`) |
| `Uno-particle-effects` | public | Empty. Real work is `uno.particles` |
| `uno.hotdesign` | private | Empty. Its name collides with the company's `unoplatform/uno.hotdesign` |
| `desktop-tutorial` | private | GitHub Desktop starter README (2022) |
| `Calculator` | private | 2-file console scaffold (2024) |
| `PuckUp` | public | 1 commit, 3,338 committed `bin/obj` files. The idea lives on in `Puck` → `dorval-youngtimers` |
| `Nexus` | public | 100% identical to `Uno-Builds/Nexus` |
| `Lumen` | public | One file, `LUMEN_Project_Kit.zip`. **Download the zip first** if you want the spec |

## 4. Rescue, then archive (snapshot repos)

| Repo | Overlap | Unique content to move into `Uno-Builds` first |
|---|---|---|
| `Uno-Builds-net10` | contains every `Uno-Builds` folder | `shortlisdt` (478 files), `FreewriteUno`, `CrmDashboard`, `Meridian-Dark`, `_catalog` (feeds `build-2026-selection`); plus branches `feat/inline-ai-integration`, `net10-upgrade` |
| `Workflow` (private) | 93% shared with net10 | `designgraphkitv0.5`, `Uno-Builds-states`, two `.bundle` git histories |
| `Build-Samples` (private) | 89% inside net10 | Variant edits of Orbital/SalesDashboard/CrmDashboard. Diff them before archiving if any are newer. |

## 5. Archive

Archive keeps the repo read-only and keeps URLs working. Use it for anything linked from a blog, deck or README.

**Duplicates of a `Uno-Builds` folder** (shared files in brackets):
`Sweather` (93%) · `ConfPass` (94%) · `FibonacciSphere` (91%) ·
`FriendSonar` (87%) · `matrix` (85%) · `LiquidMorph` (85%) · `Orbital` (72%) · `radial-action-menu` (73%) · `parallax-invitation-cards` (74% = DepthCard) ·
`memory-drift` (68% = InfiniteImage) · `QuoteCraft` (74%; UB copy is newer) · `Composer` (superseded by `UnoComposer`) ·
`Thermostat-Build` · `SantaTracker`

**Superseded, archive now:** `ChefsTest` (findings live in `Chefs`), `UnoPlatformSkills`, `LiquidGlassProbe`

**Merging into a flagship, archive after the code moves:** `Dorval`, `Puck`, `trace`, `fieldcheck-maui`,
`Tactile`, `Blur-shader`, `LiquidGlassLab`, `Hyperspeed`, `DigitalFidget`, `Meridian`, `driftline`

**Old Hot Design demos (2025)**, several of which commit thousands of `bin/obj` files:
`Habits` (6,862 bin/obj files) · `HorizontalCalendar` (6,265) · `EV-ChargingApp` (328) · `NetflixSplash` ·
`UnoGPT5SearchBar` · `EnergyDashboard` · `FCM-Push-Notifications-Test` · `Codemash`

**Labs your board already marks done or parked:** `Naoto-Light` (LightWidget), `DesignSkillEval`, `Aware`, `DeskBoard`

Keep for now, archive later: `uno-repro-storyboard-begintime` (until the issue closes),
`WPF-Migration-test`, `6-6-sample-lab`, `ComponentStatesLab`, `measures-uno`, `exploded`, `strata`.

## 6. Forks

**Keep** (your board lists open PRs; deleting a fork closes its PRs): `uno`, `uno.extensions`, `Uno.Samples`.

**Delete** (stale, no activity in 1–4 years): `awesome-mcp-servers-1` (duplicate of the next one),
`awesome-mcp-servers`, `bootstrap`, `AncestorBindingSample`, `good-first-issue`, `awesome-uno-platform`,
`workshops`, `stargazer`, `uno.resizetizer`, `Microcharts`, `discoverdotnet`, `figma-docs`,
`hacktoberfest-swag`, `ArchitectureWeekly`, `wasmweekly`.

## 7. Hygiene fixes

| Issue | Where | Fix |
|---|---|---|
| **Business cold-email list in a public repo** (37 addresses, MP Cutting Tools outreach) | `Uno-Builds/ClaudeDash/tier_a_cold_emails.md` | Move to private `mptools-site`, delete from `Uno-Builds`. It stays in git history; the addresses are public `info@` contacts, so a history rewrite is optional. |
| Trailing hyphen in repo name | `Netflix-dimmer-` | Rename to `Nightshade` (its README name) |
| `jinji` and `meridianflow-site` share 73% of files | both private | Decide which is live, then archive the other |
| ~40 repos have no real README (only Rider's `.run/Readme.md`) | flagships first | Add a README with a screenshot and run steps to every flagship |
| Committed `bin/obj` | Habits, HorizontalCalendar, PuckUp, EV-ChargingApp | Archive (see above) |
| Local-only work at risk | `SampleBuilds` (GitHub has 2 files; local MorningCard has 89 uncommitted files) | Commit and push from the local machine |
| Open PR | `Uno-Builds#3` (FieldCheck) | Merge or close. FieldCheck folds into Patina. |

No API keys or tokens were found in `Uno-Builds` (pattern scan of the current tree).

---

## 8. Suggested order

1. Run `github-cleanup.ps1` in dry-run mode, read the list, then run it for real (deletes need `gh auth refresh -s delete_repo`).
2. Rescue the `Uno-Builds-net10` / `Workflow` folders into `Uno-Builds`, then archive both.
3. Split Liveline into its own repo/NuGet. This unblocks Cargo and Meridian.
4. Ship DYT deep linking (already this week's plan), then the call-ups write path.
5. Patina map view + custody log. Those are the two backlog samples, built as features of an app that already exists.
