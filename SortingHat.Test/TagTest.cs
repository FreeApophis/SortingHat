using System;
using System.Linq;
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
    public void GivenTheSameTagInAnotherCaseThenTheTagsAreEqual()
    {
        Assert.Equal(Valid(":Tax:2018"), Valid(":TAX:2018"));
        Assert.Equal(Valid(":Tax:2018").GetHashCode(), Valid(":TAX:2018").GetHashCode());
    }

    [Fact]
    public void GivenANameInAnotherCaseThenTheNamesAreEqualAlsoBeyondAscii()
        => Assert.Equal(ValidName("Müller"), ValidName("MÜLLER"));

    [Fact]
    public void GivenATagThenItKeepsItsSpelling()
        => Assert.Equal(":Tax:IKEA", Valid(":Tax:IKEA").FullName);

    [Fact]
    public void GivenTagsThenTheyAreOrderedLikeATreeIgnoringCase()
    {
        string[] tags = [":banana", ":Apple:b", ":Cherry", ":apple", ":Apple:A"];

        Assert.Equal([":apple", ":Apple:A", ":Apple:b", ":banana", ":Cherry"], tags.Select(Valid).Order(Tag.Order).Select(tag => tag.FullName));
    }

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
    [InlineData(":Steuererklärung:2018")]
    [InlineData(":Camera:NIKON-CORPORATION")]
    [InlineData(":v1.2")]
    [InlineData(":C++:snake_case")]
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
    [InlineData(":Camera:NIKON CORPORATION")]
    [InlineData(":Müller & Co")]
    [InlineData(":a/b")]
    public void GivenTextThatIsNoTagThenItIsInvalidAndTheReasonNamesIt(string text)
        => Assert.Contains($"'{text}'", Invalid(Tag.Parse(text)));

    [Theory]
    [InlineData("2019")]
    [InlineData("v1.2")]
    [InlineData("a+b_c-d")]
    public void GivenAValidNameThenItIsParsed(string text)
        => Assert.Equal(text, ValidName(text).Value);

    [Theory]
    [InlineData("")]
    [InlineData(" x")]
    [InlineData("x ")]
    [InlineData("a:b")]
    [InlineData(":b")]
    [InlineData("two words")]
    [InlineData("a&b")]
    public void GivenTextThatIsNoNameThenItIsInvalidAndTheReasonNamesIt(string text)
        => Assert.Contains($"'{text}'", Invalid(TagName.Parse(text)));

    [Theory]
    [InlineData("NIKON CORPORATION", "NIKON-CORPORATION")]
    [InlineData("Portable Network Graphics", "Portable-Network-Graphics")]
    [InlineData("Müller & Co", "Müller-Co")]
    [InlineData("  Canon  ", "Canon")]
    [InlineData(@"C:\", "C")]
    [InlineData("v1.2", "v1.2")]
    [InlineData("C++", "C++")]
    public void GivenAValueFromAFileThenEveryRunOfOtherCharactersBecomesOneDash(string value, string name)
        => Assert.Equal(Option.Some(name), TagName.FromValue(value).Select(tagName => tagName.Value));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("&&")]
    public void GivenAValueWithoutAnyCharacterOfATagNameThenItGivesNoName(string value)
        => Assert.Equal(Option<TagName>.None, TagName.FromValue(value));

    private static Tag Valid(string text)
        => Tag.Parse(text).GetOrElse(problem => throw new InvalidOperationException(problem));

    private static TagName ValidName(string text)
        => TagName.Parse(text).GetOrElse(problem => throw new InvalidOperationException(problem));

    private static string Invalid<T>(Either<string, T> parsed)
        where T : notnull
        => parsed.Match(left: Identity, right: value => throw new InvalidOperationException($"Expected an error, but '{value}' was accepted."));
}
