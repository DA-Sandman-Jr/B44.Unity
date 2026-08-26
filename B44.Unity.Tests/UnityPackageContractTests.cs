using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Xunit;

namespace B44.Unity.Tests;

/// <summary>
/// The half of the boundary that only exists as files a Unity editor reads.
/// Unity resolves references from the assembly definition and the package
/// manifest, not from the MSBuild project, so an assertion about the compiled
/// assembly says nothing about what the editor will do.
/// </summary>
public sealed class UnityPackageContractTests
{
    private const string EngineIndependentAssemblyFile = "B44.Common.dll";

    private static readonly string RepositoryRoot =
        typeof(UnityPackageContractTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "B44UnityRepositoryRoot")
            .Value!;

    private static JsonElement ReadJson(params string[] relativePath)
    {
        string path = Path.Combine(new[] { RepositoryRoot }.Concat(relativePath).ToArray());
        Assert.True(File.Exists(path), $"Expected {path} to exist.");
        return JsonDocument.Parse(File.ReadAllText(path)).RootElement.Clone();
    }

    private static JsonElement AssemblyDefinition => ReadJson("com.b44.unity", "Runtime", "B44.Unity.asmdef");

    private static JsonElement PackageManifest => ReadJson("com.b44.unity", "package.json");

    [Fact]
    public void AssemblyDefinition_NamesTheAssemblyTheCompileProjectProduces()
    {
        // Unity compiles the package sources into an assembly named by the asmdef;
        // the compile project compiles the same files under an AssemblyName. If
        // the two disagree, the thing CI verified is not the thing Unity ships.
        Assert.Equal(
            typeof(B44.Unity.Diagnostics.UnityLogRouting).Assembly.GetName().Name,
            AssemblyDefinition.GetProperty("name").GetString());
    }

    [Fact]
    public void AssemblyDefinition_DeclaresTheEngineIndependentAssemblyExplicitly()
    {
        // Unity auto-references managed plugins into every assembly that does not
        // opt out, so this would compile with no declaration at all. Declaring it
        // makes the one dependency B44.Unity has visible in the editor's own
        // graph, and turns a missing B44.Common into a named error rather than a
        // wall of undefined symbols.
        Assert.True(AssemblyDefinition.GetProperty("overrideReferences").GetBoolean());

        string[] precompiled = AssemblyDefinition.GetProperty("precompiledReferences")
            .EnumerateArray()
            .Select(entry => entry.GetString() ?? string.Empty)
            .ToArray();

        Assert.Equal(new[] { EngineIndependentAssemblyFile }, precompiled);
    }

    [Fact]
    public void AssemblyDefinition_ReferencesNoOtherAssemblyDefinition()
    {
        // Nothing else in a Unity project may be pulled below this boundary. The
        // day that changes it should be a deliberate edit, not a side effect of
        // adding a using directive in the editor.
        Assert.Empty(AssemblyDefinition.GetProperty("references").EnumerateArray());
    }

    [Fact]
    public void AssemblyDefinition_IsRuntimeOnly()
    {
        // No platform include list: the boundary is available everywhere the game
        // runs. An editor-only or platform-restricted integration surface would be
        // a different, larger decision than this package has made.
        Assert.Empty(AssemblyDefinition.GetProperty("includePlatforms").EnumerateArray());
        Assert.Empty(AssemblyDefinition.GetProperty("excludePlatforms").EnumerateArray());
    }

    [Fact]
    public void PackageManifest_DeclaresNoUnityPackageDependencies()
    {
        // B44.Unity must stay omittable. A dependency here would make every
        // consumer of the package acquire something else, and the engine-free
        // packages it bridges to are not UPM packages in the first place.
        Assert.False(PackageManifest.TryGetProperty("dependencies", out JsonElement dependencies) &&
                     dependencies.EnumerateObject().Any());
    }

    [Fact]
    public void PackageManifest_DeclaresTheMinimumEditorVersion()
    {
        string? minimum = PackageManifest.GetProperty("unity").GetString();

        Assert.False(string.IsNullOrWhiteSpace(minimum));
        Assert.Matches(@"^\d{4}\.\d+$", minimum);
    }

    [Fact]
    public void CompileProject_TakesTheUnityRuntimeBaselineFromTheSingleDeclaration()
    {
        // The current Unity runtime generation is a packaging fact with a
        // sunset, not an architectural one. It is written down once, in
        // Directory.Build.props, so retargeting is an edit in a single place —
        // and this fails if the compile project starts restating it, which is
        // how one place quietly becomes three.
        string project = File.ReadAllText(
            Path.Combine(RepositoryRoot, "B44.Unity.Compile", "B44.Unity.Compile.csproj"));

        Assert.Contains("$(B44UnityRuntimeTargetFramework)", project, StringComparison.Ordinal);
        Assert.Contains("$(B44UnityFutureTargetFramework)", project, StringComparison.Ordinal);
        Assert.Contains("<LangVersion>$(B44UnityLangVersion)</LangVersion>", project, StringComparison.Ordinal);
    }

    [Fact]
    public void CompileProject_BuildsAgainstBothUnityRuntimeGenerations()
    {
        // Proving the sources against the modern .NET target alongside the
        // current one is what keeps the outgoing Unity runtime from becoming
        // the ceiling these sources are written to. Losing the second target
        // would still build, still pass everything else here, and quietly turn
        // the eventual retarget back into a port.
        string properties = File.ReadAllText(
            Path.Combine(RepositoryRoot, "Directory.Build.props"));

        Assert.Contains(
            "<B44UnityFutureTargetFramework>net",
            properties,
            StringComparison.Ordinal);
    }
}
