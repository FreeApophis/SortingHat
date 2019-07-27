using System.Collections.Generic;
using SortingHat.API.DI;

namespace SortingHat.Test.Mock
{
    public class MockProjects : IProjects
    {
        public static MockProjects Create()
        {
            return new MockProjects();
        }

        public IEnumerable<string> GetProjects()
        {
            throw new System.NotImplementedException();
        }

        public bool AddProject(string project)
        {
            throw new System.NotImplementedException();
        }

        public bool RemoveProject(string project)
        {
            throw new System.NotImplementedException();
        }
    }
}