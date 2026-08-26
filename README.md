# B44.Unity

Unity-side adapters for B44 games, distributed as the `com.b44.unity` UPM
package.

This is the engine-coupled counterpart to
[`B44.Common`](https://github.com/DA-Sandman-Jr/B44.Common), and the sibling of
[`B44.Godot`](https://github.com/DA-Sandman-Jr/B44.Godot). It exists as its own
repository so every other B44 repository can keep its engine-free MSBuild guard
literally true, with no carve-outs — and so Unity-side code can churn on the
editor's release cadence without dragging the engine-free packages with it.

Planned work and known defects are tracked in [`BACKLOG.md`](BACKLOG.md).

> **Status: unproven on a real editor.** The boundary, the package structure and
> the dependency direction are built and tested; the Unity Test Framework suite
> in `proving/` has never been executed, because no Unity installation was
> available when it was written. Nothing here should be treated as a
> proven-at-runtime integration until that first run happens. What *is* verified
> without an editor, and what is not, is spelled out in
> [`BACKLOG.md`](BACKLOG.md).

## Dependency architecture

```text
Unity game project
        │
        ├── com.b44.unity  (UPM package, compiled by Unity)
        │        │
        └────────┴── B44.Common.dll  (ordinary managed assemblies, under Assets/)
```

One direction, and only one. `B44.Common` does not reference this package,
cannot reference it — there is no NuGet package to reference — and a non-Unity
consumer omits it entirely by doing nothing. `EngineBoundaryTests` asserts all
of that against the built assemblies rather than against the project files that
were supposed to produce them, and `B44.Standards` 0.11.0 fails the build of any
engine-free project that acquires a Unity type or a Unity assembly reference.

Shared B44 stays ordinary portable .NET: the same assemblies serve a Godot game,
a Unity game, or a plain C# host with no engine at all.

## Layout

| Path | Contents |
|---|---|
| `com.b44.unity/` | The UPM package. `Runtime/` is the entire shipped surface. |
| `B44.Unity.Compile/` | Compiles those same sources against both Unity runtime generations — today's and the modern .NET one — so a break is a CI failure rather than a console error the next time someone opens the editor. Verification only; it produces no shipping artifact and `Pack` is blocked. |
| `B44.Unity.Tests/` | xunit.v3, no editor. The engine-free rules, the dependency direction, and the package files Unity reads. |
| `proving/B44.Unity.Proving/` | A Unity project whose Test Framework suite runs a real B44 capability through the boundary inside a real editor. |
| `scripts/` | Dependency sync and the batch-mode test run. |

## Why UPM and not NuGet

`B44.Godot` ships as a NuGet package because a Godot game is an ordinary MSBuild
project that restores NuGet. Unity is not: it compiles the project's own sources
and packages itself, and does not restore NuGet at all. So the natural
distribution here is a UPM package of source, which Unity compiles.

The two repositories are siblings, not twins. Nothing here reproduces Godot's
smoke marker and exit-code protocol, because Unity already has a test runner
with its own verdict and adding a second one would only give the two something
to disagree about.

## Building and testing

```bash
dotnet test B44.Unity.slnx
```

No Unity installation is required, and **nothing that command runs proves the
Unity integration works.** It proves the boundary's engine-free rules, that
engine-free B44 carries no Unity dependency, and that the package sources still
compile under both Unity runtime generations.

The integration itself is proved by the Unity consumer:

```powershell
./scripts/sync-proving-dependencies.ps1
./scripts/run-proving-tests.ps1 -UnityVersion 6000.0.58f1
```

`-UnityVersion` is required with no default, for the reason `B44.Godot`'s
workflow requires `godot-version`: each consumer owns the editor version it
tests against, and this repository never needs editing when Unity releases.

## How shared B44 reaches a Unity project

Unity does not restore NuGet, and shared B44 ships as NuGet packages. Shared B44
arrives as ordinary managed assemblies under `Assets/Plugins/B44/`, resolved from
an ordinary bounded `PackageReference` float — the same float every other B44
consumer uses, so versioning has no Unity-specific rules. The directory is
generated and git-ignored: it is a restore, not a vendoring.

The assembly list is never hand-maintained. `sync-proving-dependencies.ps1`
restores a throwaway project at the current baseline and copies whatever NuGet
resolved, which automatically excludes what the platform already provides — the
exact set Unity rejects as duplicate type definitions.

## The Unity runtime baseline

Unity 6 runs a Mono profile at .NET Standard 2.1, and cannot load a `net8.0`
assembly at all. `B44.Common` 0.11.2 adds `netstandard2.1` alongside `net8.0` —
no API differs by target framework, and `net8.0` consumers see no change.

**That is the whole accommodation, and it is packaging, not architecture.**
Shared B44 is and stays ordinary modern portable .NET; no other shared library is
being converted, and no API is reduced, duplicated, or contorted for the current
editor. The baseline is declared once, as
`B44UnityRuntimeTargetFramework` in [`Directory.Build.props`](Directory.Build.props),
and `B44.Unity.Compile` builds the package sources against both it and `net8.0`
so the eventual retarget is a deletion rather than a port.

`CLAUDE.md` carries the retarget checklist. As a preview of where it lands,
running the sync script with `-TargetFramework net8.0` today drops the shared-B44
payload from six assemblies to one: the whole `System.Text.Json` closure is
in-box from `net8.0` onward.

## Versioning

`B44.Unity`, `B44.Common`, `B44.Godot`, and `B44.Standards` each version
independently from their own repositories. This package's number tracks none of
them.

## License

None — all rights reserved. The source is public for reference; it is not
licensed for reuse. See [`LICENSE`](LICENSE) and
[`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md).
