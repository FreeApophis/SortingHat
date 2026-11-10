using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;
using SortingHat.API.Models;

namespace SortingHat.Desktop.Core;

/// <summary>A tag in the tag tree.</summary>
/// <param name="Tag">The tag.</param>
/// <param name="FileCount">How many files have exactly this tag, not counting its children.</param>
/// <param name="Children">Its child tags in tree order.</param>
internal sealed record TagTreeNode(Tag Tag, long FileCount, IReadOnlyList<TagTreeNode> Children)
{
    /// <summary>The last level, such as <c>2018</c> for <c>:tax:2018</c>.</summary>
    public string Name => Tag.Name.Value;

    /// <summary>The tree of every tag in <paramref name="fileCounts"/>, in tree order (<see cref="Tag.Order"/>).</summary>
    public static IReadOnlyList<TagTreeNode> Build(IReadOnlyDictionary<Tag, long> fileCounts)
        => Nodes(Option<Tag>.None, fileCounts);

    private static IReadOnlyList<TagTreeNode> Nodes(Option<Tag> parent, IReadOnlyDictionary<Tag, long> fileCounts)
        =>
        [
            .. fileCounts
                .Where(tag => tag.Key.Parent == parent)
                .OrderBy(tag => tag.Key, Tag.Order)
                .Select(tag => new TagTreeNode(tag.Key, tag.Value, Nodes(tag.Key, fileCounts))),
        ];
}
