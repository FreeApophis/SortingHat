using System;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;
using SortingHat.API.Models;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Moving tags: all of them or none, also to the top level, never into themselves.
/// The rules as pure functions, and move-tags through the real command line application with a real SQLite database.
/// </summary>
public sealed class TagMoveTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Theory]
    [InlineData(":a", ":a")]
    [InlineData(":a", ":a:b")]
    [InlineData(":a", ":A:b:c")]
    public void GivenATagMovedIntoItselfOrOneOfItsChildrenThenTheMoveIsRejected(string tag, string destination)
        => Assert.Contains($"'{tag}'", Problem([tag], Option.Some(destination)));

    [Fact]
    public void GivenTwoTagsWithTheSameNameThenTheyCannotMoveTogether()
        => Assert.Contains("'2018'", Problem([":x:2018", ":y:2018"], Option.Some(":destination")));

    [Theory]
    [InlineData(new[] { ":a:b" }, ":a")]
    [InlineData(new[] { ":a", ":b" }, ":c")]
    [InlineData(new[] { ":ab" }, ":a")]
    public void GivenTagsThatCanMoveThenThereIsNoProblem(string[] tags, string destination)
        => Assert.None(TagMoves.Problem([.. tags.Select(Valid)], Option.Some(Valid(destination))));

    [Fact]
    public void GivenTheTopLevelAsDestinationThenMoveTagsMovesTheTagThere()
    {
        _hat.TaggedFile("a.txt", ":tax:2018");

        Assert.Equal(0, _hat.Hat(["move-tags", ":tax:2018", ":"]));

        Assert.Equal(["a.txt"], _hat.FindFiles(":2018"));
    }

    [Fact]
    public void GivenTheTopLevelAlreadyHasATagWithTheSameNameThenMoveTagsReportsIt()
    {
        _hat.TaggedFile("a.txt", ":a:tax");
        _hat.TaggedFile("b.txt", ":TAX");

        Assert.NotEqual(0, _hat.Hat(["move-tags", ":a:tax", ":"]));

        Assert.Contains(_hat.Lines, line => line.Contains("':a:tax'") && line.Contains("top level"));
        Assert.Equal(["a.txt"], _hat.FindFiles(":a:tax"));
    }

    [Fact]
    public void GivenOneTagThatCannotMoveThenNoTagMoves()
    {
        _hat.TaggedFile("a.txt", ":a");
        _hat.TaggedFile("b.txt", ":b");
        _hat.TaggedFile("c.txt", ":destination:b");

        Assert.NotEqual(0, _hat.Hat(["move-tags", ":a", ":b", ":destination"]));

        Assert.Contains(_hat.Lines, line => line.Contains("':b'"));
        Assert.Equal(["a.txt"], _hat.FindFiles(":a"));
        Assert.Equal(["b.txt"], _hat.FindFiles(":b"));
    }

    [Theory]
    [InlineData(":typo")]
    [InlineData("typo")]
    public void GivenATagThatDoesNotExistOrIsNoTagThenNoTagMoves(string source)
    {
        _hat.TaggedFile("a.txt", ":a");
        _hat.Hat(["add-tags", ":destination"]);

        Assert.NotEqual(0, _hat.Hat(["move-tags", ":a", source, ":destination"]));

        Assert.Contains(_hat.Lines, line => line.Contains($"'{source}'"));
        Assert.Equal(["a.txt"], _hat.FindFiles(":a"));
    }

    [Fact]
    public void GivenADestinationThatDoesNotExistThenMoveTagsReportsIt()
    {
        _hat.TaggedFile("a.txt", ":a");

        Assert.NotEqual(0, _hat.Hat(["move-tags", ":a", ":nowhere"]));

        Assert.Contains(_hat.Lines, line => line.Contains("':nowhere'"));
    }

    [Fact]
    public void GivenATagMovedIntoItsChildThenTheTreeStaysIntact()
    {
        _hat.TaggedFile("a.txt", ":a:b");

        Assert.NotEqual(0, _hat.Hat(["move-tags", ":a", ":a:b"]));

        Assert.Equal(["a.txt"], _hat.FindFiles(":a:b"));
    }

    public void Dispose()
        => _hat.Dispose();

    private static Tag Valid(string text)
        => Assert.Right(Tag.Parse(text));

    private static string Problem(string[] tags, Option<string> destination)
        => Assert.Some(TagMoves.Problem([.. tags.Select(Valid)], destination.Select(Valid)));
}
