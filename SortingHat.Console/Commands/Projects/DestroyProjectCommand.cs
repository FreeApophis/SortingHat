using System.Collections.Generic;
using System.Linq;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Projects;

[UsedImplicitly]
internal class DestroyProjectCommand(IProjects projects, IConsoleWriter consoleWriter) : ICommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.Project;
    public string LongCommand => "remove-project";
    public Option<string> ShortCommand => Option<string>.None;
    public string ShortHelp => "Removes a project with all tags and files.";

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        => arguments.ToList() switch
        {
            [] => NotEnoughArguments(),
            var projectNames => DestroyProjects(projectNames),
        };

    private bool DestroyProjects(List<string> projectNames)
        => projectNames.All(projectName => projects.RemoveProject(projectName));

    private bool NotEnoughArguments()
    {
        consoleWriter.WriteLine("Not enough arguments given, please give a name for the project you want to create.");

        return false;
    }
}
