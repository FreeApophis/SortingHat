using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;
using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;
using SortingHat.CliAbstractions;
using SortingHat.CliAbstractions.Formatting;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class ListTagsCommand : ICommand
    {
        private readonly IProjectDatabase _db;
        private readonly IConsoleWriter _consoleWriter;

        public ListTagsCommand(IProjectDatabase db, IConsoleWriter consoleWriter)
        {
            _db = db;
            _consoleWriter = consoleWriter;
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            var tags = Tag.List(_db).ToList();
            if (tags.Any())
            {
                _consoleWriter.WriteLine("Used tags: ");

                var table = TagsTable();
                foreach (var tag in tags)
                {
                    table.Append($"* {tag.FullName}", $"({tag.FileCount})");
                }
                table.WriteTo(_consoleWriter);
            } else
            {
                _consoleWriter.WriteLine("There are currently no tags in the database.");
            }

            return true;
        }

        private ConsoleTable TagsTable()
        {
            var table = new ConsoleTable(2);

            return table;
        }

        public string LongCommand => "list-tags";
        public Option<string> ShortCommand => Option.Some("tags");
        public string ShortHelp => "Lists all avaialable tags in hierarchical form";
        public CommandGrouping CommandGrouping => CommandGrouping.Tag;

    }
}
