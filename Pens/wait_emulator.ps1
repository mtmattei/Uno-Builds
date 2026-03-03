$adb = "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe"
Write-Host "Waiting for emulator to boot..."
for ($i = 0; $i -lt 60; $i++) {
    Start-Sleep -Seconds 5
    $output = & $adb shell getprop sys.boot_completed 2>&1
    $trimmed = "$output".Trim()
    if ($trimmed -eq "1") {
        Write-Host "Emulator booted!"
        exit 0
    }
    Write-Host "Waiting... ($i)"
}
Write-Host "Timeout"
exit 1
