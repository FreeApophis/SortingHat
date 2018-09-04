using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using SortingHat.API.Models;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class RemoveTagCommand : ICommand
    {
        private readonly ITagParser _tagParser;
        private readonly IDatabase _db;
        private readonly ILogger<RemoveTagCommand> _logger;

        public RemoveTagCommand(ITagParser tagParser, IDatabase db, ILogger<RemoveTagCommand> logger)
        {
            _tagParser = tagParser;
            _db = db;
            _logger = logger;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            foreach (var tagString in arguments)
            {
                var tag = _tagParser.Parse(tagString);

                if (tag.Destroy() == false)
                {
                    _logger.LogWarning("Remove tag failed");
                }


            }

            return true;
        }

        public string LongCommand => "remove-tags";
        public string ShortCommand => null;
        public string ShortHelp => "Removes a tag from database";
        public CommandGrouping CommandGrouping => CommandGrouping.Tag;
    }
}
