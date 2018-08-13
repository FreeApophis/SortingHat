using SortingHat.API.DI;
using SortingHat.API.Models;
using System.Collections.Generic;
using System;

namespace SortingHat.CLI.Commands
{
    class ListTagsCommand : ICommand
    {
        private readonly IDatabase _db;

        public ListTagsCommand(IDatabase db)
        {
            _db = db;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            Console.WriteLine("Used tags: ");
            foreach (var tag in Tag.List(_db))
            {
                Console.WriteLine($"* {tag.FullName}");
            }
            return true;
        }

        public string LongCommand => "list-tags";
        public string ShortCommand => null;
        public string ShortHelp => "";

    }
}
