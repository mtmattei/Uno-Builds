"""Smoke pass: launch, capture screenshots + uiautomator dumps to learn the accessibility tree."""
import os, subprocess, sys, time

OUT = os.environ["OUT"]; PKG = os.environ["PKG"]
SCENARIO = sys.argv[1] if len(sys.argv) > 1 else "smoke"

def adb(*args, check=False):
    r = subprocess.run(["adb", *args], capture_output=True, text=True)
    return r.stdout

def shot(name):
    with open(os.path.join(OUT, name + ".png"), "wb") as f:
        f.write(subprocess.run(["adb", "exec-out", "screencap", "-p"], capture_output=True).stdout)

def dump(name):
    adb("shell", "uiautomator", "dump", "/sdcard/ui.xml")
    xml = adb("shell", "cat", "/sdcard/ui.xml")
    with open(os.path.join(OUT, name + ".xml"), "w") as f:
        f.write(xml)
    return xml

main = adb("shell", "cmd", "package", "resolve-activity", "--brief", PKG).strip().splitlines()[-1]
print("main activity:", main)
t0 = time.time()
print(adb("shell", "am", "start", "-W", "-n", main))
time.sleep(8)
shot("smoke-01-dashboard"); dump("smoke-01-dashboard")
print("pid:", adb("shell", "pidof", PKG))
