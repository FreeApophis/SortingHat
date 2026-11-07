using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Projects;

[UsedImplicitly]
[method: UsedImplicitly]
internal class SwitchProjectCommand(IProjects projects, ISettings settings, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    private static readonly ArgumentDefinition ProjectNameArgument = new() { Name = "project-name", Description = "The project to work with from now on." };

    public CommandGrouping CommandGrouping => CommandGrouping.Project;
    public string Name => "switch-project";
    public string Description => "Switches to an existing database with the given name.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [ProjectNameArgument];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(input.Value(ProjectNameArgument).Match(none: false, some: SwitchTo));

    private bool SwitchTo(string project)
    {
        if (projects.GetProjects().Contains(project))
        {
            settings.Set(DB.Constants.ProjectDatabaseKey, project);
            consoleWriter.WriteLine($"You switched to project '{project}'");
            return true;
        }

        consoleWriter.WriteLine($"There is no project with the name '{project}' please create it first.");
        return false;
    }
}
