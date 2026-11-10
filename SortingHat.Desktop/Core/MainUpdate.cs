using System;
using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;
using SortingHat.API.Models;
using SortingHat.API.Parser;
using SortingHat.App.Files;
using static Funcky.Functional;

namespace SortingHat.Desktop.Core;

/// <summary>The pure core of the main window: how a message changes the state, and which effects follow.</summary>
internal static class MainUpdate
{
    private static readonly Parser QueryParser = Parser.Create();

    public static Transition Update(MainState state, Message message)
        => message switch
        {
            Message.StartupWarningsLoaded loaded => new(state with { Messages = [.. state.Messages, .. loaded.Warnings] }, []),
            Message.Wrote wrote => new(state with { Messages = [.. state.Messages, wrote.Line] }, []),
            Message.TagsLoaded loaded => new(TagUpdate.Loaded(state, loaded.FileCounts), []),
            Message.TagChosen chosen => Chosen(state, chosen.Tag),
            Message.TagEditStarted started => new(TagUpdate.Started(state, started.Edit), []),
            Message.TagEditTextChanged changed => new(TagUpdate.TextChanged(state, changed.Text), []),
            Message.TagEditConfirmed => TagUpdate.Confirmed(state),
            Message.TagEditCancelled => new(TagUpdate.Cancelled(state), []),
            Message.TagEdited edited => TagUpdate.Done(state, edited.Result, SearchNow),
            Message.QueryChanged changed => QueryChanged(state, changed.Query),
            Message.SearchRequested => SearchNow(state),
            Message.Paused paused => new(state, paused.Generation == state.Search.Generation && IsSearchable(state.Search) ? [new Effect.Search(state.Search.Query, state.Search.Generation)] : []),
            Message.SearchFinished finished => finished.Generation == state.Search.Generation ? Found(state, finished.Result) : new(state, []),
            Message.ResultSelected selected => Selected(state, selected.File),
            Message.DetailsLoaded loaded => new(state.Search.Selected == Option.Some(loaded.File) ? state with { Search = state.Search with { Details = loaded.Details } } : state, []),
            Message.OpenRequested open => new(state, [new Effect.Open(open.File.Path)]),
            _ => throw new ArgumentOutOfRangeException(nameof(message), message, "Every message is handled."),
        };

    /// <summary>A valid query is searched once the user pauses; an invalid one only says why.</summary>
    private static Transition QueryChanged(MainState state, string query)
    {
        var problem = query.Trim() is ""
            ? Option<QueryProblem>.None
            : QueryParser.Parse(query).Match(left: error => Option.Some(QueryProblem.From(error)), right: _ => Option<QueryProblem>.None);

        var search = state.Search with
        {
            Query = query,
            Generation = state.Search.Generation + 1,
            Problem = problem,
            Completions = QueryCompletions.For(query, state.Tags.All),
        };

        return query.Trim() is ""
            ? new(state with { Search = Cleared(search) }, [])
            : new(state with { Search = search }, problem.Match(none: () => [new Effect.Pause(search.Generation)], some: _ => (IReadOnlyList<Effect>)[]));
    }

    /// <summary>The tag becomes the query and is searched at once.</summary>
    private static Transition Chosen(MainState state, Tag tag)
        => SearchNow(state with { Search = state.Search with { Query = tag.FullName, Problem = Option<QueryProblem>.None } });

    /// <summary>Also searches again after a change, so the results and the details show it.</summary>
    private static Transition SearchNow(MainState state)
    {
        var search = state.Search with { Generation = state.Search.Generation + 1, Completions = [] };

        return new(state with { Search = search }, IsSearchable(search) ? [new Effect.Search(search.Query, search.Generation)] : []);
    }

    private static bool IsSearchable(SearchState search)
        => search.Query.Trim() is not "" && search.Problem.Match(none: true, some: False);

    private static SearchState Cleared(SearchState search)
        => search with { Results = [], Summary = string.Empty, Selected = Option<FileAt>.None, Details = Option<FileDetails>.None };

    private static Transition Found(MainState state, Either<string, SearchResult> result)
        => result.Match(
            left: reason => new Transition(state with { Search = Cleared(state.Search) with { Summary = reason } }, []),
            right: found => Kept(state.Search.Selected, found.Files).Match(
                none: () => new Transition(state with { Search = Cleared(state.Search) with { Results = [.. found.Files.Select(ResultRow.From)], Summary = Summary(found) } }, []),
                some: selected => new Transition(
                    state with { Search = state.Search with { Results = [.. found.Files.Select(ResultRow.From)], Summary = Summary(found), Details = Option<FileDetails>.None } },
                    [new Effect.LoadDetails(selected)])));

    /// <summary>The selected file stays selected when the new results still have it, and its details are loaded again.</summary>
    private static Option<FileAt> Kept(Option<FileAt> selected, IReadOnlyList<FileAt> found)
        => selected.Where(file => found.Contains(file));

    private static string Summary(SearchResult found)
        => string.Join(
            " ",
            [
                found.Files.Count switch { 0 => "No files.", 1 => "1 file.", var count => $"{count} files." },
                .. found.UnknownTags.Count > 0 ? [$"Unknown tags: {string.Join(", ", found.UnknownTags)}."] : Array.Empty<string>(),
            ]);

    private static Transition Selected(MainState state, Option<FileAt> file)
        => new(
            state with { Search = state.Search with { Selected = file, Details = Option<FileDetails>.None } },
            file.Match(none: () => [], some: selected => (IReadOnlyList<Effect>)[new Effect.LoadDetails(selected)]));
}
