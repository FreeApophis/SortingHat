using System;
using System.Collections.Generic;
using System.Text;
using SortingHat.API.Models;

namespace SortingHat.API.DI
{
    public interface IDatabase
    {
        /// <summary>
        /// Creates the necessary Database and tables...
        /// </summary>
        void Setup();
        void StoreTag(Tag tag);
        IEnumerable<Tag> GetAllTags();
    }
}
