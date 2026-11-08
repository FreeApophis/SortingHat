using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Tags;

[UsedImplicitly]
internal class RemoveTagCommand(ITag tagStore, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    private static readonly ArgumentDefinition Tags = new() { Name = "tag", Description = "The tags to remove, for example ':tax:2018'.", Arity = Arity.OneOrMore };

    public CommandGrouping CommandGrouping => CommandGrouping.Tag;
    public string Name => "remove-tags";
    public string Description => "Removes tags from the database.";
    public string Details => "The files keep existing, they only lose the tag. A tag that still has child tags cannot be removed, remove its children first.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [Tags];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(input.Values(Tags).Aggregate(true, (removed, tag) => RemoveTag(tag) && removed));

    private bool RemoveTag(string text)
        => Tag.Parse(text).Match(
            left: Report,
            right: tag => tagStore.Destroy(tag) || Report($"The tag '{text}' does not exist or still has child tags."));

    private bool Report(string problem)
    {
        consoleWriter.WriteLine(problem);

        return false;
    }
}
