# Shared helper module for agent status hooks

function Get-ProjectName {
    param([string]$Cwd)
    if (-not $Cwd) { return "Claude Code" }
    # Extract last folder name from path as project name
    $folderName = Split-Path -Leaf $Cwd
    if ($folderName) { return $folderName }
    return "Claude Code"
}

function Get-StatusFilePath {
    $statusDir = Join-Path $env:APPDATA "AgentNotifier"
    if (-not (Test-Path $statusDir)) {
        New-Item -ItemType Directory -Path $statusDir -Force | Out-Null
    }
    return Join-Path $statusDir "agents.json"
}

function Get-AgentId {
    param([string]$SessionId)
    if (-not $SessionId) { return "claude-code-unknown" }
    return "claude-code-" + $SessionId.Substring(0, [Math]::Min(8, $SessionId.Length))
}

function Get-SessionShortId {
    param([string]$SessionId)
    if (-not $SessionId) { return "----" }
    return $SessionId.Substring(0, [Math]::Min(4, $SessionId.Length)).ToUpper()
}

function Read-AgentsFile {
    $statusFile = Get-StatusFilePath
    if (Test-Path $statusFile) {
        try {
            $existing = Get-Content $statusFile -Raw | ConvertFrom-Json
            if ($existing.agents) {
                return @($existing.agents)
            }
        } catch { }
    }
    return @()
}

function Get-ExistingAgent {
    param([array]$Agents, [string]$AgentId)
    return $Agents | Where-Object { $_.id -eq $AgentId } | Select-Object -First 1
}

function Update-AgentInList {
    param([array]$Agents, [hashtable]$NewAgent)

    $found = $false
    $updatedAgents = @()
    foreach ($agent in $Agents) {
        if ($agent.id -eq $NewAgent.id) {
            $updatedAgents += $NewAgent
            $found = $true
        } else {
            $updatedAgents += $agent
        }
    }
    if (-not $found) {
        $updatedAgents += $NewAgent
    }
    return $updatedAgents
}

function Write-AgentsFile {
    param([array]$Agents)

    $statusFile = Get-StatusFilePath
    $now = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()

    # Calculate totals
    $totalTokens = 0
    $totalCost = 0.0
    $totalElapsed = 0

    foreach ($agent in $Agents) {
        if ($agent.tokens_used) { $totalTokens += $agent.tokens_used }
        if ($agent.cost) { $totalCost += $agent.cost }
        if ($agent.session -and $agent.session.elapsed_ms) {
            $totalElapsed = [Math]::Max($totalElapsed, $agent.session.elapsed_ms)
        }
    }

    $payload = @{
        agents = $Agents
        total_tokens = $totalTokens
        total_cost = $totalCost
        total_elapsed_ms = $totalElapsed
        timestamp = (Get-Date).ToUniversalTime().ToString("o")
    }

    $json = $payload | ConvertTo-Json -Depth 5
    $json | Out-File -FilePath $statusFile -Encoding UTF8 -Force
}

function Get-PreservedValues {
    param($ExistingAgent)

    $now = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()

    return @{
        tokens_used = if ($ExistingAgent -and $ExistingAgent.tokens_used) { $ExistingAgent.tokens_used } else { 0 }
        cost = if ($ExistingAgent -and $ExistingAgent.cost) { $ExistingAgent.cost } else { 0.0 }
        start_ms = if ($ExistingAgent -and $ExistingAgent.session -and $ExistingAgent.session.start_ms) {
            $ExistingAgent.session.start_ms
        } else { $now }
        elapsed_ms = 0
    }
}

function Calculate-ElapsedMs {
    param([long]$StartMs)
    $now = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
    return [int][Math]::Max(0, $now - $StartMs)
}

function Get-TokenUsageFromTranscript {
    param([string]$TranscriptPath)

    $result = @{ tokens = 0; cost = 0.0 }

    if (-not $TranscriptPath -or -not (Test-Path $TranscriptPath)) {
        return $result
    }

    try {
        $content = Get-Content $TranscriptPath -Raw -ErrorAction SilentlyContinue
        if (-not $content) { return $result }

        # Parse JSONL format - each line is a JSON object
        $totalInput = 0
        $totalOutput = 0
        $totalCacheRead = 0

        $lines = $content -split "`n"
        foreach ($line in $lines) {
            if ($line -match '"usage"' -and $line.Trim()) {
                try {
                    $obj = $line | ConvertFrom-Json -ErrorAction SilentlyContinue
                    # Usage is nested in message.usage
                    $usage = $null
                    if ($obj.message -and $obj.message.usage) {
                        $usage = $obj.message.usage
                    } elseif ($obj.usage) {
                        $usage = $obj.usage
                    }

                    if ($usage) {
                        if ($usage.input_tokens) { $totalInput += $usage.input_tokens }
                        if ($usage.output_tokens) { $totalOutput += $usage.output_tokens }
                        if ($usage.cache_read_input_tokens) { $totalCacheRead += $usage.cache_read_input_tokens }
                    }
                } catch { }
            }
        }

        $result.tokens = $totalInput + $totalOutput + $totalCacheRead
        # Opus pricing: $15/M input, $75/M output, $1.50/M cache read
        $result.cost = ($totalInput * 0.000015) + ($totalOutput * 0.000075) + ($totalCacheRead * 0.0000015)
    }
    catch { }

    return $result
}

Export-ModuleMember -Function *
