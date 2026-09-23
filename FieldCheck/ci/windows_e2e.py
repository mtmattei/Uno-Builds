"""Smoke pass: launch, size the client area to 1440x900, screenshot, dump the UIA tree."""
import os, subprocess, sys, time
from pywinauto import Application, Desktop
from PIL import ImageGrab
import ctypes

EXE, OUT = sys.argv[1], sys.argv[2]
os.makedirs(OUT, exist_ok=True)
user32 = ctypes.windll.user32
with open(os.path.join(OUT, "screen.txt"), "w") as f:
    f.write(f"{user32.GetSystemMetrics(0)}x{user32.GetSystemMetrics(1)}\n")

app = Application(backend="uia").start(EXE)
time.sleep(10)
win = app.top_window()
win.wait("visible", timeout=60)
print("window:", win.window_text(), win.rectangle())
win.set_focus()
rect = win.rectangle(); client = win.client_rect()
print("client:", client)
win.move_window(0, 0, 1440 + (rect.width() - client.width()), 900 + (rect.height() - client.height()))
time.sleep(3)
win.capture_as_image().save(os.path.join(OUT, "smoke-01-dashboard.png"))
import io, contextlib
buf = io.StringIO()
with contextlib.redirect_stdout(buf):
    win.print_control_identifiers(depth=12)
open(os.path.join(OUT, "smoke-uia-tree.txt"), "w", encoding="utf-8").write(buf.getvalue())
app.kill()
