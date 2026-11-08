using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// What file-info tells about a path, through the real command line application with a real SQLite database and real files.
/// hat knows a file by its content, so file-info also finds content that is indexed under another path.
/// </summary>
public sealed class FileInfoTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Fact]
    public void GivenAnIndexedFileThenFileInfoShowsItsHashTagsAndPaths()
    {
        var path = _hat.TaggedFile("a.txt", ":tax:2018");

        Assert.Equal(0, _hat.Hat(["file-info", path]));

        Assert.Contains(_hat.Lines, line => line.StartsWith("File Hash: sha256:"));
        Assert.Contains("Tag: :tax:2018", _hat.Lines);
        Assert.Contains($"Path: {path}", _hat.Lines);
    }

    [Fact]
    public void GivenAFileWithSeveralNestedTagsThenFileInfoShowsAllOfThemInTreeOrder()
    {
        var path = _hat.TaggedFile("a.txt", ":tax:2018", ":Bank", ":a:b:c");

        Assert.Equal(0, _hat.Hat(["file-info", path]));

        Assert.Equal(["Tag: :a:b:c", "Tag: :Bank", "Tag: :tax:2018"], _hat.Lines.Where(line => line.StartsWith("Tag:")));
    }

    [Fact]
    public void GivenAFileThatIsNotIndexedThenFileInfoSaysSo()
    {
        var path = _hat.WriteFile("a.txt", "never tagged");

        Assert.Equal(0, _hat.Hat(["file-info", path]));

        Assert.Contains("File not in index!", _hat.Lines);
        Assert.DoesNotContain(_hat.Lines, line => line.StartsWith("File Hash:"));
    }

    [Fact]
    public void GivenACopyOfAnIndexedFileThenFileInfoShowsWhereItsContentIsIndexed()
    {
        var original = _hat.TaggedFile("a.txt", ":x");
        var copy = _hat.WriteFile("copy-of-a.txt", File.ReadAllText(original));

        Assert.Equal(0, _hat.Hat(["file-info", copy]));

        Assert.Contains(_hat.Lines, line => line.Contains("not indexed at this path"));
        Assert.Contains($"Path: {original}", _hat.Lines);
        Assert.Contains("Tag: :x", _hat.Lines);
    }

    [Fact]
    public void GivenAFileEditedSinceItWasIndexedThenFileInfoSaysItChanged()
    {
        var path = _hat.WriteFile("a.txt", "content");
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddHours(-1));
        _hat.Hat(["tag-files", ":x", path]);
        File.WriteAllText(path, "edited and longer content");

        Assert.Equal(0, _hat.Hat(["file-info", path]));

        Assert.Contains(_hat.Lines, line => line.Contains("changed since it was indexed"));
        Assert.Contains("Tag: :x", _hat.Lines);
    }

    public void Dispose()
        => _hat.Dispose();
}
