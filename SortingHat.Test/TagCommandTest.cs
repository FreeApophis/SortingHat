using System;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Changes tags through the real command line application, with a real SQLite database and real files in a temporary folder.
/// </summary>
public sealed class TagCommandTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Fact]
    public void GivenAChildTagWithoutItsParentThenRemoveTagsKeepsTheChildTag()
    {
        _hat.TaggedFile("a.txt", ":tax:2018");

        _hat.Hat(["remove-tags", ":2018"]);

        Assert.Equal(["a.txt"], _hat.FindFiles(":tax:2018"));
    }

    [Fact]
    public void GivenAChildTagWithoutItsParentThenUntagFilesKeepsTheChildTag()
    {
        var path = _hat.TaggedFile("a.txt", ":tax:2018");

        _hat.Hat(["untag-files", ":2018", path]);

        Assert.Equal(["a.txt"], _hat.FindFiles(":tax:2018"));
    }

    [Fact]
    public void GivenATagWithChildrenThenRemoveTagsKeepsTheTagAndItsFiles()
    {
        _hat.TaggedFile("a.txt", ":tax");
        _hat.TaggedFile("b.txt", ":tax:2018");

        _hat.Hat(["remove-tags", ":tax"]);

        Assert.Equal(["a.txt", "b.txt"], _hat.FindFiles(":tax"));
    }

    [Fact]
    public void GivenATagWithoutChildrenThenRemoveTagsRemovesIt()
    {
        _hat.TaggedFile("a.txt", ":tax:2018");

        _hat.Hat(["remove-tags", ":tax:2018"]);

        Assert.Empty(_hat.FindFiles(":tax:2018"));
        Assert.Contains("Unknown tag: :tax:2018", _hat.Lines);
    }

    [Fact]
    public void GivenAFreeNameThenRenameTagRenamesTheTag()
    {
        _hat.TaggedFile("a.txt", ":a");

        _hat.Hat(["rename-tag", ":a", "b"]);

        Assert.Equal(["a.txt"], _hat.FindFiles(":b"));
    }

    [Fact]
    public void GivenTheNameOfASiblingThenRenameTagKeepsBothTags()
    {
        _hat.TaggedFile("a.txt", ":a");
        _hat.TaggedFile("b.txt", ":b");

        _hat.Hat(["rename-tag", ":a", "b"]);

        Assert.Equal(["a.txt"], _hat.FindFiles(":a"));
        Assert.Equal(["b.txt"], _hat.FindFiles(":b"));
    }

    [Fact]
    public void GivenADestinationWithAChildOfTheSameNameThenMoveTagsKeepsBothTags()
    {
        _hat.TaggedFile("a.txt", ":x:2018");
        _hat.TaggedFile("b.txt", ":y:2018");

        _hat.Hat(["move-tags", ":x:2018", ":y"]);

        Assert.Equal(["a.txt"], _hat.FindFiles(":x:2018"));
        Assert.Equal(["b.txt"], _hat.FindFiles(":y:2018"));
    }

    public void Dispose()
        => _hat.Dispose();
}
