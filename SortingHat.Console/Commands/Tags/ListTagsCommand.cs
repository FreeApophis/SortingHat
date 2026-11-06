using System.Collections.Generic;
using System.Linq;
using Apophis.CLI;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Tags;

[UsedImplicitly]
internal class ListTagsCommand(ITag tagStore, IConsoleWriter consoleWriter) : ICommand
{
    public string LongCommand => "list-tags";
    public Option<string> ShortCommand => Option.Some("tags");
    public string ShortHelp => "Lists all available tags in hierarchical form";
    public CommandGrouping CommandGrouping => CommandGrouping.Tag;

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        var tags = Tag.List(tagStore).ToList();
        if (tags.Any())
        {
            consoleWriter.WriteLine("Used tags: ");

            var table = TagsTable();
            foreach (var tag in tags)
            {
                table.Append($"* {tag.FullName}", $"({tag.FileCount})");
            }

            table.WriteTo(consoleWriter);
        }
        else
        {
            consoleWriter.WriteLine("There are currently no tags in the database.");
        }

        return true;
    }

    private ConsoleTable TagsTable()
    {
        var table = new ConsoleTable(2);

        return table;
    }
}
