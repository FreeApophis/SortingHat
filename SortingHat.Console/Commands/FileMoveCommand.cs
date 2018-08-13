using System.Collections.Generic;
using System;
using System.Linq;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands
{
    class FileMoveCommand : ICommand
    {
        private IDatabase _db;

        public FileMoveCommand(IDatabase db)
        {
            _db = db;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            var path = ".";
            var search = string.Join(" ", arguments);
            Console.WriteLine($"Find Files: {search}");

            var files = _db.File.Search(search);

            if (files.Any())
            {

                foreach (var file in files)
                {
                    System.IO.File.Move(file.Path, path);
                }
            }
            else
            {
                Console.WriteLine($"No files found for your search query...");
            }

            return true;
        }

        public string LongCommand => "move-files";
        public string ShortCommand => null;
        public string ShortHelp => "";

    }
}
