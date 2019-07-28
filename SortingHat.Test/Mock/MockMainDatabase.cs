using System.Collections.Generic;
using SortingHat.API.DI;

namespace SortingHat.Test.Mock
{
    public class MockMainDatabase : IMainDatabase
    {
        private MockMainDatabase()
        {
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

        public static MockMainDatabase Create()
        {
            return new MockMainDatabase();
        }
    }
}