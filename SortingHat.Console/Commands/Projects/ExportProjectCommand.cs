using System.Collections.Generic;
using System.IO;
using System.Linq;
using apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Projects
{
    [UsedImplicitly]
    internal class ExportProjectCommand : ICommand
    {
        private readonly DatabaseSettings _databaseSettings;
        private readonly IConsoleWriter _consoleWriter;
        private const string CurrentDirectory = ".";

        public ExportProjectCommand(DatabaseSettings databaseSettings, IConsoleWriter consoleWriter)
        {
            _databaseSettings = databaseSettings;
            _consoleWriter = consoleWriter;
        }
        public CommandGrouping CommandGrouping => CommandGrouping.Project;
        public string LongCommand => "export";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Exports a project as a file.";
        public bool Execute(IEnumerable<string> lazyArguments, IOptions options)
        {
            var arguments = lazyArguments.ToList();

            return arguments.Count switch
            {
                0 => NotEnoughArguments(),
                1 => ExportProject(arguments.First(), CurrentDirectory),
                2 => ExportProject(arguments.First(), arguments.Last()),
                _ => TooManyArguments()
            };
        }

        private bool TooManyArguments()
        {
            _consoleWriter.WriteLine("Too many arguments given, please only export one project at a time.");

            return false;
        }

        private bool NotEnoughArguments()
        {
            _consoleWriter.WriteLine("Too few arguments given, please specify which project to export.");

            return false;
        }

        private bool ExportProject(string project, string destination)
        {
            File.Copy(DatabaseSource(project), DestinationPath(destination, project));

            _consoleWriter.WriteLine($"Project '{project}' has been exported to: {DestinationPath(destination, project)}");

            return true;
        }

        private string DestinationPath(string destination, string project)
        {
            return Directory.Exists(destination)
                ? Path.Combine(destination, $"{project}.hdb")
                : destination;
        }

        private string DatabaseSource(string project)
        {
            return Path.Combine(_databaseSettings.DbPath, $"{project}.db");
        }
    }
}