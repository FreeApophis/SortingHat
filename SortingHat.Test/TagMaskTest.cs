using System;
using System.IO;
using Funcky.Monads;
using SortingHat.API.AutoTag;
using SortingHat.API.Models;
using Xunit;
using static Funcky.Functional;

namespace SortingHat.Test;

/// <summary>
/// A tag mask such as ':Photos:{Camera.Make}' is parsed once against the known variables, then filled per file.
/// Pure rules: the auto-tags are hand-written, no plugin and no disk.
/// </summary>
public sealed class TagMaskTest
{
    private static readonly IAutoTag[] AutoTags =
    [
        new FixedAutoTag("Camera.Make", "Canon"),
        new FixedAutoTag("Camera", AutoTagParameter.OneOf(["Year", "Month"]), (_, argument) => argument.Text.Select(part => part == "Year" ? "2019" : "5")),
        new FixedAutoTag("Folder", AutoTagParameter.Integer, (_, argument) => argument.Number.Select(index => $"level{index}")),
        new FixedAutoTag("Nothing", AutoTagParameter.None, (_, _) => Option<string>.None),
        new FixedAutoTag("Padded", "  Canon  "),
        new FixedAutoTag("Root", @"C:\"),
        new FixedAutoTag("Spaced", "NIKON CORPORATION"),
        new FixedAutoTag("Symbols", "&&"),
        new FixedAutoTag("Broken", AutoTagParameter.None, (_, _) => throw new IOException("the file is locked")),
    ];

    [Theory]
    [InlineData(":tax:2018", ":tax:2018")]
    [InlineData(":Photos:{Camera.Make}", ":Photos:Canon")]
    [InlineData(":Taken:{Camera.Year}:{Camera.Month}", ":Taken:2019:5")]
    [InlineData(":Year-{Camera.Year}", ":Year-2019")]
    [InlineData(":{Folder.0}:{Folder.12}", ":level0:level12")]
    [InlineData(":Photos:{Padded}", ":Photos:Canon")]
    [InlineData(":Drives:{Root}", ":Drives:C")]
    [InlineData(":Photos:{Spaced}", ":Photos:NIKON-CORPORATION")]
    public void GivenAMaskThenEveryVariableIsFilledForTheFile(string mask, string tag)
        => Assert.Equal(tag, Tagged(mask).FullName);

    [Fact]
    public void GivenAVariableWhoseNameStartsWithAnotherOneThenTheLongestNameWins()
        => Assert.Equal(":Canon", Tagged(":{Camera.Make}").FullName);

    [Theory]
    [InlineData("tax", "colon")]
    [InlineData(":a::{Camera.Make}", "empty")]
    [InlineData(": a:{Camera.Make}", "letters")]
    [InlineData(":Photos:{Camera.Mkae}", "{Camera.Mkae}")]
    [InlineData(":Photos:{Lens}", "{Lens}")]
    [InlineData(":Taken:{Camera.Yeer}", "Year, Month")]
    [InlineData(":Taken:{Camera}", "Year, Month")]
    [InlineData(":{Folder.x}", "number")]
    [InlineData(":{Folder}", "number")]
    [InlineData(":{Nothing.x}", "{Nothing.x}")]
    [InlineData(":Photos:{Camera.Make", "{")]
    [InlineData(":Photos:Camera.Make}", "}")]
    [InlineData(":Photos:{}", "{}")]
    public void GivenAMaskThatIsNoValidMaskThenItIsRejectedWithTheReason(string mask, string reason)
    {
        var problem = Problem(TagMask.Parse(mask, AutoTags));

        Assert.Contains($"'{mask}'", problem);
        Assert.Contains(reason, problem);
    }

    [Fact]
    public void GivenTwoAutoTagsWithTheSameNameThenTheMaskIsRejectedInsteadOfPickingOne()
        => Assert.Contains("{Camera.Make}", Problem(TagMask.Parse(":{Camera.Make}", [.. AutoTags, new FixedAutoTag("Camera.Make", "Nikon")])));

    [Fact]
    public void GivenAFileWithoutAValueThenTheMaskGivesNoTagAndSaysWhich()
        => Assert.Equal("no value for {Nothing}", Problem(Mask(":Photos:{Nothing}").Apply(FixedAutoTag.AnyFile)));

    [Fact]
    public void GivenAValueWithoutAnyCharacterOfATagNameThenTheMaskGivesNoTagAndSaysWhich()
        => Assert.Contains("'&&'", Problem(Mask(":x:{Symbols}").Apply(FixedAutoTag.AnyFile)));

    [Fact]
    public void GivenAnAutoTagThatFailsThenTheMaskGivesNoTagAndSaysWhy()
        => Assert.Equal("{Broken} failed: the file is locked", Problem(Mask(":x:{Broken}").Apply(FixedAutoTag.AnyFile)));

    [Fact]
    public void GivenAMaskThenItIsWrittenAsItWasParsed()
        => Assert.Equal(":Photos:{Camera.Make}", Mask(":Photos:{Camera.Make}").ToString());

    [Fact]
    public void GivenSeveralVariablesThatNeedTheSameFactThenTheFileIsReadOnce()
    {
        var reads = 0;
        var cached = new FixedAutoTag("Exif", AutoTagParameter.OneOf(["Make", "Model"]), (file, argument) => argument.Text.Select(part => $"{part}{file.Remember("test.metadata", _ => ++reads)}"));
        var file = FixedAutoTag.AnyFile;

        var tag = TagMask.Parse(":{Exif.Make}:{Exif.Model}", [cached]).SelectMany(mask => mask.Apply(file));

        Assert.Equal(Either<string, string>.Right(":Make1:Model1"), tag.Select(found => found.FullName));
    }

    private static TagMask Mask(string text)
        => TagMask.Parse(text, AutoTags).GetOrElse(problem => throw new InvalidOperationException(problem));

    private static Tag Tagged(string mask)
        => Mask(mask).Apply(FixedAutoTag.AnyFile).GetOrElse(problem => throw new InvalidOperationException(problem));

    private static string Problem<T>(Either<string, T> result)
        where T : notnull
        => result.Match(left: Identity, right: value => throw new InvalidOperationException($"Expected a problem, but got '{value}'."));
}
