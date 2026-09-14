# B44.Unity Backlog

Agreed-but-not-started work and known defects. Hand-authored; never generated
and never gated by the build.

## Planned

### Run the proving suite against a real editor

The Unity Test Framework suite in `proving/B44.Unity.Proving` has never been
executed: no Unity installation was available when it was written. The first
real run is the acceptance of this package, in the same way that adopting a game
was the acceptance of `B44.Godot`.

What was verified without an editor, so that the first run starts from a
narrower list than "everything":

- The package sources compile against **both** Unity runtime generations, and
  the proving project's runtime sources compile against Unity's own reference
  assemblies, `B44.Common`, and the package.
- The PlayMode test's NUnit constraint usage compiles against NUnit, with the
  two Test Framework types it touches stubbed from the real 1.4.5 sources.
- `LogAssert.Expect` enqueues an expectation, so registering it before
  `AddComponent` — which is when `Awake` runs — is the correct order. Read from
  the 1.4.5 source, not assumed.
- `com.unity.test-framework` 1.4.5 exists on Unity's registry and resolves.
- The dependency sync resolves and copies the right assembly set, at either
  runtime generation.

What remains genuinely unverified, in the order it is likely to bite:

- **`AtomicJsonFileStore` under Unity 6's Mono runtime.** The `netstandard2.1`
  build of `B44.Common` resolves `System.Text.Json` from a package, and Unity's
  handling of that package's assemblies alongside its own .NET Standard 2.1
  profile is the single riskiest thing here. It is also the piece that
  disappears entirely on the modern runtime, where that closure is in-box.
  `B44BoundaryProbe.SaveReachedDisk` exists so a silent fall back to the
  in-memory store fails the test rather than passing it.
- **The plugin set the sync script produces**, accepted by the editor without
  duplicate-type or unresolved-reference errors.
- **Assembly definition resolution** — `overrideReferences` with
  `precompiledReferences` naming `B44.Common.dll`, across a local package and
  the project's own assemblies.

If the first run turns out to need broad compatibility work aimed only at Unity
6's managed runtime, that is the signal to stop and wait for the modern runtime
rather than to spend it — see the retarget checklist in `CLAUDE.md`.

### Retarget to the modern Unity runtime when it becomes the baseline

Tracked here because it is expected, not speculative. The checklist is in
`CLAUDE.md`; it is four small edits plus deleting `B44.Common`'s
`netstandard2.1` target, and nothing in `com.b44.unity/Runtime` is expected to
move. Running `./scripts/sync-proving-dependencies.ps1 -TargetFramework net8.0`
today already shows the destination: the shared-B44 payload drops from six
assemblies to one.

### Enable the Unity CI workflow

`.github/workflows/unity-proving.yml` is gated on a repository variable because
running an editor in CI needs a licence. It has not run.

## Deferred by decision

### Input and application shell

Production input and application-shell adapters remain deferred. Revisit keyboard,
controller and touch integration, rebinding, saved preferences, UI navigation and
focus only after B44 selects and verifies the modern Unity runtime baseline.
Require real editor/player and device evidence before claiming support; a future
runtime expectation or compile-only check is insufficient. This backlog item does
not authorize a production adapter, an older shared framework target, or duplicated
implementations.

### A shared engine abstraction

No `IB44Engine`, `IGameEngine`, or equivalent. `B44.Godot` and `B44.Unity`
adapting the same engine-free primitive is not evidence that the adapters belong
behind a common runtime abstraction, and the duplicated code between them is
currently a dozen lines. If a shared boundary is ever right, it should emerge
from two concrete implementations that turned out to need it.

### `B44.MonoGame`

Not planned. MonoGame is a design sanity check on whether shared B44 stays
callable from an ordinary C# game loop, not a target needing an adapter. The
check was run against `B44.Common` and `B44.GameSystems.Foundation` and found no
host lifecycle vocabulary below the boundary.

### Broader `netstandard2.1` conversion

Only `B44.Common` took the older target, and only because a Unity integration
could not otherwise be proved at all. No other shared library converts on that
precedent, and none of shared B44 gets a reduced, duplicated, or per-target API
to suit the current editor. See the sunset condition in `B44.Common`'s
`CLAUDE.md`.

### Player builds and IL2CPP

The proving project verifies the integration through the Test Framework, which
runs on Mono in the editor. An actual player build — and IL2CPP in particular,
where `System.Text.Json`'s reflection needs link preservation — is untested.

## Defects

None recorded.
