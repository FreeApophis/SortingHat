﻿using SortingHat.API.DI;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        private static string ShortHash(string hash)
        {
            return hash.Split(':')[1].Substring(0, 8);
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            var search = string.Join(" ", arguments.Skip(2));
            Console.WriteLine($"Find Files: {search}");

            var files = _db.File.Search(search);

            if (files.Any())
            {

                foreach (var file in files)
                {
                    Console.WriteLine($"{ShortHash(file.Hash)} {file.Size.FixedHumanSize()} {file.Path}");
                }
            }
            else
            {
                Console.WriteLine($"No files found for your search query...");
            }

            return true;
        }

        public bool Match(IEnumerable<string> arguments)
        {
            if (arguments.Count() > 2)
            {
                var matcher = new Regex("files?", RegexOptions.IgnoreCase);

                if (matcher.IsMatch(arguments.First()))
                {
                    return arguments.Skip(1).First() == "search";
                }
            }

            return false;
        }

        public string ShortHelp => "";

    }
}
