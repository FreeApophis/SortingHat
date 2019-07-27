using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.CliAbstractions;
using SortingHat.CliAbstractions.Formatting;

namespace SortingHat.CLI.Commands.Tags
{
    [UsedImplicitly]
    internal class ListTagsCommand : ICommand
    {
        private readonly ITag _tag;
        private readonly IConsoleWriter _consoleWriter;

        public ListTagsCommand(ITag tag, IConsoleWriter consoleWriter)
        {
            _tag = tag;
            _consoleWriter = consoleWriter;
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            var tags = Tag.List(_tag).ToList();
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
