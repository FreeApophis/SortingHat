using System;
using System.Collections.Generic;
using System.Linq;
using apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Projects
{
    [UsedImplicitly]
    internal class DestroyProjectCommand : ICommand
    {
        private readonly IProjects _projects;
        private readonly IConsoleWriter _consoleWriter;

        public DestroyProjectCommand(IProjects projects, IConsoleWriter consoleWriter)
        {
            _projects = projects;
            _consoleWriter = consoleWriter;
        }
        public CommandGrouping CommandGrouping => CommandGrouping.Project;
        public string LongCommand => "remove-project";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Removes a project with all tags and files.";
        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            var projects = arguments.ToList();

            return projects.Count switch
            {
                0 => NotEnoughArguments(),
                _ => DestroyProjects(projects)
            };
        }

        private bool DestroyProjects(List<string> projects)
        {
            return projects.All(project => _projects.RemoveProject(project));
        }

        private bool NotEnoughArguments()
        {
            _consoleWriter.WriteLine("Not enough arguments given, please give a name for the project you want to create.");

            return false;
        }
    }
}
