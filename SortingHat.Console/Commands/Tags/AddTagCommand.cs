using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Tags
{
    [UsedImplicitly]
    internal class AddTagCommand : ICommand
    {
        private readonly ITagParser _tagParser;

        public AddTagCommand(ITagParser tagParser)
        {
            _tagParser = tagParser;
        }

        public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        {
            return arguments.Select(_tagParser.Parse).Aggregate(true, (result, tag) => result & (tag != null && tag.Store()));
        }

        public string LongCommand => "add-tags";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "This adds a tag without any associated files to the db.";
        public CommandGrouping CommandGrouping => CommandGrouping.Tag;
    }
}
