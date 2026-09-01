> **Auto-generated from `CLAUDE.md`** — edit the sibling `CLAUDE.md` instead. Direct changes are overwritten by B44.Standards on the next synchronized build.

# B44.Unity — Unity-Side Adapters

<!-- B44 ORGANIZATION GUIDANCE: START -->
## B44 Organization Guidance

- `AGENTS.md` files are auto-generated on build; see the generated header for the source file to edit.
- Before editing or reviewing a file, read and follow every applicable `AGENTS.md` from the repository root through that file's directory. Nearer instructions override broader instructions.
- Analyzer severities live in the `B44.Standards` packaged globalconfig, never in a repository `.editorconfig`. Repository editorconfigs own style and whitespace only; tune analyzer policy upstream in the package.
- Public server/function and endpoint-owning projects set `<B44SecuritySensitive>true</B44SecuritySensitive>` in `Directory.Build.props`; B44.Standards then enables the complete SDK Security category at a target-level-pinned rule set.
- Fix shared behavior in the B44 package that owns it; do not fork or paste a local copy into a consumer repository.
- Use compatibility-bounded floating versions for internal B44 packages in every consumer, including production: pre-1.0 packages use `0.<minor>.*`, while stable packages use `<major>.*`. Package owners bump the excluded boundary for breaking changes, and consumers cross that boundary manually. Never use an unbounded `*`. Enforcement-expanding Standards changes bump the minor version and never enter an existing patch float.
- Treat roughly 350 physical lines as a review warning for production source files. New production files should normally stay at or below 500 lines; files above 650 lines require a clear cohesion-based reason.
- Existing oversized files must not grow unless the same change performs a real extraction and leaves the file smaller. Coordinators coordinate; do not evade the limit with cosmetic partial classes, one-method services, generic utility dumping grounds, or needless factories.
- `B44.Standards` fails the build on drift it can decide mechanically: an engine assembly or source generator reaching an engine-free project, a banned-symbol boundary whose analyzer is missing (which leaves the ban list inert), a `*.Tests` project that would discover no tests, a production reference to a test project, an unbounded `*` float on an internal B44 package, and — where the repository opts in — committed build debris, analyzer suppressions past its budget, and warnings that no longer fail the build. Each check names the property that turns it off; raise a budget or add an exemption in `Directory.Build.props` in the same change that needs it, so the decision is visible in review rather than silent.
- Generated guidance is verified, not trusted. Build with `-p:B44AgentSyncVerifyOnly=true` in CI so a stale `AGENTS.md`, managed `AGENTS.md` block, or `.b44/B44.Tooling.md` fails the build instead of being silently rewritten by whoever builds next. Hand-authored prose is a different thing and no build can check it: repository-local guidance that names types, counts, or responsibilities goes stale silently and actively misleads the next change, because guidance is read as instructions. Re-read the prose nearest the code you just changed.
- An architectural rule that can be stated as "this layer must not call these members" is cheap to enforce: put the members in a `BannedSymbols.<Rule>.txt` and register it with `B44BannedSymbols` on the projects the rule governs. Prefer that over leaving the rule to review forever. Rules that cannot be written as an exact list stay in the owning repository's own architecture tests.
- Extraction is judged on the capability, not on a headcount of repositories. A single real consumer is enough to extract a bounded reusable capability when it solves a recognizable reusable problem rather than a one-project quirk, its seam is small and coherent, its API stays natural and domain-facing without caller-specific assumptions, independent evidence says the reuse is real, and nothing speculative has to be built around it. That evidence can be another project, a genre or domain pattern, existing B44 work, donor or reuse findings already translated into neutral requirements, or established practice — a second consumer is one form of it, not a precondition. Keep behavior local instead when the reusable seam is unclear, when it is still strongly shaped by one project's rules, vocabulary, presentation, or implementation, or when extracting it would require machinery no caller needs yet.
- Recognizing a capability and choosing its home are separate decisions. Shared behavior belongs to the package that naturally owns it; nothing lands in `B44.Common` by default, and no package becomes a general utility dump. A primitive that turns up independently in a second repository does not by itself extract anything, but it is a strong ownership-review trigger and a reason to reconsider a shared home against a project-specific one: record it in `B44.Common`'s backlog with both call sites and settle ownership there. Nothing automates this: whether two near-identical functions are the same concept, or the same formula serving different intents, is a design judgement.
- Generalized infrastructure raises the bar rather than inheriting the bounded one. Cross-capability foundations, generalized orchestration, registries and schedulers, transaction or authority frameworks, plugin and policy architectures, portfolio-wide Standards rules, and abstractions that mostly serve hypothetical future consumers need concrete pressure from multiple independent real consumers — normally at least two — before they exist at all. The goal is a broad repertoire of useful bounded capabilities, not a universal game engine.
- Before automated analyzer fixes, baseline measurement, scripted bulk text rewrites, or consuming a freshly published package, read `.b44/B44.Tooling.md`.
- Godot writes a `.uid` file beside a script and uses it as that script's stable identifier. Commit every one Godot generates and never add `*.uid` to `.gitignore`: without a committed sidecar, references break as soon as the repository is cloned onto another machine, including a CI runner doing a fresh checkout. A sidecar Godot has not written yet is not a defect and not tracked debt — nothing requires one, no build or CI check reports a missing one, and a UID is never hand-written to satisfy a check, because a fabricated value looks authoritative and resolves to nothing. Godot generates sidecars for C# scripts under the project directory, including engine-free `Core` and test projects it never loads; those are committed too. What is checked is the sidecar that outlives its file: a tracked `.uid` or `.import` whose principal file is no longer tracked is orphaned debris and fails repository hygiene.
- Each repository keeps a root `BACKLOG.md` for agreed-but-not-started work and known defects, with defects in their own section so they stay distinct from planned work. It is authored by hand, never generated and never gated by the build — an empty file written to satisfy a check is worse than no file. Cross-repository programs live once in `B44.Common`'s backlog; a consumer's backlog links to the program and holds only its own share of the work, never a restatement that can drift.
- Isolation is by repository, not by folder. Engine- or framework-coupled adapters live in their own repository and package so engine-free build guards remain literal and release cadences stay independent.
- Keep licensing boundaries explicit. Source governed by terms different from a repository's `LICENSE` belongs behind a separately documented repository/package boundary with its provenance and required notices intact.
<!-- B44 ORGANIZATION GUIDANCE: END -->

The one B44 repository allowed to reference Unity. It exists so every other B44
repository can keep its engine-free guard literally true with no carve-outs, and
so Unity-side code can churn on the editor's release cadence without dragging the
engine-free packages with it.

Distributed as the `com.b44.unity` UPM package, consumed by B44 Unity games. Its
Godot-side sibling is [`B44.Godot`](https://github.com/DA-Sandman-Jr/B44.Godot).

## Hard Rules

- **This is the only place Unity may appear.** Anything engine-free belongs in
  `B44.Common` instead. If a type here has no `using UnityEngine` and no reason
  to live beside one, it is in the wrong repository.
- **Thin adapters only.** This is a bridge over primitives that already exist
  below the boundary, not a second home for game logic. No game rules, no game
  state, no payload schemas, no content catalogs, no scene-flow authority, no
  global service location.
- **The second-occurrence rule applies here exactly as it does to
  `B44.Common`.** A helper enters only when at least two consumers demonstrably
  need materially equivalent behavior. Until B44 has two Unity games, the
  demonstrated second occurrence may come from the Godot side: `B44.Godot`
  already shipping the same bridge is evidence the need is real and not
  Unity-specific speculation. A helper with no counterpart anywhere is not.
- **Pure logic stays testable without the editor.** Routing rules, path rules,
  parsing, and formatting go in plain classes with no Unity types;
  `MonoBehaviour` and editor types stay thin shells over them. There is no Unity
  installation on a typical CI runner and licensing one is not free, so anything
  that needs an editor to be tested effectively is tested rarely.
- **No shared engine abstraction.** `B44.Godot` and `B44.Unity` are siblings, not
  implementations of a common interface. Two adapters over the same engine-free
  primitive is not evidence that the adapters belong behind a shared runtime
  abstraction, and duplicated small adapter code is the cheaper mistake.

## Why UPM and not NuGet — Decision Record

`B44.Godot` ships as a NuGet package because a Godot game is an ordinary MSBuild
project that restores NuGet. Unity is not: it compiles the project's own sources
and packages itself, and does not restore NuGet at all. So the natural
distribution here is a UPM package of **source**, which Unity compiles, and the
`Pack` target is deliberately blocked.

That decision sets the two constraints every file in `com.b44.unity/Runtime`
lives under, both of which are looser everywhere else in B44:

1. **C# 9**, for as long as Unity 6 is the baseline. File-scoped namespaces,
   record structs, primary constructors, and collection expressions are all
   C# 10 or later and are all used freely in the rest of B44 — so they are the
   mistake most likely to arrive by copying a neighbouring repository's file.
   This is a property of the current editor, not of B44, and it leaves with it.
2. **Explicit `#nullable enable` per file.** The MSBuild project turns nullable
   annotations on repository-wide; Unity's compilation of the same files does
   not, and a file that relies on the project setting means one thing here and
   another in the editor.

`B44.Unity.Compile` exists to make both failures loud, and compiles the same
sources against **both** Unity runtime generations, so a break is a red CI run on
a machine with no editor rather than a red console the next time someone opens
Unity. It produces no shipping artifact.

## The Unity Runtime Baseline — Decision Record

**This is a packaging fact with a sunset, not an architectural one, and it is
written down in exactly one place:** `B44UnityRuntimeTargetFramework` in
`Directory.Build.props`.

Unity 6 runs a Mono profile whose API compatibility level is .NET Standard 2.1.
A `net8.0` assembly references `System.Runtime 8.0.0.0` and the editor cannot
load it — not a failure at the call site, the whole assembly is refused. Every
engine-free B44 library targeted `net8.0` only, so none of them could reach
Unity. `B44.Common` 0.11.2 adds `netstandard2.1` alongside `net8.0`: no API
differs by target framework, no type is conditioned on one, and `net8.0`
consumers see no change.

**That is the entire accommodation, and it stops there.** Shared B44 is and
stays ordinary modern portable .NET. Specifically, none of the following is on
the table to satisfy the current editor: converting other shared B44 libraries
to `netstandard2.1` without an independent reason, reduced or per-target APIs,
duplicated implementations, or contorting a shared library away from a modern
BCL API it genuinely wants. If shared code ever needs something `netstandard2.1`
cannot express, the answer is to drop that target — not to work around it.

`B44.Unity.Compile` therefore builds the package sources against the current
generation **and** `net8.0`, from the same single declaration. The second target
is not decoration: it is what stops the outgoing runtime from becoming the
ceiling these sources are quietly written to, and it is why the retarget below
is a deletion rather than a port.

## Retargeting to the Modern Unity Runtime

When the CoreCLR Unity line becomes the development baseline, the whole change
is packaging and proving. Nothing in `com.b44.unity/Runtime` is expected to move.

1. `Directory.Build.props` — set `B44UnityRuntimeTargetFramework` to the new
   generation, raise `B44UnityLangVersion`, and drop
   `B44UnityFutureTargetFramework` once it is no longer ahead of the baseline.
2. `com.b44.unity/package.json` — raise `unity` to the first editor version on
   that runtime.
3. `proving/B44.Unity.Proving` — its API compatibility level, and the editor
   version passed to `run-proving-tests.ps1`.
4. `B44.Common` — drop `netstandard2.1` from `TargetFrameworks`, along with
   `Compat/IsExternalInit.cs`, the conditional `System.Text.Json` reference, and
   the target-framework pin on the repository-wide MSBuild targets.

`scripts/sync-proving-dependencies.ps1` needs no edit at all: it reads the
baseline from step 1 and resolves whatever that generation's assets are. Running
it with `-TargetFramework net8.0` today already shows where this ends up — the
shared-B44 payload drops from six assemblies to one, because the whole
`System.Text.Json` closure is in-box from `net8.0` onward.

## How Shared B44 Reaches a Unity Project

Unity does not restore NuGet, and shared B44 ships as NuGet packages. Something
has to bridge that, and the bridge is the part of this repository most likely to
be reinvented badly, so it is stated rather than left to a script nobody reads.

**The model:** shared B44 arrives as ordinary managed assemblies under
`Assets/Plugins/B44/`, resolved by `scripts/sync-proving-dependencies.ps1` from
an ordinary bounded `PackageReference` float — the same float every other B44
consumer uses, so versioning has no Unity-specific rules. The directory is
generated and git-ignored; it is a restore, not a vendoring.

The assembly list is never hand-maintained. The script restores a throwaway
project at the baseline target framework and copies whatever NuGet resolved,
which automatically excludes the packages the platform already provides — the
exact set Unity rejects as duplicate type definitions. A hand-written list is
how that breaks, and it breaks as a wall of unrelated compile errors.

Two alternatives were considered and rejected for now:

- **NuGetForUnity.** Does the same job with a standard front end, and is the
  obvious replacement if this ever needs to be a game-facing workflow rather
  than a fixture one. Rejected today only because it adds a third-party
  dependency to the one thing that has to work before anything else can be
  diagnosed.
- **Bundling the assemblies inside `com.b44.unity`.** Rejected outright: it
  would make B44.Unity own B44.Common's version, which inverts the dependency
  direction in everything except the compiler.

For IL2CPP, these are plain managed assemblies with nothing Unity-specific about
them, which is the point — but reflection-based serialization needs link
preservation, and that is a game-side concern, not this package's.

## What Proves What

Three layers, and each is honest about the one below it:

1. `B44.Unity.Tests` — xunit.v3, no editor. Covers the engine-free rules, the
   dependency direction against the built assemblies, and the assembly
   definition and package manifest as files the editor will read. It cannot
   observe that `Debug.LogError` is ever reached, and no assertion in it should
   be read as covering that.
2. `proving/B44.Unity.Proving` — a Unity project whose Test Framework suites run
   a real B44 capability through this boundary inside a real editor. This is the
   only layer that proves the integration works.
3. `.github/workflows/unity-proving.yml` — layer 2 in CI, which needs a Unity
   licence and is therefore opt-in per repository rather than on by default.

A test that never loaded a Unity assembly must not be presented as evidence that
the Unity integration works. Where that distinction is not obvious from a test's
location, say it in the test.

## Portability Intent

Shared B44 is ordinary portable C#. The hosts are shells over it:

```text
                shared B44 / game code
                          |
        ------------------------------------
        |                 |                |
      Godot             Unity          MonoGame
      shell             shell        plain C# host
```

Godot and Unity are the actual proving environments. MonoGame is a **design
sanity check only** — there is no `B44.MonoGame` and none is planned. Its use is
as a question to ask when reviewing anything below the boundary: would this be
awkward to call from an ordinary C# game loop with no engine at all? If the
answer is yes, a host assumption has leaked downward, and the fix belongs on the
shared side rather than in a second adapter.

## Layout

- `com.b44.unity/` — the UPM package. `Runtime/` is the entire shipped surface.
- `B44.Unity.Compile/` — compiles those sources under Unity's constraints.
  Verification only; `IsPackable=false` and `Pack` errors.
- `B44.Unity.Tests/` — xunit.v3, engine-free. `<TestingPlatformDotnetTestSupport>true`
  is required for `dotnet test` to discover xunit.v3 on current SDKs.
- `proving/B44.Unity.Proving/` — the Unity consumer. See its `README.md`; its
  `ProjectSettings/` and `Assets/Plugins/B44/` are generated, not committed.
- `scripts/` — dependency sync and the batch-mode test run.

## Commands

```bash
dotnet build B44.Unity.slnx
dotnet test B44.Unity.slnx
```

```powershell
./scripts/sync-proving-dependencies.ps1
./scripts/run-proving-tests.ps1 -UnityVersion 6000.0.58f1
```

`-UnityVersion` is required with no default, for the reason `B44.Godot`'s
workflow requires `godot-version`: each consumer owns the editor version it
tests against, and this repository never needs editing when Unity releases.
