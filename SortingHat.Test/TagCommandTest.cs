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

    [Fact]
    public void GivenTagsThenAddTagsStoresThemAndListTagsShowsThem()
    {
        Assert.Equal(0, _hat.Hat(["add-tags", ":tax:2018", ":movie:good"]));

        Assert.Equal(0, _hat.Hat(["list-tags"]));
        Assert.Contains(":tax:2018|0", _hat.Rows);
        Assert.Contains(":movie:good|0", _hat.Rows);
    }

    [Fact]
    public void GivenATaggedFileThenListTagsCountsIt()
    {
        _hat.TaggedFile("a.txt", ":tax");

        Assert.Equal(0, _hat.Hat(["tags"]));
        Assert.Contains(":tax|1", _hat.Rows);
    }

    [Fact]
    public void GivenNoTagsThenListTagsSaysSo()
    {
        Assert.Equal(0, _hat.Hat(["list-tags"]));
        Assert.Equal(["There are currently no tags in the database."], _hat.Lines);
    }

    [Fact]
    public void GivenTagsInDifferentCaseThenListTagsSortsThemIgnoringCase()
    {
        Assert.Equal(0, _hat.Hat(["add-tags", ":cherry", ":Banana", ":apple"]));

        Assert.Equal(0, _hat.Hat(["list-tags"]));
        Assert.Equal(["Tag|Files", ":apple|0", ":Banana|0", ":cherry|0"], _hat.Rows);
    }

    [Fact]
    public void GivenSomethingThatIsNotATagThenAddTagsReportsIt()
    {
        Assert.NotEqual(0, _hat.Hat(["add-tags", "tax"]));
        Assert.Contains(_hat.Lines, line => line.Contains("'tax'"));
    }

    [Fact]
    public void GivenATagWithAnEmptyLevelThenAddTagsRejectsItWithTheReason()
    {
        Assert.NotEqual(0, _hat.Hat(["add-tags", ":tax::2018"]));
        Assert.Contains(_hat.Lines, line => line.Contains("':tax::2018'") && line.Contains("empty"));

        Assert.Equal(0, _hat.Hat(["list-tags"]));
        Assert.Equal(["There are currently no tags in the database."], _hat.Lines);
    }

    [Fact]
    public void GivenANewNameWithSpacesAroundThenRenameTagRejectsItWithTheReason()
    {
        _hat.TaggedFile("a.txt", ":a");

        Assert.NotEqual(0, _hat.Hat(["rename-tag", ":a", " b"]));
        Assert.Contains(_hat.Lines, line => line.Contains("' b'") && line.Contains("letters"));
        Assert.Equal(["a.txt"], _hat.FindFiles(":a"));
    }

    [Fact]
    public void GivenNoTagThenAddTagsAsksForOne()
    {
        Assert.NotEqual(0, _hat.Hat(["add-tags"]));
        Assert.Contains(_hat.Lines, line => line.Contains("<tag>"));
    }

    [Fact]
    public void GivenAnUnknownTagThenRemoveTagsReportsIt()
    {
        Assert.NotEqual(0, _hat.Hat(["remove-tags", ":typo"]));
        Assert.Contains(_hat.Lines, line => line.Contains("':typo'"));
    }

    [Fact]
    public void GivenATagAsTheNewNameThenRenameTagReportsIt()
    {
        _hat.TaggedFile("a.txt", ":a");

        Assert.NotEqual(0, _hat.Hat(["rename-tag", ":a", ":b"]));
        Assert.Contains(_hat.Lines, line => line.Contains("':b'"));
        Assert.Equal(["a.txt"], _hat.FindFiles(":a"));
    }

    [Fact]
    public void GivenOnlyTheTagThenRenameTagAsksForTheNewName()
    {
        Assert.NotEqual(0, _hat.Hat(["rename-tag", ":a"]));
        Assert.Contains(_hat.Lines, line => line.Contains("<new-name>"));
    }

    [Fact]
    public void GivenTheNameOfASiblingThenRenameTagReportsIt()
    {
        _hat.TaggedFile("a.txt", ":a");
        _hat.TaggedFile("b.txt", ":b");

        Assert.NotEqual(0, _hat.Hat(["rename-tag", ":a", "b"]));
        Assert.Contains(_hat.Lines, line => line.Contains("':a'"));
    }

    [Fact]
    public void GivenTagsAndADestinationThenMoveTagsMovesThemUnderTheDestination()
    {
        _hat.TaggedFile("a.txt", ":a");
        _hat.TaggedFile("b.txt", ":b");
        _hat.Hat(["add-tags", ":destination"]);

        Assert.Equal(0, _hat.Hat(["move-tags", ":a", ":b", ":destination"]));

        Assert.Equal(["a.txt"], _hat.FindFiles(":destination:a"));
        Assert.Equal(["b.txt"], _hat.FindFiles(":destination:b"));
    }

    [Fact]
    public void GivenOnlyOneTagThenMoveTagsAsksForTheDestination()
    {
        Assert.NotEqual(0, _hat.Hat(["move-tags", ":a"]));
        Assert.Contains(_hat.Lines, line => line.Contains("destination"));
    }

    [Fact]
    public void GivenADestinationWithAChildOfTheSameNameThenMoveTagsReportsIt()
    {
        _hat.TaggedFile("a.txt", ":x:2018");
        _hat.TaggedFile("b.txt", ":y:2018");

        Assert.NotEqual(0, _hat.Hat(["move-tags", ":x:2018", ":y"]));
        Assert.Contains(_hat.Lines, line => line.Contains("':x:2018'"));
    }

    public void Dispose()
        => _hat.Dispose();
}
