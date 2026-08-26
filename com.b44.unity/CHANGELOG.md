# Changelog

All notable changes to `com.b44.unity` are recorded here. This package versions
independently of `B44.Common`, `B44.Godot`, and `B44.Standards`.

## [0.1.0] — unreleased

Initial Unity integration boundary.

### Added

- `UnityLoggerFactory` and `UnityLogRouting` — B44 structured log events routed
  to Unity's output channels, with the severity rule kept free of Unity types so
  it is testable without an editor.
- `UnitySavePaths` — `Application.persistentDataPath` converted into the path
  B44.Common's file-backed stores accept.

### Requires

- Unity 6000.0 or newer, API compatibility level .NET Standard 2.1.
- `B44.Common` 0.11.2 or newer. Earlier versions publish only a `net8.0` target,
  which Unity cannot load.
