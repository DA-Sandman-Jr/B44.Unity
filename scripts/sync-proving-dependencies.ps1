#Requires -Version 7.0
<#
.SYNOPSIS
    Materializes the engine-free B44 assemblies the proving Unity project needs.

.DESCRIPTION
    Unity does not restore NuGet. B44's engine-free libraries ship as NuGet
    packages, so something has to put their assemblies where the editor can see
    them, and this is that something — the Unity equivalent of `dotnet restore`
    for one project.

    The set of assemblies is NOT hardcoded. A throwaway project targeting the
    current Unity runtime baseline and referencing B44.Common is restored, and
    whatever NuGet resolves as that target's runtime assets is what gets copied.
    That matters more than it looks: NuGet's asset selection already excludes
    the packages the platform provides in-box, which is exactly the set Unity
    would reject as duplicate type definitions. Guessing at that list by hand is
    how this breaks.

    Neither the target framework nor the assembly list is written down here. The
    baseline comes from B44UnityRuntimeTargetFramework in Directory.Build.props,
    so moving to the modern Unity runtime is the same single edit that moves the
    compile project — this script then resolves that generation's assets with no
    change at all.

.PARAMETER Version
    Bounded float for B44.Common, matching the repository's own reference.

.PARAMETER TargetFramework
    Overrides the baseline read from Directory.Build.props. Use it to try a
    runtime generation before adopting it, not to keep a second declaration.

.PARAMETER Source
    Extra NuGet source, for validating a build of B44.Common that has not been
    published yet.

.EXAMPLE
    ./scripts/sync-proving-dependencies.ps1
#>
[CmdletBinding()]
param(
    [string] $Version = '0.11.*',
    [string] $TargetFramework,
    [string] $Source
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$pluginDirectory = Join-Path $repositoryRoot 'proving/B44.Unity.Proving/Assets/Plugins/B44'

if (-not $TargetFramework) {
    # Read the baseline rather than restate it. -getProperty evaluates the same
    # Directory.Build.props the compile project does, so the two cannot drift.
    $compileProject = Join-Path $repositoryRoot 'B44.Unity.Compile/B44.Unity.Compile.csproj'
    $TargetFramework = (& dotnet msbuild $compileProject -getProperty:B44UnityRuntimeTargetFramework 2>$null | Select-Object -Last 1)
    if ($LASTEXITCODE -ne 0 -or -not $TargetFramework) {
        throw "Could not read B44UnityRuntimeTargetFramework from $compileProject. Pass -TargetFramework to override."
    }
    $TargetFramework = $TargetFramework.Trim()
}

$workingDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ("b44-unity-sync-" + [Guid]::NewGuid().ToString('N'))

New-Item -ItemType Directory -Path $workingDirectory -Force | Out-Null
try {
    # The target framework is the whole point of the probe: resolving against a
    # generation the editor does not run hands Unity assemblies it cannot load.
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>$TargetFramework</TargetFramework>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="B44.Common" Version="$Version" />
  </ItemGroup>
</Project>
"@ | Set-Content -Path (Join-Path $workingDirectory 'probe.csproj') -Encoding utf8

    $restoreArguments = @('restore', (Join-Path $workingDirectory 'probe.csproj'))
    if ($Source) {
        $restoreArguments += @('--source', 'https://api.nuget.org/v3/index.json', '--source', $Source)
    }

    Write-Host "Resolving B44.Common $Version for $TargetFramework..."
    & dotnet @restoreArguments | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Restore failed. B44.Common $Version must exist and must publish a $TargetFramework target."
    }

    $assets = Get-Content (Join-Path $workingDirectory 'obj/project.assets.json') -Raw | ConvertFrom-Json
    $packageFolders = $assets.packageFolders.PSObject.Properties.Name
    # Exactly one target was requested, so take it rather than matching on a
    # name NuGet has spelled two different ways across SDK versions.
    $target = $assets.targets.PSObject.Properties | Select-Object -First 1

    if (-not $target) {
        throw 'The restore produced no target at all.'
    }

    $resolved = [System.Collections.Generic.List[string]]::new()
    foreach ($library in $target.Value.PSObject.Properties) {
        if (-not $library.Value.PSObject.Properties.Name.Contains('runtime')) { continue }

        foreach ($asset in $library.Value.runtime.PSObject.Properties.Name) {
            # "_._" is NuGet saying the platform already provides this one.
            # Copying it anyway is what produces duplicate-type errors in Unity.
            if ($asset.EndsWith('_._')) { continue }

            foreach ($folder in $packageFolders) {
                # Package folders are lowercased on disk; library ids are not.
                $relative = $library.Name.ToLowerInvariant().Replace('/', [IO.Path]::DirectorySeparatorChar)
                $candidate = Join-Path $folder (Join-Path $relative $asset)
                if (Test-Path $candidate) {
                    $resolved.Add((Resolve-Path $candidate).Path)
                    break
                }
            }
        }
    }

    if ($resolved.Count -eq 0) {
        throw 'Resolved no assemblies; the probe restored but produced nothing to copy.'
    }

    if (Test-Path $pluginDirectory) {
        Remove-Item -Recurse -Force $pluginDirectory
    }
    New-Item -ItemType Directory -Path $pluginDirectory -Force | Out-Null

    foreach ($assembly in $resolved) {
        Copy-Item -Path $assembly -Destination $pluginDirectory -Force
        Write-Host ("  " + (Split-Path $assembly -Leaf))
    }

    Write-Host ""
    Write-Host "$($resolved.Count) assemblies -> $pluginDirectory"
    Write-Host 'Open the proving project in Unity, or run ./scripts/run-proving-tests.ps1.'
}
finally {
    Remove-Item -Recurse -Force $workingDirectory -ErrorAction SilentlyContinue
}
