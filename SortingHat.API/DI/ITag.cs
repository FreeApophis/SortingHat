using SortingHat.API.Models;
using System.Collections.Generic;

namespace SortingHat.API.DI
{
    public interface ITag 
    {
        bool Store(Tag tag);
        bool Destroy(Tag tag);
        bool Rename(Tag tag, string newName);

        IEnumerable<Tag> GetTags();
    }
}
