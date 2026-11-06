using System.Collections.Generic;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.DB;

namespace SortingHat.CLI.Commands.Projects;

[UsedImplicitly]
[method: UsedImplicitly]
internal class ListProjectsCommand(IProjects projects, ISettings settings, DatabaseSettings databaseSettings, IConsoleWriter consoleWriter) : ICommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.Project;
    public string LongCommand => "list-projects";
    public Option<string> ShortCommand => Option<string>.None;
    public string ShortHelp => "Lists all the project databases on this machine.";

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        foreach (var project in projects.GetProjects())
        {
            FormatProject(project);
        }

        return true;
    }

    private void FormatProject(string project)
    {
        consoleWriter.WriteLine($"{ActiveMarker(project)} {project}");
    }

    private string ActiveMarker(string project)
    {
        return project == settings.ActiveProject(databaseSettings)
            ? "*"
            : " ";
    }
}
