using System.Collections.Generic;
using Funcky;
using Funcky.Monads;
using SortingHat.API.Models;

namespace SortingHat.API.DI;

/// <summary>The tags hat knows, with their hierarchy.</summary>
public interface ITag
{
    /// <summary>Stores the <paramref name="tag"/> with every level above it.</summary>
    void Store(Tag tag);

    /// <summary>Removes the <paramref name="tag"/> from every file; false when it does not exist or still has children.</summary>
    bool Destroy(Tag tag);

    /// <summary>Renames the last level of the <paramref name="tag"/>; false when it does not exist or a sibling already has the name.</summary>
    bool Rename(Tag tag, TagName newName);

    /// <summary>
    /// Moves the <paramref name="tags"/> with their children under <paramref name="destination"/>, none for the top level.
    /// All of them move or none: the reason when a tag does not exist, a name is taken there, or a tag would move into itself.
    /// </summary>
    Either<string, Unit> Move(IReadOnlyList<Tag> tags, Option<Tag> destination);

    /// <summary>The number of files tagged with exactly the <paramref name="tag"/>.</summary>
    long FileCount(Tag tag);

    /// <summary>Every tag, each parent right before its children.</summary>
    IReadOnlyList<Tag> GetTags();
}
