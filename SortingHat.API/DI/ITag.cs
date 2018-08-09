using SortingHat.API.Models;
using System.Collections.Generic;

namespace SortingHat.API.DI
{
    public interface ITag 
    {
        bool Store(Tag tag);
        bool Destroy(Tag tag);

        IEnumerable<Tag> GetTags();
    }
}
