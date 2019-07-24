using JetBrains.Annotations;
using SortingHat.API.DI;
using System.Collections.Generic;
using System.IO;
using Funcky.Monads;
using SortingHat.ConsoleWriter;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class RepairCommand : ICommand
    {
        private readonly IMainDatabase _db;
        private readonly IConsoleWriter _consoleWriter;

        public RepairCommand(IMainDatabase db, IConsoleWriter consoleWriter)
        {
            _db = db;
            _consoleWriter = consoleWriter;
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            foreach (var path in _db.ProjectDatabase.File.GetPaths())
            {
                if (File.Exists(path) == false)
                {
                    _consoleWriter.WriteLine($"File '{path}' does not exist, removed from index");
                }
            }

            return true;
        }

        public string LongCommand => "repair";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Check each path locked in the database if the file still exists and is not corrupted / changed";
        public CommandGrouping CommandGrouping => CommandGrouping.General;
    }
}
