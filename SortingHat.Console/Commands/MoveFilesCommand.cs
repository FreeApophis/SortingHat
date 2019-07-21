﻿using JetBrains.Annotations;
using SortingHat.API.DI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Funcky.Monads;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class MoveFilesCommand : ICommand
    {
        private readonly IDatabase _db;

        public MoveFilesCommand(IDatabase db)
        {
            _db = db;
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            if (arguments.Count() != 2) throw new ArgumentOutOfRangeException(nameof(arguments));

            var search = arguments.First();
            var path = Path.Combine(Directory.GetCurrentDirectory(), arguments.Last());

            Console.WriteLine($"Find Files: {search}");

            var files = _db.File.Search(search);

            if (files.Any())
            {
                foreach (var file in files)
                {
                    File.Move(file.Path, Path.Combine(path, Path.GetFileName(file.Path)));
                }
            }
            else
            {
                Console.WriteLine("No files found for your search query...");
            }

            return true;
        }

        public string LongCommand => "move-files";
        public Option<string> ShortCommand => Option.Some("mv");
        public string ShortHelp => "This moves all files which match the search query to a specified folder location";
        public CommandGrouping CommandGrouping => CommandGrouping.File;

    }
}
