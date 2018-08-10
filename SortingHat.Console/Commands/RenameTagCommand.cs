using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands
{
    class RenameTagCommand : ICommand
    {
        private const string Command = "tag rename";
        private readonly ILogger<RenameTagCommand> _logger;
        private readonly IDatabase _db;

        public RenameTagCommand(ILogger<RenameTagCommand> logger, IDatabase db)
        {
            _logger = logger;
            _db = db;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            if (arguments.Count() == 4)
            {
                var tag = Tag.Parse(arguments.Skip(2).First());

                tag.Rename(_db, arguments.Skip(3).First());
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
                    return arguments.Skip(1).First() == "rename";
                }
            }

            return false;
        }

        public string ShortHelp => "Renames a tag from database";

    }
}
