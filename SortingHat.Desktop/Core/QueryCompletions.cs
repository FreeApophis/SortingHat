using System;
using System.Collections.Generic;
using System.Linq;
using SortingHat.API.Models;

namespace SortingHat.Desktop.Core;

/// <summary>
/// Completes the word the user is typing at the end of a query: a tag after ':', or an operator.
/// Every completion is the whole query, so picking one replaces the query.
/// </summary>
internal static class QueryCompletions
{
    private const int Maximum = 10;

    private static readonly IReadOnlyList<string> Operators = ["and", "or", "not"];

    public static IReadOnlyList<string> For(string query, IReadOnlyList<Tag> tags)
    {
        var start = query.LastIndexOfAny([' ', '\t', '(', ')', '!']) + 1;
        var (before, word) = (query[..start], query[start..]);

        return
        [
            .. Candidates(word, tags)
                .Where(candidate => candidate.StartsWith(word, StringComparison.OrdinalIgnoreCase) && !candidate.Equals(word, StringComparison.OrdinalIgnoreCase))
                .Take(Maximum)
                .Select(candidate => before + candidate),
        ];
    }

    private static IEnumerable<string> Candidates(string word, IReadOnlyList<Tag> tags)
        => word switch
        {
            "" => [],
            [':', ..] => tags.Order(Tag.Order).Select(tag => tag.FullName),
            _ => Operators,
        };
}
