using System.Collections.Generic;
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

        public Dictionary<string, long> GetStatistics()
        {
            throw new System.NotImplementedException();
        }

        public IFile File { get; }
        public ITag Tag { get; }
    }
}