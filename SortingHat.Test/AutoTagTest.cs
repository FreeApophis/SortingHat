using System;
using System.IO;
using Autofac;
using SortingHat.API.AutoTag;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Tags files with auto-tag masks through the real command line application, with real files in a temporary folder.
/// </summary>
public sealed class AutoTagTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Fact]
    public void GivenAMaskWithTheUpdateYearThenTheFileIsTaggedWithTheYearOfItsLastWrite()
    {
        var path = _hat.WriteFile("a.txt", "a");
        File.SetLastWriteTime(path, new DateTime(2003, 4, 5, 6, 7, 8, DateTimeKind.Local));

        Assert.Equal(0, _hat.Hat(["tag-files", ":updated:{UpdatedAt.Year}", path]));

        Assert.Equal(["a.txt"], _hat.FindFiles(":updated:2003"));
    }

    [Theory]
    [InlineData(0, "invoices")]
    [InlineData(1, "archive")]
    public void GivenAMaskWithAFolderCountedFromTheFileThenTheFileIsTaggedWithThatFolder(int index, string folder)
    {
        var path = FileIn(Path.Combine("archive", "invoices"), "a.txt");

        Assert.Equal(0, _hat.Hat(["tag-files", $":folder:{{Folder.FromFile.{index}}}", path]));

        Assert.Equal(["a.txt"], _hat.FindFiles($":folder:{folder}"));
    }

    [Fact]
    public void GivenAMaskWithAnUnknownVariableThenNothingIsTaggedAndTheVariableIsNamed()
    {
        var path = _hat.WriteFile("a.txt", "a");

        Assert.NotEqual(0, _hat.Hat(["tag-files", ":ok", ":Photos:{Camera.Mkae}", path]));

        Assert.Contains(_hat.Lines, line => line.Contains("{Camera.Mkae}"));
        Assert.Equal(0, _hat.Hat(["list-tags"]));
        Assert.Equal(["There are currently no tags in the database."], _hat.Lines);
    }

    [Fact]
    public void GivenFilesWithoutAValueForAVariableThenTheyAreSkippedWithOneSummaryAndTheOtherTagsStay()
    {
        using var hat = HatSandbox.WithPluginFixtures("Working");
        var first = hat.WriteFile("a.txt", "a");
        var second = hat.WriteFile("b.txt", "b");

        Assert.Equal(0, hat.Hat(["tag-files", ":document", ":Photos:{Camera.Make}", first, second]));

        Assert.Single(hat.Lines, line => line.Contains("':Photos:{Camera.Make}'") && line.Contains("2 files") && line.Contains("no value for {Camera.Make}"));
        Assert.Equal(["a.txt", "b.txt"], hat.FindFiles(":document"));
        Assert.Empty(hat.FindFiles(":Photos"));
    }

    [Fact]
    public void GivenAnAutoTagThatFailsThenTheFileKeepsItsOtherTagsAndTheFailureIsReported()
    {
        using var hat = new HatSandbox(builder => builder.RegisterInstance(new FixedAutoTag("Broken", AutoTagParameter.None, (_, _) => throw new IOException("the file is locked"))).As<IAutoTag>());
        var path = hat.WriteFile("a.txt", "a");

        Assert.Equal(0, hat.Hat(["tag-files", ":document", ":x:{Broken}", path]));

        Assert.Contains(hat.Lines, line => line.Contains("1 file") && line.Contains("{Broken} failed: the file is locked"));
        Assert.Equal(["a.txt"], hat.FindFiles(":document"));
    }

    [Theory]
    [InlineData("{CreatedAt.<Year|Month|Day|DayOfWeek|Hour|Minute|Second>}")]
    [InlineData("{Folder.FromRoot.<number>}")]
    [InlineData("{FileType.Category}")]
    public void GivenTheHelpThenItListsEveryVariableWithItsArguments(string variable)
    {
        using var hat = HatSandbox.WithPluginFixtures("Working");

        Assert.Equal(0, hat.Hat(["help"]));

        Assert.Contains(hat.Lines, line => line.Contains(variable));
    }

    public void Dispose()
        => _hat.Dispose();

    private string FileIn(string folder, string name)
    {
        var root = Path.GetDirectoryName(_hat.WriteFile("marker.txt", "marker")) ?? string.Empty;
        Directory.CreateDirectory(Path.Combine(root, folder));

        return _hat.WriteFile(Path.Combine(folder, name), name);
    }
}
