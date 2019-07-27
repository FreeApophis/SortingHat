using System;
using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.CliAbstractions;

namespace SortingHat.CLI.Commands
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
        public string LongCommand => "create-project";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Creates a new project database with a given name.";
        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            var project = arguments.First();

            _projects.AddProject(project);
            _consoleWriter.WriteLine($"Project '{project}' has been created, and we switched to the project.");
            return true;
        }
    }
}
