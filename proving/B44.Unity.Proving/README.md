# B44.Unity proving consumer

A Unity project that exists only to run a real B44 capability through the
`com.b44.unity` boundary inside a real editor. It is the only thing in this
repository that proves the Unity integration works — everything in
`B44.Unity.Tests` runs without an editor and proves something narrower.

## Running it

```powershell
../../scripts/sync-proving-dependencies.ps1
../../scripts/run-proving-tests.ps1 -UnityVersion <your editor version>
```

The first command puts the engine-free B44 assemblies under
`Assets/Plugins/B44/`; Unity does not restore NuGet, so nothing here compiles
until it has run. The second runs the PlayMode suite in batch mode and reads the
result file rather than trusting the editor's exit code, so "no verdict was ever
reached" is reported as its own outcome instead of as a failing test.

To work in the editor instead, run the sync and open this folder in Unity.

## What it proves

`B44BoundaryProbe` is a `MonoBehaviour`, deliberately. A static method called
from a test would show that the assemblies link, which the compile project
already covers; running from `Awake` in a live player loop is what shows the
boundary works in an assembled Unity application. In one pass it:

1. builds an engine-free `StructuredGameLogger` over the package's Unity
   routing, and confirms B44's own verbosity rules dropped a below-threshold
   event — the difference between B44's logger running and something forwarding
   a string;
2. sends `Application.unityVersion` across as a plain value and asserts it comes
   back inside a line `B44.Common` formatted, which then reaches `Debug.Log`;
3. resolves a save path from `Application.persistentDataPath` through
   `UnitySavePaths` and round-trips a `ProvingState` through B44's file-backed
   store.

`ProvingState` has no `UnityEngine` using, no `[Serializable]`, and is not a
`ScriptableObject`. That is the point: a Unity game's data is persisted and
restored by an engine-free capability that never learns Unity exists.

## What is not committed

`ProjectSettings/` and `Assets/Plugins/` are both generated, and both
deliberately absent from the repository.

Settings are left to the editor so that a consumer's Unity writes its own
defaults rather than inheriting a hand-written file nobody validated. The one
setting that matters is **API compatibility level: .NET Standard 2.1**, which is
Unity 6's default — if an editor defaults otherwise, set it, or `B44.Common.dll`
will not load.

`run-proving-tests.ps1` writes `ProjectSettings/ProjectVersion.txt` before each
run so the editor and the Hub agree on the version.
