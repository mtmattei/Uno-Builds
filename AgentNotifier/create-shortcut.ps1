# Create desktop shortcut for AgentNotifier

$publishDir = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "publish"
$exePath = Join-Path $publishDir "AgentNotifier.exe"
$iconPath = Join-Path $publishDir "Assets\icon.ico"
$shortcutPath = Join-Path ([Environment]::GetFolderPath("Desktop")) "Agent Notifier.lnk"

if (-not (Test-Path $exePath)) {
    Write-Host "Error: AgentNotifier.exe not found. Run publish.ps1 first." -ForegroundColor Red
    exit 1
}

$WshShell = New-Object -ComObject WScript.Shell
$shortcut = $WshShell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $exePath
$shortcut.WorkingDirectory = $publishDir
$shortcut.Description = "Claude Agent Swarm Dashboard"

# Use icon if exists
if (Test-Path $iconPath) {
    $shortcut.IconLocation = $iconPath
}

$shortcut.Save()

Write-Host "Desktop shortcut created: $shortcutPath" -ForegroundColor Green
