"""FieldCheck Windows end-to-end verification (GitHub-hosted windows-latest runner).

Launches the unpackaged Release build, sizes the client area to the 1440x900 reference
viewport, drives the workflows through UI Automation (pywinauto 'uia' backend) and writes
screenshots plus checks.json. Screenshots use PrintWindow(PW_RENDERFULLCONTENT) so they are
independent of desktop resolution/occlusion.
"""
import ctypes, json, os, re, subprocess, sys, time
from ctypes import wintypes

from PIL import Image
from pywinauto import Application, Desktop, keyboard

EXE, OUT = sys.argv[1], sys.argv[2]
SCENARIO = sys.argv[3] if len(sys.argv) > 3 else "full"
SHOTS = os.path.join(OUT, "screens"); os.makedirs(SHOTS, exist_ok=True)
DATA_DIR = os.path.join(os.environ["LOCALAPPDATA"], "FieldCheck")
PHOTO = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "mock-data", "inspection-photo.png"))
checks = []
user32 = ctypes.windll.user32
gdi32 = ctypes.windll.gdi32
user32.SetProcessDPIAware()


def check(cid, ok, detail=""):
    checks.append({"id": cid, "pass": bool(ok), "detail": detail})
    print(("PASS " if ok else "FAIL ") + cid + " " + str(detail), flush=True)


class App:
    def __init__(self, mode=None, clean=False):
        if clean and os.path.isdir(DATA_DIR):
            import shutil; shutil.rmtree(DATA_DIR)
        env = dict(os.environ)
        if mode:
            env["FIELDCHECK_DATA_MODE"] = mode
        else:
            env.pop("FIELDCHECK_DATA_MODE", None)
        t0 = time.time()
        self.proc = subprocess.Popen([EXE], env=env)
        self.app = Application(backend="uia")
        for _ in range(60):
            try:
                self.app.connect(process=self.proc.pid, timeout=1)
                self.win = self.app.top_window()
                self.win.wait("visible", timeout=2)
                break
            except Exception:
                time.sleep(0.5)
        self.startup_s = round(time.time() - t0, 2)
        self.resize(1440, 900)

    def resize(self, cw, ch):
        """Sizes the window so the client area is exactly cw x ch (retries while the frame settles)."""
        hwnd = self.win.handle
        user32.ShowWindow(hwnd, 9)  # SW_RESTORE
        for _ in range(6):
            rect, client = wintypes.RECT(), wintypes.RECT()
            user32.GetWindowRect(hwnd, ctypes.byref(rect)); user32.GetClientRect(hwnd, ctypes.byref(client))
            if (client.right, client.bottom) == (cw, ch):
                break
            dw = (rect.right - rect.left) - client.right; dh = (rect.bottom - rect.top) - client.bottom
            user32.SetWindowPos(hwnd, 0, 0, 0, cw + dw, ch + dh, 0x0004 | 0x0040)
            time.sleep(1.2)
        try:
            self.win.set_focus()
        except Exception:
            pass

    def client_size(self):
        c = wintypes.RECT(); user32.GetClientRect(self.win.handle, ctypes.byref(c)); return c.right, c.bottom

    def shot(self, name):
        hwnd = self.win.handle
        rect, client = wintypes.RECT(), wintypes.RECT()
        user32.GetWindowRect(hwnd, ctypes.byref(rect)); user32.GetClientRect(hwnd, ctypes.byref(client))
        w, h = rect.right - rect.left, rect.bottom - rect.top
        hdc = user32.GetWindowDC(hwnd); mdc = gdi32.CreateCompatibleDC(hdc)
        bmp = gdi32.CreateCompatibleBitmap(hdc, w, h); gdi32.SelectObject(mdc, bmp)
        user32.PrintWindow(hwnd, mdc, 2)
        bmi = ctypes.create_string_buffer(40)
        ctypes.memmove(bmi, ctypes.byref(ctypes.c_uint32(40)), 4)
        ctypes.cast(ctypes.byref(bmi, 4), ctypes.POINTER(ctypes.c_int32))[0] = w
        ctypes.cast(ctypes.byref(bmi, 8), ctypes.POINTER(ctypes.c_int32))[0] = -h
        ctypes.cast(ctypes.byref(bmi, 12), ctypes.POINTER(ctypes.c_uint16))[0] = 1
        ctypes.cast(ctypes.byref(bmi, 14), ctypes.POINTER(ctypes.c_uint16))[0] = 32
        buf = ctypes.create_string_buffer(w * h * 4)
        gdi32.GetDIBits(mdc, bmp, 0, h, buf, bmi, 0)
        img = Image.frombuffer("RGB", (w, h), buf, "raw", "BGRX", 0, 1)
        pt = wintypes.POINT(0, 0); user32.ClientToScreen(hwnd, ctypes.byref(pt))
        ox, oy = pt.x - rect.left, pt.y - rect.top
        img.crop((ox, oy, ox + client.right, oy + client.bottom)).save(os.path.join(SHOTS, name + ".png"))
        gdi32.DeleteObject(bmp); gdi32.DeleteDC(mdc); user32.ReleaseDC(hwnd, hdc)

    def find(self, name, ctype=None, timeout=8, regex=False):
        """Finds a descendant by UIA Name (exact, or regex search anywhere in the name)."""
        rx = re.compile(name, re.I | re.S) if regex else None
        end = time.time() + timeout
        while True:
            try:
                elements = self.win.descendants(control_type=ctype) if ctype else self.win.descendants()
            except Exception:
                elements = []
            for e in elements:
                n = e.element_info.name or ""
                if (rx.search(n) if rx else n == name):
                    return e
            if time.time() > end:
                raise TimeoutError(f"not found: {name} ({ctype})")
            time.sleep(0.4)

    def exists(self, name, ctype=None, timeout=2, regex=True):
        try:
            self.find(name, ctype, timeout, regex)
            return True
        except Exception:
            return False

    def invoke(self, name, ctype="Button", regex=False):
        el = self.find(name, ctype, regex=regex)
        try:
            el.invoke()
        except Exception:
            el.click_input()
        time.sleep(1.2)
        return el

    def click(self, name, ctype="Button"):
        """Real mouse click at the element's centre. Used where Invoke would block (a modal dialog opened on the
        UI thread, e.g. the Skia Win32 file dialog makes UIA Invoke time out)."""
        from pywinauto import mouse
        el = self.find(name, ctype)
        r = el.rectangle()
        mouse.click(coords=((r.left + r.right) // 2, (r.top + r.bottom) // 2))
        time.sleep(1.2)

    def texts(self):
        return [e.window_text() for e in self.win.descendants() if e.window_text()]

    def set_text(self, name, value):
        el = self.find(name, "Edit")
        el.set_focus()
        keyboard.send_keys("^a{BACKSPACE}", pause=0.02)
        if value:
            keyboard.send_keys(value.replace(" ", "{SPACE}").replace("(", "{(}").replace(")", "{)}").replace(";", "{;}"), pause=0.01, with_spaces=True)
        time.sleep(0.6)

    def kill(self):
        self.proc.kill(); self.proc.wait(10); time.sleep(1)


def select(app, name):
    el = app.find(name, "RadioButton")
    try:
        el.select()
    except Exception:
        el.click_input()
    time.sleep(0.8)


def toggle(app, name, ctype):
    el = app.find(name, ctype)
    try:
        el.toggle()
    except Exception:
        el.click_input()
    time.sleep(0.8)


def has(app, pattern):
    rx = re.compile(pattern, re.I)
    return any(rx.search(t) for t in app.texts())


def fill_valid(app, condition="Good", temp="27", issue=None):
    select(app, f"Condition {condition}")
    app.set_text("Temperature in degrees Celsius, required, -50 to 250", temp)
    for item in ["Guards and covers secure", "No visible leaks or damage", "Area clear and accessible"]:
        toggle(app, item, "CheckBox")
    if issue:
        app.set_text("Issue description, required", issue)


def picker_window(timeout=10):
    end = time.time() + timeout
    while time.time() < end:
        for w in Desktop(backend="uia").windows():
            try:
                if w.window_text() in ("Open", "Ouvrir") or w.class_name() == "#32770":
                    return w
                for c in w.children():
                    if c.class_name() == "#32770":
                        return c
            except Exception:
                pass
        time.sleep(0.5)
    return None


def run_full():
    app = App(clean=True)
    check("J04.clean_first_run", app.exists("Needs attention|NEEDS ATTENTION", timeout=20), f"startup={app.startup_s}s")
    check("A06.dashboard_usable", has(app, "^12$") and has(app, "View all assets"), f"client={app.client_size()}")
    app.shot("01-dashboard")
    check("C02.counts", has(app, "^7$") and has(app, "^3$") and has(app, "^2$"))

    # Sidebar navigation
    app.invoke("History"); check("B09.sidebar_history", app.exists("completed inspections"))
    app.invoke("Dashboard"); check("B09.sidebar_dashboard", app.exists("Needs attention|NEEDS ATTENTION"))
    app.invoke("Assets"); check("B09.sidebar_assets", app.exists("equipment records"))
    app.shot("02-assets-no-selection")

    # Master/detail
    item = app.find("^Cooling Tower 07, CT-007", "Button", regex=True)
    item.click_input(); time.sleep(1.5)
    check("B10.master_detail", app.exists("Start inspection") and app.exists("Equipment records|equipment records") and has(app, "Open-circuit cooling tower"))
    app.shot("02-assets-master-detail")
    check("F03.reference_viewport", app.client_size() == (1440, 900), str(app.client_size()))

    # Search / filters
    app.set_text("Search assets by name, ID, type or location", "compressor")
    check("C03.search_type", has(app, "Compressor 012") and not has(app, "Booster Pump 104"))
    app.set_text("Search assets by name, ID, type or location", "zzz")
    check("E05.no_results", app.exists("No matching assets")); app.shot("02-assets-no-results")
    app.invoke("Clear search")
    select(app, "Filter Critical")
    check("C04.filter_critical", has(app, "Emergency Fan 90") and not has(app, "Booster Pump 104"))
    select(app, "Show all statuses")

    # Keyboard focus order (Tab) on the Assets page
    app.find("Search assets by name, ID, type or location", "Edit").set_focus()
    from pywinauto.uia_defines import IUIA
    focus_names = []
    for _ in range(8):
        keyboard.send_keys("{TAB}"); time.sleep(0.4)
        focus_names.append(IUIA().iuia.GetFocusedElement().CurrentName)
    app.shot("focus-visible")
    check("F07.keyboard_tab_reaches_controls", "Filter Critical" in focus_names or "Show all statuses" in focus_names, str(focus_names))

    # Start inspection -> cancel does not save
    app.find("^Cooling Tower 07, CT-007", "Button", regex=True).click_input(); time.sleep(1)
    app.invoke("Start inspection")
    check("B04.new_inspection", app.exists("New inspection"))
    app.shot("03-new-inspection-empty")
    submit = app.find("Submit inspection", "Button")
    check("D14.submit_disabled", not submit.is_enabled())
    app.invoke("Cancel inspection")
    check("B11.cancel_returns", app.exists("equipment records") and not has(app, "INS-24092"))

    # Fill the form
    app.find("^Cooling Tower 07, CT-007", "Button", regex=True).click_input(); time.sleep(1)
    app.invoke("Start inspection")
    select(app, "Condition Attention")
    check("D11.issue_shown", app.exists("Issue description, required", "Edit"))
    app.set_text("Temperature in degrees Celsius, required, -50 to 250", "251")
    check("D04.temp_error", app.exists("between -50 and 250"))
    app.shot("03-new-inspection-validation")
    app.set_text("Temperature in degrees Celsius, required, -50 to 250", "27")
    for item in ["Guards and covers secure", "No visible leaks or damage", "Area clear and accessible"]:
        toggle(app, item, "CheckBox")
    app.set_text("Issue description, required", "Basin-level alarm intermittent; inspect fan vibration.")

    # File picker: cancel, then select
    app.click("Attach photo or file")
    dlg = picker_window()
    check("D07.picker_opens", dlg is not None)
    if dlg:
        keyboard.send_keys("{ESC}"); time.sleep(1.5)
    check("D09.picker_cancel", app.exists("Choose file") and app.find("Submit inspection", "Button").is_enabled())
    app.click("Attach photo or file")
    dlg = picker_window()
    if dlg:
        time.sleep(1)
        keyboard.send_keys("%n"); time.sleep(0.3)
        keyboard.send_keys(PHOTO.replace(" ", "{SPACE}"), with_spaces=True); time.sleep(0.3)
        keyboard.send_keys("{ENTER}"); time.sleep(2)
    check("D08.picker_filename", app.exists("inspection-photo.png"))
    app.invoke("Remove attachment")
    check("J02.remove_attachment", app.exists("Choose file") and not app.exists("inspection-photo.png", timeout=1))
    app.click("Attach photo or file")
    dlg = picker_window()
    if dlg:
        time.sleep(1)
        keyboard.send_keys("%n"); time.sleep(0.3)
        keyboard.send_keys(PHOTO.replace(" ", "{SPACE}"), with_spaces=True); time.sleep(0.3)
        keyboard.send_keys("{ENTER}"); time.sleep(2)
    check("D08.picker_reselect_after_remove", app.exists("inspection-photo.png"))
    app.shot("03-new-inspection")

    # Submit (double activation)
    submit = app.find("Submit inspection", "Button")
    try:
        submit.invoke(); submit.invoke()
    except Exception:
        pass
    check("B05.success", app.exists("Inspection saved", timeout=10) and app.exists("INS-24092"))
    app.shot("04-inspection-success")
    app.invoke("View asset")
    check("D18.asset_reflects_newest", app.exists("Attention condition") and app.exists("INS-24092") and app.exists("Sep 23, 2026"))
    app.shot("after-save-master-detail")
    app.invoke("History")
    check("C09.history_new_first", app.exists("INS-24092"))
    app.shot("05-history")
    app.set_text("Search inspections by asset name or ID", "boiler")
    check("C07.history_search", has(app, "Boiler 02") and not has(app, "Cooling Tower 07"))
    app.set_text("Search inspections by asset name or ID", "")
    select(app, "Filter Attention")
    check("C08.history_filter", has(app, "Air Handler 203") and not has(app, "Booster Pump 104"))
    select(app, "Show all conditions")

    # Responsive contraction
    for w in (1100, 900, 760):
        app.resize(w, 800); time.sleep(1)
        app.shot(f"responsive-history-{w}")
    app.invoke("Assets"); app.shot("responsive-assets-760")
    app.find("^Cooling Tower 07, CT-007", "Button", regex=True).click_input(); time.sleep(1.5)
    check("F04.narrow_single_pane", app.exists("Asset detail"))
    app.shot("responsive-detail-760")
    app.invoke("Start inspection"); app.shot("responsive-form-760")
    app.invoke("Cancel inspection")
    app.resize(1440, 900)

    # Kill + relaunch persistence
    app.kill()
    app = App()
    app.invoke("History")
    check("C10.persisted_after_relaunch", app.exists("INS-24092"), f"startup={app.startup_s}s")
    app.shot("persistence-after-relaunch")
    # Minimize / restore
    user32.ShowWindow(app.win.handle, 6); time.sleep(1.5); user32.ShowWindow(app.win.handle, 9); time.sleep(1.5)
    check("J06.minimize_restore", app.exists("INS-24092"))
    app.kill()


def run_text_scale():
    """Windows 'Make text bigger' (Accessibility TextScaleFactor) at 130%."""
    import winreg
    key = winreg.CreateKey(winreg.HKEY_CURRENT_USER, r"Software\Microsoft\Accessibility")
    winreg.SetValueEx(key, "TextScaleFactor", 0, winreg.REG_DWORD, 130)
    try:
        app = App()
        app.shot("text-scale-130-dashboard")
        app.invoke("Assets")
        app.find("^Cooling Tower 07, CT-007", "Button", regex=True).click_input(); time.sleep(1.5)
        app.invoke("Start inspection")
        app.shot("text-scale-130-form")
        check("F05.windows_text_scale_130", app.exists("Submit inspection") and app.exists("Cancel inspection"))
        app.kill()
    finally:
        winreg.SetValueEx(key, "TextScaleFactor", 0, winreg.REG_DWORD, 100)


def run_states():
    for mode, text, cid in [("empty", "No assets registered", "E02.empty"), ("error", "Couldn't load data", "E03.error")]:
        app = App(mode=mode)
        check(cid, app.exists(text, timeout=10)); app.shot(f"state-{mode}")
        app.kill()
    app = App(mode="error-once")
    app.exists("Couldn't load data", timeout=10)
    app.invoke("Retry loading data")
    check("E04.retry", app.exists("Needs attention|NEEDS ATTENTION", timeout=10))
    app.kill()
    app = App(mode="slow")
    check("E01.loading", app.exists("Loading assets", timeout=3)); app.shot("state-loading")
    app.kill()
    app = App(mode="save-error")
    app.invoke("Assets"); app.find("Panel LP-44, PNL-044, Operational", "Button").click_input(); time.sleep(1)
    app.invoke("Start inspection"); fill_valid(app)
    app.invoke("Submit inspection")
    check("E08.save_error", app.exists("wasn't saved", timeout=6) and not app.exists("Inspection saved", timeout=1))
    app.shot("state-save-error")
    app.kill()


def set_resolution(w, h):
    class DEVMODE(ctypes.Structure):
        _fields_ = [("dmDeviceName", ctypes.c_wchar * 32), ("dmSpecVersion", ctypes.c_ushort), ("dmDriverVersion", ctypes.c_ushort),
                    ("dmSize", ctypes.c_ushort), ("dmDriverExtra", ctypes.c_ushort), ("dmFields", ctypes.c_ulong),
                    ("dmPositionX", ctypes.c_long), ("dmPositionY", ctypes.c_long), ("dmDisplayOrientation", ctypes.c_ulong),
                    ("dmDisplayFixedOutput", ctypes.c_ulong), ("dmColor", ctypes.c_short), ("dmDuplex", ctypes.c_short),
                    ("dmYResolution", ctypes.c_short), ("dmTTOption", ctypes.c_short), ("dmCollate", ctypes.c_short),
                    ("dmFormName", ctypes.c_wchar * 32), ("dmLogPixels", ctypes.c_ushort), ("dmBitsPerPel", ctypes.c_ulong),
                    ("dmPelsWidth", ctypes.c_ulong), ("dmPelsHeight", ctypes.c_ulong), ("dmDisplayFlags", ctypes.c_ulong),
                    ("dmDisplayFrequency", ctypes.c_ulong), ("dmICMMethod", ctypes.c_ulong), ("dmICMIntent", ctypes.c_ulong),
                    ("dmMediaType", ctypes.c_ulong), ("dmDitherType", ctypes.c_ulong), ("dmReserved1", ctypes.c_ulong),
                    ("dmReserved2", ctypes.c_ulong), ("dmPanningWidth", ctypes.c_ulong), ("dmPanningHeight", ctypes.c_ulong)]
    dm = DEVMODE(); dm.dmSize = ctypes.sizeof(DEVMODE)
    user32.EnumDisplaySettingsW(None, -1, ctypes.byref(dm))
    dm.dmPelsWidth, dm.dmPelsHeight = w, h
    dm.dmFields = 0x80000 | 0x100000
    return user32.ChangeDisplaySettingsW(ctypes.byref(dm), 0)


def dump_tree(name):
    try:
        wins = [w for w in Desktop(backend="uia").windows() if "FieldCheck" in w.window_text()]
        lines = []
        for w in wins:
            for e in w.descendants():
                ei = e.element_info
                lines.append(f"{ei.control_type}\t{ei.name!r}\t{ei.automation_id!r}\t{e.rectangle()}")
        open(os.path.join(OUT, name + ".txt"), "w", encoding="utf-8").write("\n".join(lines))
    except Exception as ex:
        open(os.path.join(OUT, name + ".txt"), "w").write(repr(ex))


try:
    rc = set_resolution(1920, 1080)
    time.sleep(2)
    with open(os.path.join(OUT, "screen.txt"), "w") as f:
        f.write(f"ChangeDisplaySettings rc={rc}\n")
        f.write(f"{user32.GetSystemMetrics(0)}x{user32.GetSystemMetrics(1)}\n")
    if SCENARIO in ("full", "all"):
        run_full()
    if SCENARIO in ("full", "states", "all"):
        run_states()
    if SCENARIO in ("full", "all"):
        run_text_scale()
    if SCENARIO == "smoke":
        app = App(clean=True)
        check("A06.launch", app.exists("Needs attention|NEEDS ATTENTION", timeout=20), f"startup={app.startup_s}s")
        app.shot("smoke-dashboard")
        import io, contextlib
        buf = io.StringIO()
        with contextlib.redirect_stdout(buf):
            app.win.print_control_identifiers(depth=8)
        open(os.path.join(OUT, "uia-tree.txt"), "w", encoding="utf-8").write(buf.getvalue())
        app.kill()
except Exception as ex:
    import traceback
    check("driver.exception", False, traceback.format_exc())
    dump_tree("uia-tree-at-failure")
finally:
    json.dump(checks, open(os.path.join(OUT, "checks.json"), "w"), indent=2)
    print(f"{sum(c['pass'] for c in checks)}/{len(checks)} checks passed")
