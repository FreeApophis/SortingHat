using System.Collections.Generic;

namespace SortingHat.API.Parser;

/// <summary>Why a query is not valid.</summary>
/// <param name="Query">The query as it was given.</param>
/// <param name="Position">Where in the <paramref name="Query"/> the problem is, the length of the query when it ends too early.</param>
/// <param name="Reason">What is wrong there.</param>
/// <param name="Expected">What could stand at the <paramref name="Position"/> instead.</param>
/// <param name="IsIncomplete">The query ended where more was expected: typing on can still make it valid.</param>
public sealed record ParseError(string Query, int Position, string Reason, IReadOnlyList<QueryPart> Expected, bool IsIncomplete)
{
    public string Message
        => $"'{Query}' is not a valid query: {Reason}.";
}
