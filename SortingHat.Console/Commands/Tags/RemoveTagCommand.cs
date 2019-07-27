using System.Collections.Generic;
using Funcky.Monads;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Tags
{
    [UsedImplicitly]
    internal class RemoveTagCommand : ICommand
    {
        private readonly ITagParser _tagParser;
        private readonly IMainDatabase _db;
        private readonly ILogger<RemoveTagCommand> _logger;

        public RemoveTagCommand(ITagParser tagParser, IMainDatabase db, ILogger<RemoveTagCommand> logger)
        {
            _tagParser = tagParser;
            _db = db;
            _logger = logger;
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            foreach (var tagString in arguments)
            {
                var tag = _tagParser.Parse(tagString);

                if (tag is null)
                {
                    _logger.LogWarning($"Remove tag '{tagString}' failed (parse)");
                }
                else if (tag.Destroy() == false)
                {
                    _logger.LogWarning($"Remove tag '{tagString}' failed (db)");
                }
            }

            return true;
        }

        public string LongCommand => "remove-tags";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Removes a tag from database";
        public CommandGrouping CommandGrouping => CommandGrouping.Tag;
    }
}
