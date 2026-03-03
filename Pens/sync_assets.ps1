$assetsPath = "C:\Users\Platform006\OneDrive - Uno Platform\Desktop\unOS\UnoAgent\AI-builds\Pens\Pens\Assets"
Get-ChildItem -Recurse $assetsPath | ForEach-Object {
    try {
        if (-not $_.PSIsContainer) {
            $bytes = [System.IO.File]::ReadAllBytes($_.FullName)
            Write-Host "OK: $($_.FullName) ($($bytes.Length) bytes)"
        }
    } catch {
        Write-Host "FAIL: $($_.FullName) - $($_.Exception.Message)"
    }
}
Write-Host "Done syncing assets"
