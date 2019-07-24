using JetBrains.Annotations;
using SortingHat.API.DI;
using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;
using SortingHat.API;
using SortingHat.CliAbstractions;
using SortingHat.CliAbstractions.Formatting;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class FindFilesCommand : ICommand
    {
        private readonly IMainDatabase _db;
        private readonly IConsoleWriter _consoleWriter;

        public FindFilesCommand(IMainDatabase db, IConsoleWriter consoleWriter)
        {
            _db = db;
            _consoleWriter = consoleWriter;
        }

        private ConsoleTable FileTable()
        {
            var table = new ConsoleTable(4);

            return table;
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            var search = string.Join(" ", arguments);
            _consoleWriter.WriteLine($"Find Files: {search}");

            var files = _db.ProjectDatabase.File.Search(search);

            if (files.Any()) {
                var table = FileTable();
                foreach (var file in files) {
                    if (options.HasOption(null, "open")) {
                        FileHelper.OpenWithAssociatedProgram(file.Path);
                    }

                    table.Append(file.Hash.Result.ShortHash(), file.CreatedAt, file.Size.HumanSize(), file.Path);
                }
                table.WriteTo(_consoleWriter);
            } else {
                _consoleWriter.WriteLine("No files found for your search query...");
            }

            return true;
        }



        public string LongCommand => "find-files";
        public Option<string> ShortCommand => Option.Some("ff");
        public string ShortHelp => "Finds all files matching the search query";
        public CommandGrouping CommandGrouping => CommandGrouping.File;
    }
}
