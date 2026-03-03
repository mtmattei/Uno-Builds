# Hook: UserPromptSubmit - Set status to working, preserve metrics

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module "$scriptDir\AgentHelper.psm1" -Force

$inputJson = [Console]::In.ReadToEnd() | ConvertFrom-Json
$sessionId = $inputJson.session_id
$cwd = $inputJson.cwd
$agentId = Get-AgentId -SessionId $sessionId
$projectName = Get-ProjectName -Cwd $cwd

$agents = Read-AgentsFile
$existing = Get-ExistingAgent -Agents $agents -AgentId $agentId
$preserved = Get-PreservedValues -ExistingAgent $existing

# Preserve existing name if set
$name = if ($existing -and $existing.name -and $existing.name -ne "Claude Code") { $existing.name } else { $projectName }

$agentObj = @{
    id = $agentId
    name = $name
    model = "claude-opus-4"
    status = "working"
    label = "THINKING"
    message = "Processing request..."
    current_task = "Analyzing prompt..."
    task_tag = ""
    tokens_used = $preserved.tokens_used
    token_limit = 200000
    cost = $preserved.cost
    rate = 25.0
    queue_position = 1
    progress = $null
    is_waiting_for_input = $false
    session = @{
        id = Get-SessionShortId -SessionId $sessionId
        task = "PROC"
        start_ms = $preserved.start_ms
        elapsed_ms = Calculate-ElapsedMs -StartMs $preserved.start_ms
    }
}

$updatedAgents = Update-AgentInList -Agents $agents -NewAgent $agentObj
Write-AgentsFile -Agents $updatedAgents

Write-Output '{"continue": true}'
exit 0
