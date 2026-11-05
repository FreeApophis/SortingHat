using System.Collections.Generic;
using apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.DB;

namespace SortingHat.CLI.Commands.Projects
{
    [UsedImplicitly]
    internal class ListProjectsCommand : ICommand
    {
        private readonly IProjects _projects;
        private readonly ISettings _settings;
        private readonly DatabaseSettings _databaseSettings;
        private readonly IConsoleWriter _consoleWriter;

        [UsedImplicitly]
        public ListProjectsCommand(IProjects projects, ISettings settings, DatabaseSettings databaseSettings, IConsoleWriter consoleWriter)
        {
            _projects = projects;
            _settings = settings;
            _databaseSettings = databaseSettings;
            _consoleWriter = consoleWriter;
        }
        public CommandGrouping CommandGrouping => CommandGrouping.Project;
        public string LongCommand => "list-projects";
        public Option<string> ShortCommand => Option<string>.None;
        public string ShortHelp => "Lists all the project databases on this machine.";

        public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        {
            foreach (var project in _projects.GetProjects())
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
            return project == _settings.ActiveProject(_databaseSettings)
                ? "*"
                : " ";
        }
    }
}
