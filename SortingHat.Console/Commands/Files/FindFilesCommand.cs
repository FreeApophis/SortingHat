using System.Collections.Generic;
using System.Linq;
using apophis.CLI;
using apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.CLI.Options;

namespace SortingHat.CLI.Commands.Files
{
    [UsedImplicitly]
    internal class FindFilesCommand : ICommand
    {
        private readonly IFile _file;
        private readonly IConsoleWriter _consoleWriter;

        public FindFilesCommand(IFile file, IConsoleWriter consoleWriter)
        {
            _file = file;
            _consoleWriter = consoleWriter;
        }

        private ConsoleTable FileTable()
        {
            var table = new ConsoleTable(4);

            return table;
        }

        public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        {
            var search = string.Join(" ", arguments);
            _consoleWriter.WriteLine($"Find Files: {search}");

            var files = _file.Search(search).ToList();

            if (files.Any())
            {
                var table = FileTable();
                foreach (var file in files)
                {
                    if (options.HasOption(new OpenOption()))
                    {
                        FileHelper.OpenWithAssociatedProgram(file.Path);
                    }

                    table.Append(file.Hash.Result.ShortHash(), file.CreatedAt, file.Size.HumanSize(), file.Path);
                }
                table.WriteTo(_consoleWriter);
            }
            else
            {
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
