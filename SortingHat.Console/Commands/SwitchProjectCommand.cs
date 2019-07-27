using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.CliAbstractions;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class SwitchProjectCommand : ICommand
    {
        private readonly IProjects _projects;
        private readonly ISettings _settings;
        private readonly IConsoleWriter _consoleWriter;

        [UsedImplicitly]
        public SwitchProjectCommand(IProjects projects, ISettings settings, IConsoleWriter consoleWriter)
        {
            _projects = projects;
            _settings = settings;
            _consoleWriter = consoleWriter;
        }

        public CommandGrouping CommandGrouping => CommandGrouping.Project;
        public string LongCommand => "switch-project";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Switches to an existing database with the given name.";
        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            var project = arguments.First();

            if (_projects.GetProjects().Contains(project))
            {
                _settings[DB.Constants.ProjectDatabaseKey] = project;
                _consoleWriter.WriteLine($"You switched to project '{project}'");
                return true;
            }

            _consoleWriter.WriteLine($"There is no project with the name '{project}' please create it first.");
            return false;
        }
    }
}