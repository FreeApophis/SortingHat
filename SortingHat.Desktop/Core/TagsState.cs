using System.Collections.Generic;
using Funcky.Monads;
using SortingHat.API.Models;

namespace SortingHat.Desktop.Core;

/// <summary>The tags of the project: the tree with file counts, and the change the user is making to it.</summary>
/// <param name="All">Every tag in tree order, for completing the query.</param>
/// <param name="Tree">The top-level tags with their children.</param>
/// <param name="Edit">The rename, move or removal the user started and has not confirmed yet.</param>
/// <param name="Problem">Why the <paramref name="Edit"/> cannot be done as it is.</param>
internal sealed record TagsState(IReadOnlyList<Tag> All, IReadOnlyList<TagTreeNode> Tree, Option<TagEdit> Edit, Option<string> Problem)
{
    public static TagsState Empty { get; } = new([], [], Option<TagEdit>.None, Option<string>.None);
}
