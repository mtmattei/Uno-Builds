$statusDir = Join-Path $env:APPDATA "AgentNotifier"
$statusFile = Join-Path $statusDir "agents.json"

$testData = @{
    agents = @(
        @{
            id = "agent-001"
            name = "Code Analyzer"
            model = "opus-4.5"
            status = "working"
            label = "PROC"
            message = "Analyzing..."
            current_task = "Analyzing codebase structure and generating documentation"
            task_tag = "ANLZ"
            tokens_used = 3247
            token_limit = 200000
            cost = 0.0487
            rate = 25.0
            queue_position = 1
            progress = 65
            is_waiting_for_input = $false
            session = @{
                id = "A1B2"
                task = "ANLZ"
                elapsed_ms = 127000
            }
        },
        @{
            id = "agent-002"
            name = "Test Writer"
            model = "sonnet-4"
            status = "working"
            label = "PROC"
            message = "Writing tests..."
            current_task = "Generating unit tests for API endpoints"
            task_tag = "TEST"
            tokens_used = 1856
            token_limit = 200000
            cost = 0.0231
            rate = 30.0
            queue_position = 2
            progress = $null
            is_waiting_for_input = $false
            session = @{
                id = "C3D4"
                task = "TEST"
                elapsed_ms = 64000
            }
        },
        @{
            id = "agent-003"
            name = "PR Reviewer"
            model = "opus-4.5"
            status = "finished"
            label = "DONE"
            message = "Review complete"
            current_task = ""
            task_tag = "REVW"
            tokens_used = 2103
            token_limit = 200000
            cost = 0.0315
            rate = 0.0
            queue_position = 3
            progress = $null
            is_waiting_for_input = $false
            session = @{
                id = "E5F6"
                task = "REVW"
                elapsed_ms = 89000
            }
        }
    )
    total_tokens = 7206
    total_cost = 0.1033
    total_elapsed_ms = 127000
    timestamp = (Get-Date).ToUniversalTime().ToString("o")
}

$testData | ConvertTo-Json -Depth 4 | Out-File -FilePath $statusFile -Encoding UTF8 -Force
Write-Output "Test data written to $statusFile"
