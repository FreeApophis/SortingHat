using SortingHat.API.DI;
using SortingHat.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;

namespace SortingHat.CLI.Commands
{
    class ListTagsCommand : ICommand
    {
        private IServices _services;

        public ListTagsCommand(IServices services)
        {
            _services = services;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            Console.WriteLine("Used tags: ");
            foreach (var tag in Tag.List())
            {
                Console.WriteLine($"* {tag.FullName}");
            }
            return true;
        }

        public bool Match(IEnumerable<string> arguments)
        {
            if (arguments.Count() > 2)
            {
                var matcher = new Regex("tags?", RegexOptions.IgnoreCase);

                if (matcher.IsMatch(arguments.First()))
                {
                    return arguments.Skip(1).First() == "list";
                }
            }

            return false;
        }
    }
}
