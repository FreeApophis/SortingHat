using System.Collections.Generic;
using SortingHat.API.DI;

namespace SortingHat.Test.Mock
{
    public class MockMainDatabase : IMainDatabase
    {
        public MockMainDatabase(MockProjectDatabase mockProjectDatabase, IProjects projects, ISettings settings)
        {
            ProjectDatabase = mockProjectDatabase;
            Projects = projects;
            Settings = settings;
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
        public IProjects Projects { get; }
        public ISettings Settings { get; }

        public static MockMainDatabase Create()
        {
            return new MockMainDatabase(MockProjectDatabase.Create(), MockProjects.Create(), MockSettings.Create());
        }
    }
}