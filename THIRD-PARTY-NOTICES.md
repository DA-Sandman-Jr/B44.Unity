# Third-Party Notices

This file exists because B44 policy requires it in any repository that may
carry vendored, ported, or converted third-party code. It is the single place
that surface is tracked.

## Currently: none

No third-party source is vendored, ported, or converted into this repository.
Every file here is B44's own work.

`UnityEngine.Modules` is referenced by `B44.Unity.Compile` with
`ExcludeAssets="all"` and `PrivateAssets="all"`: its assemblies are used as
compile-time references only, are never copied to output, and are not
redistributed, so this is not a vendoring event and creates no obligation here.
A consuming game ships Unity's runtime under its own Unity licence, as it would
with or without this package.

The assemblies `scripts/sync-proving-dependencies.ps1` copies into the proving
project — `B44.Common` and the `System.Text.Json` closure — are resolved from
NuGet at run time into a git-ignored directory. Nothing is committed here and
nothing is redistributed from this repository. Microsoft's packages are
MIT-licensed; a consuming game that ships them carries that notice.

## If that changes

Adding vendored, ported, or converted third-party code means adding an entry
below with the upstream name, version or commit, licence, and what was taken.
Note that converting or hand-porting does **not** shed the upstream licence — a
port is a derivative work and the attribution obligation follows it. See the
isolation and clean-room rules in the B44 organization guidance before starting.
