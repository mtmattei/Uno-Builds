# Hook: PermissionRequest - Set waiting status when permission dialog appears

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module "$scriptDir\AgentHelper.psm1" -Force

$inputJson = [Console]::In.ReadToEnd() | ConvertFrom-Json
$sessionId = $inputJson.session_id
$cwd = $inputJson.cwd
$agentId = Get-AgentId -SessionId $sessionId
$projectName = Get-ProjectName -Cwd $cwd
$toolName = $inputJson.tool_name

$agents = Read-AgentsFile
$existing = Get-ExistingAgent -Agents $agents -AgentId $agentId
$preserved = Get-PreservedValues -ExistingAgent $existing

# Preserve existing name if set
$name = if ($existing -and $existing.name -and $existing.name -ne "Claude Code") { $existing.name } else { $projectName }

$agentObj = @{
    id = $agentId
    name = $name
    model = "claude-opus-4"
    status = "waiting"
    label = "WAITING"
    message = "Permission required for $toolName"
    current_task = "Awaiting approval..."
    task_tag = $toolName
    tokens_used = $preserved.tokens_used
    token_limit = 200000
    cost = $preserved.cost
    rate = 0.0
    queue_position = 1
    progress = $null
    is_waiting_for_input = $true
    session = @{
        id = Get-SessionShortId -SessionId $sessionId
        task = "PERM"
        start_ms = $preserved.start_ms
        elapsed_ms = Calculate-ElapsedMs -StartMs $preserved.start_ms
    }
}

$updatedAgents = Update-AgentInList -Agents $agents -NewAgent $agentObj
Write-AgentsFile -Agents $updatedAgents

Write-Output '{"continue": true}'
exit 0
