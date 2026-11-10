using Autofac;
using Funcky.Extensions;
using SortingHat.API.Models;
using SortingHat.App.Tags;
using Xunit;

namespace SortingHat.Test;

public class TagEditsTest
{
    [Theory]
    [InlineData(":tax:2018", "2019", ":tax:2019")]
    [InlineData(":tax", "taxes", ":taxes")]
    public void GivenATagThenRenameGivesTheRenamedTag(string tag, string newName, string renamed)
    {
        using var hat = new HatSandbox();
        hat.TaggedFile("a.pdf", tag);

        var result = hat.InScope(scope => scope.Resolve<TagEdits>().Rename(Parse(tag), Assert.Right(TagName.Parse(newName))));

        Assert.Right(Parse(renamed), result);
    }

    [Fact]
    public void GivenASiblingWithTheNewNameThenRenameSaysWhy()
    {
        using var hat = new HatSandbox();
        hat.TaggedFile("a.pdf", ":tax:2018", ":tax:2019");

        var result = hat.InScope(scope => scope.Resolve<TagEdits>().Rename(Parse(":tax:2018"), Assert.Right(TagName.Parse("2019"))));

        Assert.Left("The tag ':tax:2018' does not exist, or one of its siblings is already named '2019'.", result);
    }

    [Fact]
    public void GivenATagWithChildrenThenRemoveSaysWhy()
    {
        using var hat = new HatSandbox();
        hat.TaggedFile("a.pdf", ":tax:2018");

        var result = hat.InScope(scope => scope.Resolve<TagEdits>().Remove(Parse(":tax")));

        Assert.Left("The tag ':tax' does not exist or still has child tags.", result);
    }

    private static Tag Parse(string tag)
        => Assert.Right(Tag.Parse(tag));
}
