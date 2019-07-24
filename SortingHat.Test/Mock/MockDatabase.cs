using System.Collections.Generic;
using SortingHat.API.DI;
using SortingHat.Test.Mock;

namespace SortingHat.Test
{
    public class MockProjectDatabase : IProjectDatabase
    {
        public MockFileStore MockFile { get; }
        public MockTagStore MockTag { get; }

        private MockProjectDatabase(MockFileStore file, MockTagStore tag)
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

        public static MockProjectDatabase Create()
        {
            return new MockProjectDatabase(MockFileStore.Create(), MockTagStore.Create());
        }
    }
}