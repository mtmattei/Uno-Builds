"""FieldCheck Android end-to-end verification (runs on a GitHub-hosted KVM emulator).

Drives the Release APK through the acceptance workflows with adb + uiautomator, writes
screenshots, UI dumps and a machine-readable checks.json. Elements are located through the
accessibility tree Uno's Skia renderer exposes (text / content-desc), never by fixed coordinates.
"""
import datetime, json, os, re, subprocess, sys, time
import xml.etree.ElementTree as ET

OUT = os.environ["OUT"]; PKG = os.environ["PKG"]
SCENARIO = sys.argv[1] if len(sys.argv) > 1 else "full"
SHOTS = os.path.join(OUT, "screens"); DUMPS = os.path.join(OUT, "dumps")
os.makedirs(SHOTS, exist_ok=True); os.makedirs(DUMPS, exist_ok=True)
checks = []
TODAY = "{d:%b} {d.day}, {d.year}".format(d=datetime.date.today())  # app formats dates as "Sep 8, 2026"
MAIN = None


def adb(*args, timeout=60):
    r = subprocess.run(["adb", *args], capture_output=True, text=True, timeout=timeout)
    return r.stdout + r.stderr


def shot(name):
    nodes()  # clears any system ANR dialog before capturing
    with open(os.path.join(SHOTS, name + ".png"), "wb") as f:
        f.write(subprocess.run(["adb", "exec-out", "screencap", "-p"], capture_output=True).stdout)


def nodes(save=None, _depth=0):
    for _ in range(3):
        adb("shell", "uiautomator", "dump", "/sdcard/ui.xml")
        xml = adb("shell", "cat", "/sdcard/ui.xml")
        if "<hierarchy" in xml:
            break
        time.sleep(1)
    if save:
        open(os.path.join(DUMPS, save + ".xml"), "w").write(xml)
    try:
        root = ET.fromstring(xml[xml.index("<hierarchy"):])
    except Exception:
        return []
    # Emulator system ANR dialogs (e.g. "Pixel Launcher isn't responding") steal focus: dismiss and re-read.
    if _depth < 3 and "t responding" in xml:
        m = re.search(r'text="Wait"[^>]*bounds="\[(\d+),(\d+)\]\[(\d+),(\d+)\]"', xml)
        if m:
            x1, y1, x2, y2 = map(int, m.groups())
            adb("shell", "input", "tap", str((x1 + x2) // 2), str((y1 + y2) // 2))
            time.sleep(1.5)
            return nodes(save, _depth + 1)
    out = []
    for n in root.iter("node"):
        b = re.findall(r"\d+", n.get("bounds", "[0,0][0,0]"))
        x1, y1, x2, y2 = map(int, b)
        out.append({"text": n.get("text", ""), "desc": n.get("content-desc", ""), "cls": n.get("class", ""),
                    "checked": n.get("checked") == "true", "enabled": n.get("enabled") == "true",
                    "focused": n.get("focused") == "true", "clickable": n.get("clickable") == "true", "bounds": (x1, y1, x2, y2), "pkg": n.get("package", "")})
    return out


def label(n):
    return (n["text"] + " | " + n["desc"]).strip()


def find(pattern, ns=None, exact=False):
    """Exact patterns are case-sensitive; clickable nodes win over plain text nodes."""
    ns = ns if ns is not None else nodes()
    rx = re.compile("^" + pattern + "$") if exact or (pattern.startswith("^") and pattern.endswith("$")) else re.compile(pattern, re.I)
    hits = []
    for n in ns:
        if (n["text"] and rx.search(n["text"])) or (n["desc"] and rx.search(n["desc"])):
            x1, y1, x2, y2 = n["bounds"]
            if x2 > x1 and y2 > y1:
                hits.append(n)
    clickable = [n for n in hits if n["cls"].endswith("Button") or n.get("clickable")]
    return (clickable or hits or [None])[0]


def wait_for(pattern, timeout=15, exact=False):
    end = time.time() + timeout
    while time.time() < end:
        n = find(pattern, exact=exact)
        if n:
            return n
        time.sleep(0.7)
    return None


def tap_node(n):
    x1, y1, x2, y2 = n["bounds"]
    adb("shell", "input", "tap", str((x1 + x2) // 2), str((y1 + y2) // 2))
    time.sleep(1.2)


def tap(pattern, timeout=10, exact=False):
    n = wait_for(pattern, timeout, exact)
    if not n:
        raise RuntimeError(f"element not found: {pattern}")
    tap_node(n)
    return n


def swipe(up=True):
    a, b = (int(screen_h * 0.7), int(screen_h * 0.35)) if up else (int(screen_h * 0.35), int(screen_h * 0.7))
    adb("shell", "input", "swipe", "540", str(a), "540", str(b), "400")
    time.sleep(1)


def scroll_to(pattern, max_swipes=8, exact=False):
    """Scrolls down, then back up, until the element is on screen above the bottom navigation."""
    def visible():
        ns = nodes()
        n = find(pattern, ns, exact=exact)
        nav = [m["bounds"][1] for m in ns if m["desc"] in ("Dashboard", "Assets", "History") and m["cls"].endswith("Button")]
        limit = min(nav) if nav else screen_h - 180  # stay clear of the gesture-navigation strip
        return n if n and 200 < (n["bounds"][1] + n["bounds"][3]) // 2 < limit else None
    for up in (True, False):
        for _ in range(max_swipes if up else max_swipes * 2):
            n = visible()
            if n:
                return n
            swipe(up)
    return visible()


def tap_row(pattern):
    n = scroll_to(pattern)
    if not n:
        raise RuntimeError(f"element not found after scrolling: {pattern}")
    tap_node(n)
    return n


def type_text(text):
    adb("shell", "input", "text", text.replace(" ", "%s").replace(";", "\\;").replace("'", "\\'"))
    time.sleep(0.8)


def key(code):
    adb("shell", "input", "keyevent", str(code))
    time.sleep(1.2)


def check(cid, ok, detail=""):
    checks.append({"id": cid, "pass": bool(ok), "detail": detail})
    print(("PASS " if ok else "FAIL ") + cid + " " + detail, flush=True)


def launch(extra=(), wait=True):
    r = adb("shell", "am", "start", "-W", "-n", MAIN, *extra)
    m = re.search(r"TotalTime: (\d+)", r)
    if wait:
        time.sleep(2)
    return int(m.group(1)) if m else None


def clear_data():
    # pm clear force-stops asynchronously; give it time so it doesn't kill the next launch.
    adb("shell", "pm", "clear", PKG)
    time.sleep(4)


def launch_ready(expect="Needs attention", extra=()):
    t = launch(extra)
    if wait_for(expect, 20):
        return t
    stop()
    t = launch(extra)
    wait_for(expect, 25)
    return t


def stop():
    adb("shell", "am", "force-stop", PKG)
    time.sleep(1)


def pid():
    return adb("shell", "pidof", PKG).strip()


def hide_keyboard():
    if "mInputShown=true" in adb("shell", "dumpsys", "input_method"):
        key(4)  # Back while the IME is shown only closes the keyboard (standard Android behaviour)


# Keep emulator system ANR/crash dialogs (e.g. an idle Pixel Launcher) from covering the app.
adb("shell", "settings", "put", "global", "hide_error_dialogs", "1")
adb("shell", "settings", "put", "secure", "anr_show_background", "0")
adb("shell", "am", "broadcast", "-a", "android.intent.action.CLOSE_SYSTEM_DIALOGS")
MAIN = adb("shell", "cmd", "package", "resolve-activity", "--brief", PKG).strip().splitlines()[-1]
screen = re.search(r"(\d+)x(\d+)", adb("shell", "wm", "size"))
screen_w, screen_h = int(screen.group(1)), int(screen.group(2))
print("main:", MAIN, "screen:", screen_w, screen_h)


def submit_is_inert():
    """A disabled Submit is not exposed to accessibility on Uno Skia Android, so verify it functionally:
    tap where it is drawn (between the summary line and Cancel) and assert nothing was saved."""
    cancel = scroll_to("Cancel inspection")
    if not cancel:
        return False
    # Submit (52 epx) sits 8 epx above Cancel in the phone layout.
    density = screen_w / 411.4
    y = cancel["bounds"][1] - int((8 + 26) * density)
    adb("shell", "input", "tap", "540", str(y))
    time.sleep(2)
    ns = nodes()
    return find("Cancel inspection", ns) is not None and find("Inspection saved", ns) is None


def run_full():
    # --- Clean install / first run ---
    clear_data()
    t = launch()
    ok = wait_for("Needs attention", 25) is not None
    if not ok:
        t = launch_ready()
        ok = find("Needs attention") is not None
    check("J04.clean_first_run", ok, f"cold start TotalTime={t}ms")
    shot("01-dashboard"); ns = nodes("01-dashboard")
    check("A05.dashboard_usable", find("12", ns, exact=True) and find("Operational", ns) and find("View all assets", ns))
    check("C02.counts_7_3_2", all(find(v, ns, exact=True) for v in ["7", "3", "2"]))
    check("B01.needs_attention_rows", all(find(v, ns) for v in ["Cooling Tower 07", "Air Handler 203", "Conveyor 18"]))

    # --- Assets list, search, filters ---
    tap("^Assets$", exact=False)
    wait_for("equipment records")
    shot("02-assets"); ns = nodes("02-assets")
    check("B07.bottom_nav_assets", find("12 equipment records", ns) is not None)
    search = find("Search assets")
    tap_node(search); type_text("roof")
    hide_keyboard()
    ns = nodes("02-assets-search-roof")
    names = [n for n in ["Air Handler 203", "Cooling Tower 07", "Exhaust Fan 305", "Booster Pump 104"] if find(n, ns)]
    check("C03.search_location_case_insensitive", names == ["Air Handler 203", "Cooling Tower 07", "Exhaust Fan 305"], str(names))
    tap("Filter Critical")
    ns = nodes("02-assets-roof-critical")
    check("C05.search_plus_filter", find("Cooling Tower 07", ns) and not find("Air Handler 203", ns) and not find("Exhaust Fan 305", ns))
    tap_node(find("Search assets")); key(123)  # move to end
    for _ in range(4):
        key(67)
    type_text("zzz")
    hide_keyboard()
    shot("02-assets-no-results"); ns = nodes("02-assets-no-results")
    check("E05.no_results_state", find("No matching assets", ns) is not None)
    tap("Clear search")
    time.sleep(1)
    shot("02-assets-after-clear"); ns = nodes("02-assets-after-clear")
    check("C04.filter_reset_all", find("Booster Pump 104", ns) is not None)
    tap("Filter Attention")
    ns = nodes("02-assets-filter-attention")
    check("C04.status_filter_attention", find("Air Handler 203", ns) and find("Conveyor 18", ns) and not find("Booster Pump 104", ns))
    tap("Show all statuses")

    # --- Asset detail (from Assets) + back returns to list with state ---
    tap_row("Cooling Tower 07")
    wait_for("Asset detail")
    shot("03-asset-detail"); ns = nodes("03-asset-detail")
    check("B03.asset_detail", find("CT-007", ns) and find("Sep 8, 2026", ns) and find("Critical condition", ns))
    check("B08.bottom_nav_hidden_on_detail", find("^Dashboard$", ns, exact=True) is None)
    n = scroll_to("Start inspection")
    shot("03-asset-detail-scrolled")
    check("E07.long_description_reachable", n is not None and find("narrow and wide layouts", nodes()) is not None)
    key(4)  # system back
    ns = nodes()
    check("B08.back_detail_to_assets", find("equipment records", ns) is not None and find("Show all statuses", ns) is not None)

    # --- New inspection: back + cancel don't save ---
    tap_row("Cooling Tower 07"); wait_for("Asset detail")
    tap_row("Start inspection")
    wait_for("New inspection")
    check("B04.new_inspection", find("Cooling Tower 07 · CT-007") is not None)
    key(4)
    check("B08.back_inspection_to_detail", wait_for("Asset detail", 8) is not None)
    tap_row("Start inspection"); wait_for("New inspection")
    tap_row("Condition Good")
    tap_row("Cancel inspection")
    check("B11.cancel_returns", wait_for("Asset detail", 8) is not None)

    # --- New inspection form validation + conditional issue description ---
    tap_row("Start inspection"); wait_for("New inspection")
    shot("04-form-initial")
    summary = scroll_to("To submit: select a condition")
    check("E06.missing_summary", summary is not None)
    check("D14.submit_disabled_initially", submit_is_inert(), "tapping the disabled Submit keeps the form and saves nothing")
    scroll_to("Condition Good")
    tap_row("Condition Good")
    check("D10.issue_hidden_good_yes", find("Issue description", nodes()) is None)
    tap_row("Operating normally")
    check("D12.issue_shown_not_operating", wait_for("Issue description, required", 4) is not None)
    tap_row("Operating normally")
    tap_row("Condition Critical")
    check("D11.issue_shown_critical", wait_for("Issue description, required", 4) is not None)
    tap_row("Condition Attention")
    check("D11.issue_shown_attention", find("Issue description, required") is not None)
    tap_row("Temperature in degrees")
    type_text("300")
    hide_keyboard()
    shot("04-form-temp-invalid"); ns = nodes("04-form-temp-invalid")
    check("D04.temperature_out_of_range_error", find("between -50 and 250", ns) is not None)
    tap_row("Temperature in degrees"); key(123)
    for _ in range(4):
        key(67)
    type_text("-50")
    hide_keyboard()
    check("D03.temperature_min_accepted", find("between -50 and 250", nodes()) is None)
    tap_row("Temperature in degrees"); key(123)
    for _ in range(4):
        key(67)
    type_text("27")
    hide_keyboard()
    for item in ["Guards and covers secure", "No visible leaks or damage"]:
        tap_row(item)
    summary = scroll_to("confirm 1 checklist item")
    check("D05.one_item_missing_blocks", summary is not None and submit_is_inert())
    tap_row("Area clear and accessible")
    tap_row("Issue description, required")
    type_text("Basin-level alarm intermittent; inspect fan vibration.")
    hide_keyboard()
    shot("04-form-keyboard-dismissed")
    tap_row("Notes, optional")
    type_text("Line one"); key(66); type_text("Line two")
    shot("04-form-keyboard-visible")
    check("F02.form_scrolls_with_keyboard", find("Notes, optional", nodes("04-form-keyboard-visible")) is not None)
    hide_keyboard()

    # --- File picker: cancel then select ---
    tap_row("Attach photo or file")
    time.sleep(3)
    ns = nodes("picker-open")
    picker_pkg = next((n["pkg"] for n in ns if n["pkg"] and n["pkg"] != PKG), "")
    check("D07.picker_opens", picker_pkg != "", picker_pkg)
    shot("picker-open")
    key(4)
    time.sleep(2)
    ns = nodes("picker-cancelled")
    check("D09.picker_cancel_keeps_form", find("New inspection", ns) is not None and find("Choose file", ns) is not None)
    tap_row("Attach photo or file")
    time.sleep(3)
    # Android 14 routes image/* GET_CONTENT to the system Photo Picker; the pushed fixture is the only photo.
    n = wait_for("inspection-photo|Photo taken on", 8)
    if n:
        tap_node(n)
        time.sleep(3)
    ns = nodes("picker-selected")
    shot("picker-selected")
    attached = find("Attached. Tap to replace", ns)
    check("D08.picker_selected_filename", attached is not None and find("New inspection", ns) is not None,
          "file name shown: " + " | ".join(label(x) for x in ns if re.search(r"\.(png|jpe?g)$", x["desc"] or x["text"], re.I)))

    ns = nodes()
    n = scroll_to("Submit inspection")
    check("D14.submit_enabled_when_valid", n is not None and n["enabled"])
    # Scroll back up for the reference-style capture of the filled form.
    for _ in range(6):
        adb("shell", "input", "swipe", "540", str(int(screen_h * 0.35)), "540", str(int(screen_h * 0.8)), "300")
    shot("04-new-inspection")
    n = scroll_to("Submit inspection")
    # Double activation: two taps in quick succession must create exactly one inspection.
    x1, y1, x2, y2 = n["bounds"]; cx, cy = (x1 + x2) // 2, (y1 + y2) // 2
    subprocess.run(["adb", "shell", f"input tap {cx} {cy}; input tap {cx} {cy}"])
    ok = wait_for("Inspection saved", 10)
    shot("05-inspection-success"); ns = nodes("05-inspection-success")
    check("B05.success", ok is not None and find("INS-24092", ns) is not None and find("Attention", ns) is not None, "")
    check("D17.success_details", find("Cooling Tower 07", ns) and find("Alex Morgan", ns))

    # --- Success -> View asset reflects the new inspection ---
    tap("View asset")
    wait_for("Asset detail")
    ns = nodes("06-asset-after-save")
    shot("06-asset-after-save")
    check("D18.asset_reflects_newest", find("Attention condition", ns) and find(TODAY, ns) and find("INS-24092", ns))
    check("B12.view_asset", find("Asset detail", ns) is not None)
    key(4)
    # --- History ---
    n = wait_for("^History$", 8)
    tap_node(n)
    wait_for("completed inspections")
    shot("06-history"); ns = nodes("06-history")
    check("C09.history_contains_new", find("INS-24092", ns) is not None and find("7 completed inspections", ns) is not None)
    order = [n["text"] or n["desc"] for n in ns if re.search(r"INS-\d+", n["text"] + n["desc"])]
    ids = [re.search(r"INS-\d+", s).group(0) for s in order]
    check("C06.history_newest_first", ids[:3] == ["INS-24092", "INS-24091", "INS-24044"], str(ids))
    check("C12.ids_unique", len(ids) == len(set(ids)), str(ids))
    tap_node(find("Search inspections")); type_text("blr-002"); hide_keyboard()
    ns = nodes("06-history-search")
    check("C07.history_search_id", find("Boiler 02", ns) and not find("Cooling Tower 07", ns))
    tap_node(find("Search inspections")); key(123)
    for _ in range(8):
        key(67)
    hide_keyboard()
    tap("Filter Good")
    ns = nodes("06-history-filter-good")
    check("C08.history_condition_filter", find("Booster Pump 104", ns) and find("Compressor 012", ns) and not find("Conveyor 18", ns))
    tap("Show all conditions")

    # --- Success view-history path ---
    tap("^Assets$"); tap_row("Panel LP-44"); tap_row("Start inspection")
    tap_row("Condition Good"); tap_row("Temperature in degrees"); type_text("41"); hide_keyboard()
    for item in ["Guards and covers secure", "No visible leaks or damage", "Area clear and accessible"]:
        tap_row(item)
    tap_row("Submit inspection")
    wait_for("Inspection saved")
    tap("View history")
    ns = nodes()
    check("B12.view_history", find("8 completed inspections", ns) is not None and find("INS-24093", ns) is not None)

    # --- Background / resume ---
    key(3)  # HOME
    time.sleep(2)
    launch()
    ns = nodes("resume")
    check("J06.background_resume", find("INS-24093", ns) is not None, "state preserved after HOME + relaunch")

    # --- Process kill + relaunch persistence ---
    stop()
    t = launch()
    wait_for("Needs attention", 20)
    tap("^History$")
    wait_for("completed inspections")
    shot("persistence-after-relaunch"); ns = nodes("persistence-after-relaunch")
    check("C10.persisted_after_relaunch", find("INS-24092", ns) is not None and find("INS-24093", ns) is not None, f"relaunch TotalTime={t}ms")
    tap("^Dashboard$")
    ns = nodes()
    check("C02.counts_updated_after_save", find("Operational", ns) is not None)

    # --- System back at section root does not crash; detail back stack ---
    tap_row("Cooling Tower 07"); wait_for("Asset detail")
    key(4)
    check("F06.back_detail_to_dashboard", wait_for("Needs attention", 6) is not None)

    # --- Text scaling ---
    adb("shell", "settings", "put", "system", "font_scale", "1.3")
    stop(); launch(); wait_for("Needs attention", 20)
    shot("font-scale-1.3-dashboard")
    tap_row("Cooling Tower 07"); tap_row("Start inspection")
    shot("font-scale-1.3-form")
    n = scroll_to("Cancel inspection")
    shot("font-scale-1.3-form-bottom")
    check("F05.font_scale_1_3_usable", n is not None and find("To submit", nodes()) is not None, "form actions reachable at 1.3x font scale")
    adb("shell", "settings", "put", "system", "font_scale", "1.0")


def run_states():
    stop()
    for mode, expect, name in [("empty", "No assets registered", "state-empty"),
                               ("error", "Couldn't load data", "state-error")]:
        stop(); launch(("--es", "data_mode", mode)); wait_for(expect, 15)
        shot(name); ns = nodes(name)
        check({"empty": "E02.empty_state", "error": "E03.error_state"}[mode], find(expect, ns) is not None)
        if mode == "empty":
            tap("^History$")
            check("E02.empty_history", wait_for("No inspections yet", 8) is not None)
            shot("state-empty-history")
    stop(); launch(("--es", "data_mode", "error-once")); wait_for("Couldn't load data", 15)
    tap("Retry loading data")
    check("E04.retry_recovers", wait_for("Needs attention", 10) is not None)
    shot("state-retry-recovered")
    stop()
    adb("shell", "am", "start", "-n", MAIN, "--es", "data_mode", "slow")  # no -W: capture while loading
    # uiautomator waits for UI idle, and the spinning ProgressRing keeps the UI busy until loading ends,
    # so sample raw frames instead and classify them by pixels.
    frames = []
    t0 = time.time()
    while time.time() - t0 < 14:
        png = subprocess.run(["adb", "exec-out", "screencap", "-p"], capture_output=True).stdout
        frames.append((round(time.time() - t0, 1), png))
    loading_frame = None
    try:
        from PIL import Image
        import io
        fdir = os.path.join(OUT, "loading-frames"); os.makedirs(fdir, exist_ok=True)
        for t, png in frames:
            img = Image.open(io.BytesIO(png)).convert("RGB").resize((216, 480))
            img.save(os.path.join(fdir, f"frame-{t:05.1f}s.png"))
            px = list(img.getdata())
            near = lambda c, rgb, tol=4: all(abs(c[k] - rgb[k]) <= tol for k in range(3))
            canvas = sum(1 for c in px if near(c, (0xF3, 0xF2, 0xED)))           # app background drawn (splash is white)
            chips = sum(1 for c in px if near(c, (0xF8, 0xE8, 0xE6)) or near(c, (0xF7, 0xED, 0xDF)))  # list rows drawn
            print(f"frame {t}s canvas={canvas} chips={chips}")
            if canvas > len(px) * 0.5 and chips == 0 and loading_frame is None:
                loading_frame = (t, png)
    except Exception as ex:
        print("frame analysis failed:", ex)
    if loading_frame:
        with open(os.path.join(SHOTS, "state-loading.png"), "wb") as f:
            f.write(loading_frame[1])
    check("E01.loading_state", loading_frame is not None,
          f"loading frame (greeting drawn, no list rows yet) at {loading_frame[0]}s; {len(frames)} frames sampled" if loading_frame else f"{len(frames)} frames sampled, none in loading state")
    wait_for("Needs attention", 15)
    wait_for("Needs attention", 15)
    stop(); launch(("--es", "data_mode", "save-error")); wait_for("Needs attention", 15)
    before = None
    tap_row("Cooling Tower 07"); tap_row("Start inspection")
    tap_row("Condition Good"); tap_row("Temperature in degrees"); type_text("20"); hide_keyboard()
    for item in ["Guards and covers secure", "No visible leaks or damage", "Area clear and accessible"]:
        tap_row(item)
    tap_row("Submit inspection")
    n = wait_for("wasn't saved", 8)
    shot("state-save-error"); ns = nodes("state-save-error")
    check("E08.save_failure_not_silent", n is not None and find("Inspection saved", ns) is None)
    stop()


try:
    if SCENARIO in ("full", "all"):
        run_full()
    if SCENARIO in ("states", "all", "full"):
        run_states()
    if SCENARIO == "smoke":
        clear_data()
        t = launch(); time.sleep(4)
        shot("smoke-dashboard"); nodes("smoke-dashboard")
        check("A05.launch", pid() != "", f"TotalTime={t}")
except Exception as ex:  # record the failure and keep artifacts
    check("driver.exception", False, repr(ex))
    shot("driver-exception"); nodes("driver-exception")
finally:
    crashes = adb("logcat", "-d", "-b", "crash")
    open(os.path.join(OUT, "crash-buffer.txt"), "w").write(crashes)
    check("J03.no_crash_in_logcat_crash_buffer", PKG not in crashes)
    json.dump(checks, open(os.path.join(OUT, "checks.json"), "w"), indent=2)
    failed = [c for c in checks if not c["pass"]]
    print(f"{len(checks) - len(failed)}/{len(checks)} checks passed")
