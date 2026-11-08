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
internal class AddTagCommand(ITag tagStore, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    private static readonly ArgumentDefinition Tags = new() { Name = "tag", Description = "The tags to add, for example ':tax:2018'.", Arity = Arity.OneOrMore };

    public CommandGrouping CommandGrouping => CommandGrouping.Tag;
    public string Name => "add-tags";
    public string Description => "Adds a tag without files.";
    public string Details => "A tag starts with a colon, and colons separate its levels: ':tax:2018' is the tag '2018' under ':tax'. A level can contain letters, digits, '-', '_', '+' and '.'. Tagging files creates their tags as well.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [Tags];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(input.Values(Tags).Aggregate(true, (added, tag) => AddTag(tag) && added));

    private bool AddTag(string text)
        => Tag.Parse(text).Match(left: Report, right: Store);

    private bool Store(Tag tag)
    {
        tagStore.Store(tag);

        return true;
    }

    private bool Report(string problem)
    {
        consoleWriter.WriteLine(problem);

        return false;
    }
}
