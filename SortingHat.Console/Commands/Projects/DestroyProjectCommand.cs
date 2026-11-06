using System.Collections.Generic;
using System.Linq;
using Apophis.CLI.Reader;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.DB;

namespace SortingHat.CLI.Commands.Projects;

[UsedImplicitly]
internal class DestroyProjectCommand(IProjects projects, ISettings settings, DatabaseSettings databaseSettings, IConsoleWriter consoleWriter, IConsoleReader consoleReader) : ICommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.Project;
    public string LongCommand => "remove-project";
    public Option<string> ShortCommand => Option<string>.None;
    public string ShortHelp => "Removes a project with all tags and files.";

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        => arguments.ToList() switch
        {
            [] => NotEnoughArguments(),
            var projectNames => projectNames.Aggregate(true, (removed, projectName) => RemoveProject(projectName) && removed),
        };

    /// <summary>Only an existing project that is not in use can be removed. The name matches in any case, like the database file.</summary>
    private static Either<string, string> Removable(string name, IEnumerable<string> existingProjects, string activeProject)
        => existingProjects
            .FirstOrNone(project => ProjectName.IsSameName(name, project))
            .Match(
                none: () => Either<string, string>.Left($"There is no project with the name '{name}'."),
                some: project => ProjectName.IsSameName(project, activeProject)
                    ? Either<string, string>.Left($"The project '{project}' is in use, switch to another project before you remove it.")
                    : Either<string, string>.Right(project));

    /// <summary>Anything but an explicit yes keeps the project, also an empty answer or the end of the input.</summary>
    private static bool IsYes(string answer)
        => answer.Trim().ToLowerInvariant() is "y" or "yes";

    private bool RemoveProject(string name)
        => Removable(name, projects.GetProjects(), settings.ActiveProject(databaseSettings))
            .Match(left: Reject, right: ConfirmAndRemove);

    private bool ConfirmAndRemove(string project)
    {
        consoleWriter.WriteLine($"Removing the project '{project}' deletes its database with all its files and tags. The files on disk stay where they are.");
        consoleWriter.WriteLine($"Remove the project '{project}'? [y/N]");

        return IsYes(consoleReader.ReadLine())
            ? Remove(project)
            : Keep(project);
    }

    private bool Remove(string project)
    {
        projects.RemoveProject(project);
        consoleWriter.WriteLine($"Project '{project}' removed.");

        return true;
    }

    private bool Keep(string project)
    {
        consoleWriter.WriteLine($"Project '{project}' kept.");

        return false;
    }

    private bool Reject(string problem)
    {
        consoleWriter.WriteLine(problem);

        return false;
    }

    private bool NotEnoughArguments()
    {
        consoleWriter.WriteLine("Not enough arguments given, please give the name of the project you want to remove.");

        return false;
    }
}
