using SortingHat.API.DI;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SortingHat.CLI.Commands
{
    class FindFilesCommand : ICommand
    {
        private readonly IDatabase _db;

        public FindFilesCommand(IDatabase db)
        {
            _db = db;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            var search = string.Join(" ", arguments);
            Console.WriteLine($"Find Files: {search}");

            var files = _db.File.Search(search);

            if (files.Any())
            {

                foreach (var file in files)
                {
                    Console.WriteLine($"{FormatHelper.ShortHash(file.Hash)} {file.Size.FixedHumanSize()} {file.Path}");
                }
            }
            else
            {
                Console.WriteLine("No files found for your search query...");
            }

            return true;
        }

        public string LongCommand => "find-files";
        public string ShortCommand => null;

        public string ShortHelp => "";

    }
}
