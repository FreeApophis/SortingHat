using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.Test
{
    internal class MockDB : IDatabase
    {
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
    }
}