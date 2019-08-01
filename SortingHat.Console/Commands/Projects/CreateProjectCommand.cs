using System.Collections.Generic;
using System.Linq;
using apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Projects
{
    [UsedImplicitly]

    internal class CreateProjectCommand : ICommand
    {
        private readonly IProjects _projects;
        private readonly IConsoleWriter _consoleWriter;

        [UsedImplicitly]
        public CreateProjectCommand(IProjects projects, IConsoleWriter consoleWriter)
        {
            _projects = projects;
            _consoleWriter = consoleWriter;
        }
        public CommandGrouping CommandGrouping => CommandGrouping.Project;
        public string LongCommand => "new-project";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Creates a new project database with a given name.";
        public bool Execute(IEnumerable<string> lazyArguments, IOptionParser options)
        {
            var arguments = lazyArguments.ToList();

            return arguments.Count switch
            {
                0 => NotEnoughArguments(),
                1 => CreateProject(arguments.First()),
                _ => TooManyArguments()
            };
        }

        private bool TooManyArguments()
        {
            _consoleWriter.WriteLine("Too many arguments given, only one project at a time can be created.");

            return false;
        }

        private bool NotEnoughArguments()
        {
            _consoleWriter.WriteLine("Not enough arguments given, please give a name for the project you want to create.");

            return false;
        }

        private bool CreateProject(string project)
        {
            _projects.AddProject(project);
            _consoleWriter.WriteLine($"Project '{project}' has been created, and we switched to the project.");

            return true;
        }
    }
}
