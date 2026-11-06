using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.DB;

namespace SortingHat.CLI.Commands.Projects;

[UsedImplicitly]
[method: UsedImplicitly]
internal class ListProjectsCommand(IProjects projects, ISettings settings, DatabaseSettings databaseSettings, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.Project;
    public string Name => "list-projects";
    public string Description => "Lists all the project databases on this machine.";
    public string Details => "The active project is marked with an asterisk.";

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        projects.GetProjects().ForEach(project => consoleWriter.WriteLine($"{ActiveMarker(project)} {project}"));

        return Task.FromResult(true);
    }

    private string ActiveMarker(string project)
        => project == settings.ActiveProject(databaseSettings)
            ? "*"
            : " ";
}
