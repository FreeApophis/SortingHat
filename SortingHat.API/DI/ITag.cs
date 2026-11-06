using System.Collections.Generic;
using SortingHat.API.Models;

namespace SortingHat.API.DI;

public interface ITag
{
    bool Store(Tag tag);

    bool Destroy(Tag tag);

    bool Rename(Tag tag, string newName);

    bool Move(Tag tag, Tag destinationTag);

    long FileCount(Tag tag);

    IEnumerable<Tag> GetTags();
}
