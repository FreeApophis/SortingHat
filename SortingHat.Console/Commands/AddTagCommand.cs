﻿using SortingHat.API.DI;
using SortingHat.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SortingHat.CLI.Commands
{
    class AddTagCommand : ICommand
    {
        private readonly IServices _services;

        public AddTagCommand(IServices services)
        {
            _services = services;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            foreach (var tagString in arguments.Skip(2))
            {
                var tag = Tag.Parse(tagString);

                return tag.Store(_services);
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
                    return arguments.Skip(1).First() == "add";
                }
            }

            return false;
        }
    }
}
