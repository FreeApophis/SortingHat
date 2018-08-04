using SortingHat.API.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SortingHat.CLI.Commands
{
    class FindFilesCommand : ICommand
    {
        private readonly IServices _services;

        public FindFilesCommand(IServices services)
        {
            _services = services;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            var search = string.Join(" ", arguments.Skip(2));
            Console.WriteLine($"Find Files: {search}");

            _services.DB.File.Search(arguments.First());
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
