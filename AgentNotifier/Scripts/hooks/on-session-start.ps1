# Hook: SessionStart - Initialize agent when session begins

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module "$scriptDir\AgentHelper.psm1" -Force

$inputJson = [Console]::In.ReadToEnd() | ConvertFrom-Json
$sessionId = $inputJson.session_id
$cwd = $inputJson.cwd
$agentId = Get-AgentId -SessionId $sessionId
$projectName = Get-ProjectName -Cwd $cwd
$now = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()

$agents = Read-AgentsFile
$existing = Get-ExistingAgent -Agents $agents -AgentId $agentId

$agentObj = @{
    id = $agentId
    name = $projectName
    model = "claude-opus-4"
    status = "idle"
    label = "READY"
    message = "Session started - awaiting command"
    current_task = ""
    task_tag = ""
    tokens_used = 0
    token_limit = 200000
    cost = 0.0
    rate = 0.0
    queue_position = 1
    progress = $null
    is_waiting_for_input = $false
    session = @{
        id = Get-SessionShortId -SessionId $sessionId
        task = "NONE"
        start_ms = $now
        elapsed_ms = 0
    }
}

$updatedAgents = Update-AgentInList -Agents $agents -NewAgent $agentObj
Write-AgentsFile -Agents $updatedAgents

Write-Output '{"continue": true}'
exit 0
