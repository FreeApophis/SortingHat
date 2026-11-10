using SortingHat.API.Parser;

namespace SortingHat.Desktop.Core;

/// <summary>Why the query is not valid, as the query bar shows it.</summary>
/// <param name="Reason">What is wrong, such as "a '(' is not closed".</param>
/// <param name="Position">Where in the query the problem is.</param>
/// <param name="IsIncomplete">The query only ends too early: typing on can make it valid, so it is a hint rather than an error.</param>
internal sealed record QueryProblem(string Reason, int Position, bool IsIncomplete)
{
    public static QueryProblem From(ParseError error)
        => new(error.Reason, error.Position, error.IsIncomplete);
}
