# B44.Unity

Unity-side adapters for B44 games. The engine-coupled counterpart to the
engine-free [`B44.Common`](https://github.com/DA-Sandman-Jr/B44.Common), and the
sibling of [`B44.Godot`](https://github.com/DA-Sandman-Jr/B44.Godot).

It converts Unity environment concerns into the representations B44 already
expects, and projects B44 results back into Unity. It contains no game logic.

## Installing

Add the package, then put the engine-free assemblies where Unity can see them.
Unity does not restore NuGet, so the second step is yours:

```jsonc
// Packages/manifest.json
{
  "dependencies": {
    "com.b44.unity": "https://github.com/DA-Sandman-Jr/B44.Unity.git?path=/com.b44.unity#v0.1.0"
  }
}
```

`B44.Common.dll` and the assemblies it references must be under `Assets/`
— through NuGetForUnity, or by copying them in. Take the **`netstandard2.1`**
assets: a `net8.0` assembly cannot be loaded by Unity at all. Version 0.11.2 is
the first that publishes a `netstandard2.1` target.

`scripts/sync-proving-dependencies.ps1` in this repository does exactly that
copy for the proving project, and derives the assembly list from NuGet's own
`netstandard2.1` resolution rather than hardcoding it. It is worth reading
before writing your own.

## What's in it

| Namespace | Type | Purpose |
|---|---|---|
| `B44.Unity.Diagnostics` | `UnityLoggerFactory` | Routes `StructuredGameLogger` events to `Debug.Log` / `LogWarning` / `LogError` |
| `B44.Unity.Diagnostics` | `UnityLogRouting` | The severity-to-channel rule, free of Unity types so it is testable without an editor |
| `B44.Unity.Persistence` | `UnitySavePaths` | Turns `Application.persistentDataPath` into the path B44's file-backed stores take |

`SavePaths.ResolveAppData` in B44.Common is the wrong answer under Unity on
every platform that is not desktop — it resolves the application-data folder,
which on Android and iOS is not where a player's save belongs and can come back
empty. `UnitySavePaths` is why that helper never needs to be reached for.

## Requirements

- Unity 6000.0 or newer, API compatibility level **.NET Standard 2.1**.
- `B44.Common` 0.11.2 or newer, `netstandard2.1` assets.

## Versioning

`B44.Unity`, `B44.Common`, `B44.Godot`, and `B44.Standards` each version
independently from their own repositories. This package's number tracks none of
them.

## License

None — all rights reserved. The source is public for reference; it is not
licensed for reuse. See [`LICENSE.md`](LICENSE.md).
