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
            bool result = true;

            foreach (var tagString in arguments)
            {
                var tag = Tag.Parse(tagString);

                result &= tag.Store(_db);
            }

            return result;
        }

        public string LongCommand => "add-tag";
        public string ShortCommand => null;
        public string ShortHelp => "";

    }
}
