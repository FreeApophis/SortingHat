using System;
using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.CliAbstractions;

namespace SortingHat.CLI.Commands.Projects
{
    [UsedImplicitly]
    internal class ImportProjectCommand : ICommand
    {
        private readonly IConsoleWriter _consoleWriter;

        [UsedImplicitly]
        public ImportProjectCommand(IConsoleWriter consoleWriter)
        {
            _consoleWriter = consoleWriter;
        }

        public CommandGrouping CommandGrouping => CommandGrouping.Project;
        public string LongCommand => "import";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Imports a project file into the db.";
        public bool Execute(IEnumerable<string> lazyArguments, IOptions options)
        {
            var arguments = lazyArguments.ToList();

            return arguments.Count switch
            {
                0 => NotEnoughArguments(),
                1 => ImportProject(arguments.Last()),
                2 => ImportProject(arguments.First(), arguments.Last()),
                _ => TooManyArguments()
            };
        }

        private bool TooManyArguments()
        {
            _consoleWriter.WriteLine("Too many arguments given, please give filePath and/or projectName.");

            return false;
        }

        private bool ImportProject(string filePath, string project)
        {
            throw new NotImplementedException();
        }

        private bool ImportProject(string projectOrFilePath)
        {
            throw new NotImplementedException();
        }

        private bool NotEnoughArguments()
        {
            throw new NotImplementedException();
        }
    }
}