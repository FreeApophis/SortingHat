using System.Collections.Generic;
using SortingHat.API.DI;
using SortingHat.Test.Mock;

namespace SortingHat.Test
{
    public class MockDatabase : IDatabase
    {
        public MockFileStore MockFile { get; }
        public MockTagStore MockTag { get; }

        private MockDatabase(MockFileStore file, MockTagStore tag)
        {
            MockFile = file;
            MockTag = tag;
        }

        public IFile File => MockFile;
        public ITag Tag => MockTag;

        public void Setup()
        {
        }

        public void TearDown()
        {
        }

        public Dictionary<string, long> GetStatistics()
        {
            throw new System.NotImplementedException();
        }

        public static MockDatabase Create()
        {
            return new MockDatabase(MockFileStore.Create(), MockTagStore.Create());
        }
    }
}