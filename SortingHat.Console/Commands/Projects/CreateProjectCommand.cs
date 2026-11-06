using System.Collections.Generic;
using System.Linq;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Projects;

[UsedImplicitly]
[method: UsedImplicitly]
internal class CreateProjectCommand(IProjects projects, IConsoleWriter consoleWriter) : ICommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.Project;
    public string LongCommand => "new-project";
    public Option<string> ShortCommand => Option<string>.None;
    public string ShortHelp => "Creates a new project database with a given name.";

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        => arguments.ToList() switch
        {
            [] => NotEnoughArguments(),
            [var projectName] => CreateProject(projectName),
            _ => TooManyArguments(),
        };

    private bool TooManyArguments()
    {
        consoleWriter.WriteLine("Too many arguments given, only one project at a time can be created.");

        return false;
    }

    private bool NotEnoughArguments()
    {
        consoleWriter.WriteLine("Not enough arguments given, please give a name for the project you want to create.");

        return false;
    }

    private bool CreateProject(string project)
    {
        projects.AddProject(project);
        consoleWriter.WriteLine($"Project '{project}' has been created, and we switched to the project.");

        return true;
    }
}
