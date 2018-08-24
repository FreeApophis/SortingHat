using SortingHat.API.DI;

namespace SortingHat.Test
{
    public class MockDatabase : IDatabase
    {
        public void Setup()
        {

        }

        public void TearDown()
        {

        }

        public IFile File { get; }
        public ITag Tag { get; }
    }
}