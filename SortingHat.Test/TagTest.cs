using System;
using Funcky.Monads;
using SortingHat.API.Models;
using Xunit;
using static Funcky.Functional;

namespace SortingHat.Test;

/// <summary>A tag is parsed once from its text; colons separate its levels. Pure rules, no database.</summary>
public sealed class TagTest
{
    [Fact]
    public void GivenATagWithLevelsThenItKnowsItsNameAndItsParent()
    {
        var tag = Valid(":tax:2018");

        Assert.Equal(("2018", Option.Some(":tax")), (tag.Name.Value, tag.Parent.Select(parent => parent.FullName)));
    }

    [Fact]
    public void GivenATagWithOneLevelThenItHasNoParent()
        => Assert.Equal(Option<Tag>.None, Valid(":tax").Parent);

    [Fact]
    public void GivenATagThenItIsWrittenAsItWasParsed()
        => Assert.Equal((":grandfather:father:child", ":grandfather:father:child"), (Valid(":grandfather:father:child").FullName, Valid(":grandfather:father:child").ToString()));

    [Fact]
    public void GivenTheSameTextThenTheTagsAreEqual()
        => Assert.Equal(Valid(":created:2018"), Valid(":created:2018"));

    [Fact]
    public void GivenTheSameNameUnderAnotherParentThenTheTagsDiffer()
        => Assert.NotEqual(Valid(":created:2018"), Valid(":2018"));

    [Fact]
    public void GivenATagAndANameThenTheChildHasTheTagAsParent()
        => Assert.Equal(Valid(":tax:2018"), Valid(":tax").Child(ValidName("2018")));

    [Fact]
    public void GivenANameThenTheRootTagHasOnlyThatLevel()
        => Assert.Equal(Valid(":tax"), Tag.Root(ValidName("tax")));

    [Theory]
    [InlineData(":tag")]
    [InlineData(":grand father:father:child with whitespace")]
    [InlineData(":Steuererklärung:2018")]
    [InlineData(":Camera:NIKON CORPORATION")]
    [InlineData(":v1.2")]
    public void GivenAValidTagThenItIsParsed(string text)
        => Assert.Equal(text, Valid(text).FullName);

    [Theory]
    [InlineData("")]
    [InlineData("tag")]
    [InlineData("grandfather:father:child")]
    [InlineData(":")]
    [InlineData("::")]
    [InlineData(":a::b")]
    [InlineData("::first:::last")]
    [InlineData(":a:")]
    [InlineData(": a")]
    [InlineData(":a :b")]
    [InlineData(":a:\tb")]
    public void GivenTextThatIsNoTagThenItIsInvalidAndTheReasonNamesIt(string text)
        => Assert.Contains($"'{text}'", Invalid(Tag.Parse(text)));

    [Theory]
    [InlineData("2019")]
    [InlineData("two words")]
    public void GivenAValidNameThenItIsParsed(string text)
        => Assert.Equal(text, ValidName(text).Value);

    [Theory]
    [InlineData("")]
    [InlineData(" x")]
    [InlineData("x ")]
    [InlineData("a:b")]
    [InlineData(":b")]
    public void GivenTextThatIsNoNameThenItIsInvalidAndTheReasonNamesIt(string text)
        => Assert.Contains($"'{text}'", Invalid(TagName.Parse(text)));

    private static Tag Valid(string text)
        => Tag.Parse(text).GetOrElse(problem => throw new InvalidOperationException(problem));

    private static TagName ValidName(string text)
        => TagName.Parse(text).GetOrElse(problem => throw new InvalidOperationException(problem));

    private static string Invalid<T>(Either<string, T> parsed)
        where T : notnull
        => parsed.Match(left: Identity, right: value => throw new InvalidOperationException($"Expected an error, but '{value}' was accepted."));
}
