using SortingHat.API.DI;
using SortingHat.API.Models;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class AddTagCommand : ICommand
    {
        private readonly TagParser _tagParser;

        public AddTagCommand(TagParser tagParser)
        {
            _tagParser = tagParser;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            return arguments.Select(_tagParser.Parse).Aggregate(true, (result, tag) => result & tag.Store());
        }

        public string LongCommand => "add-tags";
        public string ShortCommand => null;
        public string ShortHelp => "This adds a tag without any associated files to the db.";

    }
}
