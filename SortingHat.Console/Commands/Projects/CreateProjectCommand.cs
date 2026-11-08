using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Projects;

[UsedImplicitly]
[method: UsedImplicitly]
internal class CreateProjectCommand(IProjects projects, DatabaseSettings databaseSettings, IConsoleWriter consoleWriter, ApplicationName application) : IDeclaredCommand
{
    private static readonly ArgumentDefinition ProjectNameArgument = new() { Name = "project-name", Description = "The name of the new project: letters, digits, '-' and '_'." };

    public CommandGrouping CommandGrouping => CommandGrouping.Project;
    public string Name => "new-project";
    public string Description => "Creates a new project with its own database.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [ProjectNameArgument];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(input.Value(ProjectNameArgument).Match(none: false, some: CreateProject));

    private bool CreateProject(string project)
        => ProjectName.ParseNew(project, projects.GetProjects(), databaseSettings.Name)
            .Match(left: Reject, right: Create);

    private bool Reject(string problem)
    {
        consoleWriter.WriteLine(problem);

        return false;
    }

    private bool Create(ProjectName project)
    {
        projects.AddProject(project);
        consoleWriter.WriteLine($"Project '{project}' created. '{application.Name} switch-project {project}' makes it the active project.");

        return true;
    }
}
