using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.CLI.Output;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Funcky.Monads;
using SortingHat.ConsoleWriter;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class DuplicateFileCommand : ICommand
    {
        private readonly IDatabase _db;
        private readonly IConsoleWriter _consoleWriter;

        public DuplicateFileCommand(IDatabase db, IConsoleWriter consoleWriter)
        {
            _db = db;
            _consoleWriter = consoleWriter;
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            foreach (var file in _db.File.GetDuplicates())
            {
                _consoleWriter.WriteLine("--- Duplicate ---");
                _consoleWriter.WriteLine($"CreatedAt (oldest): {file.CreatedAt}");
                _consoleWriter.WriteLine($"File Size: {file.Size}");
                _consoleWriter.WriteLine($"File Tags: {string.Join(", ", file.GetTags().Result.Select(t => t.FullName))}");

                var table = FileTable();
                foreach (var path in file.GetPaths().Result)
                {
                    var fileInfo = new FileInfo(path);

                    if (fileInfo.Exists)
                    {
                        table.Append(fileInfo.CreationTimeUtc, fileInfo.Length.HumanSize(), fileInfo.Name, string.Empty, fileInfo.Directory.FullName);
                    }
                    else
                    {
                        table.Append(string.Empty, string.Empty, fileInfo.Name, "*", fileInfo.Directory.FullName);
                    }

                }

                table.WriteTo(_consoleWriter);
                _consoleWriter.WriteLine();
            }

            _consoleWriter.WriteLine("Files with an asterix (*) are in the database, but at the given path they seem to be deleted.");

            return true;
        }

        private ConsoleTable FileTable()
        {
            var table = new ConsoleTable(5);

            return table;
        }

        public string LongCommand => "duplicate";
        public Option<string> ShortCommand => Option.Some("d");
        public string ShortHelp => "Find duplicate files in your dms";
        public CommandGrouping CommandGrouping => CommandGrouping.File;
    }
}
