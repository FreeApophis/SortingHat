using System.Collections.Generic;

namespace SortingHat.API.DI
{
    public interface IProjects
    {
        IEnumerable<string> GetProjects();

        bool AddProject(string project);

        bool RemoveProject(string project);
    }
}
