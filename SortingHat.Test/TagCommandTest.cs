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

    public void Dispose()
        => _hat.Dispose();
}
