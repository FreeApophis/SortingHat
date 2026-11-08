using System.Collections.Generic;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace SortingHat.API.Models;

/// <summary>The rules of moving tags that need no database: a tag cannot move into itself, and two tags with the same name cannot move together.</summary>
public static class TagMoves
{
    /// <summary>Why the <paramref name="tags"/> cannot move under <paramref name="destination"/> (none: the top level), none when nothing speaks against it.</summary>
    public static Option<string> Problem(IReadOnlyList<Tag> tags, Option<Tag> destination)
        => IntoItself(tags, destination).OrElse(() => SameNames(tags));

    private static Option<string> IntoItself(IReadOnlyList<Tag> tags, Option<Tag> destination)
        => from target in destination
           from tag in tags.FirstOrNone(tag => tag.IsSelfOrAncestorOf(target))
           select $"'{tag}' cannot move into itself or one of its children, such as '{target}'";

    private static Option<string> SameNames(IReadOnlyList<Tag> tags)
        => tags
            .GroupBy(tag => tag.Name)
            .FirstOrNone(sameName => sameName.Count() > 1)
            .Select(sameName => $"{string.Join(" and ", sameName.Select(tag => $"'{tag}'"))} cannot move together, they are all named '{sameName.Key}'");
}
