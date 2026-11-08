using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using JetBrains.Annotations;
using SortingHat.API.DI;
using Spectre.Console;

namespace SortingHat.CLI.Commands.Tags;

[UsedImplicitly]
internal class ListTagsCommand(ITag tagStore, IConsoleWriter consoleWriter, IAnsiConsole console) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.Tag;
    public string Name => "list-tags";
    public IReadOnlyList<string> Aliases => ["tags"];
    public string Description => "Lists all tags with the number of their files.";
    public string Details => "The number after each tag counts the files tagged with exactly that tag.";

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        var tags = tagStore.GetTags();
        if (tags.Count > 0)
        {
            var counts = tagStore.FileCounts();
            var table = LightTable.Create(LightTable.Column("Tag"), LightTable.Column("Files").RightAligned());
            tags.ForEach(tag => table.AddCells(tag.FullName, $"{counts.GetValueOrNone(tag).GetOrElse(0L)}"));

            console.Write(table);
        }
        else
        {
            consoleWriter.WriteLine("There are currently no tags in the database.");
        }

        return Task.FromResult(true);
    }
}
