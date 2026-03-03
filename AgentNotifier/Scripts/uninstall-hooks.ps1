# Uninstall Agent Notifier hooks from Claude Code

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Agent Notifier - Hooks Uninstaller" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Claude settings location
$ClaudeDir = Join-Path $env:USERPROFILE ".claude"
$SettingsFile = Join-Path $ClaudeDir "settings.json"

if (-not (Test-Path $SettingsFile)) {
    Write-Host "No Claude settings file found. Nothing to uninstall." -ForegroundColor Yellow
    exit 0
}

Write-Host "Loading Claude settings..." -ForegroundColor Yellow
$settingsContent = Get-Content $SettingsFile -Raw
$settings = $settingsContent | ConvertFrom-Json -AsHashtable

if ($settings.ContainsKey("hooks")) {
    Write-Host "Removing hooks configuration..." -ForegroundColor Yellow
    $settings.Remove("hooks")

    $settings | ConvertTo-Json -Depth 10 | Out-File -FilePath $SettingsFile -Encoding UTF8 -Force

    Write-Host ""
    Write-Host "Hooks have been removed." -ForegroundColor Green
    Write-Host "Restart Claude Code CLI to apply changes." -ForegroundColor White
} else {
    Write-Host "No hooks configuration found." -ForegroundColor Yellow
}

Write-Host ""
