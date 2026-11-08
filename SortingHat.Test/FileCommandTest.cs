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
    public void GivenTheHelpOfCopyOrMoveThenItOffersADryRun(string command)
    {
        Assert.Equal(0, _hat.Hat([command, "--help"]));

        Assert.Contains(_hat.Lines, line => line.Contains("<search>"));
        Assert.Contains(_hat.Lines, line => line.Contains("<destination>"));
        Assert.Contains(_hat.Lines, line => line.Contains("-n, --dry-run"));
    }

    [Theory]
    [InlineData("--dry-run")]
    [InlineData("-n")]
    public void GivenADryRunThenCopyFilesShowsWhatItWouldCopyButCopiesNothing(string dryRun)
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        var destination = Destination(path);

        Assert.Equal(0, _hat.Hat(["cp", ":x", destination, dryRun]));

        Assert.Empty(Directory.GetFiles(destination));
        Assert.Contains(_hat.Lines, line => line.Contains(path) && line.Contains(Path.Combine(destination, "a.txt")));
        Assert.Contains("Dry run: nothing was copied.", _hat.Lines);
    }

    [Fact]
    public void GivenADryRunThenMoveFilesMovesNothingAndKeepsThePathsInTheDatabase()
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        var destination = Destination(path);

        Assert.Equal(0, _hat.Hat(["mv", ":x", destination, "--dry-run"]));
        Assert.Contains("Dry run: nothing was moved.", _hat.Lines);

        Assert.True(File.Exists(path));
        Assert.Empty(Directory.GetFiles(destination));
        Assert.Equal(["a.txt"], _hat.FindFiles(":x"));
    }

    [Fact]
    public void GivenADryRunWithAnExistingTargetThenItReportsTheSkipAndPredictsTheFailure()
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        var destination = Destination(path);
        File.WriteAllText(Path.Combine(destination, "a.txt"), "keep me");

        Assert.NotEqual(0, _hat.Hat(["cp", ":x", destination, "--dry-run"]));

        Assert.Contains(_hat.Lines, line => line.Contains("skipped") && line.Contains(path));
        Assert.Equal("keep me", File.ReadAllText(Path.Combine(destination, "a.txt")));
    }

    [Fact]
    public void GivenADryRunWithAMissingDestinationThenItReportsItAndCreatesNothing()
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        var missing = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, "missing");

        Assert.NotEqual(0, _hat.Hat(["cp", ":x", missing, "-n"]));

        Assert.Contains(_hat.Lines, line => line.Contains("does not exist"));
        Assert.False(Directory.Exists(missing));
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
    public void GivenAMovedFileThenFindFilesShowsItsNewPath()
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        var destination = Destination(path);

        _hat.Hat(["mv", ":x", destination]);

        _hat.Hat(["ff", ":x"]);
        Assert.Contains(_hat.Lines, line => line.TrimEnd().EndsWith(Path.Combine(destination, "a.txt")));
        Assert.DoesNotContain(_hat.Lines, line => line.TrimEnd().EndsWith(path));
    }

    [Fact]
    public void GivenAMovedFileThenCopyFilesCopiesItFromItsNewPath()
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        var destination = Destination(path);
        var copies = Directory.CreateDirectory(Path.Combine(destination, "copies")).FullName;

        _hat.Hat(["mv", ":x", destination]);

        Assert.Equal(0, _hat.Hat(["cp", ":x", copies]));
        Assert.True(File.Exists(Path.Combine(copies, "a.txt")));
    }

    [Fact]
    public void GivenAMovedFileThenRepairDoesNotReportItMissing()
    {
        var path = _hat.TaggedFile("a.txt", ":x");

        _hat.Hat(["mv", ":x", Destination(path)]);

        _hat.Hat(["repair"]);
        Assert.DoesNotContain(_hat.Lines, line => line.Contains("missing"));
    }

    [Fact]
    public void GivenTwoCopiesOfAFileThenMoveFilesMovesOnlyTheChosenOneInTheDatabase()
    {
        var first = _hat.TaggedFile("a.txt", ":x");
        var second = Path.Combine(Path.GetDirectoryName(first) ?? string.Empty, "copy-of-a.txt");
        File.Copy(first, second);
        _hat.Hat(["tag-files", ":x", second]);
        var destination = Destination(first);
        _hat.Press(ConsoleKey.Enter);

        Assert.Equal(0, _hat.Hat(["mv", ":x", destination]));

        _hat.Hat(["ff", ":x"]);
        Assert.Contains(_hat.Lines, line => line.TrimEnd().EndsWith(Path.Combine(destination, "a.txt")));
        Assert.Contains(_hat.Lines, line => line.TrimEnd().EndsWith(second));
        Assert.DoesNotContain(_hat.Lines, line => line.TrimEnd().EndsWith(first));
    }

    [Fact]
    public void GivenAMissingDestinationFolderThenNothingIsCopiedAndItIsReported()
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        var missing = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, "missing");

        Assert.NotEqual(0, _hat.Hat(["cp", ":x", missing]));

        Assert.Contains(_hat.Lines, line => line.Contains(missing) && line.Contains("does not exist"));
        Assert.False(Directory.Exists(missing));
    }

    [Fact]
    public void GivenAnExistingTargetThenThatFileIsSkippedAndTheOthersAreCopied()
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        _hat.TaggedFile("b.txt", ":x");
        var destination = Destination(path);
        File.WriteAllText(Path.Combine(destination, "a.txt"), "keep me");

        Assert.NotEqual(0, _hat.Hat(["cp", ":x", destination]));

        Assert.Equal("keep me", File.ReadAllText(Path.Combine(destination, "a.txt")));
        Assert.True(File.Exists(Path.Combine(destination, "b.txt")));
        Assert.Contains(_hat.Lines, line => line.Contains("skipped") && line.Contains(path));
    }

    [Fact]
    public void GivenTwoFilesWithTheSameNameThenOnlyOneIsCopied()
    {
        var first = TaggedFileIn("one", "scan.txt", "first scan", ":x");
        var second = TaggedFileIn("two", "scan.txt", "second scan", ":x");
        var destination = Destination(first);

        Assert.NotEqual(0, _hat.Hat(["cp", ":x", destination]));

        Assert.Single(Directory.GetFiles(destination));
        Assert.Contains(_hat.Lines, line => line.Contains("skipped") && (line.Contains(first) || line.Contains(second)));
    }

    [Fact]
    public void GivenASourceDeletedOutsideHatThenItIsSkippedAndTheOthersAreCopied()
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        _hat.TaggedFile("b.txt", ":x");
        var destination = Destination(path);
        File.Delete(path);

        Assert.NotEqual(0, _hat.Hat(["cp", ":x", destination]));

        Assert.True(File.Exists(Path.Combine(destination, "b.txt")));
        Assert.Contains(_hat.Lines, line => line.Contains("skipped") && line.Contains(path));
    }

    [Fact]
    public void GivenCopiesOfAFileOfWhichOnlyOneExistsThenItIsTakenWithoutAsking()
    {
        var first = _hat.TaggedFile("a.txt", ":x");
        var second = Path.Combine(Path.GetDirectoryName(first) ?? string.Empty, "copy-of-a.txt");
        File.Copy(first, second);
        _hat.Hat(["tag-files", ":x", second]);
        var destination = Destination(first);
        File.Delete(first);

        Assert.Equal(0, _hat.Hat(["cp", ":x", destination]));

        Assert.True(File.Exists(Path.Combine(destination, "copy-of-a.txt")));
        Assert.DoesNotContain(_hat.Lines, line => line.Contains("several copies"));
    }

    [Fact]
    public void GivenTwoCopiesOfAFileThenTheArrowKeysChooseTheOneToCopy()
    {
        var first = _hat.TaggedFile("a.txt", ":x");
        var second = Path.Combine(Path.GetDirectoryName(first) ?? string.Empty, "copy-of-a.txt");
        File.Copy(first, second);
        _hat.Hat(["tag-files", ":x", second]);
        var destination = Destination(first);
        _hat.Press(ConsoleKey.DownArrow, ConsoleKey.Enter);

        Assert.Equal(0, _hat.Hat(["cp", ":x", destination]));

        Assert.Equal([Path.Combine(destination, "copy-of-a.txt")], Directory.GetFiles(destination));
    }

    [Fact]
    public void GivenTwoCopiesOfAFileWithoutAnInteractiveTerminalThenOnlyThatFileIsSkipped()
    {
        var first = _hat.TaggedFile("a.txt", ":x");
        var second = Path.Combine(Path.GetDirectoryName(first) ?? string.Empty, "copy-of-a.txt");
        File.Copy(first, second);
        _hat.Hat(["tag-files", ":x", second]);
        _hat.TaggedFile("b.txt", ":x");
        var destination = Destination(first);

        Assert.NotEqual(0, _hat.Hat(["cp", ":x", destination]));

        Assert.Equal([Path.Combine(destination, "b.txt")], Directory.GetFiles(destination));
        Assert.Contains(_hat.Lines, line => line.Contains("several copies") && line.Contains("interactive terminal"));
    }

    [Fact]
    public void GivenAnExistingTargetThenMoveFilesLeavesTheFileAndItsPathAlone()
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        var destination = Destination(path);
        File.WriteAllText(Path.Combine(destination, "a.txt"), "keep me");

        Assert.NotEqual(0, _hat.Hat(["mv", ":x", destination]));

        Assert.True(File.Exists(path));
        Assert.Equal(["a.txt"], _hat.FindFiles(":x"));
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

    [Fact]
    public void GivenADuplicateWhosePathIsGoneThenDuplicateMarksItMissing()
    {
        var first = _hat.TaggedFile("a.txt");
        var second = Path.Combine(Path.GetDirectoryName(first) ?? string.Empty, "copy-of-a.txt");
        File.Copy(first, second);
        _hat.Hat(["tag-files", second]);
        File.Delete(second);

        Assert.Equal(0, _hat.Hat(["duplicate"]));
        Assert.Contains(_hat.Rows, row => row.StartsWith("missing|") && row.EndsWith("|copy-of-a.txt"));
    }

    [Fact]
    public void GivenNoDuplicatesThenDuplicateSaysSo()
    {
        _hat.TaggedFile("a.txt");

        Assert.Equal(0, _hat.Hat(["duplicate"]));
        Assert.Equal(["There are no duplicates."], _hat.Lines);
    }

    [Fact]
    public void GivenAFileNameWithBracketsThenFindFilesShowsItAsItIs()
    {
        _hat.TaggedFile("scan [1].txt", ":x");

        Assert.Equal(["scan [1].txt"], _hat.FindFiles(":x"));
    }

    public void Dispose()
        => _hat.Dispose();

    private static string Destination(string file)
        => Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(file) ?? string.Empty, "destination")).FullName;

    /// <summary>A file in a subfolder of the sandbox, so two files can have the same name.</summary>
    private string TaggedFileIn(string folder, string name, string content, string tag)
    {
        var root = Path.GetDirectoryName(_hat.TaggedFile($"{folder}-marker.txt")) ?? string.Empty;
        var path = Path.Combine(Directory.CreateDirectory(Path.Combine(root, folder)).FullName, name);
        File.WriteAllText(path, content);
        _hat.Hat(["tag-files", tag, path]);

        return path;
    }
}
