using Autofac;
using SortingHat.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;

namespace SortingHat.CLI.Commands
{
    class ListTagsCommand : ICommand
    {
        private readonly IContainer _container;

        public ListTagsCommand(IContainer container)
        {
            _container = container;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            Console.WriteLine("Used tags: ");
            foreach (var tag in Tag.List(_container))
            {
                Console.WriteLine($"* {tag.FullName}");
            }
            return true;
        }

        public bool Match(IEnumerable<string> arguments)
        {
            if (arguments.Count() >= 2)
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
