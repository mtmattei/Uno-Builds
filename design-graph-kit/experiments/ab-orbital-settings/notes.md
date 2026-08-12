# Supplementary Notes — Orbital Settings (arm C only)

Free-prose interaction and semantics notes accompanying `brief.md`. These carry
the same information as the Design Graph given to arm B, but unstructured —
the way a PM/designer might annotate a handoff after talking to engineering.

## Interactions & states

- **Page entrance:** the four cards fade in and rise ~16px on load, staggered
  100 ms apart (0/100/200/300 ms), roughly 350 ms each, ease-out.
- **Save:** persists the display name; on click the button's label becomes
  `Saved!` for 1.5 seconds, then reverts to `Save`. (Persistence itself goes
  through the app's settings service — out of scope for a standalone page.)
- **Clear Recent Projects:** clears the list, then shows a dialog — title
  `Cleared`, body `Recent projects list has been cleared.`, single `OK`
  close button.
- **Uno Platform Documentation:** opens
  `https://platform.uno/docs/articles/intro.html` in the default browser.
- **Open Data Folder:** target folder is app-determined and not specified
  here; leave the actual behavior to the app layer.
- **Header search pill:** opens some kind of search/command UI that is still
  TBD — render it, but do not wire a click action.

## Reuse & naming (from the codebase conventions)

- The four section containers share one card treatment; the ABOUT label/value
  rows are one repeated pattern; the PATHS label-over-value fields are another
  (keep the two patterns distinct); the three action buttons share one quiet
  button style.
- Existing developer names, if you want to match them: sections
  `ProfileSection` / `AboutSection` / `PathsSection` / `ActionsSection`;
  name input `UsernameBox`; save button `SaveUsernameButton`; action buttons
  `ClearRecentsButton` / `OpenDataFolderButton` / `OpenDocsButton`.
- The ABOUT values bind to env/status sources (`EnvStatus.UnoSdkVersion`,
  `DotNetDisplay`, `EnvStatus.Renderer`, `PlatformInfo`); PATHS values to
  `ProjectRoot` / `RecentsPath` / `SkillsPath`; the version label to
  `VersionDisplay` with fallback `v0.1.0-alpha`. No ViewModel exists in this
  exercise — placeholders with these names in mind are fine.
- Treat the brief's recurring colors/spacings/radii as a small named token
  set rather than scattering raw values.
