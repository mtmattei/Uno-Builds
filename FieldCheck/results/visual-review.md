# FieldCheck — Visual review (Uno Platform)

Implementation screenshots: `results/screenshots/android/` and `results/screenshots/windows/`.
They come from the final CI verification run; the raw per-run captures are in `results/ci/run-*/`.

## Capture conditions

| Target | Reference | Implementation capture |
|---|---|---|
| Android | 412 × 915 px | Emulator `pixel_6`, API 34, 1080 × 2400 px at 420 dpi = **411.4 × 914.3 dp** (scale 2.625). The status bar and gesture bar are included; the reference omits them. |
| Windows | 1440 × 900 px | windows-latest runner at 1920 × 1080, window **client area sized to 1440 × 900**, 100 % scale, captured with PrintWindow and cropped to the client area. |

Fonts: Roboto (Android) and Segoe UI Variable (Windows) — the platform system sans (`UnoDefaultFont=None`).
The references use a different grotesk, so glyph widths and x-heights differ slightly throughout.
Per ACCEPTANCE_CRITERIA H this is rasterization/font difference, not structural divergence.

## Global differences (all screens)

- **Icons**: the reference navigation and search icons render as placeholder glyph boxes ("▯", tofu).
  The implementation draws real outline icons: a four-square grid for Dashboard, the reference's own diamond for Assets,
  a list-with-corner icon for History, and a magnifier for search. Size, stroke weight and placement follow the reference.
- **Weights**: titles use Medium weight. Roboto Medium on Android reads slightly heavier than the reference.
- **Android system bars**: the reference is a bare 412 × 915 canvas. The device captures include the status bar
  (content is offset by the safe-area inset, about 24 dp) and the gesture handle below the bottom navigation.

## H01 — Dashboard

| Aspect | Android | Windows |
|---|---|---|
| Structure | Match: micro brand label, greeting, facility/date line, "12 assets" metric, three condition summaries, rule, NEEDS ATTENTION rows, primary button, bottom nav. | Match: 232 px sidebar (FIELDCHECK + accent "F" mark, nav items, "Alex Morgan / Facility A" footer), greeting, metric, summaries at the reference's 182 px columns, rule, rows with chip column and chevron, left-aligned "View all assets". |
| Content | **Different by spec**: all 5 Attention/Critical assets are listed (Critical first). The reference shows 4 and omits EF-090 Emergency Fan 90, but the spec says the list contains the Attention *and* Critical assets and the counts show 2 Critical. The greeting follows the device clock ("Good evening" in the evening captures). | Same as Android. |
| Spacing/alignment | 20 px side margins; vertical rhythm within ±6 px of the reference. The metric block sits about 50 px lower because of the status-bar inset. | 40 px content margin (x = 272 as in the reference). |
| Typography | Hierarchy matches (48 metric, 30 title, 17 row titles, 13 secondary, 11 micro labels). | Same. |
| Colour | Tokens applied exactly (Canvas, Ink, Muted, Rule, Accent, semantic trio, soft chip fills). | Same. |
| Borders/radii | 1 px rules, 13 px chip radius, 12 px button radius. | Same. |
| Sizing/density | Rows 110 px (3 text lines) vs the reference's ~92 px: Roboto line height is taller. | Rows 86 px, as in the reference. |

## H02 — Assets

- **Android**: matches. Title, "12 equipment records", search field with leading icon, filter pills (All filled Ink),
  3-line rows with status chip and chevron.
- **Windows**: matches the master pane — 461 px pane on CanvasRaised with a right rule, search, pills, 2-line rows
  and chips. The rows have no chevron, as in the reference.
- **Differences**:
  - Pill labels are 13 px Roboto, so "Operational" is about 8 px wider.
  - Between 1000 and 1280 px (not a reference size) the master is 380 px and pills scroll horizontally.

## H03 — Asset Detail (Android) / master-detail (Windows)

- **Android**: matches. Back chevron + "Asset detail", 30 px asset title, chip, 2 × 2 label/value sheet, rule,
  DESCRIPTION (the full CT-007 text wraps across 7 lines), rule, LATEST INSPECTION in semantic colour, notes,
  full-width "Start inspection".
  - **Addition**: a muted meta line "INS-24072 · Sep 8, 2026 · Sam Rivera" under the latest-inspection summary.
    It identifies which record is shown and is used to verify D18.
- **Windows**: matches. The selected row gets a Surface fill and a 4 px Accent bar. The detail pane has the title and
  chip, "Start inspection" top-right (199 × 52), the label/value sheet in two columns, then description and latest inspection.
  - The description measure is 620 px wide; the reference is about 605 px.

## H04 — New Inspection

- **Android**: structure matches — condition segments (selected = Ink fill), OPERATING NORMALLY with "Yes" and an
  Ink/Accent toggle, temperature field, three accent checkboxes, ISSUE DESCRIPTION with an orange "Required" marker,
  the attach field, and the primary submit.
  - **Additions required by the spec but absent from the mobile reference**: the NOTES field; the validation summary
    line ("To submit: …"); and a quiet Cancel button under Submit (spec: "Cancel returns without saving").
  - The toggle is Fluent's 40 × 20 geometry; the reference is about 48 × 28.
- **Windows**: matches the reference's two-column composition — left 381 px column (condition, operating, temperature),
  CHECKLIST in the right column, full-width rule, then Issue description, Notes and Attach in an 821 px column.
  Cancel (quiet) and Submit (195 × 52) sit right-aligned at the bottom.
- **Attachment**: after picking, the field shows a check icon, the file name and a Remove button.
  On Android the system Photo Picker supplies its own display name (e.g. `1000000016.png`).

## H05 — Inspection Success

- **Match on both targets**: 68 px Accent circle with a check, "Inspection saved", the generated ID, a rule,
  asset name + condition chip, "date · Alex Morgan", the local-storage sentence, and two actions
  (Android: stacked full width; Windows: side by side, 191 px each).
- **ID**: the generated ID is INS-24092 (next after the fixture's highest, INS-24091); the reference shows INS-24102.
  REFERENCE_INDEX allows differences in generated values.

## H06 — History

- **Android**: matches — title, count, search, condition pills, rows (asset name, "ID · timestamp", condition chip).
  - **Addition required by the spec**: the inspector name on each row ("Each row shows … inspector"). It is not in the
    mobile reference, so rows are one line taller.
- **Windows**: matches the table composition — ASSET / INSPECTION / INSPECTOR / RESULT headers over rules,
  search (415 px) with pills to its right, 66 px rows.
- **Content**: the implementation lists all 7 records (6 fixtures + the new one).
  The reference shows 6 rows and omits INS-24072 Cooling Tower 07 while its subtitle says "7 completed inspections".

## Responsive behaviour

- **Android** was verified at 412 dp portrait and at 1.3× font scale. Nothing is clipped, and the form stays
  reachable with the keyboard open.
- **Windows** was verified at 1440, 1100, 900 and 760 px:
  - The sidebar persists at every width (760 × 560 minimum window size).
  - Assets collapses to a single pane below 1000 px.
  - The form becomes a single column below 1100 px.
  - History drops the table header and uses stacked rows.
  - No horizontal scrolling of primary content.

## Verdict

No structural divergence and no redesign. The remaining differences are font metrics, real icons in place of
placeholder glyphs, and additions the spec requires but the references omit: Notes, Cancel, the inspector on rows,
and all Needs-attention assets.
