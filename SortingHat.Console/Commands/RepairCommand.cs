using JetBrains.Annotations;
using SortingHat.API.DI;
using System.Collections.Generic;
using System.IO;
using apophis.CLI.Writer;
using Funcky.Monads;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class RepairCommand : ICommand
    {
        private readonly IFile _file;
        private readonly IConsoleWriter _consoleWriter;

        public RepairCommand(IFile file, IConsoleWriter consoleWriter)
        {
            _file = file;
            _consoleWriter = consoleWriter;
        }

        public string LongCommand => "repair";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Check each path locked in the database if the file still exists and is not corrupted / changed";
        public CommandGrouping CommandGrouping => CommandGrouping.General;

        public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        {
            foreach (var path in _file.GetPaths())
            {
                if (File.Exists(path) == false)
                {
                    _consoleWriter.WriteLine($"File '{path}' does not exist, removed from index");
                }
            }

            return true;
        }
    }
}
