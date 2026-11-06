using System.Collections.Generic;
using System.Linq;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Projects;

[UsedImplicitly]
[method: UsedImplicitly]
internal class SwitchProjectCommand(IProjects projects, ISettings settings, IConsoleWriter consoleWriter) : ICommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.Project;
    public string LongCommand => "switch-project";
    public Option<string> ShortCommand => Option<string>.None;
    public string ShortHelp => "Switches to an existing database with the given name.";

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        var project = arguments.First();

        if (projects.GetProjects().Contains(project))
        {
            settings[DB.Constants.ProjectDatabaseKey] = project;
            consoleWriter.WriteLine($"You switched to project '{project}'");
            return true;
        }

        consoleWriter.WriteLine($"There is no project with the name '{project}' please create it first.");
        return false;
    }
}