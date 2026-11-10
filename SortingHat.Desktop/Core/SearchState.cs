using System.Collections.Generic;
using Funcky.Monads;
using SortingHat.API.Models;
using SortingHat.App.Files;

namespace SortingHat.Desktop.Core;

/// <summary>The search of the main window.</summary>
/// <param name="Query">The query as the user typed it.</param>
/// <param name="Generation">Counts the searches; a result of an older one arrives too late and is dropped.</param>
/// <param name="Problem">Why the query is no valid query, while it is not.</param>
/// <param name="Completions">Whole queries the user can pick, completing the last word of <paramref name="Query"/>.</param>
/// <param name="Results">The files the last search found.</param>
/// <param name="Summary">One line about the results: how many, unknown tags, or why the search failed.</param>
/// <param name="Selected">The result whose details are shown.</param>
/// <param name="Details">What the database knows about <paramref name="Selected"/>, once loaded.</param>
internal sealed record SearchState(
    string Query,
    int Generation,
    Option<QueryProblem> Problem,
    IReadOnlyList<string> Completions,
    IReadOnlyList<ResultRow> Results,
    string Summary,
    Option<FileAt> Selected,
    Option<FileDetails> Details)
{
    public static SearchState Empty { get; } = new(string.Empty, 0, Option<QueryProblem>.None, [], [], string.Empty, Option<FileAt>.None, Option<FileDetails>.None);
}
