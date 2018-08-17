using SortingHat.API.DI;
using SortingHat.API.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SortingHat.CLI.Commands
{
    internal class RenameTagCommand : ICommand
    {
        private readonly IDatabase _db;
        private readonly ILogger<RenameTagCommand> _logger;

        public RenameTagCommand(IDatabase db, ILogger<RenameTagCommand> logger)
        {
            _db = db;
            _logger = logger;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            if (arguments.Count() == 2)
            {
                var tag = Tag.Parse(arguments.First());

                if (tag.Rename(_db, arguments.Skip(1).First()) == false)
                {
                    _logger.LogWarning("Remove tag failed");
                }
            }

            return true;
        }

        public string LongCommand => "rename-tag";
        public string ShortCommand => null;

        public string ShortHelp => "Renames a tag from database";
    }
}
