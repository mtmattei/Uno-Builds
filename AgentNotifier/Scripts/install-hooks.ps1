# Install Agent Notifier hooks for Claude Code
# Run this script to configure Claude Code to send status updates to the widget

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Agent Notifier - Hooks Installer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get the script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$HooksDir = Join-Path $ScriptDir "hooks"

# Claude settings location
$ClaudeDir = Join-Path $env:USERPROFILE ".claude"
$SettingsFile = Join-Path $ClaudeDir "settings.json"

Write-Host "Hooks directory: $HooksDir" -ForegroundColor Gray
Write-Host "Claude settings: $SettingsFile" -ForegroundColor Gray
Write-Host ""

# Ensure Claude directory exists
if (-not (Test-Path $ClaudeDir)) {
    Write-Host "Creating Claude config directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $ClaudeDir -Force | Out-Null
}

# Load existing settings or create new
$settings = @{}
if (Test-Path $SettingsFile) {
    Write-Host "Loading existing Claude settings..." -ForegroundColor Yellow
    $settingsContent = Get-Content $SettingsFile -Raw
    if ($settingsContent) {
        $settings = $settingsContent | ConvertFrom-Json -AsHashtable
    }
}

# Convert hooks directory path for use in commands
$HooksDirEscaped = $HooksDir -replace '\\', '\\\\'

# Create hooks configuration
$hooksConfig = @{
    "SessionStart" = @(
        @{
            "matcher" = ".*"
            "hooks" = @(
                @{
                    "type" = "command"
                    "command" = "powershell -ExecutionPolicy Bypass -File `"$HooksDir\on-session-start.ps1`""
                    "timeout" = 5
                    "async" = $true
                }
            )
        }
    )
    "UserPromptSubmit" = @(
        @{
            "matcher" = ".*"
            "hooks" = @(
                @{
                    "type" = "command"
                    "command" = "powershell -ExecutionPolicy Bypass -File `"$HooksDir\on-prompt-submit.ps1`""
                    "timeout" = 5
                    "async" = $true
                }
            )
        }
    )
    "PreToolUse" = @(
        @{
            "matcher" = ".*"
            "hooks" = @(
                @{
                    "type" = "command"
                    "command" = "powershell -ExecutionPolicy Bypass -File `"$HooksDir\on-tool-start.ps1`""
                    "timeout" = 5
                    "async" = $true
                }
            )
        }
    )
    "Notification" = @(
        @{
            "matcher" = ".*"
            "hooks" = @(
                @{
                    "type" = "command"
                    "command" = "powershell -ExecutionPolicy Bypass -File `"$HooksDir\on-notification.ps1`""
                    "timeout" = 5
                    "async" = $true
                }
            )
        }
    )
    "Stop" = @(
        @{
            "matcher" = ".*"
            "hooks" = @(
                @{
                    "type" = "command"
                    "command" = "powershell -ExecutionPolicy Bypass -File `"$HooksDir\on-stop.ps1`""
                    "timeout" = 5
                    "async" = $true
                }
            )
        }
    )
}

# Merge hooks into settings
$settings["hooks"] = $hooksConfig

# Save settings
Write-Host "Writing Claude settings..." -ForegroundColor Yellow
$settings | ConvertTo-Json -Depth 10 | Out-File -FilePath $SettingsFile -Encoding UTF8 -Force

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Installation Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "The Agent Notifier hooks have been installed." -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Restart Claude Code CLI to load the new hooks" -ForegroundColor White
Write-Host "  2. Make sure the Agent Notifier widget is running" -ForegroundColor White
Write-Host "  3. Start using Claude Code - the widget will update!" -ForegroundColor White
Write-Host ""
Write-Host "Status file location:" -ForegroundColor Gray
Write-Host "  $env:APPDATA\AgentNotifier\status.json" -ForegroundColor Gray
Write-Host ""
