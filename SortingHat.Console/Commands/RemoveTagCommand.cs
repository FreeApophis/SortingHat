﻿using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using SortingHat.API.Models;
using System.Collections.Generic;
using System.Linq;

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
            foreach (var tagString in arguments)
            {
                var tag = Tag.Parse(tagString);

                tag.Destroy(_db);
            }

            return true;
        }

        public string LongCommand => "remove-tags";
        public string ShortCommand => null;
        public string ShortHelp => "Removes a tag from database";

    }
}
