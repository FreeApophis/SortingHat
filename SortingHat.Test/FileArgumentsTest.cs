using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Files, folders and file patterns on the command line, through the real command line application with real folders.
/// The folders: scans/a.pdf, scans/b.jpg, scans/2018/c.pdf and other/d.pdf.
/// </summary>
public sealed class FileArgumentsTest : IDisposable
{
    private readonly HatSandbox _hat = new();
    private readonly string _a;
    private readonly string _b;
    private readonly string _c;
    private readonly string _scans;
    private readonly List<string> _links = [];

    public FileArgumentsTest()
    {
        _a = _hat.WriteFile(Path.Combine("scans", "a.pdf"), "a");
        _b = _hat.WriteFile(Path.Combine("scans", "b.jpg"), "b");
        _c = _hat.WriteFile(Path.Combine("scans", "2018", "c.pdf"), "c");
        _hat.WriteFile(Path.Combine("other", "d.pdf"), "d");
        _scans = Path.GetDirectoryName(_a) ?? throw new InvalidOperationException("a.pdf has no folder");
    }

    [Fact]
    public void GivenAPatternThenItMatchesTheFilesInItsFolder()
    {
        Assert.Equal(0, _hat.Hat(["file-info", Path.Combine(_scans, "*.pdf")]));

        Assert.Equal([_a], ListedFiles());
    }

    [Fact]
    public void GivenAPatternInAnotherFolderWithRecursionThenItSearchesThatFolderAndItsSubfolders()
    {
        Assert.Equal(0, _hat.Hat(["file-info", "-r", Path.Combine(_scans, "*.pdf")]));

        Assert.Equal([_c, _a], ListedFiles());
    }

    [Fact]
    public void GivenAFolderWithRecursionThenItMeansEveryFileInIt()
    {
        Assert.Equal(0, _hat.Hat(["file-info", "-r", _scans]));

        Assert.Equal([_c, _a, _b], ListedFiles());
    }

    [Fact]
    public void GivenAFolderWithoutRecursionThenHatAsksForRecursionAndDoesNothing()
    {
        Assert.NotEqual(0, _hat.Hat(["file-info", _scans]));

        Assert.Contains($"'{_scans}' is a folder, use -r for everything in it or '{Path.Combine(_scans, "*")}' for its files.", _hat.Lines);
        Assert.Empty(ListedFiles());
    }

    [Fact]
    public void GivenAPatternThatMatchesNothingThenNothingIsTagged()
    {
        var nothing = Path.Combine(_scans, "*.doc");

        Assert.NotEqual(0, _hat.Hat(["tag-files", ":x", _a, nothing]));

        Assert.Contains($"'{nothing}' matches no file.", _hat.Lines);
        Assert.Contains("Nothing was tagged.", _hat.Lines);
        Assert.Empty(_hat.FindFiles(":x"));
    }

    [Fact]
    public void GivenAFileThatDoesNotExistThenHatSaysSo()
    {
        var missing = Path.Combine(_scans, "missing.pdf");

        Assert.NotEqual(0, _hat.Hat(["file-info", missing]));

        Assert.Contains($"'{missing}' is no file or folder.", _hat.Lines);
    }

    [Fact]
    public void GivenAPatternInAFolderThatDoesNotExistThenHatSaysSo()
    {
        var missing = Path.Combine(_scans, "missing", "*.pdf");

        Assert.NotEqual(0, _hat.Hat(["file-info", missing]));

        Assert.Contains($"'{missing}' matches no file, there is no folder '{Path.Combine(_scans, "missing")}'.", _hat.Lines);
    }

    [Fact]
    public void GivenAWildcardInAFolderNameThenHatSaysWhereWildcardsGo()
    {
        var pattern = Path.Combine(_scans, "20*", "c.pdf");

        Assert.NotEqual(0, _hat.Hat(["file-info", pattern]));

        Assert.Contains($"'{pattern}': wildcards are only allowed in the file name.", _hat.Lines);
    }

    [Fact]
    public void GivenTheSameFileTwiceThenItIsUsedOnce()
    {
        Assert.Equal(0, _hat.Hat(["file-info", _a, Path.Combine(_scans, "*.pdf")]));

        Assert.Equal([_a], ListedFiles());
    }

    [Fact]
    public void GivenAPathTypedInAnotherCaseThenHatUsesTheRealCase()
    {
        // only Windows (and macOS) find a file under another case
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        Assert.Equal(0, _hat.Hat(["file-info", _a.ToUpperInvariant()]));

        Assert.Equal([_a], ListedFiles());
    }

    [Fact]
    public void GivenALinkBackToAParentFolderThenRecursionDoesNotFollowIt()
    {
        FolderLink(Path.Combine(_scans, "2018", "loop"), _scans);

        Assert.Equal(0, _hat.Hat(["file-info", "-r", Path.Combine(_scans, "*.pdf")]));

        Assert.Equal([_c, _a], ListedFiles());
    }

    public void Dispose()
    {
        // only the link itself: deleting the sandbox recursively is refused on a junction
        _links.ForEach(link => Directory.Delete(link));
        _hat.Dispose();
    }

    /// <summary>A junction on Windows, which needs no rights, unlike a symbolic link; a symbolic link elsewhere.</summary>
    private void FolderLink(string link, string target)
    {
        if (OperatingSystem.IsWindows())
        {
            using var mklink = Process.Start(new ProcessStartInfo("cmd.exe", ["/c", "mklink", "/J", link, target]) { UseShellExecute = false, RedirectStandardOutput = true })
                ?? throw new InvalidOperationException("cmd.exe did not start");
            mklink.WaitForExit();
        }
        else
        {
            Directory.CreateSymbolicLink(link, target);
        }

        _links.Add(link);
        Assert.True(Directory.Exists(Path.Combine(link, "2018")), "the link leads back to the folder");
    }

    private IReadOnlyList<string> ListedFiles()
        => [.. _hat.Lines.Where(line => line.StartsWith("File: ")).Select(line => line["File: ".Length..])];
}
