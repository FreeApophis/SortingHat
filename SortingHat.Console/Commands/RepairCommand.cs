using System.Collections.Generic;
using System;
using System.IO;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands
{
    internal class RepairCommand : ICommand
    {
        private readonly IDatabase _db;

        public RepairCommand(IDatabase db)
        {
            _db = db;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            foreach (var path in _db.File.GetPaths())
            {
                if (File.Exists(path) == false)
                {
                    Console.WriteLine($"File '{path}' does not exist, removed from index");
                }
            }

            return true;
        }

        public string LongCommand => "repair";
        public string ShortCommand => null;

        public string ShortHelp => "Check each path locked in the database if the file still exists and is not corrupted / changed";
    }
}
