using System.Collections.Generic;
using SortingHat.API.DI;

namespace SortingHat.Test.Mock
{
    public class MockProjectDatabase : IProjectDatabase
    {
        private MockProjectDatabase()
        {
        }

        public Dictionary<string, long> GetStatistics()
        {
            throw new System.NotImplementedException();
        }

        public static MockProjectDatabase Create()
        {
            return new MockProjectDatabase();
        }
    }
}