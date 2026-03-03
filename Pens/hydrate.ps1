$iconDir = "C:\Users\Platform006\OneDrive - Uno Platform\Desktop\unOS\UnoAgent\AI-builds\Pens\Pens\Assets\Icons"
$files = @("icon.svg", "icon_foreground.svg")
foreach ($f in $files) {
    $path = Join-Path $iconDir $f
    Write-Host "Trying to hydrate: $path"
    try {
        attrib -U +P $path 2>$null
        Start-Sleep -Milliseconds 500
        $content = Get-Content -Path $path -Raw -ErrorAction Stop
        Write-Host "SUCCESS: $f ($($content.Length) chars)"
    } catch {
        Write-Host "FAILED: $f - $_"
    }
}
