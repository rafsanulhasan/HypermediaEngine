#!/usr/bin/env pwsh
# enforce-tests.ps1
# Hook: Stop — blocks the agent from finishing if tests have not passed.
# Called from .github/hooks/enforce-tests.json (PostToolUse / Stop event).

param(
    [Parameter(ValueFromPipeline = $true)]
    [string]$InputJson
)

$input_data = $InputJson | ConvertFrom-Json -ErrorAction SilentlyContinue

# Avoid infinite loops: if already continuing from a stop hook, let it pass.
if ($input_data.stop_hook_active -eq $true) {
    exit 0
}

$testResult = & dotnet test 2>&1
if ($LASTEXITCODE -ne 0) {
    $output = @{
        hookSpecificOutput = @{
            hookEventName = "Stop"
            decision      = "block"
            reason        = "dotnet test failed. Fix all test failures before finishing."
        }
    } | ConvertTo-Json -Depth 5
    Write-Output $output
    exit 0
}

exit 0
