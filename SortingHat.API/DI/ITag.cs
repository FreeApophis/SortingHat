using System.Collections.Generic;
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

    /// <summary>Moves the <paramref name="tag"/> with its children under <paramref name="destinationTag"/>; false when one does not exist or the name is taken there.</summary>
    bool Move(Tag tag, Tag destinationTag);

    /// <summary>The number of files tagged with exactly the <paramref name="tag"/>.</summary>
    long FileCount(Tag tag);

    /// <summary>Every tag, each parent right before its children.</summary>
    IReadOnlyList<Tag> GetTags();
}
