using System.Collections.Generic;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.Test.Mock
{
    public class MockTagStore : ITag
    {
        public List<Tag> Tags { get; } = new List<Tag>();

        private MockTagStore()
        {
        }

        public bool Destroy(Tag tag)
        {
            throw new System.NotImplementedException();
        }

        public long FileCount(Tag tag)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Tag> GetTags()
        {
            throw new System.NotImplementedException();
        }

        public bool Move(Tag tag, Tag destinationTag)
        {
            throw new System.NotImplementedException();
        }

        public bool Rename(Tag tag, string newName)
        {
            throw new System.NotImplementedException();
        }

        public bool Store(Tag tag)
        {
            Tags.Add(tag);

            return true;
        }

        public static MockTagStore Create()
        {
            return new MockTagStore();
        }
    }
}