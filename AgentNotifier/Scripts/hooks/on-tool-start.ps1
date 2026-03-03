# Hook: PreToolUse - Set status to working with tool info, preserve metrics

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module "$scriptDir\AgentHelper.psm1" -Force

$inputJson = [Console]::In.ReadToEnd() | ConvertFrom-Json
$toolName = $inputJson.tool_name
$sessionId = $inputJson.session_id
$cwd = $inputJson.cwd
$agentId = Get-AgentId -SessionId $sessionId
$projectName = Get-ProjectName -Cwd $cwd

# Map tool names to friendly task info
$taskInfo = switch -Regex ($toolName) {
    "^Bash$" { @{ code = "EXEC"; desc = "Executing shell command..." } }
    "^Read$" { @{ code = "READ"; desc = "Reading file contents..." } }
    "^Write$" { @{ code = "WRIT"; desc = "Writing file to disk..." } }
    "^Edit$" { @{ code = "EDIT"; desc = "Editing file contents..." } }
    "^Glob$" { @{ code = "SRCH"; desc = "Searching for files..." } }
    "^Grep$" { @{ code = "GREP"; desc = "Searching file contents..." } }
    "^Task$" { @{ code = "TASK"; desc = "Spawning sub-agent task..." } }
    "^WebFetch$" { @{ code = "WEB"; desc = "Fetching web content..." } }
    "^WebSearch$" { @{ code = "WEB"; desc = "Searching the web..." } }
    "^mcp__" { @{ code = "MCP"; desc = "Calling MCP tool..." } }
    default { @{ code = "WORK"; desc = "Processing request..." } }
}

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
    label = "PROCESSING"
    message = "Using $toolName..."
    current_task = $taskInfo.desc
    task_tag = $toolName
    tokens_used = $preserved.tokens_used
    token_limit = 200000
    cost = $preserved.cost
    rate = 25.0
    queue_position = 1
    progress = $null
    is_waiting_for_input = $false
    session = @{
        id = Get-SessionShortId -SessionId $sessionId
        task = $taskInfo.code
        start_ms = $preserved.start_ms
        elapsed_ms = Calculate-ElapsedMs -StartMs $preserved.start_ms
    }
}

$updatedAgents = Update-AgentInList -Agents $agents -NewAgent $agentObj
Write-AgentsFile -Agents $updatedAgents

Write-Output '{"continue": true}'
exit 0
