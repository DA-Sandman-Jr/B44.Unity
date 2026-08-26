using System;
using System.IO;
using B44.Unity.Persistence;
using Xunit;

namespace B44.Unity.Tests;

/// <summary>
/// The path rule, with the persistent data directory supplied by the test.
/// <c>UnitySavePaths.Resolve</c> itself reads <c>Application.persistentDataPath</c>
/// and is therefore only exercised inside Unity.
/// </summary>
public sealed class UnitySavePathsTests
{
    private const string Root = "/tmp/persistent";

    [Fact]
    public void Combine_PlacesTheSaveUnderThePersistentDataDirectory()
    {
        string combined = UnitySavePaths.Combine(Root, "campaign.json");

        Assert.Equal(Path.Combine(Root, "campaign.json"), combined);
    }

    [Fact]
    public void Combine_KeepsNestedDirectories()
    {
        string combined = UnitySavePaths.Combine(Root, Path.Combine("saves", "slot1.json"));

        Assert.Equal(Path.Combine(Root, "saves", "slot1.json"), combined);
    }

    [Fact]
    public void Combine_RejectsARootedSavePath()
    {
        // Path.Combine would discard the persistent data directory entirely and
        // return the rooted path, putting the save somewhere the platform may not
        // let the game write — with no error until the first save fails.
        string rooted = Path.Combine(Path.GetTempPath(), "elsewhere.json");

        ArgumentException error = Assert.Throws<ArgumentException>(
            () => UnitySavePaths.Combine(Root, rooted));

        Assert.Equal("relativePath", error.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Combine_RejectsAnAbsentPersistentDataDirectory(string? root)
    {
        // Application.persistentDataPath is empty before the player has started,
        // and Path.Combine would happily return a relative path from it.
        ArgumentException error = Assert.Throws<ArgumentException>(
            () => UnitySavePaths.Combine(root!, "campaign.json"));

        Assert.Equal("persistentDataRoot", error.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Combine_RejectsAnAbsentSavePath(string? relativePath)
    {
        ArgumentException error = Assert.Throws<ArgumentException>(
            () => UnitySavePaths.Combine(Root, relativePath!));

        Assert.Equal("relativePath", error.ParamName);
    }
}
