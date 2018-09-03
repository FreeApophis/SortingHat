﻿using SortingHat.API.DI;
using System.Collections.Generic;
using System.Linq;
using System;
using JetBrains.Annotations;
using SortingHat.CLI.Output;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class FindFilesCommand : ICommand
    {
        private readonly IDatabase _db;

        public FindFilesCommand(IDatabase db)
        {
            _db = db;
        }

        private ConsoleTable FileTable()
        {
            var table = new ConsoleTable(4);

            return table;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            var search = string.Join(" ", arguments);
            Console.WriteLine($"Find Files: {search}");

            var files = _db.File.Search(search);

            if (files.Any())
            {
                var table = FileTable();
                foreach (var file in files)
                {
                    table.Append(file.Hash.Result.ShortHash(), file.CreatedAt, file.Size.HumanSize(),  file.Path);
                }
                Console.WriteLine(table.ToString());
            }
            else
            {
                Console.WriteLine("No files found for your search query...");
            }

            return true;
        }

        public string LongCommand => "find-files";
        public string ShortCommand => "ff";

        public string ShortHelp => "Finds all files matching the search query";

    }
}
