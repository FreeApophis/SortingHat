using System.Collections.Generic;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.Test
{
    internal class MockDB : IDatabase
    {
        public IEnumerable<Tag> GetAllTags()
        {
            throw new System.NotImplementedException();
        }

        public void Setup()
        {
            throw new System.NotImplementedException();
        }

        public void StoreTag(Tag tag)
        {
            if (tag.Parent != null)
            {
                StoreTag(tag.Parent);
            }
        }

        public void TagFile(File file, Tag tag)
        {
            throw new System.NotImplementedException();
        }

        public void UntagFile(File file, Tag tag)
        {
            throw new System.NotImplementedException();
        }
    }
}