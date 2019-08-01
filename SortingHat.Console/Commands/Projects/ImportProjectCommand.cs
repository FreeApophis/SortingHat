using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using apophis.CLI.Writer;
using apophis.FileSystem;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Projects
{
    [UsedImplicitly]
    internal class ImportProjectCommand : ICommand
    {
        private readonly IConsoleWriter _consoleWriter;
        private readonly ICopyFile _copyFile;
        private readonly DatabaseSettings _databaseSettings;
        private readonly IProjects _projects;

        [UsedImplicitly]
        public ImportProjectCommand(IConsoleWriter consoleWriter, ICopyFile copyFile, DatabaseSettings databaseSettings, IProjects projects)
        {
            _consoleWriter = consoleWriter;
            _copyFile = copyFile;
            _databaseSettings = databaseSettings;
            _projects = projects;
        }

        public CommandGrouping CommandGrouping => CommandGrouping.Project;
        public string LongCommand => "import";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Imports a project file into the db.";
        public bool Execute(IEnumerable<string> lazyArguments, IOptionParser options)
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
            _consoleWriter.WriteLine("Too many arguments given, please give only filePath and projectName.");

            return false;
        }

        private bool ImportProject(string filePath, string projectName)
        {
            _copyFile.Copy(filePath, DestinationPath(projectName));
            _projects.AddProject(projectName);

            _consoleWriter.WriteLine($"New project '{projectName}' imported.");

            return true;
        }

        private bool ImportProject(string filePath)
        {
            var projectName = Path.GetFileNameWithoutExtension(filePath);

            if (projectName == null)
            {
                throw new NullReferenceException(nameof(projectName));
            }

            return ImportProject(filePath, projectName);
        }

        private bool NotEnoughArguments()
        {
            throw new NotImplementedException();
        }

        private string DestinationPath(string project)
        {
            return Path.Combine(_databaseSettings.DbPath, $"{project}.db");
        }
    }
}