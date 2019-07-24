using System.Collections.Generic;
using SortingHat.API.DI;

namespace SortingHat.Test.Mock
{
    public class MockMainDatabase : IMainDatabase
    {
        public MockMainDatabase(MockProjectDatabase mockProjectDatabase)
        {
            ProjectDatabase = mockProjectDatabase;
        }

        public void Setup()
        {
            throw new System.NotImplementedException();
        }

        public void TearDown()
        {
            throw new System.NotImplementedException();
        }

        public Dictionary<string, long> GetStatistics()
        {
            throw new System.NotImplementedException();
        }

        public IProjectDatabase ProjectDatabase { get; }
        public IEnumerable<string> ProjectDatabases { get; }
        public ISettings Settings { get; }

        public static MockMainDatabase Create()
        {
            return new MockMainDatabase(MockProjectDatabase.Create());
        }
    }
}