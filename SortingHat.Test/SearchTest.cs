using System;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Searches through the real command line application, with a real SQLite database and real files in a temporary folder.
/// </summary>
public sealed class SearchTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Fact]
    public void GivenATagThenTheFilesWithThatTagAreFound()
    {
        _hat.TaggedFile("a.txt", ":x");
        _hat.TaggedFile("b.txt", ":y");

        Assert.Equal(["a.txt"], _hat.FindFiles(":x"));
    }

    [Fact]
    public void GivenOrThenTheFilesWithEitherTagAreFound()
    {
        _hat.TaggedFile("a.txt", ":x");
        _hat.TaggedFile("b.txt", ":y");
        _hat.TaggedFile("c.txt", ":z");

        Assert.Equal(["a.txt", "b.txt"], _hat.FindFiles(":x or :y"));
    }

    [Fact]
    public void GivenAndThenOnlyTheFilesWithBothTagsAreFound()
    {
        _hat.TaggedFile("a.txt", ":x", ":y");
        _hat.TaggedFile("b.txt", ":x");

        Assert.Equal(["a.txt"], _hat.FindFiles(":x and :y"));
    }

    [Fact]
    public void GivenAndNotThenTheFilesWithTheExcludedTagAreNotFound()
    {
        _hat.TaggedFile("a.txt", ":x", ":y");
        _hat.TaggedFile("b.txt", ":x");

        Assert.Equal(["b.txt"], _hat.FindFiles(":x and not :y"));
    }

    [Fact]
    public void GivenNotThenFilesWithoutAnyTagAreFound()
    {
        _hat.TaggedFile("a.txt", ":x", ":y");
        _hat.TaggedFile("b.txt", ":x");
        _hat.TaggedFile("c.txt");

        Assert.Equal(["b.txt", "c.txt"], _hat.FindFiles("not :y"));
    }

    [Fact]
    public void GivenAParentTagThenTheFilesTaggedWithItsChildTagsAreFound()
    {
        _hat.TaggedFile("a.txt", ":photo:holiday:2018");
        _hat.TaggedFile("b.txt", ":photo");
        _hat.TaggedFile("c.txt", ":other");

        Assert.Equal(["a.txt", "b.txt"], _hat.FindFiles(":photo"));
    }

    [Fact]
    public void GivenAChildTagThenTheFilesTaggedWithItsParentTagAreNotFound()
    {
        _hat.TaggedFile("a.txt", ":photo:holiday");
        _hat.TaggedFile("b.txt", ":photo");

        Assert.Equal(["a.txt"], _hat.FindFiles(":photo:holiday"));
    }

    [Fact]
    public void GivenNotAParentTagThenTheFilesTaggedWithItsChildTagsAreNotFound()
    {
        _hat.TaggedFile("a.txt", ":photo:holiday");
        _hat.TaggedFile("b.txt", ":other");

        Assert.Equal(["b.txt"], _hat.FindFiles("not :photo"));
    }

    [Fact]
    public void GivenAnUnknownTagThenNoFilesAreFoundAndTheTagIsReported()
    {
        _hat.TaggedFile("a.txt", ":x");

        Assert.Empty(_hat.FindFiles(":typo"));
        Assert.Contains("Unknown tag: :typo", _hat.Lines);
    }

    [Fact]
    public void GivenAChildTagWithoutItsParentThenItIsAnUnknownTag()
    {
        _hat.TaggedFile("a.txt", ":tax:2018");

        Assert.Empty(_hat.FindFiles(":2018"));
        Assert.Contains("Unknown tag: :2018", _hat.Lines);
    }

    [Theory]
    [InlineData(":a :b")]
    [InlineData(":a and")]
    [InlineData("(:a or :b")]
    public void GivenAnIllegalQueryThenFindFilesReportsItInsteadOfFindingNothing(string query)
    {
        _hat.TaggedFile("a.txt", ":a");

        Assert.NotEqual(0, _hat.Hat(["find-files", query]));

        Assert.Contains(_hat.Lines, line => line.Contains($"'{query}' is not a valid query"));
        Assert.DoesNotContain(_hat.Lines, line => line.Contains("No files found"));
    }

    [Fact]
    public void GivenAnIllegalQueryThenCopyFilesReportsIt()
    {
        var path = _hat.TaggedFile("a.txt", ":a");
        var destination = System.IO.Directory.CreateDirectory(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path) ?? string.Empty, "out")).FullName;

        Assert.NotEqual(0, _hat.Hat(["copy-files", ":a :b", destination]));

        Assert.Contains(_hat.Lines, line => line.Contains("':a :b' is not a valid query"));
    }

    public void Dispose()
        => _hat.Dispose();
}
