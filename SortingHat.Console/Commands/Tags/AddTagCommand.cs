using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Tags;

[UsedImplicitly]
internal class AddTagCommand(ITagParser tagParser, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    private static readonly ArgumentDefinition Tags = new() { Name = "tag", Description = "The tags to add, for example ':tax:2018'.", Arity = Arity.OneOrMore };

    public CommandGrouping CommandGrouping => CommandGrouping.Tag;
    public string Name => "add-tags";
    public string Description => "This adds a tag without any associated files to the db.";
    public string Details => "A tag starts with a colon, and colons separate its levels: ':tax:2018' is the tag '2018' under ':tax'. Tagging files creates their tags as well.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [Tags];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(input.Values(Tags).Aggregate(true, (added, tag) => AddTag(tag) && added));

    private bool AddTag(string text)
        => Option.FromNullable(tagParser.Parse(text)).Match(
            none: () => Report($"'{text}' is not a tag, a tag starts with a colon, for example ':tax:2018'."),
            some: tag => tag.Store());

    private bool Report(string problem)
    {
        consoleWriter.WriteLine(problem);

        return false;
    }
}
