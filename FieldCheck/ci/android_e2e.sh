#!/usr/bin/env bash
# Android verification driver (runs inside reactivecircus/android-emulator-runner).
# Usage: android_e2e.sh <apk> <outdir> <scenario>
set -u
APK="$1"; OUT="$2"; SCENARIO="${3:-smoke}"
PKG=com.fieldcheck.app
mkdir -p "$OUT"
export OUT PKG
adb wait-for-device
adb shell getprop ro.build.version.release > "$OUT/device-android-version.txt"
adb shell wm size > "$OUT/device-screen.txt"; adb shell wm density >> "$OUT/device-screen.txt"
adb install -r "$APK" > "$OUT/install.txt" 2>&1
adb push "$(dirname "$0")/../mock-data/inspection-photo.png" /sdcard/Download/inspection-photo.png
adb shell am broadcast -a android.intent.action.MEDIA_SCANNER_SCAN_FILE -d file:///sdcard/Download/inspection-photo.png > /dev/null
adb logcat -c
python3 "$(dirname "$0")/android_e2e.py" "$SCENARIO" 2>&1 | tee "$OUT/e2e-log.txt"
RC=${PIPESTATUS[0]}
adb logcat -d > "$OUT/logcat.txt"
exit $RC
