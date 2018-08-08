using SortingHat.CLI;
using SortingHat.API.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Autofac;

namespace SortingHat.CLI.Commands
{
    class FindFilesCommand : ICommand
    {
        private readonly IContainer _container;

        public FindFilesCommand(IContainer container)
        {
            _container = container;
        }

        private static string ShortHash(string hash)
        {
            return hash.Split(':')[1].Substring(0, 8);
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            var search = string.Join(" ", arguments.Skip(2));
            Console.WriteLine($"Find Files: {search}");

            using (var scope = _container.BeginLifetimeScope())
            {
                var db = scope.Resolve<IDatabase>();
                var files = db.File.Search(search);

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
    }
}
