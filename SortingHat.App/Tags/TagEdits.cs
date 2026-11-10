using System.Collections.Generic;
using Funcky;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.App.Tags;

/// <summary>Changes to the tag tree, with the reason when a change is not possible.</summary>
[UsedImplicitly]
public sealed class TagEdits(ITag tagStore)
{
    /// <summary>Renames the last level of the <paramref name="tag"/>.</summary>
    /// <returns>The renamed tag, such as <c>:tax:2019</c> for <c>:tax:2018</c> and <c>2019</c>.</returns>
    public Either<string, Tag> Rename(Tag tag, TagName newName)
        => tagStore.Rename(tag, newName)
            ? Renamed(tag, newName)
            : Either<string, Tag>.Left($"The tag '{tag}' does not exist, or one of its siblings is already named '{newName}'.");

    /// <summary>Moves the <paramref name="tags"/> with their children and files under the <paramref name="destination"/>, or to the top level; all or none.</summary>
    public Either<string, Unit> Move(IReadOnlyList<Tag> tags, Option<Tag> destination)
        => tagStore.Move(tags, destination);

    /// <summary>Removes the <paramref name="tag"/>; its files stay, they only lose the tag.</summary>
    public Either<string, Unit> Remove(Tag tag)
        => tagStore.Destroy(tag)
            ? Unit.Value
            : Either<string, Unit>.Left($"The tag '{tag}' does not exist or still has child tags.");

    private static Tag Renamed(Tag tag, TagName newName)
        => tag.Parent.Match(none: () => Tag.Root(newName), some: parent => parent.Child(newName));
}
