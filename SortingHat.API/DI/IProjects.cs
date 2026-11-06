using System.Collections.Generic;
using SortingHat.API.Models;

namespace SortingHat.API.DI;

public interface IProjects
{
    IEnumerable<string> GetProjects();

    bool AddProject(ProjectName project);

    bool RemoveProject(string project);
}
