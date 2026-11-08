using System;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// The progress of commands that work through many files, through the real command line application.
/// A terminal shows a progress bar; when the output goes to a script, it stays plain lines.
/// </summary>
public sealed class ProgressTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Fact]
    public void GivenATerminalThenTagFilesShowsWhichFileItIsTagging()
    {
        var path = _hat.WriteFile("a.txt", "a");
        _hat.Terminal();

        Assert.Equal(0, _hat.Hat(["tag-files", ":x", path]));

        Assert.Contains(_hat.Lines, line => line.Contains("Tagging 1/1: a.txt"));
    }

    [Fact]
    public void GivenATerminalThenTagFilesStillReportsWhatItTagged()
    {
        var path = _hat.WriteFile("a.txt", "a");
        _hat.Terminal();

        _hat.Hat(["tag-files", ":x", path]);

        Assert.Contains(_hat.Lines, line => line.Contains($"File {path} queued with tag :x"));
    }

    [Fact]
    public void GivenATerminalThenUntagFilesShowsWhichFileItIsUntagging()
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        _hat.Terminal();

        Assert.Equal(0, _hat.Hat(["untag-files", ":x", path]));

        Assert.Contains(_hat.Lines, line => line.Contains("Untagging 1/1: a.txt"));
    }

    [Fact]
    public void GivenATerminalThenRepairRehashShowsWhichFileItIsHashing()
    {
        _hat.TaggedFile("a.txt", ":x");
        _hat.Terminal();

        Assert.Equal(0, _hat.Hat(["repair", "--rehash"]));

        Assert.Contains(_hat.Lines, line => line.Contains("Hashing 1/1: a.txt"));
    }

    [Fact]
    public void GivenOutputForAScriptThenTagFilesWritesOnlyWhatItTagged()
    {
        var path = _hat.WriteFile("a.txt", "a");

        Assert.Equal(0, _hat.Hat(["tag-files", ":x", path]));

        Assert.Equal([$"File {path} queued with tag :x"], _hat.Lines);
    }

    public void Dispose()
        => _hat.Dispose();
}
