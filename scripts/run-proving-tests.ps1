#Requires -Version 7.0
<#
.SYNOPSIS
    Runs the proving project's Unity Test Framework suite in batch mode.

.DESCRIPTION
    This is the only verification in the repository that actually exercises the
    Unity integration. Everything under B44.Unity.Tests runs without an editor
    and proves other things.

    Unity's own exit codes are not enough on their own: an editor that failed to
    start, failed to compile, or died mid-run can exit non-zero without ever
    producing a verdict, and that is a different problem from a failing
    assertion. The result file is therefore treated as the authority and its
    absence is reported as its own outcome.

.PARAMETER UnityVersion
    Required with no default, for the same reason B44.Godot's reusable workflow
    requires godot-version: each consumer owns the editor version it tests
    against, and this repository never needs editing when Unity releases.

.PARAMETER UnityPath
    The editor executable, when it is not in the Unity Hub's default location.

.PARAMETER TestPlatform
    PlayMode by default. PlayMode is what demonstrates a running application;
    EditMode exists here only as a faster compile check.

.EXAMPLE
    ./scripts/run-proving-tests.ps1 -UnityVersion 6000.0.58f1
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $UnityVersion,

    [string] $UnityPath,

    [ValidateSet('PlayMode', 'EditMode')]
    [string] $TestPlatform = 'PlayMode'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repositoryRoot 'proving/B44.Unity.Proving'
$pluginDirectory = Join-Path $projectPath 'Assets/Plugins/B44'

if (-not (Test-Path (Join-Path $pluginDirectory 'B44.Common.dll'))) {
    throw "The engine-free B44 assemblies are not in the project. Run ./scripts/sync-proving-dependencies.ps1 first."
}

function Resolve-UnityExecutable {
    if ($UnityPath) {
        if (-not (Test-Path $UnityPath)) { throw "No Unity editor at $UnityPath." }
        return $UnityPath
    }

    $candidates = @(
        "C:/Program Files/Unity/Hub/Editor/$UnityVersion/Editor/Unity.exe",
        "/Applications/Unity/Hub/Editor/$UnityVersion/Unity.app/Contents/MacOS/Unity",
        "$HOME/Unity/Hub/Editor/$UnityVersion/Editor/Unity"
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) { return (Resolve-Path $candidate).Path }
    }

    throw @"
No Unity $UnityVersion found in the Unity Hub's default locations:
$($candidates -join [Environment]::NewLine)
Install it through the Hub, or pass -UnityPath.
"@
}

$unity = Resolve-UnityExecutable
Write-Host "Editor: $unity"

# Pin the project to the editor being run, so the Hub and the editor agree and
# an upgrade prompt cannot stall a batch-mode run.
$settingsDirectory = Join-Path $projectPath 'ProjectSettings'
New-Item -ItemType Directory -Path $settingsDirectory -Force | Out-Null
"m_EditorVersion: $UnityVersion" | Set-Content -Path (Join-Path $settingsDirectory 'ProjectVersion.txt') -Encoding utf8

$resultsPath = Join-Path $projectPath 'TestResults.xml'
Remove-Item $resultsPath -ErrorAction SilentlyContinue

$arguments = @(
    '-batchmode'
    '-nographics'
    '-runTests'
    '-projectPath', $projectPath
    '-testPlatform', $TestPlatform
    '-testResults', $resultsPath
    '-logFile', '-'
)

Write-Host "Running $TestPlatform tests..."
& $unity @arguments
$editorExitCode = $LASTEXITCODE

if (-not (Test-Path $resultsPath)) {
    # No result file means no verdict was ever reached: a compile error, a
    # licence the editor could not acquire, or a crash. Distinct from a failing
    # test, and diagnosed completely differently.
    Write-Error "Unity exited $editorExitCode without writing $resultsPath. The suite never reported a verdict — check the log above for compile errors or a licence failure."
    exit 1
}

[xml] $results = Get-Content $resultsPath
$run = $results.'test-run'
Write-Host ""
Write-Host "total=$($run.total) passed=$($run.passed) failed=$($run.failed) skipped=$($run.skipped)"

foreach ($case in $results.SelectNodes('//test-case[@result="Failed"]')) {
    Write-Host ""
    Write-Host "FAILED $($case.fullname)"
    Write-Host $case.failure.message.InnerText
}

if ([int] $run.failed -gt 0 -or $editorExitCode -ne 0) {
    exit 1
}

Write-Host ""
Write-Host "B44.Unity boundary verified in Unity $UnityVersion."
