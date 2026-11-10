using System.Collections.Generic;
using Funcky.Extensions;
using SortingHat.API.Models;
using SortingHat.Desktop.Core;
using Xunit;

namespace SortingHat.Desktop.Test;

public sealed class QueryCompletionsTest
{
    private static readonly IReadOnlyList<Tag> Tags = [Tag(":tax:2019"), Tag(":tax"), Tag(":tax:2018"), Tag(":bank")];

    [Fact]
    public void GivenTheStartOfATagThenTheMatchingTagsCompleteTheQueryInTreeOrder()
        => Assert.Equal([":bank and :tax", ":bank and :tax:2018", ":bank and :tax:2019"], QueryCompletions.For(":bank and :ta", Tags));

    [Fact]
    public void GivenATagInAnotherCaseThenItStillCompletes()
        => Assert.Equal([":tax:2018", ":tax:2019"], QueryCompletions.For(":TAX:", Tags));

    [Fact]
    public void GivenTheStartOfAnOperatorThenItCompletes()
        => Assert.Equal([":tax and"], QueryCompletions.For(":tax an", Tags));

    [Fact]
    public void GivenACompleteWordThenThereIsNothingToComplete()
        => Assert.Empty(QueryCompletions.For(":bank", Tags));

    [Fact]
    public void GivenATagAfterAParenthesisThenItCompletes()
        => Assert.Equal(["(:bank"], QueryCompletions.For("(:b", Tags));

    private static Tag Tag(string text)
        => Assert.Right(API.Models.Tag.Parse(text));
}
