﻿using JetBrains.Annotations;
using SortingHat.API.DI;
using System;
using System.Collections.Generic;
using System.IO;
using Funcky.Monads;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class RepairCommand : ICommand
    {
        private readonly IDatabase _db;

        public RepairCommand(IDatabase db)
        {
            _db = db;
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
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
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Check each path locked in the database if the file still exists and is not corrupted / changed";
        public CommandGrouping CommandGrouping => CommandGrouping.General;
    }
}
