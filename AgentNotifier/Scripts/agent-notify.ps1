# Agent Notifier CLI - Updates the Agent Status Widget
# Usage: agent-notify.ps1 -Status <status> [-Message <message>] [-Task <task>] [-Session <id>] [-Progress <0-100>] [-AgentId <id>] [-AgentName <name>] [-Model <model>] [-TokensUsed <count>]

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("idle", "working", "waiting", "finished", "error")]
    [string]$Status,

    [string]$Message,
    [string]$Task = "TASK",
    [string]$TaskTag = "",
    [string]$Session,
    [int]$Progress = -1,
    [string]$AgentId = "agent-1",
    [string]$AgentName = "Claude Code",
    [string]$Model = "claude-opus-4",
    [int]$TokensUsed = 0,
    [int]$TokenLimit = 200000,
    [switch]$WaitingForInput
)

# Status file location
$statusDir = Join-Path $env:APPDATA "AgentNotifier"
$statusFile = Join-Path $statusDir "agents.json"

# Ensure directory exists
if (-not (Test-Path $statusDir)) {
    New-Item -ItemType Directory -Path $statusDir -Force | Out-Null
}

# Default labels and messages
$labels = @{
    "idle" = "IDLE"
    "working" = "PROCESSING"
    "waiting" = "WAITING"
    "finished" = "COMPLETE"
    "error" = "ERROR"
}

$defaultMessages = @{
    "idle" = "Awaiting agent connection..."
    "working" = "Task in progress..."
    "waiting" = "Input required"
    "finished" = "Task completed successfully"
    "error" = "Task failed"
}

# Generate session ID if not provided
if (-not $Session) {
    $Session = [System.Guid]::NewGuid().ToString().Substring(0, 4).ToUpper()
}

# Read existing agents file or create new
$existingAgents = @()
if (Test-Path $statusFile) {
    try {
        $existing = Get-Content $statusFile -Raw | ConvertFrom-Json
        if ($existing.agents) {
            $existingAgents = @($existing.agents)
        }
    } catch {
        $existingAgents = @()
    }
}

# Build the agent object
$agentObj = @{
    id = $AgentId
    name = $AgentName
    model = $Model
    status = $Status
    label = $labels[$Status]
    message = if ($Message) { $Message } else { $defaultMessages[$Status] }
    task_tag = $TaskTag
    tokens_used = $TokensUsed
    token_limit = $TokenLimit
    is_waiting_for_input = $WaitingForInput.IsPresent -or $Status -eq "waiting"
    session = @{
        id = $Session
        task = $Task.ToUpper()
        elapsed_ms = 0
    }
}

# Add progress only for working state
if ($Progress -ge 0 -and $Status -eq "working") {
    $agentObj["progress"] = $Progress
}

# Update or add agent in list
$found = $false
$updatedAgents = @()
foreach ($agent in $existingAgents) {
    if ($agent.id -eq $AgentId) {
        $updatedAgents += $agentObj
        $found = $true
    } else {
        $updatedAgents += $agent
    }
}
if (-not $found) {
    $updatedAgents += $agentObj
}

# Build final payload
$payload = @{
    agents = $updatedAgents
    timestamp = (Get-Date).ToUniversalTime().ToString("o")
}

# Write to file
$json = $payload | ConvertTo-Json -Depth 4
$json | Out-File -FilePath $statusFile -Encoding UTF8 -Force

Write-Host "Status updated: $AgentName ($AgentId) -> $Status"
