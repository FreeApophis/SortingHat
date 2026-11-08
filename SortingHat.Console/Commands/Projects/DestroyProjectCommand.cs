using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
internal class DestroyProjectCommand(IProjects projects, ISettings settings, DatabaseSettings databaseSettings, IConsoleWriter consoleWriter, IUserPrompt prompt) : IDeclaredCommand
{
    private static readonly ArgumentDefinition ProjectNames = new() { Name = "project-name", Description = "The projects to remove, in any letter case.", Arity = Arity.OneOrMore };

    private static readonly OptionDefinition Yes = new() { Name = "yes", Description = "Removes the projects without asking, for scripts and other terminals where hat cannot ask." };

    public CommandGrouping CommandGrouping => CommandGrouping.Project;
    public string Name => "remove-project";
    public string Description => "Removes a project with all tags and files.";

    public string Details
        => """
           BE CAREFUL this deletes the project's database with all its files and tags.
           The files on disk stay where they are. hat asks before it deletes anything,
           only 'y' removes the project, Enter or 'n' keeps it. Where hat cannot ask,
           such as in a script, --yes removes the project without asking.

           The active project cannot be removed, switch to another project first.
           """;

    public IReadOnlyList<ArgumentDefinition> Arguments => [ProjectNames];
    public IReadOnlyList<OptionDefinition> Options => [Yes];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(input.Values(ProjectNames).Aggregate(true, (removed, projectName) => RemoveProject(projectName, input.Has(Yes)) && removed));

    /// <summary>Only an existing project that is not in use can be removed. The name matches in any case, like the database file.</summary>
    private static Either<string, ProjectName> Removable(string name, IEnumerable<string> existingProjects, string activeProject, string mainDatabaseName)
        => existingProjects
            .FirstOrNone(project => ProjectName.IsSameName(name, project))
            .Match(
                none: () => Either<string, string>.Left($"There is no project with the name '{name}'."),
                some: project => ProjectName.IsSameName(project, activeProject)
                    ? Either<string, string>.Left($"The project '{project}' is in use, switch to another project before you remove it.")
                    : Either<string, string>.Right(project))
            .SelectMany(project => ProjectName.Parse(project, mainDatabaseName));

    private bool RemoveProject(string name, bool confirmed)
        => Removable(name, projects.GetProjects(), settings.ActiveProject(databaseSettings), databaseSettings.Name)
            .Match(left: Reject, right: project => confirmed ? Remove(project) : ConfirmAndRemove(project));

    /// <summary>Only an explicit yes removes the project; Enter keeps it.</summary>
    private bool ConfirmAndRemove(ProjectName project)
    {
        consoleWriter.WriteLine($"Removing the project '{project}' deletes its database with all its files and tags. The files on disk stay where they are.");

        return prompt.Confirm($"Remove the project '{project}'?").Match(
            none: () => CannotAsk(project),
            some: yes => yes ? Remove(project) : Keep(project));
    }

    private bool CannotAsk(ProjectName project)
    {
        consoleWriter.WriteLine($"Project '{project}' kept: hat can only ask in an interactive terminal, '--yes' removes it without asking.");

        return false;
    }

    private bool Remove(ProjectName project)
    {
        projects.RemoveProject(project);
        consoleWriter.WriteLine($"Project '{project}' removed.");

        return true;
    }

    private bool Keep(ProjectName project)
    {
        consoleWriter.WriteLine($"Project '{project}' kept.");

        return false;
    }

    private bool Reject(string problem)
    {
        consoleWriter.WriteLine(problem);

        return false;
    }
}
