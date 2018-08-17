using SortingHat.API.DI;
using SortingHat.API.Models;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.CLI.Commands
{
    class AddTagCommand : ICommand
    {
        private readonly IDatabase _db;

        public AddTagCommand(IDatabase db)
        {
            _db = db;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            return arguments.Select(Tag.Parse).Aggregate(true, (result, tag) => result & tag.Store(_db));
        }

        public string LongCommand => "add-tags";
        public string ShortCommand => null;
        public string ShortHelp => "";

    }
}
