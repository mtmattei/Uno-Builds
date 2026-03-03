# Hook: Notification - Set waiting status when input needed

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module "$scriptDir\AgentHelper.psm1" -Force

$inputJson = [Console]::In.ReadToEnd() | ConvertFrom-Json
$sessionId = $inputJson.session_id
$cwd = $inputJson.cwd
$agentId = Get-AgentId -SessionId $sessionId
$projectName = Get-ProjectName -Cwd $cwd

# Log notification type for debugging
$notificationType = $inputJson.type
$title = $inputJson.title
$message = $inputJson.message

$agents = Read-AgentsFile
$existing = Get-ExistingAgent -Agents $agents -AgentId $agentId
$preserved = Get-PreservedValues -ExistingAgent $existing

# Preserve existing name if set
$name = if ($existing -and $existing.name -and $existing.name -ne "Claude Code") { $existing.name } else { $projectName }

# Check various notification types that indicate waiting for input
$isWaiting = $false
$statusMessage = "Notification received"

if ($title -match "permission|Permission|approve|Approve|allow|Allow") {
    $isWaiting = $true
    $statusMessage = "Permission required"
}
elseif ($title -match "input|Input|question|Question|waiting|Waiting") {
    $isWaiting = $true
    $statusMessage = "Input required"
}
elseif ($message -match "permission|approve|allow|waiting for|requires") {
    $isWaiting = $true
    $statusMessage = "Action required"
}

if ($isWaiting) {
    $agentObj = @{
        id = $agentId
        name = $name
        model = "claude-opus-4"
        status = "waiting"
        label = "WAITING"
        message = $statusMessage
        current_task = if ($title) { $title } else { "Awaiting response..." }
        task_tag = ""
        tokens_used = $preserved.tokens_used
        token_limit = 200000
        cost = $preserved.cost
        rate = 0.0
        queue_position = 1
        progress = $null
        is_waiting_for_input = $true
        session = @{
            id = Get-SessionShortId -SessionId $sessionId
            task = "WAIT"
            start_ms = $preserved.start_ms
            elapsed_ms = Calculate-ElapsedMs -StartMs $preserved.start_ms
        }
    }

    $updatedAgents = Update-AgentInList -Agents $agents -NewAgent $agentObj
    Write-AgentsFile -Agents $updatedAgents
}

Write-Output '{"continue": true}'
exit 0
