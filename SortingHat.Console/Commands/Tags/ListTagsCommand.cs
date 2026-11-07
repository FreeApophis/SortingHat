using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Tags;

[UsedImplicitly]
internal class ListTagsCommand(ITag tagStore, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.Tag;
    public string Name => "list-tags";
    public IReadOnlyList<string> Aliases => ["tags"];
    public string Description => "Lists all available tags in hierarchical form";
    public string Details => "The number after each tag counts the files tagged with exactly that tag.";

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        var tags = tagStore.GetTags();
        if (tags.Count > 0)
        {
            consoleWriter.WriteLine("Used tags: ");

            var table = new ConsoleTable(2);
            foreach (var tag in tags)
            {
                table.Append($"* {tag.FullName}", $"({tagStore.FileCount(tag)})");
            }

            table.WriteTo(consoleWriter);
        }
        else
        {
            consoleWriter.WriteLine("There are currently no tags in the database.");
        }

        return Task.FromResult(true);
    }
}
