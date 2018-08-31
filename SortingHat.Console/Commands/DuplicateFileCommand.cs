using SortingHat.API.DI;
using SortingHat.API.Models;
using System;
using System.Collections.Generic;

namespace SortingHat.CLI.Commands
{
    class DuplicateFileCommand : ICommand
    {
        private readonly IDatabase _db;

        public string LongCommand => "duplicate";
        public string ShortCommand => "d";
        public string ShortHelp => "Find duplicate files in your dms";

        public DuplicateFileCommand(IDatabase db)
        {
            _db = db;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            foreach (var file in _db.File.GetDuplicates())
            {
                Console.WriteLine($"Duplicate:");
                Console.WriteLine($"CreatedAt (oldest): {file.CreatedAt}");
                Console.WriteLine($"File Size: {file.Size}");
                Console.WriteLine($"File Hash: {file.Hash.Result}");

                foreach (var tag in file.GetTags())
                {
                    Console.WriteLine($"Tag: {tag.FullName}");
                }

                foreach (var name in file.GetNames())
                {
                    Console.WriteLine($"Name: {name}");
                }

                foreach (var path in file.GetPaths())
                {
                    Console.WriteLine($"Path: {path}");
                }

            }

            return true;
        }
    }
}
