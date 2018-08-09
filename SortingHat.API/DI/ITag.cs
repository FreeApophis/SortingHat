using SortingHat.API.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SortingHat.API.DI
{
    public interface ITag 
    {
        bool Store(Tag tag);
        bool Destroy(Tag tag);

        IEnumerable<Tag> GetTags();
    }
}
