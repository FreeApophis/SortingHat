using System.Collections.Generic;
using SortingHat.API.Models;

namespace SortingHat.API.DI;

/// <summary>The projects: each one has its own database next to the main database.</summary>
public interface IProjects
{
    /// <summary>The names of all projects, as they were created.</summary>
    IReadOnlyList<string> GetProjects();

    void AddProject(ProjectName project);

    /// <summary>Deletes the database of the <paramref name="project"/> and forgets it.</summary>
    void RemoveProject(ProjectName project);
}
