using System.Collections.Generic;
using System.Collections.Immutable;
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
internal class MoveTagCommand(ITag tagStore, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    private const string TopLevel = ":";

    /// <summary>Only the last argument can take many values, so the sources and the destination share one argument.</summary>
    private static readonly ArgumentDefinition Tags = new() { Name = "tags", Description = "The tags to move, followed by their new parent tag, or ':' for the top level.", Arity = Arity.OneOrMore };

    public CommandGrouping CommandGrouping => CommandGrouping.Tag;
    public string Name => "move-tags";
    public string Description => "Moves tags with their children and files under another parent tag.";

    public string Details
        => """
           The last tag is the new parent: 'move-tags :a :b :destination' turns ':a' into ':destination:a' and ':b' into ':destination:b'.
           ':' as the last tag moves them to the top level: 'move-tags :tax:2018 :' turns ':tax:2018' into ':2018'.
           Either all tags move or none.
           """;

    public IReadOnlyList<ArgumentDefinition> Arguments => [Tags];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(input.Values(Tags).ToArray() switch
        {
            [.. var sources, var destination] when sources.Length > 0 => MoveTags(sources, destination),
            _ => Report("Give the destination tag after the tags to move, for example 'move-tags :a :destination'."),
        });

    private static Either<string, Option<Tag>> Destination(string text)
        => text == TopLevel
            ? Either<string, Option<Tag>>.Right(Option<Tag>.None)
            : Tag.Parse(text).Select(Option.Some);

    private static Either<string, ImmutableList<Tag>> Sources(IEnumerable<string> texts)
        => texts.Aggregate(
            Either<string, ImmutableList<Tag>>.Right([]),
            (parsed, text) => from tags in parsed
                              from tag in Tag.Parse(text)
                              select tags.Add(tag));

    private bool MoveTags(IEnumerable<string> sourceTexts, string destinationText)
        => (from destination in Destination(destinationText)
            from sources in Sources(sourceTexts)
            from moved in tagStore.Move(sources, destination)
            select $"Moved {string.Join(", ", sources.Select(tag => $"'{tag}'"))} to {destination.Match(none: "the top level", some: tag => $"'{tag}'")}.")
            .Match(left: Report, right: Done);

    private bool Done(string message)
    {
        consoleWriter.WriteLine(message);

        return true;
    }

    private bool Report(string problem)
    {
        consoleWriter.WriteLine(problem);

        return false;
    }
}
