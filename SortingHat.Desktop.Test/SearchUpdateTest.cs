using System;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;
using SortingHat.API.Models;
using SortingHat.Desktop.Core;
using Xunit;

namespace SortingHat.Desktop.Test;

/// <summary>How the pure core searches: valid queries after a pause, stale results dropped, details for the selection.</summary>
public sealed class SearchUpdateTest
{
    private static readonly FileAt Pdf = new(new File(Assert.Right(Hash.Parse($"sha256:{new string('a', 64)}")), 1200, new DateTime(2018, 4, 1, 0, 0, 0, DateTimeKind.Utc)), @"C:\scans\a.pdf", Option<PathFingerprint>.None);

    [Fact]
    public void GivenAValidQueryThenItIsSearchedAfterAPause()
    {
        var transition = MainUpdate.Update(MainState.Initial, new Message.QueryChanged(":tax"));

        Assert.Equal([new Effect.Pause(transition.State.Search.Generation)], transition.Effects);
        Assert.None(transition.State.Search.Problem);
    }

    [Fact]
    public void GivenThePauseOfTheCurrentQueryThenItIsSearched()
    {
        var state = Typed(":tax");

        var transition = MainUpdate.Update(state, new Message.Paused(state.Search.Generation));

        Assert.Equal([new Effect.Search(":tax", state.Search.Generation)], transition.Effects);
    }

    [Fact]
    public void GivenThePauseOfAnOlderQueryThenNothingIsSearched()
    {
        var state = Typed(":ta", ":tax");

        var transition = MainUpdate.Update(state, new Message.Paused(state.Search.Generation - 1));

        Assert.Empty(transition.Effects);
    }

    [Fact]
    public void GivenAnInvalidQueryThenItSaysWhyAndSearchesNothing()
    {
        var transition = MainUpdate.Update(MainState.Initial, new Message.QueryChanged(":tax or or"));

        var problem = Assert.Some(transition.State.Search.Problem);
        Assert.False(problem.IsIncomplete);
        Assert.Empty(transition.Effects);
    }

    [Fact]
    public void GivenAQueryThatEndsTooEarlyThenTheProblemIsOnlyAHint()
    {
        var transition = MainUpdate.Update(MainState.Initial, new Message.QueryChanged(":tax and"));

        Assert.True(Assert.Some(transition.State.Search.Problem).IsIncomplete);
    }

    [Fact]
    public void GivenEnterThenTheQueryIsSearchedAtOnce()
    {
        var state = Typed(":tax");

        var transition = MainUpdate.Update(state, new Message.SearchRequested());

        Assert.Equal([new Effect.Search(":tax", transition.State.Search.Generation)], transition.Effects);
    }

    [Fact]
    public void GivenTheResultOfTheCurrentSearchThenItIsShown()
    {
        var state = Typed(":tax");

        var found = MainUpdate.Update(state, new Message.SearchFinished(state.Search.Generation, new SearchResult([Pdf], [":bank"]))).State.Search;

        Assert.Equal(["a.pdf"], found.Results.Select(row => row.Name));
        Assert.Equal("1 file. Unknown tags: :bank.", found.Summary);
    }

    [Fact]
    public void GivenTheResultOfAnOlderSearchThenItIsDropped()
    {
        var state = Typed(":ta", ":tax");

        var found = MainUpdate.Update(state, new Message.SearchFinished(state.Search.Generation - 1, new SearchResult([Pdf], []))).State.Search;

        Assert.Empty(found.Results);
    }

    [Fact]
    public void GivenAFailedSearchThenTheSummarySaysWhy()
    {
        var state = Typed(":tax");

        var found = MainUpdate.Update(state, new Message.SearchFinished(state.Search.Generation, Either<string, SearchResult>.Left("the database is locked"))).State.Search;

        Assert.Equal("the database is locked", found.Summary);
    }

    [Fact]
    public void GivenAnEmptyQueryThenTheResultsAreCleared()
    {
        var state = Typed(":tax");
        state = MainUpdate.Update(state, new Message.SearchFinished(state.Search.Generation, new SearchResult([Pdf], []))).State;

        var transition = MainUpdate.Update(state, new Message.QueryChanged(string.Empty));

        Assert.Empty(transition.State.Search.Results);
        Assert.Empty(transition.Effects);
    }

    [Fact]
    public void GivenASelectedResultThenItsDetailsAreLoaded()
    {
        var transition = MainUpdate.Update(MainState.Initial, new Message.ResultSelected(Pdf));

        Assert.Equal([new Effect.LoadDetails(Pdf)], transition.Effects);
    }

    [Fact]
    public void GivenOpenThenTheFileIsOpenedAtItsPath()
        => Assert.Equal([new Effect.Open(@"C:\scans\a.pdf")], MainUpdate.Update(MainState.Initial, new Message.OpenRequested(Pdf)).Effects);

    private static MainState Typed(params string[] queries)
        => queries.Aggregate(MainState.Initial, (state, query) => MainUpdate.Update(state, new Message.QueryChanged(query)).State);
}
