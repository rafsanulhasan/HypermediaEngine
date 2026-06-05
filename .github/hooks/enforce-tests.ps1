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

# Run the dedicated testing solution when available to avoid ambiguous root-level discovery.
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRootPath = Resolve-Path (Join-Path $scriptDir "..\..")
$testingSolutionPath = Join-Path $repoRootPath "HypermediaEngine.Testing.slnx"

if (Test-Path $testingSolutionPath) {
    $testResult = & dotnet test $testingSolutionPath 2>&1
}
else {
    $testResult = & dotnet test 2>&1
}
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
