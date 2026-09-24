# FieldCheck — Acceptance results (Uno Platform)

Evidence run: `run-25-full` (source commit `8bd3a03`), CI outputs in `results/ci/run-25-full/`.

**PASS 95 · FAIL 1 · NOT TESTED 0**

Legend: `A:` Android emulator driver check, `W:` Windows UIA driver check, `D:` Desktop driver check, unit tests = NUnit suite on net10.0.

| ID | Criterion | Status | Evidence |
|---|---|---|---|
| A01 | Project uses .NET 10 and the frozen framework assignment | **PASS** | net10.0 TFMs + Uno.Sdk 6.7.30 (app/FieldCheck/global.json, FieldCheck.csproj) |
| A02 | Exact framework/SDK versions recorded | **PASS** | results/environment.json, results/dependencies.txt |
| A03 | Android Release restores and builds with zero errors | **PASS** | Android Release build: succeeded, 0 warning(s) |
| A04 | Windows Release restores and builds with zero errors | **PASS** | Windows Release build: succeeded, 0 warning(s) |
| A05 | Android launches to a usable Dashboard | **PASS** | A:A05.dashboard_usable: pass<br>A:J04.clean_first_run: pass (cold start TotalTime=5524ms) |
| A06 | Windows launches to a usable Dashboard | **PASS** | W:A06.dashboard_usable: pass (client=(1440, 900))<br>W:J04.clean_first_run: pass (startup=1.75s) |
| A07 | Compiler warnings recorded; avoidable app warnings resolved | **PASS** | Android 0 / Windows 0 warning(s) in Release builds |
| A08 | Desktop head builds and launches; incremental effort recorded separately | **PASS** | Desktop head (Skia, Win32 host) UIA driver: 36/36 checks; Linux X11 head verified locally under Xvfb (results/screenshots/desktop-linux)<br>*Uno-only, after core checkpoint* |
| B01 | Dashboard exists and matches reference hierarchy | **PASS** | A:B01.needs_attention_rows: pass<br>A:C02.counts_7_3_2: pass<br>W:C02.counts: pass<br>H01 |
| B02 | Assets exists | **PASS** | A:B07.bottom_nav_assets: pass<br>W:B09.sidebar_assets: pass |
| B03 | Asset Detail exists | **PASS** | A:B03.asset_detail: pass<br>W:B10.master_detail: pass |
| B04 | New Inspection exists | **PASS** | A:B04.new_inspection: pass<br>W:B04.new_inspection: pass |
| B05 | Inspection Success exists | **PASS** | A:B05.success: pass<br>W:B05.success: pass |
| B06 | History exists | **PASS** | A:C09.history_contains_new: pass<br>W:C09.history_new_first: pass |
| B07 | Android primary navigation reaches Dashboard / Assets / History | **PASS** | A:B07.bottom_nav_assets: pass<br>A:B12.view_history: pass<br>A:C10.persisted_after_relaunch: pass (relaunch TotalTime=3428ms)<br>A:C02.counts_updated_after_save: pass<br>*bottom navigation used for all three destinations* |
| B08 | Android back: Inspection -> Asset Detail -> originating list | **PASS** | A:B08.back_inspection_to_detail: pass<br>A:B08.back_detail_to_assets: pass<br>A:F06.back_detail_to_dashboard: pass<br>A:B08.bottom_nav_hidden_on_detail: pass |
| B09 | Windows sidebar reaches Dashboard / Assets / History | **PASS** | W:B09.sidebar_dashboard: pass<br>W:B09.sidebar_assets: pass<br>W:B09.sidebar_history: pass |
| B10 | Windows wide Assets view is master/detail | **PASS** | W:B10.master_detail: pass<br>W:F03.reference_viewport: pass ((1440, 900)) |
| B11 | Cancel from New Inspection returns without creating an inspection | **PASS** | A:B11.cancel_returns: pass<br>W:B11.cancel_returns: pass<br>unit tests: 47 passed, 0 failed |
| B12 | Success actions navigate to the correct Asset and History destinations | **PASS** | A:B12.view_asset: pass<br>A:B12.view_history: pass<br>W:D18.asset_reflects_newest: pass<br>unit tests: 47 passed, 0 failed |
| C01 | Seed contains all 12 supplied assets | **PASS** | unit tests: 47 passed, 0 failed<br>A:A05.dashboard_usable: pass |
| C02 | Dashboard counts derived from data | **PASS** | unit tests: 47 passed, 0 failed<br>A:C02.counts_7_3_2: pass<br>A:C02.counts_updated_after_save: pass<br>*counts change after a save (4 Attention / 1 Critical)* |
| C03 | Assets search matches name, ID, type, location case-insensitively | **PASS** | unit tests: 47 passed, 0 failed<br>A:C03.search_location_case_insensitive: pass (['Air Handler 203', 'Cooling Tower 07', 'Exhaust Fan 305'])<br>W:C03.search_type: pass |
| C04 | Asset status filter | **PASS** | unit tests: 47 passed, 0 failed<br>A:C04.status_filter_attention: pass<br>A:C04.filter_reset_all: pass<br>W:C04.filter_critical: pass |
| C05 | Search + status filter combine | **PASS** | unit tests: 47 passed, 0 failed<br>A:C05.search_plus_filter: pass |
| C06 | History is newest-first | **PASS** | unit tests: 47 passed, 0 failed<br>A:C06.history_newest_first: pass (['INS-24092', 'INS-24091', 'INS-24044', 'INS-24086', 'INS-24065', 'INS-24058', 'INS-24072']) |
| C07 | History search matches asset name or ID | **PASS** | unit tests: 47 passed, 0 failed<br>A:C07.history_search_id: pass<br>W:C07.history_search: pass |
| C08 | History condition filter works | **PASS** | unit tests: 47 passed, 0 failed<br>A:C08.history_condition_filter: pass<br>W:C08.history_filter: pass |
| C09 | Submitted inspection persisted locally | **PASS** | unit tests: 47 passed, 0 failed<br>A:C09.history_contains_new: pass<br>W:C09.history_new_first: pass |
| C10 | Inspection remains after full process termination and relaunch | **PASS** | unit tests: 47 passed, 0 failed<br>A:C10.persisted_after_relaunch: pass (relaunch TotalTime=3428ms)<br>W:C10.persisted_after_relaunch: pass (startup=0.79s)<br>*Android: am force-stop + relaunch; Windows: process kill + relaunch* |
| C11 | Saving does not mutate benchmark fixture files | **PASS** | unit tests: 47 passed, 0 failed<br>*SHA-256 of mock-data before/after save; fixtures are embedded read-only* |
| C12 | Generated inspection IDs unique and persisted | **PASS** | unit tests: 47 passed, 0 failed<br>A:C12.ids_unique: pass (['INS-24092', 'INS-24091', 'INS-24044', 'INS-24086', 'INS-24065', 'INS-24058', 'INS-24072']) |
| D01 | Condition supports Good / Attention / Critical | **PASS** | A:D10.issue_hidden_good_yes: pass<br>A:D11.issue_shown_critical: pass<br>A:D11.issue_shown_attention: pass<br>W:D11.issue_shown: pass |
| D02 | Operating normally supports Yes/No | **PASS** | A:D12.issue_shown_not_operating: pass<br>unit tests: 47 passed, 0 failed |
| D03 | Temperature accepts -50 through 250 inclusive | **PASS** | unit tests: 47 passed, 0 failed<br>A:D03.temperature_min_accepted: pass |
| D04 | Out-of-range temperature shows error and blocks submit | **PASS** | unit tests: 47 passed, 0 failed<br>A:D04.temperature_out_of_range_error: pass<br>W:D04.temp_error: pass |
| D05 | All three checklist items individually required | **PASS** | unit tests: 47 passed, 0 failed<br>A:D05.one_item_missing_blocks: pass |
| D06 | Notes accepts multiline optional content | **PASS** | unit tests: 47 passed, 0 failed<br>A:F02.form_scrolls_with_keyboard: pass<br>android 04-new-inspection (two-line notes) |
| D07 | File/image picker opens from Attach photo/file | **PASS** | A:D07.picker_opens: pass (com.google.android.providers.media.module)<br>W:D07.picker_opens: pass<br>*Android 14 system Photo Picker; Windows Win32 Open dialog* |
| D08 | Selecting a file shows filename | **PASS** | A:D08.picker_selected_filename: pass (file name shown: | 1000000016.png)<br>W:D08.picker_filename: pass |
| D09 | Cancelling the picker leaves the form usable | **PASS** | A:D09.picker_cancel_keeps_form: pass<br>W:D09.picker_cancel: pass<br>unit tests: 47 passed, 0 failed |
| D10 | Issue Description hidden for Good + Yes | **PASS** | unit tests: 47 passed, 0 failed<br>A:D10.issue_hidden_good_yes: pass |
| D11 | Issue Description shown for Attention or Critical | **PASS** | unit tests: 47 passed, 0 failed<br>A:D11.issue_shown_attention: pass<br>A:D11.issue_shown_critical: pass<br>W:D11.issue_shown: pass |
| D12 | Issue Description shown when Operating normally is No | **PASS** | unit tests: 47 passed, 0 failed<br>A:D12.issue_shown_not_operating: pass |
| D13 | When shown, Issue Description is required | **PASS** | unit tests: 47 passed, 0 failed<br>*plus the on-screen summary 'describe the issue' (Android screenshots)* |
| D14 | Submit disabled until all currently-required fields valid | **PASS** | A:D14.submit_disabled_initially: pass (tapping the disabled Submit keeps the form and saves nothing)<br>A:D14.submit_enabled_when_valid: pass<br>W:D14.submit_disabled: pass<br>unit tests: 47 passed, 0 failed |
| D15 | Duplicate activation during save cannot create duplicates | **PASS** | unit tests: 47 passed, 0 failed<br>A:B05.success: pass<br>A:C12.ids_unique: pass (['INS-24092', 'INS-24091', 'INS-24044', 'INS-24086', 'INS-24065', 'INS-24058', 'INS-24072'])<br>*unit test with slow repository; double tap (Android) / double invoke (Windows) produced exactly one new record* |
| D16 | Successful submit creates exactly one inspection | **PASS** | unit tests: 47 passed, 0 failed<br>A:C09.history_contains_new: pass<br>*History shows 7 completed inspections after one submit* |
| D17 | Success view shows generated ID, asset and result | **PASS** | A:D17.success_details: pass<br>A:B05.success: pass<br>W:B05.success: pass<br>unit tests: 47 passed, 0 failed |
| D18 | Asset Detail reflects the newest inspection after save | **PASS** | A:D18.asset_reflects_newest: pass<br>W:D18.asset_reflects_newest: pass<br>unit tests: 47 passed, 0 failed |
| E01 | Initial data load has an intentional loading state | **FAIL** | A:E01.loading_state: FAIL<br>W:E01.loading: pass<br>*deterministic 'slow' repository mode* |
| E02 | Empty repository mode shows a deliberate empty state | **PASS** | A:E02.empty_state: pass<br>A:E02.empty_history: pass<br>W:E02.empty: pass<br>unit tests: 47 passed, 0 failed |
| E03 | Read failure shows a user-safe error state | **PASS** | A:E03.error_state: pass<br>W:E03.error: pass<br>unit tests: 47 passed, 0 failed |
| E04 | Error state exposes Retry and recovers | **PASS** | A:E04.retry_recovers: pass<br>W:E04.retry: pass<br>unit tests: 47 passed, 0 failed |
| E05 | Zero-match search shows no-results distinct from empty | **PASS** | A:E05.no_results_state: pass<br>W:E05.no_results: pass<br>unit tests: 47 passed, 0 failed |
| E06 | Validation messages identify what must be corrected | **PASS** | A:E06.missing_summary: pass<br>A:D04.temperature_out_of_range_error: pass<br>unit tests: 47 passed, 0 failed |
| E07 | CT-007 long description readable without clipping | **PASS** | A:E07.long_description_reachable: pass<br>android 03-asset-detail, windows 02-assets-master-detail |
| E08 | Persistence failure does not silently report success | **PASS** | A:E08.save_failure_not_silent: pass<br>W:E08.save_error: pass<br>unit tests: 47 passed, 0 failed |
| F01 | Android 412x915 has no clipped primary content | **PASS** | android screens (1080x2400 @ 420dpi = 411x914 dp) |
| F02 | Android form reachable with keyboard visible / dismissed | **PASS** | A:F02.form_scrolls_with_keyboard: pass |
| F03 | Windows 1440x900 matches master/detail composition | **PASS** | W:F03.reference_viewport: pass ((1440, 900))<br>W:B10.master_detail: pass<br>H03 |
| F04 | Windows contraction reflows before unusable | **PASS** | W:F04.narrow_single_pane: pass<br>windows responsive-*<br>*1100/900/760 px; 760x560 minimum window size keeps the sidebar* |
| F05 | Text scaling does not make workflows unusable | **PASS** | A:F05.font_scale_1_3_usable: pass (form actions reachable at 1.3x font scale)<br>W:F05.windows_text_scale_130: pass |
| F06 | Android system back follows navigation expectation | **PASS** | A:F06.back_detail_to_dashboard: pass<br>A:B08.back_inspection_to_detail: pass |
| F07 | Windows keyboard tab reaches controls in logical order | **PASS** | W:F07.keyboard_tab_reaches_controls: pass (['Show all statuses', 'Filter Operational', 'Filter Attention', 'Filter Critical', 'Booster Pump 104, PMP-104, Operation) |
| F08 | File picker appropriate to each platform | **PASS** | A:D07.picker_opens: pass (com.google.android.providers.media.module)<br>W:D07.picker_opens: pass |
| G01 | Interactive elements have accessible names | **PASS** | W:F07.keyboard_tab_reaches_controls: pass (['Show all statuses', 'Filter Operational', 'Filter Attention', 'Filter Critical', 'Booster Pump 104, PMP-104, Operation)<br>A:B01.needs_attention_rows: pass<br>*rows, nav, chips, inputs and icon buttons carry AutomationProperties.Name; drivers locate every control by accessible name* |
| G02 | Status communicated by text and colour | **PASS** | all list screens<br>*StatusChip always renders the label* |
| G03 | Visible keyboard focus indicator on Windows | **PASS** | windows focus-visible |
| G04 | Primary touch/click targets ~44 px minimum | **PASS** | Themes/FieldCheck.xaml (MinHeight 44-55 on buttons, chips, segments, checkboxes, rows) |
| G05 | Form errors associated with fields, not colour alone | **PASS** | A:D04.temperature_out_of_range_error: pass<br>A:E06.missing_summary: pass<br>*error text sits directly under its field; summary names each missing item* |
| G06 | Contrast consistent with the supplied palette | **PASS** | Ink/Canvas 15.8:1, Muted/Canvas 4.7:1, Success chip 5.7:1, Critical chip 5.5:1, Attention chip 4.3:1 (supplied palette pair, 13 px text) |
| H01 | Dashboard screenshot | **PASS** | H01 |
| H02 | Assets screenshot | **PASS** | H02 |
| H03 | Asset Detail (Android) / master-detail (Windows) screenshot | **PASS** | H03 |
| H04 | New Inspection screenshot | **PASS** | H04 |
| H05 | Inspection Success screenshot | **PASS** | H05 |
| H06 | History screenshot | **PASS** | H06 |
| I01 | MVVM is used | **PASS** | ViewModels/* (CommunityToolkit.Mvvm); views bind via x:Bind |
| I02 | Persistence separated from Views | **PASS** | Services/JsonFieldCheckRepository.cs behind IFieldCheckRepository |
| I03 | Business/validation logic not in code-behind | **PASS** | NewInspectionViewModel.Validate; code-behind limited to VM lookup, parameters, clicks, width detection |
| I04 | No dead code / abandoned duplicate implementation | **PASS** | review (unused ListView styles and IME workarounds removed) |
| I05 | No hard-coded test-only shortcut | **PASS** | data modes are launch-time repository decorators; no UI switch |
| I06 | Dependencies limited and recorded | **PASS** | results/dependencies.txt |
| I07 | Critical behaviour has automated tests | **PASS** | unit tests: 47 passed, 0 failed |
| I08 | Tests not weakened to make the build green | **PASS** | unit tests: 47 passed, 0 failed<br>*tests only extended; driver fixes never relaxed an app assertion* |
| J01 | No placeholder/lorem ipsum/debug UI | **PASS** | all screens |
| J02 | No dead buttons or links | **PASS** | W:J02.remove_attachment: pass<br>A:B12.view_history: pass<br>A:E04.retry_recovers: pass<br>A:C04.filter_reset_all: pass<br>*every button exercised on at least one platform* |
| J03 | No reproducible crash in the required workflow | **PASS** | A:J03.no_crash_in_logcat_crash_buffer: pass<br>Windows driver completed without exception |
| J04 | Clean install / first run succeeds | **PASS** | A:J04.clean_first_run: pass (cold start TotalTime=5524ms)<br>W:J04.clean_first_run: pass (startup=1.75s) |
| J05 | Process kill + relaunch preserves submitted inspection | **PASS** | A:C10.persisted_after_relaunch: pass (relaunch TotalTime=3428ms)<br>W:C10.persisted_after_relaunch: pass (startup=0.79s) |
| J06 | Background/resume does not corrupt state | **PASS** | A:J06.background_resume: pass (state preserved after HOME + relaunch)<br>W:J06.minimize_restore: pass |
| J07 | Picker cancel and failure paths don't break the app | **PASS** | A:D09.picker_cancel_keeps_form: pass<br>W:D09.picker_cancel: pass<br>unit tests: 47 passed, 0 failed<br>*failure path (picker throws) verified by unit test* |
| J08 | No secrets/credentials in source or output | **PASS** | grep for keys/tokens/passwords: none |
| J09 | Primary workflow exercised end-to-end on Android and Windows | **PASS** | A:C10.persisted_after_relaunch: pass (relaunch TotalTime=3428ms)<br>W:C10.persisted_after_relaunch: pass (startup=0.79s)<br>A:B05.success: pass<br>W:B05.success: pass |
| J10 | Release builds used for final verification | **PASS** | Android Release build: succeeded, 0 warning(s)<br>Windows Release build: succeeded, 0 warning(s)<br>*CI builds -c Release; the drivers run those binaries* |
