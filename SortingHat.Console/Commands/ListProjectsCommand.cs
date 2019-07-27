using System.Collections.Generic;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.CliAbstractions;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class ListProjectsCommand : ICommand
    {
        private readonly IMainDatabase _mainDatabase;
        private readonly IConsoleWriter _consoleWriter;

        [UsedImplicitly]
        public ListProjectsCommand(IMainDatabase mainDatabase, IConsoleWriter consoleWriter)
        {
            _mainDatabase = mainDatabase;
            _consoleWriter = consoleWriter;
        }
        public CommandGrouping CommandGrouping => CommandGrouping.Project;
        public string LongCommand => "list-projects";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Lists all the project databases on this machine.";
        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            foreach (var project in _mainDatabase.Projects.GetProjects())
            {
                FormatProject(project);
            }

            return true;
        }

        private void FormatProject(string project)
        {
            _consoleWriter.WriteLine($"{ActiveMarker(project)} {project}");
        }

        private string ActiveMarker(string project)
        {
            return project == "Default"
                ? "*"
                : " ";
        }
    }
}
