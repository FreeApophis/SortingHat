using SortingHat.API.DI;
using SortingHat.API.Models;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class RenameTagCommand : ICommand
    {
        private readonly ILogger<RenameTagCommand> _logger;
        private readonly ITagParser _tagParser;

        public RenameTagCommand(ILogger<RenameTagCommand> logger, ITagParser tagParser)
        {
            _logger = logger;
            _tagParser = tagParser;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            if (arguments.Count() == 2)
            {
                var tag = _tagParser.Parse(arguments.First());

                if (tag.Rename(arguments.Skip(1).First()) == false)
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
