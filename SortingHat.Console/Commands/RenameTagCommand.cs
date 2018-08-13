using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using SortingHat.API.Models;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.CLI.Commands
{
    class RenameTagCommand : ICommand
    {
        private readonly ILogger<RenameTagCommand> _logger;
        private readonly IDatabase _db;

        public RenameTagCommand(ILogger<RenameTagCommand> logger, IDatabase db)
        {
            _logger = logger;
            _db = db;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            if (arguments.Count() == 2)
            {
                var tag = Tag.Parse(arguments.First());

                tag.Rename(_db, arguments.Skip(1).First());
            }

            return true;
        }

        public string LongCommand => "rename-tag";
        public string ShortCommand => null;

        public string ShortHelp => "Renames a tag from database";
    }
}
