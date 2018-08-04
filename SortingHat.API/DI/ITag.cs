using SortingHat.API.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SortingHat.API.DI
{
    public interface ITag 
    {
        void Store(Tag tag);
        void Destroy(Tag tag);

        IEnumerable<Tag> GetTags();
    }
}
