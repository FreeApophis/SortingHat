using System;
using System.IO;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Works with files through the real command line application, with a real SQLite database and real files in a temporary folder.
/// </summary>
public sealed class FileCommandTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Theory]
    [InlineData("tag-files")]
    [InlineData("untag-files")]
    [InlineData("file-info")]
    public void GivenTheHelpOfACommandForFilesThenItOffersRecursionButNoUnusedOptions(string command)
    {
        Assert.Equal(0, _hat.Hat([command, "--help"]));

        Assert.Contains(_hat.Lines, line => line.Contains("-r, --recursive"));
        Assert.DoesNotContain(_hat.Lines, line => line.Contains("--dry-run") || line.Contains("--open") || line.Contains("--verbose"));
    }

    [Fact]
    public void GivenTheHelpOfFindFilesThenItOffersToOpenTheFiles()
    {
        Assert.Equal(0, _hat.Hat(["find-files", "--help"]));

        Assert.Contains(_hat.Lines, line => line.Contains("--open"));
        Assert.DoesNotContain(_hat.Lines, line => line.Contains("--recursive") || line.Contains("--dry-run"));
    }

    [Theory]
    [InlineData("copy-files")]
    [InlineData("move-files")]
    public void GivenTheHelpOfCopyOrMoveThenItDoesNotOfferADryRunThatDoesNothing(string command)
    {
        Assert.Equal(0, _hat.Hat([command, "--help"]));

        Assert.Contains(_hat.Lines, line => line.Contains("<search>"));
        Assert.Contains(_hat.Lines, line => line.Contains("<destination>"));
        Assert.DoesNotContain(_hat.Lines, line => line.Contains("--dry-run"));
    }

    [Theory]
    [InlineData("tag-files")]
    [InlineData("untag-files")]
    [InlineData("file-info")]
    [InlineData("find-files")]
    public void GivenNoArgumentsThenTheCommandAsksForThem(string command)
        => Assert.NotEqual(0, _hat.Hat([command]));

    [Fact]
    public void GivenAQueryInSeparateWordsThenFindFilesJoinsThem()
    {
        _hat.TaggedFile("a.txt", ":x", ":y");
        _hat.TaggedFile("b.txt", ":x");

        Assert.Equal(0, _hat.Hat(["ff", ":x", "and", "not", ":y"]));
        Assert.Contains(_hat.Lines, line => line.TrimEnd().EndsWith("b.txt"));
        Assert.DoesNotContain(_hat.Lines, line => line.TrimEnd().EndsWith("a.txt"));
    }

    [Fact]
    public void GivenATagThenUntagFilesRemovesItFromTheFile()
    {
        var path = _hat.TaggedFile("a.txt", ":x", ":y");

        Assert.Equal(0, _hat.Hat(["untag-files", ":x", path]));

        Assert.Empty(_hat.FindFiles(":x"));
        Assert.Equal(["a.txt"], _hat.FindFiles(":y"));
    }

    [Fact]
    public void GivenASearchAndADestinationThenCopyFilesCopiesTheFiles()
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        var destination = Destination(path);

        Assert.Equal(0, _hat.Hat(["cp", ":x", destination]));

        Assert.True(File.Exists(Path.Combine(destination, "a.txt")));
        Assert.True(File.Exists(path));
    }

    [Fact]
    public void GivenASearchAndADestinationThenMoveFilesMovesTheFiles()
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        var destination = Destination(path);

        Assert.Equal(0, _hat.Hat(["mv", ":x", destination]));

        Assert.True(File.Exists(Path.Combine(destination, "a.txt")));
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void GivenOnlyASearchThenCopyFilesAsksForTheDestination()
    {
        Assert.NotEqual(0, _hat.Hat(["copy-files", ":x"]));
        Assert.Contains(_hat.Lines, line => line.Contains("destination"));
    }

    [Fact]
    public void GivenATaggedFileThenFileInfoShowsItsTags()
    {
        var path = _hat.TaggedFile("a.txt", ":tax:2018");

        Assert.Equal(0, _hat.Hat(["info", path]));
        Assert.Contains("Tag: :tax:2018", _hat.Lines);
    }

    [Fact]
    public void GivenTwoFilesWithTheSameContentThenDuplicateListsThem()
    {
        var first = _hat.TaggedFile("a.txt");
        var second = Path.Combine(Path.GetDirectoryName(first) ?? string.Empty, "copy-of-a.txt");
        File.Copy(first, second);
        _hat.Hat(["tag-files", second]);

        Assert.Equal(0, _hat.Hat(["d"]));
        Assert.Contains("--- Duplicate ---", _hat.Lines);
        Assert.Contains(_hat.Lines, line => line.Contains("copy-of-a.txt"));
    }

    public void Dispose()
        => _hat.Dispose();

    private static string Destination(string file)
        => Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(file) ?? string.Empty, "destination")).FullName;
}
