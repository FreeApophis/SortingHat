using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Which file events the commands raise, and when, through the real command line application with a handler like a plugin's.
/// Every event comes after hat stored the change; a dry run raises none.
/// </summary>
public sealed class FileEventsTest : IDisposable
{
    private readonly HatSandbox _hat = new(builder => builder.RegisterType<FileEventReporter>().AsImplementedInterfaces());

    [Fact]
    public void GivenANewFileThenTagFilesRaisesAddedAndThenTagged()
    {
        var path = _hat.WriteFile("a.txt", "a");

        _hat.Hat(["tag-files", ":x", path]);

        Assert.Equal(["heard: added a.txt", "heard: tagged a.txt with :x"], Heard());
    }

    [Fact]
    public void GivenANewFileWithoutTagsThenTagFilesRaisesAdded()
    {
        var path = _hat.WriteFile("a.txt", "a");

        _hat.Hat(["tag-files", path]);

        Assert.Equal(["heard: added a.txt"], Heard());
    }

    [Fact]
    public void GivenAKnownFileThenTagFilesRaisesOnlyTagged()
    {
        var path = OldFile("a.txt", "a");
        _hat.Hat(["tag-files", ":x", path]);

        _hat.Hat(["tag-files", ":y", path]);

        Assert.Equal(["heard: tagged a.txt with :y"], Heard());
    }

    [Fact]
    public void GivenTheSameContentAtANewPathThenTagFilesRaisesAdded()
    {
        _hat.Hat(["tag-files", _hat.WriteFile("a.txt", "same")]);

        _hat.Hat(["tag-files", _hat.WriteFile("b.txt", "same")]);

        Assert.Equal(["heard: added b.txt"], Heard());
    }

    [Fact]
    public void GivenAnEditedFileThenTagFilesRaisesChangedAndThenTagged()
    {
        var path = OldFile("a.txt", "content");
        _hat.Hat(["tag-files", ":x", path]);
        File.WriteAllText(path, "edited and longer content");

        _hat.Hat(["tag-files", ":y", path]);

        Assert.Equal(2, Heard().Count);
        Assert.StartsWith("heard: changed a.txt from ", Heard()[0]);
        Assert.Equal("heard: tagged a.txt with :y", Heard()[1]);
    }

    [Fact]
    public void GivenAHiddenEditThenRepairRehashRaisesChanged()
    {
        var path = HiddenEdit("a.txt");

        _hat.Hat(["repair", "--rehash"]);

        Assert.Single(Heard(), line => line.StartsWith("heard: changed a.txt from "));
    }

    [Fact]
    public void GivenADryRunThenRepairRehashRaisesNothing()
    {
        HiddenEdit("a.txt");

        _hat.Hat(["repair", "--rehash", "--dry-run"]);

        Assert.Empty(Heard());
    }

    [Fact]
    public void GivenTagsThenUntagFilesRaisesUntaggedOncePerFile()
    {
        var a = _hat.TaggedFile("a.txt", ":x", ":y");
        var b = _hat.TaggedFile("b.txt", ":x");

        _hat.Hat(["untag-files", ":x", ":y", a, b]);

        Assert.Equal(["heard: untagged a.txt from :x, :y", "heard: untagged b.txt from :x, :y"], Heard());
    }

    [Fact]
    public void GivenMoveFilesThenItRaisesMovedWithTheDestination()
    {
        _hat.TaggedFile("a.txt", ":x");
        var destination = _hat.CreateFolder("moved");

        Assert.Equal(0, _hat.Hat(["move-files", ":x", destination]));

        Assert.Equal([$"heard: moved a.txt to {Path.Combine(destination, "a.txt")}"], Heard());
    }

    [Fact]
    public void GivenADryRunThenMoveFilesRaisesNothing()
    {
        _hat.TaggedFile("a.txt", ":x");

        _hat.Hat(["move-files", ":x", _hat.CreateFolder("moved"), "--dry-run"]);

        Assert.Empty(Heard());
    }

    [Fact]
    public void GivenCopyFilesThenItRaisesNothing()
    {
        _hat.TaggedFile("a.txt", ":x");

        _hat.Hat(["copy-files", ":x", _hat.CreateFolder("copies")]);

        Assert.Empty(Heard());
    }

    public void Dispose()
        => _hat.Dispose();

    private IReadOnlyList<string> Heard()
        => [.. _hat.Lines.Where(line => line.StartsWith("heard:"))];

    /// <summary>A file last written an hour ago, so its fingerprint is trustworthy right away.</summary>
    private string OldFile(string name, string content)
    {
        var path = _hat.WriteFile(name, content);
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddHours(-1));

        return path;
    }

    /// <summary>A tagged file whose content changes while its size and timestamp stay, which only repair --rehash finds.</summary>
    private string HiddenEdit(string name)
    {
        var path = OldFile(name, "content");
        _hat.Hat(["tag-files", ":x", path]);

        var timestamp = File.GetLastWriteTimeUtc(path);
        File.WriteAllText(path, "CONTENT");
        File.SetLastWriteTimeUtc(path, timestamp);

        return path;
    }
}
