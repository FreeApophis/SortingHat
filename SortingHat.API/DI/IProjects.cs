using System.Collections.Generic;
using Funcky;
using Funcky.Monads;
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

    /// <summary>Writes a consistent copy of the database of the <paramref name="project"/> to the new file <paramref name="destination"/>, also while it is in use.</summary>
    Either<string, Unit> Export(ProjectName project, string destination);

    /// <summary>Adds the <paramref name="projectFile"/>, made by <see cref="Export"/>, as the new <paramref name="project"/>; nothing changes when it is no project this hat can open.</summary>
    Either<string, Unit> Import(string projectFile, ProjectName project);
}
