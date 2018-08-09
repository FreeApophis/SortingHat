using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using SortingHat.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SortingHat.CLI.Commands
{
    class RemoveTagCommand : ICommand
    {
        private readonly IDatabase _db;
        private readonly ILogger<RemoveTagCommand> _logger;

        public RemoveTagCommand(IDatabase db, ILogger<RemoveTagCommand> logger)
        {
            _db = db;
            _logger = logger;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            foreach (var tagString in arguments.Skip(2))
            {
                var tag = Tag.Parse(tagString);

                tag.Destroy(_db);
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
                    return arguments.Skip(1).First() == "remove";
                }
            }

            return false;
        }

        public string ShortHelp => "Removes a tag from database";
    }
}
