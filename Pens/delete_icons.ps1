$iconDir = "C:\Users\Platform006\OneDrive - Uno Platform\Desktop\unOS\UnoAgent\AI-builds\Pens\Pens\Assets\Icons"
$files = @("icon.svg", "icon_foreground.svg")
foreach ($f in $files) {
    $path = Join-Path $iconDir $f
    if (Test-Path $path) {
        Remove-Item -Force $path -ErrorAction SilentlyContinue
        Write-Host "Removed: $path"
    }
}
Write-Host "Done"
