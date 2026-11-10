using System.Collections.Generic;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;
using SortingHat.API.Models;
using SortingHat.Desktop.Core;
using Xunit;

namespace SortingHat.Desktop.Test;

/// <summary>The pure core of the tag tree.</summary>
public sealed class TagUpdateTest
{
    private static readonly IReadOnlyDictionary<Tag, long> FileCounts = new Dictionary<Tag, long>
    {
        [Tag(":tax:2019")] = 2,
        [Tag(":bank")] = 1,
        [Tag(":tax")] = 0,
        [Tag(":tax:2018")] = 3,
    };

    [Fact]
    public void GivenTheFileCountsThenTheTreeHasTheTopLevelTagsWithTheirChildrenInTreeOrder()
    {
        var tree = TagTreeNode.Build(FileCounts);

        Assert.Equal(["bank", "tax"], tree.Select(node => node.Name));
        Assert.Equal([("2018", 3L), ("2019", 2L)], tree[1].Children.Select(node => (node.Name, node.FileCount)));
    }

    [Fact]
    public void GivenLoadedTagsThenTheyCompleteTheQuery()
    {
        var state = MainUpdate.Update(MainState.Initial, new Message.TagsLoaded(FileCounts)).State;

        Assert.Equal([Tag(":bank"), Tag(":tax"), Tag(":tax:2018"), Tag(":tax:2019")], state.Tags.All);
    }

    [Fact]
    public void GivenAChosenTagThenItBecomesTheQueryAndIsSearchedAtOnce()
    {
        var transition = MainUpdate.Update(MainState.Initial, new Message.TagChosen(Tag(":tax:2018")));

        Assert.Equal(":tax:2018", transition.State.Search.Query);
        Assert.Equal([new Effect.Search(":tax:2018", transition.State.Search.Generation)], transition.Effects);
    }

    [Fact]
    public void GivenAConfirmedRenameThenTheTagIsRenamed()
    {
        var state = Started(new TagEdit.Renaming(Tag(":tax:2018"), "2018"), "2019");

        var transition = MainUpdate.Update(state, new Message.TagEditConfirmed());

        Assert.Equal([new Effect.RenameTag(Tag(":tax:2018"), Assert.Right(TagName.Parse("2019")))], transition.Effects);
    }

    [Fact]
    public void GivenANewNameThatIsNoTagNameThenTheEditBarSaysWhyAndNothingChanges()
    {
        var state = Started(new TagEdit.Renaming(Tag(":tax:2018"), "2018"), "20 19");

        var transition = MainUpdate.Update(state, new Message.TagEditConfirmed());

        Assert.Some(transition.State.Tags.Problem);
        Assert.Empty(transition.Effects);
    }

    [Theory]
    [InlineData(":", false)]
    [InlineData(":archive", true)]
    public void GivenAConfirmedMoveThenTheTagMovesUnderTheDestinationOrToTheTopLevel(string destination, bool underATag)
    {
        var state = Started(new TagEdit.Moving(Tag(":tax:2018"), ":tax"), destination);

        var transition = MainUpdate.Update(state, new Message.TagEditConfirmed());

        Assert.Equal([new Effect.MoveTag(Tag(":tax:2018"), underATag ? Option.Some(Tag(destination)) : Option<Tag>.None)], transition.Effects);
    }

    [Fact]
    public void GivenAConfirmedRemovalThenTheTagIsRemoved()
    {
        var state = MainUpdate.Update(MainState.Initial, new Message.TagEditStarted(new TagEdit.Removing(Tag(":bank")))).State;

        Assert.Equal([new Effect.RemoveTag(Tag(":bank"))], MainUpdate.Update(state, new Message.TagEditConfirmed()).Effects);
    }

    [Fact]
    public void GivenADoneChangeThenTheBarClosesAndTheTagsAndTheResultsAreLoadedAgain()
    {
        var state = Started(new TagEdit.Renaming(Tag(":tax:2018"), "2018"), "2019");
        state = MainUpdate.Update(state, new Message.QueryChanged(":tax")).State;

        var transition = MainUpdate.Update(state, new Message.TagEdited(Either<string, string>.Right("Renamed ':tax:2018' to ':tax:2019'.")));

        Assert.None(transition.State.Tags.Edit);
        Assert.Equal("Renamed ':tax:2018' to ':tax:2019'.", transition.State.Messages.Last());
        Assert.Equal([new Effect.LoadTags(), new Effect.Search(":tax", transition.State.Search.Generation)], transition.Effects);
    }

    [Fact]
    public void GivenARefusedChangeThenTheBarStaysOpenWithTheReason()
    {
        var state = Started(new TagEdit.Renaming(Tag(":tax:2018"), "2018"), "2019");

        var transition = MainUpdate.Update(state, new Message.TagEdited(Either<string, string>.Left("a sibling is already named '2019'")));

        Assert.Some(transition.State.Tags.Edit);
        Assert.Some("a sibling is already named '2019'", transition.State.Tags.Problem);
    }

    [Fact]
    public void GivenCancelThenTheBarCloses()
    {
        var state = Started(new TagEdit.Renaming(Tag(":tax:2018"), "2018"), "2019");

        Assert.None(MainUpdate.Update(state, new Message.TagEditCancelled()).State.Tags.Edit);
    }

    private static MainState Started(TagEdit edit, string text)
        => new Message[] { new Message.TagEditStarted(edit), new Message.TagEditTextChanged(text) }
            .Aggregate(MainState.Initial, (state, message) => MainUpdate.Update(state, message).State);

    private static Tag Tag(string text)
        => Assert.Right(API.Models.Tag.Parse(text));
}
