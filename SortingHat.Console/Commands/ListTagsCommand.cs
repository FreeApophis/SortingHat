using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;
using System.Collections.Generic;
using Funcky.Monads;
using SortingHat.ConsoleWriter;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class ListTagsCommand : ICommand
    {
        private readonly IDatabase _db;
        private readonly IConsoleWriter _consoleWriter;

        public ListTagsCommand(IDatabase db, IConsoleWriter consoleWriter)
        {
            _db = db;
            _consoleWriter = consoleWriter;
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            _consoleWriter.WriteLine("Used tags: ");
            foreach (var tag in Tag.List(_db))
            {
                _consoleWriter.WriteLine($"* {tag.FullName}  ({tag.FileCount})");
            }
            return true;
        }

        public string LongCommand => "list-tags";
        public Option<string> ShortCommand => Option.Some("tags");
        public string ShortHelp => "Lists all avaialable tags in hierarchical form";
        public CommandGrouping CommandGrouping => CommandGrouping.Tag;

    }
}
