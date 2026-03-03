# Hook: PostToolUse - Update after tool completes, extract token usage

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module "$scriptDir\AgentHelper.psm1" -Force

$inputJson = [Console]::In.ReadToEnd() | ConvertFrom-Json
$toolName = $inputJson.tool_name
$sessionId = $inputJson.session_id
$cwd = $inputJson.cwd
$transcriptPath = $inputJson.transcript_path
$agentId = Get-AgentId -SessionId $sessionId
$projectName = Get-ProjectName -Cwd $cwd

$agents = Read-AgentsFile
$existing = Get-ExistingAgent -Agents $agents -AgentId $agentId
$preserved = Get-PreservedValues -ExistingAgent $existing

# Preserve existing name if set
$name = if ($existing -and $existing.name -and $existing.name -ne "Claude Code") { $existing.name } else { $projectName }

# Try to get token usage from transcript
$tokenData = Get-TokenUsageFromTranscript -TranscriptPath $transcriptPath
$tokensUsed = if ($tokenData.tokens -gt 0) { $tokenData.tokens } else { $preserved.tokens_used }
$cost = if ($tokenData.cost -gt 0) { $tokenData.cost } else { $preserved.cost }

$agentObj = @{
    id = $agentId
    name = $name
    model = "claude-opus-4"
    status = "working"
    label = "PROCESSING"
    message = "Completed $toolName"
    current_task = "Continuing..."
    task_tag = $toolName
    tokens_used = $tokensUsed
    token_limit = 200000
    cost = $cost
    rate = 25.0
    queue_position = 1
    progress = $null
    is_waiting_for_input = $false
    session = @{
        id = Get-SessionShortId -SessionId $sessionId
        task = "WORK"
        start_ms = $preserved.start_ms
        elapsed_ms = Calculate-ElapsedMs -StartMs $preserved.start_ms
    }
}

$updatedAgents = Update-AgentInList -Agents $agents -NewAgent $agentObj
Write-AgentsFile -Agents $updatedAgents

Write-Output '{"continue": true}'
exit 0
